import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import IconButton from '@mui/material/IconButton';
import Switch from '@mui/material/Switch';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import FormControlLabel from '@mui/material/FormControlLabel';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import { DataGrid } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import PageHeader from '../components/PageHeader';
import LanguageTabs from '../components/LanguageTabs';
import {
  fetchNewsArticles,
  createNewsArticle,
  updateNewsArticle,
  deleteNewsArticle,
} from '../api/news';

const CHIP_COLOR_OPTIONS = [
  { value: 'success', label: 'Success (green)' },
  { value: 'info', label: 'Info (blue)' },
  { value: 'warning', label: 'Warning (orange)' },
  { value: 'error', label: 'Error (red)' },
  { value: 'default', label: 'Default (grey)' },
];

const EMPTY_FORM = {
  translations: {},
  chipLabel: '',
  chipColor: 'info',
  iconName: '',
  date: '',
  sortOrder: 0,
  isPublished: false,
  scheduledPublishDate: '',
};

export default function NewsPage() {
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
      const { data } = await fetchNewsArticles();
      setRows(data);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load news articles', 'error');
    } finally {
      setLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleOpenCreate = useCallback(() => {
    setEditingId(null);
    setForm({
      ...EMPTY_FORM,
      date: new Date().toISOString().split('T')[0],
    });
    setDialogLang('en');
    setDialogOpen(true);
  }, []);

  const handleOpenEdit = useCallback((row) => {
    setEditingId(row.id);
    setForm({
      translations: row.translations || {},
      chipLabel: row.chipLabel || '',
      chipColor: row.chipColor || 'info',
      iconName: row.iconName || '',
      date: row.date ? row.date.split('T')[0] : '',
      sortOrder: row.sortOrder || 0,
      isPublished: row.isPublished || false,
      scheduledPublishDate: row.scheduledPublishDate ? row.scheduledPublishDate.split('T')[0] + 'T' + (row.scheduledPublishDate.split('T')[1] || '00:00').substring(0, 5) : '',
    });
    setDialogLang('en');
    setDialogOpen(true);
  }, []);

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
        translations: form.translations,
        chipLabel: form.chipLabel,
        chipColor: form.chipColor,
        iconName: form.iconName,
        date: form.date,
        sortOrder: parseInt(form.sortOrder, 10) || 0,
        isPublished: form.isPublished,
        scheduledPublishDate: form.scheduledPublishDate || null,
      };
      if (editingId) {
        await updateNewsArticle(editingId, payload);
        showSnackbar('Article updated');
      } else {
        await createNewsArticle(payload);
        showSnackbar('Article created');
      }
      setDialogOpen(false);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to save article', 'error');
    } finally {
      setSaving(false);
    }
  }, [editingId, form, loadData, showSnackbar]);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteNewsArticle(deletingRow.id);
      showSnackbar('Article deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete article', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const columns = [
    {
      field: 'title',
      headerName: 'Title (En)',
      flex: 1,
      minWidth: 250,
      valueGetter: (value, row) => row.translations?.en?.title || '',
    },
    {
      field: 'date',
      headerName: 'Date',
      width: 120,
      valueFormatter: (value) =>
        value ? new Date(value).toLocaleDateString() : '--',
    },
    {
      field: 'chipLabel',
      headerName: 'Chip',
      width: 130,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) =>
        params.value ? (
          <Chip
            label={params.value}
            size="small"
            color={params.row.chipColor || 'default'}
            variant="outlined"
          />
        ) : (
          '--'
        ),
    },
    {
      field: 'isPublished',
      headerName: 'Published',
      width: 110,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => {
        if (params.value) return <Chip label="Yes" size="small" color="success" />;
        if (params.row.scheduledPublishDate) return <Chip label="Scheduled" size="small" color="warning" />;
        return <Chip label="No" size="small" variant="outlined" />;
      },
    },
    {
      field: 'sortOrder',
      headerName: 'Order',
      width: 80,
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'seo',
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
        title="News"
        subtitle="Manage news articles"
        action={
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            New Article
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
        <DialogTitle>{editingId ? 'Edit Article' : 'New Article'}</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1, mb: 2 }}>
            <LanguageTabs value={dialogLang} onChange={setDialogLang} />
          </Box>

          <TextField
            label="Title"
            fullWidth
            margin="normal"
            value={currentTranslation.title || ''}
            onChange={(e) => setTranslationField(dialogLang, 'title', e.target.value)}
          />
          <TextField
            label="Content"
            fullWidth
            margin="normal"
            multiline
            rows={4}
            value={currentTranslation.content || ''}
            onChange={(e) => setTranslationField(dialogLang, 'content', e.target.value)}
          />

          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>Display Settings</Typography>

          <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
            <TextField
              label="Chip Label"
              value={form.chipLabel}
              onChange={(e) => setForm((prev) => ({ ...prev, chipLabel: e.target.value }))}
              sx={{ flex: 1 }}
            />
            <TextField
              label="Chip Color"
              select
              value={form.chipColor}
              onChange={(e) => setForm((prev) => ({ ...prev, chipColor: e.target.value }))}
              sx={{ flex: 1 }}
            >
              {CHIP_COLOR_OPTIONS.map((opt) => (
                <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
              ))}
            </TextField>
          </Box>

          <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
            <TextField
              label="Icon Name"
              value={form.iconName}
              onChange={(e) => setForm((prev) => ({ ...prev, iconName: e.target.value }))}
              placeholder="MUI icon name"
              sx={{ flex: 1 }}
            />
            <TextField
              label="Date"
              type="date"
              value={form.date}
              onChange={(e) => setForm((prev) => ({ ...prev, date: e.target.value }))}
              sx={{ flex: 1 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="Sort Order"
              type="number"
              value={form.sortOrder}
              onChange={(e) => setForm((prev) => ({ ...prev, sortOrder: e.target.value }))}
              sx={{ width: 120 }}
            />
          </Box>

          <FormControlLabel
            control={
              <Switch
                checked={form.isPublished}
                onChange={(e) => setForm((prev) => ({ ...prev, isPublished: e.target.checked, ...(e.target.checked ? { scheduledPublishDate: '' } : {}) }))}
              />
            }
            label="Published"
            sx={{ mt: 2 }}
          />
          {!form.isPublished && (
            <TextField
              label="Scheduled Publish Date"
              type="datetime-local"
              fullWidth
              margin="normal"
              value={form.scheduledPublishDate || ''}
              onChange={(e) => setForm((prev) => ({ ...prev, scheduledPublishDate: e.target.value }))}
              helperText="Leave empty for manual publishing"
              slotProps={{ inputLabel: { shrink: true } }}
            />
          )}

          <Divider sx={{ my: 2 }} />
          <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>SEO (optional)</Typography>
          <TextField
            label="SEO Title"
            fullWidth
            margin="normal"
            value={currentTranslation.seoTitle || ''}
            onChange={(e) => setTranslationField(dialogLang, 'seoTitle', e.target.value)}
            helperText={`${(currentTranslation.seoTitle || '').length}/160`}
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
          <Button variant="contained" onClick={handleSave} disabled={saving || !form.translations?.en?.title}>
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Article</DialogTitle>
        <DialogContent>
          Are you sure you want to delete <strong>{deletingRow?.translations?.en?.title}</strong>? This action cannot be undone.
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
