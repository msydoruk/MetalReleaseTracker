import { useState, useEffect, useCallback, useRef } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import { DataGrid } from '@mui/x-data-grid';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '../components/PageHeader';
import LanguageTabs from '../components/LanguageTabs';
import { fetchAlbums, fetchAlbumById, updateAlbum, deleteAlbum, generateAlbumSeo, bulkGenerateAlbumSeo } from '../api/albums';
import AutoFixHighIcon from '@mui/icons-material/AutoFixHigh';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'New', label: 'New' },
  { value: 'PreOrder', label: 'Pre-Order' },
  { value: 'Restock', label: 'Restock' },
  { value: 'Unavailable', label: 'Unavailable' },
];

const STATUS_COLORS = {
  New: 'success',
  PreOrder: 'info',
  Restock: 'warning',
  Unavailable: 'error',
};

const STOCK_STATUS_OPTIONS = [
  { value: '', label: 'None' },
  { value: 'InStock', label: 'In Stock' },
  { value: 'OutOfStock', label: 'Out of Stock' },
  { value: 'PreOrder', label: 'Pre-Order' },
  { value: 'Unknown', label: 'Unknown' },
];

const EMPTY_FORM = {
  name: '',
  genre: '',
  price: '',
  status: '',
  stockStatus: '',
  description: '',
  label: '',
  press: '',
  translations: {},
};

export default function AlbumsPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [deletingRow, setDeletingRow] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [dialogLang, setDialogLang] = useState('en');
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
      if (statusFilter) {
        params.status = statusFilter;
      }
      const { data } = await fetchAlbums(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load albums', 'error');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search, statusFilter, showSnackbar]);

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

  const handleStatusFilterChange = useCallback((e) => {
    setStatusFilter(e.target.value);
    setPaginationModel((prev) => ({ ...prev, page: 0 }));
  }, []);

  const handleOpenEdit = useCallback(async (row) => {
    try {
      setSaving(true);
      const { data } = await fetchAlbumById(row.id);
      setEditingId(row.id);
      setForm({
        name: data.name || '',
        genre: data.genre || '',
        price: data.price != null ? String(data.price) : '',
        status: data.status || '',
        stockStatus: data.stockStatus || '',
        description: data.description || '',
        label: data.label || '',
        press: data.press || '',
        translations: data.translations || {},
      });
      setDialogLang('en');
      setDialogOpen(true);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load album details', 'error');
    } finally {
      setSaving(false);
    }
  }, [showSnackbar]);

  const handleOpenDelete = useCallback((row) => {
    setDeletingRow(row);
    setDeleteDialogOpen(true);
  }, []);

  const setTranslationField = (lang, field, value) => {
    setForm(prev => ({
      ...prev,
      translations: {
        ...prev.translations,
        [lang]: { ...(prev.translations[lang] || {}), [field]: value }
      }
    }));
  };

  const handleSave = useCallback(async () => {
    try {
      setSaving(true);
      const payload = {
        name: form.name,
        genre: form.genre,
        price: form.price ? parseFloat(form.price) : null,
        status: form.status,
        stockStatus: form.stockStatus,
        description: form.description,
        label: form.label,
        press: form.press,
        translations: form.translations,
      };
      await updateAlbum(editingId, payload);
      showSnackbar('Album updated');
      setDialogOpen(false);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to save album', 'error');
    } finally {
      setSaving(false);
    }
  }, [editingId, form, loadData, showSnackbar]);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteAlbum(deletingRow.id);
      showSnackbar('Album deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete album', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const columns = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
    { field: 'bandName', headerName: 'Band', flex: 0.8, minWidth: 150 },
    { field: 'distributorName', headerName: 'Distributor', flex: 0.6, minWidth: 120 },
    { field: 'sku', headerName: 'SKU', width: 130 },
    {
      field: 'price',
      headerName: 'Price',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      valueFormatter: (value) => (value != null ? `${value.toFixed(2)}` : '--'),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) =>
        params.value ? (
          <Chip
            label={params.value}
            size="small"
            color={STATUS_COLORS[params.value] || 'default'}
            variant="outlined"
          />
        ) : (
          '--'
        ),
    },
    { field: 'media', headerName: 'Media', width: 80, align: 'center', headerAlign: 'center' },
    {
      field: 'createdDate',
      headerName: 'Created',
      width: 120,
      valueFormatter: (value) =>
        value ? new Date(value).toLocaleDateString() : '--',
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
        params.row.translations?.en?.seoTitle ? (
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
          <Tooltip title="Delete">
            <IconButton size="small" color="error" onClick={() => handleOpenDelete(params.row)}>
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  const currentTranslation = form.translations[dialogLang] || {};

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader
        title="Albums"
        subtitle="Manage albums"
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<AutoFixHighIcon />}
            onClick={async () => {
              try {
                showSnackbar('Generating SEO for albums without SEO data...');
                const { data } = await bulkGenerateAlbumSeo(50);
                showSnackbar(`SEO generated for ${data.processed} albums`);
                loadData();
              } catch (err) {
                showSnackbar(err.response?.data?.error || 'Bulk SEO failed', 'error');
              }
            }}
          >
            Bulk Generate SEO
          </Button>
        }
      />

      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}>
        <TextField
          placeholder="Search albums..."
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
        <TextField
          select
          size="small"
          value={statusFilter}
          onChange={handleStatusFilterChange}
          sx={{ width: 180 }}
          label="Status"
        >
          {STATUS_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </TextField>
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

      {/* Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Album</DialogTitle>
        <DialogContent>
          <TextField
            label="Name"
            fullWidth
            margin="normal"
            value={form.name}
            onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
          />
          <TextField
            label="Genre"
            fullWidth
            margin="normal"
            value={form.genre}
            onChange={(e) => setForm((prev) => ({ ...prev, genre: e.target.value }))}
          />
          <TextField
            label="Price"
            fullWidth
            margin="normal"
            type="number"
            value={form.price}
            onChange={(e) => setForm((prev) => ({ ...prev, price: e.target.value }))}
          />
          <TextField
            label="Status"
            fullWidth
            margin="normal"
            select
            value={form.status}
            onChange={(e) => setForm((prev) => ({ ...prev, status: e.target.value }))}
          >
            {STATUS_OPTIONS.filter((opt) => opt.value).map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Stock Status"
            fullWidth
            margin="normal"
            select
            value={form.stockStatus}
            onChange={(e) => setForm((prev) => ({ ...prev, stockStatus: e.target.value }))}
          >
            {STOCK_STATUS_OPTIONS.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Label"
            fullWidth
            margin="normal"
            value={form.label}
            onChange={(e) => setForm((prev) => ({ ...prev, label: e.target.value }))}
          />
          <TextField
            label="Press"
            fullWidth
            margin="normal"
            value={form.press}
            onChange={(e) => setForm((prev) => ({ ...prev, press: e.target.value }))}
          />
          <TextField
            label="Description"
            fullWidth
            margin="normal"
            multiline
            rows={3}
            value={form.description}
            onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
          />
          <Divider sx={{ my: 2 }} />
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2" color="text.secondary">SEO (optional overrides)</Typography>
            {editingId && (
              <Button
                size="small"
                startIcon={<AutoFixHighIcon />}
                onClick={async () => {
                  try {
                    setSaving(true);
                    const { data } = await generateAlbumSeo(editingId);
                    if (data.success) {
                      setTranslationField(dialogLang, 'seoTitle', data.seoTitle || '');
                      setTranslationField(dialogLang, 'seoDescription', data.seoDescription || '');
                      setTranslationField(dialogLang, 'seoKeywords', data.seoKeywords || '');
                      showSnackbar('SEO generated by AI');
                    } else {
                      showSnackbar(data.error || 'AI generation failed', 'error');
                    }
                  } catch (err) {
                    showSnackbar(err.response?.data?.error || 'AI generation failed', 'error');
                  } finally {
                    setSaving(false);
                  }
                }}
                disabled={saving}
              >
                Generate with AI
              </Button>
            )}
          </Box>

          <Box sx={{ mt: 1, mb: 2 }}>
            <LanguageTabs value={dialogLang} onChange={setDialogLang} />
          </Box>

          <TextField
            label="SEO Title"
            fullWidth
            margin="normal"
            value={currentTranslation.seoTitle || ''}
            onChange={(e) => setTranslationField(dialogLang, 'seoTitle', e.target.value)}
            helperText={`${(currentTranslation.seoTitle || '').length}/160 - Leave empty for auto-generated`}
            inputProps={{ maxLength: 160 }}
          />
          <TextField
            label="SEO Description"
            fullWidth
            margin="normal"
            multiline
            rows={2}
            value={currentTranslation.seoDescription || ''}
            onChange={(e) => setTranslationField(dialogLang, 'seoDescription', e.target.value)}
            helperText={`${(currentTranslation.seoDescription || '').length}/320`}
            inputProps={{ maxLength: 320 }}
          />
          <TextField
            label="SEO Keywords"
            fullWidth
            margin="normal"
            value={currentTranslation.seoKeywords || ''}
            onChange={(e) => setTranslationField(dialogLang, 'seoKeywords', e.target.value)}
            helperText="Comma-separated keywords"
            inputProps={{ maxLength: 500 }}
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
        <DialogTitle>Delete Album</DialogTitle>
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
