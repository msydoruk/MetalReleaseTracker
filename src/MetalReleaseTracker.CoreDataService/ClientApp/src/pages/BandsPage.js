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
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import { DataGrid } from '@mui/x-data-grid';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '../components/PageHeader';
import { fetchBands, fetchBandById, updateBand, deleteBand, generateBandSeo, bulkGenerateBandSeo } from '../api/bands';
import AutoFixHighIcon from '@mui/icons-material/AutoFixHigh';

const EMPTY_FORM = {
  name: '',
  description: '',
  genre: '',
  metalArchivesUrl: '',
  formationYear: '',
  isVisible: true,
  seoTitle: '',
  seoDescription: '',
  seoKeywords: '',
};

export default function BandsPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [deletingRow, setDeletingRow] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
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
      const { data } = await fetchBands(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load bands', 'error');
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

  const handleOpenEdit = useCallback(async (row) => {
    try {
      setSaving(true);
      const { data } = await fetchBandById(row.id);
      setEditingId(row.id);
      setForm({
        name: data.name || '',
        description: data.description || '',
        genre: data.genre || '',
        metalArchivesUrl: data.metalArchivesUrl || '',
        formationYear: data.formationYear || '',
        isVisible: data.isVisible !== undefined ? data.isVisible : true,
        seoTitle: data.seoTitle || '',
        seoDescription: data.seoDescription || '',
        seoKeywords: data.seoKeywords || '',
      });
      setDialogOpen(true);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load band details', 'error');
    } finally {
      setSaving(false);
    }
  }, [showSnackbar]);

  const handleOpenDelete = useCallback((row) => {
    setDeletingRow(row);
    setDeleteDialogOpen(true);
  }, []);

  const handleSave = useCallback(async () => {
    try {
      setSaving(true);
      const payload = {
        ...form,
        formationYear: form.formationYear ? parseInt(form.formationYear, 10) : null,
      };
      await updateBand(editingId, payload);
      showSnackbar('Band updated');
      setDialogOpen(false);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to save band', 'error');
    } finally {
      setSaving(false);
    }
  }, [editingId, form, loadData, showSnackbar]);

  const handleDelete = useCallback(async () => {
    try {
      setSaving(true);
      await deleteBand(deletingRow.id);
      showSnackbar('Band deleted');
      setDeleteDialogOpen(false);
      setDeletingRow(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to delete band', 'error');
    } finally {
      setSaving(false);
    }
  }, [deletingRow, loadData, showSnackbar]);

  const columns = [
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
    { field: 'genre', headerName: 'Genre', flex: 0.8, minWidth: 150 },
    {
      field: 'formationYear',
      headerName: 'Formation Year',
      width: 140,
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'albumCount',
      headerName: 'Album Count',
      type: 'number',
      width: 130,
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
        title="Bands"
        subtitle="Manage bands"
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<AutoFixHighIcon />}
            onClick={async () => {
              try {
                showSnackbar('Generating SEO for bands without SEO data...');
                const { data } = await bulkGenerateBandSeo(50);
                showSnackbar(`SEO generated for ${data.processed} bands`);
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

      <Box sx={{ mb: 2 }}>
        <TextField
          placeholder="Search bands..."
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

      {/* Edit Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Band</DialogTitle>
        <DialogContent>
          <FormControlLabel
            control={
              <Switch
                checked={form.isVisible}
                onChange={(e) => setForm((prev) => ({ ...prev, isVisible: e.target.checked }))}
              />
            }
            label="Visible"
            sx={{ mb: 1 }}
          />
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
            label="Formation Year"
            fullWidth
            margin="normal"
            type="number"
            value={form.formationYear}
            onChange={(e) => setForm((prev) => ({ ...prev, formationYear: e.target.value }))}
          />
          <TextField
            label="Metal Archives URL"
            fullWidth
            margin="normal"
            value={form.metalArchivesUrl}
            onChange={(e) => setForm((prev) => ({ ...prev, metalArchivesUrl: e.target.value }))}
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
                    const { data } = await generateBandSeo(editingId);
                    if (data.success) {
                      setForm((prev) => ({
                        ...prev,
                        seoTitle: data.seoTitle || '',
                        seoDescription: data.seoDescription || '',
                        seoKeywords: data.seoKeywords || '',
                      }));
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
          <TextField
            label="SEO Title"
            fullWidth
            margin="normal"
            value={form.seoTitle}
            onChange={(e) => setForm((prev) => ({ ...prev, seoTitle: e.target.value }))}
            helperText={`${(form.seoTitle || '').length}/160 - Leave empty for auto-generated`}
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
          <Button variant="contained" onClick={handleSave} disabled={saving || !form.name}>
            {saving ? 'Saving...' : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Delete Band</DialogTitle>
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
