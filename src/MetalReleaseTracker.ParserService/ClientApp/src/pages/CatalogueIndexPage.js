import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import InputAdornment from '@mui/material/InputAdornment';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import SearchIcon from '@mui/icons-material/Search';
import ListAltIcon from '@mui/icons-material/ListAlt';
import { DataGrid } from '@mui/x-data-grid';
import StatusChip from '../components/StatusChip';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import FilterBar from '../components/FilterBar';
import { CATALOGUE_INDEX_STATUS, DISTRIBUTOR_CODES, MEDIA_TYPES } from '../constants';
import { fetchCatalogueIndex, updateCatalogueIndexStatus, batchUpdateCatalogueIndexStatus } from '../api/catalogueIndex';

const SORT_FIELD_MAP = { bandName: 0, albumTitle: 1, distributorCode: 2, status: 3, createdAt: 4, updatedAt: 5 };

export default function CatalogueIndexPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [selectedIds, setSelectedIds] = useState([]);
  const [batchStatus, setBatchStatus] = useState('');
  const [search, setSearch] = useState(searchParams.get('search') || '');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);
  const distributorCode = searchParams.get('distributorCode') || '';
  const status = searchParams.get('status') || '';
  const sortBy = searchParams.get('sortBy') || '';
  const sortAscending = searchParams.get('sortAscending') !== 'false';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (distributorCode) params.distributorCode = distributorCode;
      if (status !== '') params.status = status;
      if (sortBy && SORT_FIELD_MAP[sortBy] !== undefined) {
        params.sortBy = SORT_FIELD_MAP[sortBy];
        params.sortAscending = sortAscending;
      }
      const { data } = await fetchCatalogueIndex(params);
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load catalogue index', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, distributorCode, status, sortBy, sortAscending]);

  useEffect(() => { load(); }, [load]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => { if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k]; });
    setSearchParams(params);
  };

  const handleStatusChange = async (id, newStatus) => {
    try {
      await updateCatalogueIndexStatus(id, newStatus);
      setSnackbar({ open: true, message: 'Status updated', severity: 'success' });
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to update status', severity: 'error' });
    }
  };

  const handleBatchUpdate = async () => {
    if (!selectedIds.length || batchStatus === '') return;
    try {
      await batchUpdateCatalogueIndexStatus(selectedIds, parseInt(batchStatus, 10));
      setSnackbar({ open: true, message: `Updated ${selectedIds.length} entries`, severity: 'success' });
      setSelectedIds([]);
      setBatchStatus('');
      load();
    } catch {
      setSnackbar({ open: true, message: 'Batch update failed', severity: 'error' });
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
      field: 'bandName',
      headerName: 'Band',
      flex: 1,
      minWidth: 150,
      renderCell: ({ value }) => (
        <Box sx={{ fontWeight: 500, color: 'rgba(255, 255, 255, 0.95)' }}>{value}</Box>
      ),
    },
    {
      field: 'albumTitle',
      headerName: 'Album',
      flex: 1,
      minWidth: 150,
    },
    {
      field: 'mediaType',
      headerName: 'Media',
      width: 80,
      renderCell: ({ value }) => {
        const label = value !== null && value !== undefined ? (MEDIA_TYPES[value] || value) : null;
        return label ? (
          <Chip label={label} size="small" variant="outlined" sx={{ borderColor: 'rgba(255,255,255,0.15)', fontSize: '0.7rem' }} />
        ) : '-';
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 170,
      renderCell: ({ row }) => (
        <FormControl size="small" fullWidth>
          <Select
            value={row.status}
            onChange={(e) => handleStatusChange(row.id, e.target.value)}
            size="small"
            sx={{
              fontSize: '0.8rem',
              '& .MuiSelect-select': { py: 0.5 },
              backgroundColor: 'rgba(255, 255, 255, 0.03)',
            }}
          >
            {Object.entries(CATALOGUE_INDEX_STATUS).map(([val, cfg]) => (
              <MenuItem key={val} value={parseInt(val, 10)}>
                <StatusChip value={parseInt(val, 10)} statusMap={CATALOGUE_INDEX_STATUS} />
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      ),
    },
    {
      field: 'bandReferenceName',
      headerName: 'Matched Band',
      width: 150,
      renderCell: ({ value }) => value ? (
        <Chip label={value} size="small" sx={{ backgroundColor: 'rgba(76, 175, 80, 0.12)', color: '#66bb6a', fontSize: '0.75rem' }} />
      ) : <Box sx={{ color: 'rgba(255,255,255,0.25)' }}>-</Box>,
    },
    {
      field: 'updatedAt',
      headerName: 'Updated',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <PageHeader
        icon={<ListAltIcon />}
        title="Catalogue Index"
        subtitle={rowCount > 0 ? `${rowCount} entries across all distributors` : undefined}
      />
      <FilterBar>
        <TextField
          size="small"
          placeholder="Search..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') updateParams({ search, page: 1 }); }}
          sx={{ width: 220 }}
          InputProps={{
            startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 20, opacity: 0.5 }} /></InputAdornment>,
          }}
        />
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
            value={status}
            label="Status"
            onChange={(e) => updateParams({ status: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(CATALOGUE_INDEX_STATUS).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
        {selectedIds.length > 0 && (
          <>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Batch Status</InputLabel>
              <Select value={batchStatus} label="Batch Status" onChange={(e) => setBatchStatus(e.target.value)}>
                {Object.entries(CATALOGUE_INDEX_STATUS).map(([val, cfg]) => (
                  <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button variant="contained" size="small" onClick={handleBatchUpdate} disabled={batchStatus === ''}>
              Update {selectedIds.length} selected
            </Button>
          </>
        )}
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
        pageSizeOptions={[10, 25, 50, 100]}
        checkboxSelection
        rowSelectionModel={selectedIds}
        onRowSelectionModelChange={(ids) => setSelectedIds(ids)}
        sx={{ flexGrow: 1, minHeight: 0, '& .MuiDataGrid-virtualScroller': { overflowX: 'hidden' } }}
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
