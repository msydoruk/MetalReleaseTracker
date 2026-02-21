import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import Chip from '@mui/material/Chip';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import SyncIcon from '@mui/icons-material/Sync';
import { DataGrid } from '@mui/x-data-grid';
import StatusChip from '../components/StatusChip';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import FilterBar from '../components/FilterBar';
import { PARSING_STATUS, DISTRIBUTOR_CODES } from '../constants';
import { fetchParsingSessions, updateParsingSessionStatus } from '../api/parsingSessions';

const SORT_FIELD_MAP = { distributorCode: 0, lastUpdatedDate: 1, parsingStatus: 2, eventCount: 3 };

export default function ParsingSessionsPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);
  const distributorCode = searchParams.get('distributorCode') || '';
  const parsingStatus = searchParams.get('parsingStatus') || '';
  const sortBy = searchParams.get('sortBy') || '';
  const sortAscending = searchParams.get('sortAscending') !== 'false';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (distributorCode) params.distributorCode = distributorCode;
      if (parsingStatus !== '') params.parsingStatus = parsingStatus;
      if (sortBy && SORT_FIELD_MAP[sortBy] !== undefined) {
        params.sortBy = SORT_FIELD_MAP[sortBy];
        params.sortAscending = sortAscending;
      }
      const { data } = await fetchParsingSessions(params);
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load parsing sessions', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, distributorCode, parsingStatus, sortBy, sortAscending]);

  useEffect(() => { load(); }, [load]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => { if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k]; });
    setSearchParams(params);
  };

  const handleStatusChange = async (id, newStatus) => {
    try {
      await updateParsingSessionStatus(id, newStatus);
      setSnackbar({ open: true, message: 'Status updated', severity: 'success' });
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to update status', severity: 'error' });
    }
  };

  const columns = [
    {
      field: 'distributorCode',
      headerName: 'Distributor',
      width: 180,
      renderCell: ({ value }) => <DistributorChip code={value} />,
    },
    {
      field: 'lastUpdatedDate',
      headerName: 'Last Updated',
      width: 180,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
    {
      field: 'parsingStatus',
      headerName: 'Status',
      width: 170,
      renderCell: ({ row }) => (
        <FormControl size="small" fullWidth>
          <Select
            value={row.parsingStatus}
            onChange={(e) => handleStatusChange(row.id, e.target.value)}
            size="small"
            sx={{
              fontSize: '0.8rem',
              '& .MuiSelect-select': { py: 0.5 },
              backgroundColor: 'rgba(255, 255, 255, 0.03)',
            }}
          >
            {Object.entries(PARSING_STATUS).map(([val, cfg]) => (
              <MenuItem key={val} value={parseInt(val, 10)}>
                <StatusChip value={parseInt(val, 10)} statusMap={PARSING_STATUS} />
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      ),
    },
    {
      field: 'eventCount',
      headerName: 'Events',
      width: 100,
      type: 'number',
      renderCell: ({ value }) => (
        <Chip
          label={value}
          size="small"
          sx={{ backgroundColor: 'rgba(183, 28, 28, 0.15)', color: '#e53935', fontWeight: 600, minWidth: 32 }}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        icon={<SyncIcon />}
        title="Parsing Sessions"
        subtitle={rowCount > 0 ? `${rowCount} sessions` : undefined}
      />
      <FilterBar>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Distributor</InputLabel>
          <Select
            value={distributorCode}
            label="Distributor"
            onChange={(e) => updateParams({ distributorCode: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(DISTRIBUTOR_CODES).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={parsingStatus}
            label="Status"
            onChange={(e) => updateParams({ parsingStatus: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(PARSING_STATUS).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </FilterBar>
      <DataGrid
        rows={rows}
        columns={columns}
        rowCount={rowCount}
        loading={loading}
        paginationMode="server"
        sortingMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(m) => updateParams({ page: m.page + 1, pageSize: m.pageSize })}
        onSortModelChange={(m) => {
          if (m.length) updateParams({ sortBy: m[0].field, sortAscending: m[0].sort === 'asc' });
          else updateParams({ sortBy: '', sortAscending: '' });
        }}
        pageSizeOptions={[10, 25, 50]}
        onRowClick={(params) => navigate(`/parsing-sessions/${params.id}`)}
        sx={{ height: 'calc(100vh - 260px)', cursor: 'pointer' }}
        disableRowSelectionOnClick
      />
      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
      >
        <Alert severity={snackbar.severity} variant="filled">{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
