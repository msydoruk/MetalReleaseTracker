import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import IconButton from '@mui/material/IconButton';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import { DataGrid } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import PageHeader from '../components/PageHeader';
import {
  fetchDistributors,
  createDistributor,
  updateDistributor,
  deleteDistributor,
} from '../api/distributors';

const EMPTY_FORM = { name: '', code: '', descriptionEn: '', descriptionUa: '', country: '', countryFlag: '', logoUrl: '', websiteUrl: '' };

export default function DistributorsPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [deletingRow, setDeletingRow] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await fetchDistributors();
      setRows(data);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load distributors', 'error');
    } finally {
      setLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleOpenCreate = useCallback(() => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setDialogOpen(true);
  }, []);

  const handleOpenEdit = useCallback((row) => {
    setEditingId(row.id);
    setForm({
      name: row.name || '', code: row.code || '',
      descriptionEn: row.descriptionEn || '', descriptionUa: row.descriptionUa || '',
      country: row.country || '', countryFlag: row.countryFlag || '',
      logoUrl: row.logoUrl || '', websiteUrl: row.websiteUrl || '',
    });
    setDialogOpen(true);
  }, []);

  const handleOpenDelete = useCallback((row) => {
    setDeletingRow(row);
    setDeleteDialogOpen(true);
  }, []);

  const handleSave = useCallback(async () => {
    try {
      setSaving(true);
      if (editingId) {
        await updateDistributor(editingId, form);
        showSnackbar('Distributor updated');
      } else {
        await createDistributor(form);
        showSnackbar('Distributor created');
      }
      setDialogOpen(false);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to save distributor', 'error');
    } finally {
      setSaving(false);
    }
  }, [editingId, form, loadData, showSnackbar]);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteDistributor(deletingRow.id);
      showSnackbar('Distributor deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete distributor', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const columns = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
    { field: 'code', headerName: 'Code', flex: 0.7, minWidth: 150 },
    {
      field: 'albumCount',
      headerName: 'Album Count',
      type: 'number',
      width: 130,
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <Box>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => handleOpenEdit(params.row)}>
              <EditIcon fontSize="small" />
            </IconButton>
          </Tooltip>
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
      <PageHeader
        title="Distributors"
        subtitle="Manage distributors"
        action={
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            New Distributor
          </Button>
        }
      />

      <Box sx={{ flexGrow: 1, minHeight: 0 }}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
          disableRowSelectionOnClick
          autoHeight={false}
          sx={{ height: '100%' }}
        />
      </Box>

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingId ? 'Edit Distributor' : 'New Distributor'}</DialogTitle>
        <DialogContent>
          <TextField
            label="Name"
            fullWidth
            margin="normal"
            value={form.name}
            onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
          />
          <TextField
            label="Code"
            fullWidth
            margin="normal"
            value={form.code}
            onChange={(e) => setForm((prev) => ({ ...prev, code: e.target.value }))}
            disabled={!!editingId}
            helperText={editingId ? 'Code cannot be changed after creation' : ''}
          />
          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>Details</Typography>
          <TextField
            label="Description (EN)"
            fullWidth
            margin="normal"
            multiline
            rows={3}
            value={form.descriptionEn}
            onChange={(e) => setForm((prev) => ({ ...prev, descriptionEn: e.target.value }))}
          />
          <TextField
            label="Description (UA)"
            fullWidth
            margin="normal"
            multiline
            rows={3}
            value={form.descriptionUa}
            onChange={(e) => setForm((prev) => ({ ...prev, descriptionUa: e.target.value }))}
          />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Country"
              fullWidth
              margin="normal"
              value={form.country}
              onChange={(e) => setForm((prev) => ({ ...prev, country: e.target.value }))}
            />
            <TextField
              label="Flag"
              margin="normal"
              sx={{ width: 100 }}
              value={form.countryFlag}
              onChange={(e) => setForm((prev) => ({ ...prev, countryFlag: e.target.value }))}
            />
          </Box>
          <TextField
            label="Logo URL"
            fullWidth
            margin="normal"
            value={form.logoUrl}
            onChange={(e) => setForm((prev) => ({ ...prev, logoUrl: e.target.value }))}
          />
          <TextField
            label="Website URL"
            fullWidth
            margin="normal"
            value={form.websiteUrl}
            onChange={(e) => setForm((prev) => ({ ...prev, websiteUrl: e.target.value }))}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving || !form.name}>
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Distributor</DialogTitle>
        <DialogContent>
          Are you sure you want to delete <strong>{deletingRow?.name}</strong>? This action cannot be undone.
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
