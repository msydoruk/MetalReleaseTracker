import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import IconButton from '@mui/material/IconButton';
import Switch from '@mui/material/Switch';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import FormControlLabel from '@mui/material/FormControlLabel';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import { DataGrid } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import PageHeader from '../components/PageHeader';
import {
  fetchNavigationItems,
  createNavigationItem,
  updateNavigationItem,
  deleteNavigationItem,
} from '../api/navigation';

const EMPTY_FORM = {
  titleEn: '',
  titleUa: '',
  path: '',
  iconName: '',
  sortOrder: 0,
  isVisible: true,
  isProtected: false,
  seoTitle: '',
  seoDescription: '',
  seoKeywords: '',
};

export default function NavigationPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [deletingRow, setDeletingRow] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [dialogLang, setDialogLang] = useState('en');

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await fetchNavigationItems();
      setRows(data);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load navigation items', 'error');
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
    setDialogLang('en');
    setDialogOpen(true);
  }, []);

  const handleOpenEdit = useCallback((row) => {
    setEditingId(row.id);
    setForm({
      titleEn: row.titleEn || '',
      titleUa: row.titleUa || '',
      path: row.path || '',
      iconName: row.iconName || '',
      sortOrder: row.sortOrder || 0,
      isVisible: row.isVisible !== undefined ? row.isVisible : true,
      isProtected: row.isProtected || false,
      seoTitle: row.seoTitle || '',
      seoDescription: row.seoDescription || '',
      seoKeywords: row.seoKeywords || '',
    });
    setDialogLang('en');
    setDialogOpen(true);
  }, []);

  const handleOpenDelete = useCallback((row) => {
    setDeletingRow(row);
    setDeleteDialogOpen(true);
  }, []);

  const handleSave = useCallback(async () => {
    try {
      setSaving(true);
      const payload = {
        ...form,
        sortOrder: parseInt(form.sortOrder, 10) || 0,
      };
      if (editingId) {
        await updateNavigationItem(editingId, payload);
        showSnackbar('Navigation item updated');
      } else {
        await createNavigationItem(payload);
        showSnackbar('Navigation item created');
      }
      setDialogOpen(false);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to save navigation item', 'error');
    } finally {
      setSaving(false);
    }
  }, [editingId, form, loadData, showSnackbar]);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteNavigationItem(deletingRow.id);
      showSnackbar('Navigation item deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete navigation item', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const titleField = dialogLang === 'ua' ? 'titleUa' : 'titleEn';

  const columns = [
    { field: 'titleEn', headerName: 'Title (EN)', flex: 1, minWidth: 160 },
    { field: 'path', headerName: 'Path', flex: 0.8, minWidth: 140 },
    { field: 'iconName', headerName: 'Icon', width: 140 },
    {
      field: 'sortOrder',
      headerName: 'Order',
      width: 80,
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'isVisible',
      headerName: 'Visible',
      width: 100,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Yes' : 'No'}
          size="small"
          color={params.value ? 'success' : 'default'}
          variant="outlined"
        />
      ),
    },
    {
      field: 'seoTitle',
      headerName: 'SEO',
      width: 70,
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) =>
        params.row.seoTitle ? (
          <Tooltip title="SEO configured">
            <CheckCircleIcon fontSize="small" color="success" />
          </Tooltip>
        ) : (
          <Tooltip title="No SEO data">
            <CancelIcon fontSize="small" sx={{ color: 'rgba(255,255,255,0.2)' }} />
          </Tooltip>
        ),
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
          {!params.row.isProtected && (
            <Tooltip title="Delete">
              <IconButton size="small" color="error" onClick={() => handleOpenDelete(params.row)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader
        title="Navigation"
        subtitle="Manage navigation menu items"
        action={
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            Add Item
          </Button>
        }
      />

      <Box sx={{ flexGrow: 1, minHeight: 0 }}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          pageSizeOptions={[10, 25]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          disableRowSelectionOnClick
          autoHeight={false}
          sx={{ height: '100%' }}
        />
      </Box>

      {/* Create / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Edit Navigation Item' : 'New Navigation Item'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 1, mb: 2 }}>
            <ToggleButtonGroup
              value={dialogLang}
              exclusive
              onChange={(e, value) => { if (value) setDialogLang(value); }}
              size="small"
            >
              <ToggleButton value="en" sx={{ px: 2 }}>EN</ToggleButton>
              <ToggleButton value="ua" sx={{ px: 2 }}>UA</ToggleButton>
            </ToggleButtonGroup>
            <Typography variant="caption" color="text.secondary">
              Editing {dialogLang === 'ua' ? 'Ukrainian' : 'English'} title
            </Typography>
          </Box>

          <TextField
            label="Title"
            fullWidth
            margin="normal"
            value={form[titleField]}
            onChange={(e) => setForm((prev) => ({ ...prev, [titleField]: e.target.value }))}
          />
          <TextField
            label="Path"
            fullWidth
            margin="normal"
            value={form.path}
            onChange={(e) => setForm((prev) => ({ ...prev, path: e.target.value }))}
            placeholder="/example"
          />
          <TextField
            label="Icon"
            fullWidth
            margin="normal"
            value={form.iconName}
            onChange={(e) => setForm((prev) => ({ ...prev, iconName: e.target.value }))}
            placeholder="MUI icon name"
          />
          <TextField
            label="Sort Order"
            fullWidth
            margin="normal"
            type="number"
            value={form.sortOrder}
            onChange={(e) => setForm((prev) => ({ ...prev, sortOrder: e.target.value }))}
          />
          <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={form.isVisible}
                  onChange={(e) => setForm((prev) => ({ ...prev, isVisible: e.target.checked }))}
                />
              }
              label="Visible"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={form.isProtected}
                  onChange={(e) => setForm((prev) => ({ ...prev, isProtected: e.target.checked }))}
                />
              }
              label="Protected"
            />
          </Box>

          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>SEO (optional)</Typography>
          <TextField
            label="SEO Title"
            fullWidth
            margin="normal"
            value={form.seoTitle}
            onChange={(e) => setForm((prev) => ({ ...prev, seoTitle: e.target.value }))}
            helperText={`${(form.seoTitle || '').length}/160`}
            inputProps={{ maxLength: 160 }}
          />
          <TextField
            label="SEO Description"
            fullWidth
            margin="normal"
            multiline
            rows={2}
            value={form.seoDescription}
            onChange={(e) => setForm((prev) => ({ ...prev, seoDescription: e.target.value }))}
            helperText={`${(form.seoDescription || '').length}/320`}
            inputProps={{ maxLength: 320 }}
          />
          <TextField
            label="SEO Keywords"
            fullWidth
            margin="normal"
            value={form.seoKeywords}
            onChange={(e) => setForm((prev) => ({ ...prev, seoKeywords: e.target.value }))}
            helperText="Comma-separated keywords"
            inputProps={{ maxLength: 500 }}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving || !form.titleEn || !form.path}>
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Navigation Item</DialogTitle>
        <DialogContent>
          Are you sure you want to delete <strong>{deletingRow?.titleEn}</strong>? This action cannot be undone.
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
