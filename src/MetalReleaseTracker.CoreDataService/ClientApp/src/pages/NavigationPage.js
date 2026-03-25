import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Switch from '@mui/material/Switch';
import Typography from '@mui/material/Typography';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import FormControlLabel from '@mui/material/FormControlLabel';
import Divider from '@mui/material/Divider';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import NavigationIcon from '@mui/icons-material/Navigation';
import PageHeader from '../components/PageHeader';
import { fetchNavigationItems, createNavigationItem, updateNavigationItem } from '../api/navigation';

const EMPTY_FORM = {
  titleEn: '',
  titleUa: '',
  path: '',
  iconName: '',
  sortOrder: 0,
  isVisible: true,
};

export default function NavigationPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editedFields, setEditedFields] = useState({});
  const [savingId, setSavingId] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [creating, setCreating] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await fetchNavigationItems();
      setItems(data);
      setEditedFields({});
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load navigation items', 'error');
    } finally {
      setLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleFieldChange = useCallback((id, field, value) => {
    setEditedFields((prev) => ({
      ...prev,
      [id]: { ...prev[id], [field]: value },
    }));
  }, []);

  const getFieldValue = useCallback(
    (item, field) => {
      if (editedFields[item.id] && editedFields[item.id][field] !== undefined) {
        return editedFields[item.id][field];
      }
      return item[field];
    },
    [editedFields]
  );

  const hasChanges = useCallback(
    (id) => {
      return editedFields[id] && Object.keys(editedFields[id]).length > 0;
    },
    [editedFields]
  );

  const handleSaveRow = useCallback(
    async (item) => {
      try {
        setSavingId(item.id);
        const edited = editedFields[item.id] || {};
        const payload = {
          titleEn: edited.titleEn !== undefined ? edited.titleEn : item.titleEn,
          titleUa: edited.titleUa !== undefined ? edited.titleUa : item.titleUa,
          path: edited.path !== undefined ? edited.path : item.path,
          iconName: edited.iconName !== undefined ? edited.iconName : item.iconName,
          sortOrder:
            edited.sortOrder !== undefined ? parseInt(edited.sortOrder, 10) : item.sortOrder,
          isVisible: edited.isVisible !== undefined ? edited.isVisible : item.isVisible,
        };
        await updateNavigationItem(item.id, payload);
        showSnackbar(`"${payload.titleEn}" updated`);
        loadData();
      } catch (err) {
        showSnackbar(err.response?.data?.message || 'Failed to update navigation item', 'error');
      } finally {
        setSavingId(null);
      }
    },
    [editedFields, loadData, showSnackbar]
  );

  const handleCreate = useCallback(async () => {
    try {
      setCreating(true);
      const payload = {
        ...form,
        sortOrder: parseInt(form.sortOrder, 10) || 0,
      };
      await createNavigationItem(payload);
      showSnackbar('Navigation item created');
      setDialogOpen(false);
      setForm(EMPTY_FORM);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to create navigation item', 'error');
    } finally {
      setCreating(false);
    }
  }, [form, loadData, showSnackbar]);

  return (
    <Box>
      <PageHeader
        title="Navigation"
        subtitle="Manage navigation menu items"
        action={
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
            Add Item
          </Button>
        }
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : items.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 6 }}>
          <NavigationIcon sx={{ fontSize: 48, color: 'rgba(255,255,255,0.2)', mb: 1 }} />
          <Typography color="text.secondary">No navigation items configured yet.</Typography>
        </Box>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {items.map((item) => (
            <Card
              key={item.id}
              sx={{
                borderLeft: hasChanges(item.id) ? '3px solid #ff9800' : '3px solid transparent',
                transition: 'border-color 0.2s',
              }}
            >
              <CardContent sx={{ p: 2.5 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      {item.titleEn}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      ({item.path})
                    </Typography>
                  </Box>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={getFieldValue(item, 'isVisible')}
                        onChange={(e) => handleFieldChange(item.id, 'isVisible', e.target.checked)}
                        size="small"
                      />
                    }
                    label={
                      <Typography variant="caption" color="text.secondary">
                        Visible
                      </Typography>
                    }
                    labelPlacement="start"
                    sx={{ mr: 0 }}
                  />
                </Box>

                <Divider sx={{ mb: 2, borderColor: 'rgba(255,255,255,0.06)' }} />

                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 2 }}>
                  <TextField
                    label="Title (EN)"
                    size="small"
                    value={getFieldValue(item, 'titleEn')}
                    onChange={(e) => handleFieldChange(item.id, 'titleEn', e.target.value)}
                    sx={{ flex: 1, minWidth: 160 }}
                  />
                  <TextField
                    label="Title (UA)"
                    size="small"
                    value={getFieldValue(item, 'titleUa')}
                    onChange={(e) => handleFieldChange(item.id, 'titleUa', e.target.value)}
                    sx={{ flex: 1, minWidth: 160 }}
                  />
                  <TextField
                    label="Path"
                    size="small"
                    value={getFieldValue(item, 'path')}
                    onChange={(e) => handleFieldChange(item.id, 'path', e.target.value)}
                    sx={{ flex: 1, minWidth: 140 }}
                  />
                  <TextField
                    label="Icon"
                    size="small"
                    value={getFieldValue(item, 'iconName')}
                    onChange={(e) => handleFieldChange(item.id, 'iconName', e.target.value)}
                    sx={{ flex: 0.6, minWidth: 120 }}
                  />
                  <TextField
                    label="Sort Order"
                    size="small"
                    type="number"
                    value={getFieldValue(item, 'sortOrder')}
                    onChange={(e) => handleFieldChange(item.id, 'sortOrder', e.target.value)}
                    sx={{ width: 110 }}
                  />
                </Box>

                <Button
                  variant="outlined"
                  size="small"
                  startIcon={
                    savingId === item.id ? (
                      <CircularProgress size={16} color="inherit" />
                    ) : (
                      <SaveIcon />
                    )
                  }
                  onClick={() => handleSaveRow(item)}
                  disabled={savingId === item.id || !hasChanges(item.id)}
                >
                  {savingId === item.id ? 'Saving...' : 'Save'}
                </Button>
              </CardContent>
            </Card>
          ))}
        </Box>
      )}

      {/* Create Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Navigation Item</DialogTitle>
        <DialogContent>
          <TextField
            label="Title (EN)"
            fullWidth
            margin="normal"
            value={form.titleEn}
            onChange={(e) => setForm((prev) => ({ ...prev, titleEn: e.target.value }))}
          />
          <TextField
            label="Title (UA)"
            fullWidth
            margin="normal"
            value={form.titleUa}
            onChange={(e) => setForm((prev) => ({ ...prev, titleUa: e.target.value }))}
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
          <FormControlLabel
            control={
              <Switch
                checked={form.isVisible}
                onChange={(e) => setForm((prev) => ({ ...prev, isVisible: e.target.checked }))}
              />
            }
            label="Visible"
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleCreate}
            disabled={creating || !form.titleEn || !form.path}
          >
            {creating ? 'Creating...' : 'Create'}
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
