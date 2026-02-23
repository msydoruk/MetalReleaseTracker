import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import { fetchCategorySettings, updateCategorySettings } from '../../api/settings';

export default function BandReferenceTab() {
  const [bandRefSettings, setBandRefSettings] = useState({});
  const [flareSettings, setFlareSettings] = useState({});
  const [loading, setLoading] = useState(true);
  const [bandRefSaving, setBandRefSaving] = useState(false);
  const [flareSaving, setFlareSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [bandRefRes, flareRes] = await Promise.all([
        fetchCategorySettings('band-reference'),
        fetchCategorySettings('flaresolverr'),
      ]);
      setBandRefSettings(bandRefRes.data.settings || {});
      setFlareSettings(flareRes.data.settings || {});
    } catch {
      setSnackbar({ open: true, message: 'Failed to load settings', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleBandRefSave = async () => {
    setBandRefSaving(true);
    try {
      await updateCategorySettings('band-reference', bandRefSettings);
      setSnackbar({ open: true, message: 'Band Reference settings saved', severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: 'Failed to save settings', severity: 'error' });
    } finally {
      setBandRefSaving(false);
    }
  };

  const handleFlareSave = async () => {
    setFlareSaving(true);
    try {
      await updateCategorySettings('flaresolverr', flareSettings);
      setSnackbar({ open: true, message: 'FlareSolverr settings saved', severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: 'Failed to save settings', severity: 'error' });
    } finally {
      setFlareSaving(false);
    }
  };

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>Band Reference Settings</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Configuration for syncing Ukrainian bands from Metal Archives.
      </Typography>
      <Card sx={{ border: '1px solid rgba(255, 255, 255, 0.08)', mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label="Metal Archives Base URL"
              value={bandRefSettings.MetalArchivesBaseUrl || ''}
              onChange={(e) => setBandRefSettings((prev) => ({ ...prev, MetalArchivesBaseUrl: e.target.value }))}
              size="small"
              fullWidth
            />
            <TextField
              label="Sync Country Code"
              value={bandRefSettings.SyncCountryCode || ''}
              onChange={(e) => setBandRefSettings((prev) => ({ ...prev, SyncCountryCode: e.target.value }))}
              size="small"
              fullWidth
              helperText="ISO country code (e.g., UA for Ukraine)"
            />
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Min Request Delay (ms)"
                type="number"
                value={bandRefSettings.MinRequestDelayMs || ''}
                onChange={(e) => setBandRefSettings((prev) => ({ ...prev, MinRequestDelayMs: e.target.value }))}
                size="small"
                fullWidth
              />
              <TextField
                label="Max Request Delay (ms)"
                type="number"
                value={bandRefSettings.MaxRequestDelayMs || ''}
                onChange={(e) => setBandRefSettings((prev) => ({ ...prev, MaxRequestDelayMs: e.target.value }))}
                size="small"
                fullWidth
              />
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
              <Button variant="contained" size="small" onClick={handleBandRefSave} disabled={bandRefSaving}>
                {bandRefSaving ? <CircularProgress size={20} /> : 'Save'}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Divider sx={{ my: 3, borderColor: 'rgba(255, 255, 255, 0.08)' }} />

      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>FlareSolverr Settings</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Configuration for the FlareSolverr proxy used to bypass Cloudflare protection.
      </Typography>
      <Card sx={{ border: '1px solid rgba(255, 255, 255, 0.08)' }}>
        <CardContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label="FlareSolverr Base URL"
              value={flareSettings.BaseUrl || ''}
              onChange={(e) => setFlareSettings((prev) => ({ ...prev, BaseUrl: e.target.value }))}
              size="small"
              fullWidth
            />
            <TextField
              label="Max Timeout (ms)"
              type="number"
              value={flareSettings.MaxTimeoutMs || ''}
              onChange={(e) => setFlareSettings((prev) => ({ ...prev, MaxTimeoutMs: e.target.value }))}
              size="small"
              fullWidth
            />
            <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
              <Button variant="contained" size="small" onClick={handleFlareSave} disabled={flareSaving}>
                {flareSaving ? <CircularProgress size={20} /> : 'Save'}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
        <Alert severity={snackbar.severity} variant="filled">{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
