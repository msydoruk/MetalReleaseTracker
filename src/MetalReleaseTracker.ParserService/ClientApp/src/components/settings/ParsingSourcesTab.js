import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Chip from '@mui/material/Chip';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import TextField from '@mui/material/TextField';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import EditIcon from '@mui/icons-material/Edit';
import LinkIcon from '@mui/icons-material/Link';
import { fetchParsingSources, updateParsingSource, fetchCategorySettings, updateCategorySettings } from '../../api/settings';
import { DISTRIBUTOR_CODES } from '../../constants';

export default function ParsingSourcesTab() {
  const [sources, setSources] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingSource, setEditingSource] = useState(null);
  const [form, setForm] = useState({ parsingUrl: '', isEnabled: true });
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [generalSettings, setGeneralSettings] = useState({});
  const [generalSaving, setGeneralSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [sourcesRes, generalRes] = await Promise.all([
        fetchParsingSources(),
        fetchCategorySettings('general-parser'),
      ]);
      setSources(sourcesRes.data);
      setGeneralSettings(generalRes.data.settings || {});
    } catch {
      setSnackbar({ open: true, message: 'Failed to load settings', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleEdit = (source) => {
    setEditingSource(source);
    setForm({ parsingUrl: source.parsingUrl, isEnabled: source.isEnabled });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await updateParsingSource(editingSource.id, form);
      setSnackbar({ open: true, message: 'Source updated', severity: 'success' });
      setDialogOpen(false);
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to update source', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const handleGeneralSave = async () => {
    setGeneralSaving(true);
    try {
      await updateCategorySettings('general-parser', generalSettings);
      setSnackbar({ open: true, message: 'Parser settings saved', severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: 'Failed to save settings', severity: 'error' });
    } finally {
      setGeneralSaving(false);
    }
  };

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Configure parsing sources for each distributor and general parser delay settings.
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
        {sources.map((source) => (
          <Card key={source.id} sx={{ border: '1px solid rgba(255, 255, 255, 0.08)' }}>
            <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, minWidth: 0 }}>
                  <LinkIcon sx={{ color: 'rgba(255, 255, 255, 0.3)', fontSize: 20 }} />
                  <Box sx={{ minWidth: 0 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        {DISTRIBUTOR_CODES[source.distributorCode]?.label || source.name}
                      </Typography>
                      <Chip
                        label={source.isEnabled ? 'Enabled' : 'Disabled'}
                        size="small"
                        color={source.isEnabled ? 'success' : 'default'}
                        variant="outlined"
                        sx={{ height: 18, fontSize: '0.65rem' }}
                      />
                    </Box>
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 500 }}
                    >
                      {source.parsingUrl}
                    </Typography>
                  </Box>
                </Box>
                <IconButton size="small" onClick={() => handleEdit(source)}>
                  <EditIcon fontSize="small" />
                </IconButton>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>

      <Divider sx={{ my: 3, borderColor: 'rgba(255, 255, 255, 0.08)' }} />

      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>General Parser Settings</Typography>
      <Card sx={{ border: '1px solid rgba(255, 255, 255, 0.08)' }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <TextField
              label="Min Delay Between Requests (sec)"
              type="number"
              value={generalSettings.MinDelayBetweenRequestsSeconds || ''}
              onChange={(e) => setGeneralSettings((prev) => ({ ...prev, MinDelayBetweenRequestsSeconds: e.target.value }))}
              size="small"
              fullWidth
            />
            <TextField
              label="Max Delay Between Requests (sec)"
              type="number"
              value={generalSettings.MaxDelayBetweenRequestsSeconds || ''}
              onChange={(e) => setGeneralSettings((prev) => ({ ...prev, MaxDelayBetweenRequestsSeconds: e.target.value }))}
              size="small"
              fullWidth
            />
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <Button variant="contained" size="small" onClick={handleGeneralSave} disabled={generalSaving}>
              {generalSaving ? <CircularProgress size={20} /> : 'Save'}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onClose={() => !saving && setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Parsing Source</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
          <Typography variant="body2" color="text.secondary">
            {DISTRIBUTOR_CODES[editingSource?.distributorCode]?.label || editingSource?.name}
          </Typography>
          <TextField
            label="Parsing URL"
            value={form.parsingUrl}
            onChange={(e) => setForm((prev) => ({ ...prev, parsingUrl: e.target.value }))}
            size="small"
            fullWidth
          />
          <FormControlLabel
            control={
              <Switch
                checked={form.isEnabled}
                onChange={(e) => setForm((prev) => ({ ...prev, isEnabled: e.target.checked }))}
              />
            }
            label="Enabled"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)} disabled={saving}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving || !form.parsingUrl}>
            {saving ? <CircularProgress size={20} /> : 'Save'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
        <Alert severity={snackbar.severity} variant="filled">{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
