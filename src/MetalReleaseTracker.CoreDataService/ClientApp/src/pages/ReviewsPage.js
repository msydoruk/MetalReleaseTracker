import { useState, useEffect, useCallback, useRef } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import InputAdornment from '@mui/material/InputAdornment';
import { DataGrid } from '@mui/x-data-grid';
import DeleteIcon from '@mui/icons-material/Delete';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '../components/PageHeader';
import { fetchReviews, deleteReview } from '../api/reviews';

export default function ReviewsPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deletingRow, setDeletingRow] = useState(null);
  const [saving, setSaving] = useState(false);
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
      const { data } = await fetchReviews(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load reviews', 'error');
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

  const handleOpenDelete = useCallback((row) => {
    setDeletingRow(row);
    setDeleteDialogOpen(true);
  }, []);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteReview(deletingRow.id);
      showSnackbar('Review deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete review', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const columns = [
    { field: 'userName', headerName: 'UserName', flex: 0.6, minWidth: 140 },
    {
      field: 'message',
      headerName: 'Message',
      flex: 1.5,
      minWidth: 300,
      renderCell: (params) => {
        const text = params.value || '';
        return (
          <Tooltip title={text.length > 100 ? text : ''} placement="bottom-start">
            <Box
              sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                width: '100%',
              }}
            >
              {text.length > 100 ? `${text.substring(0, 100)}...` : text}
            </Box>
          </Tooltip>
        );
      },
    },
    {
      field: 'createdDate',
      headerName: 'Created',
      width: 140,
      valueFormatter: (value) =>
        value ? new Date(value).toLocaleDateString() : '--',
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 100,
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <Box>
          <Tooltip title="Delete">
            <IconButton size="small" color="error" onClick={() => handleOpenDelete(params.row)}>
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader title="Reviews" subtitle="Moderate reviews" />

      <Box sx={{ mb: 2 }}>
        <TextField
          placeholder="Search by username or content..."
          size="small"
          onChange={handleSearchChange}
          sx={{ width: 320 }}
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

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Review</DialogTitle>
        <DialogContent>
          Are you sure you want to delete this review by <strong>{deletingRow?.userName}</strong>? This action cannot be undone.
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleDelete} disabled={saving}>
            {saving ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar */}
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
