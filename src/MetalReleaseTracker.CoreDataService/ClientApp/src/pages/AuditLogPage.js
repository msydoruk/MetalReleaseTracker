import { useState, useEffect, useCallback, useRef } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import { DataGrid } from '@mui/x-data-grid';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '../components/PageHeader';
import { fetchAuditLogs } from '../api/auditLog';

const getMethodColor = (method) => {
  switch (method) {
    case 'POST': return 'success';
    case 'PUT': return 'warning';
    case 'DELETE': return 'error';
    default: return 'default';
  }
};

const getStatusColor = (code) => {
  if (code >= 200 && code < 300) return 'success';
  if (code >= 400 && code < 500) return 'warning';
  if (code >= 500) return 'error';
  return 'default';
};

const columns = [
  {
    field: 'timestamp',
    headerName: 'Timestamp',
    width: 180,
    valueFormatter: (value) => {
      if (!value) return '';
      return new Date(value).toLocaleString();
    },
  },
  { field: 'userName', headerName: 'User', flex: 0.6, minWidth: 120 },
  {
    field: 'httpMethod',
    headerName: 'Method',
    width: 100,
    align: 'center',
    headerAlign: 'center',
    renderCell: (params) => (
      <Chip
        label={params.value}
        size="small"
        color={getMethodColor(params.value)}
        variant="outlined"
        sx={{ fontWeight: 600, minWidth: 65 }}
      />
    ),
  },
  { field: 'action', headerName: 'Action', flex: 0.8, minWidth: 150 },
  { field: 'entityType', headerName: 'Entity', flex: 0.5, minWidth: 100 },
  { field: 'entityId', headerName: 'Entity ID', flex: 0.7, minWidth: 120 },
  {
    field: 'responseStatusCode',
    headerName: 'Status',
    width: 90,
    align: 'center',
    headerAlign: 'center',
    renderCell: (params) => (
      <Chip
        label={params.value}
        size="small"
        color={getStatusColor(params.value)}
        variant="outlined"
      />
    ),
  },
];

export default function AuditLogPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const searchTimeoutRef = useRef(null);

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const params = {
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
      };
      if (search) {
        params.search = search;
      }
      const { data } = await fetchAuditLogs(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load audit logs', 'error');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search, showSnackbar]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSearchChange = useCallback((e) => {
    const value = e.target.value;
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    searchTimeoutRef.current = setTimeout(() => {
      setSearch(value);
      setPaginationModel((prev) => ({ ...prev, page: 0 }));
    }, 400);
  }, []);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader title="Audit Log" subtitle="Admin write operations history" />

      <Box sx={{ mb: 2 }}>
        <TextField
          placeholder="Search actions, users, paths..."
          size="small"
          onChange={handleSearchChange}
          sx={{ width: 360 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" sx={{ color: 'rgba(255,255,255,0.4)' }} />
                </InputAdornment>
              ),
            },
          }}
        />
      </Box>

      <Box sx={{ flexGrow: 1, minHeight: 0 }}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          rowCount={totalRows}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 25, 50]}
          disableRowSelectionOnClick
          autoHeight={false}
          sx={{ height: '100%' }}
        />
      </Box>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
