import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
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
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import CurrencyExchangeIcon from '@mui/icons-material/CurrencyExchange';
import PageHeader from '../components/PageHeader';
import { fetchCurrencies, createCurrency, updateCurrency } from '../api/currencies';

const EMPTY_FORM = {
  code: '',
  symbol: '',
  rateToEur: '',
  isEnabled: true,
  sortOrder: 0,
};

export default function CurrenciesPage() {
  const [currencies, setCurrencies] = useState([]);
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
      const { data } = await fetchCurrencies();
      setCurrencies(data);
      setEditedFields({});
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load currencies', 'error');
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
    (currency, field) => {
      if (editedFields[currency.id] && editedFields[currency.id][field] !== undefined) {
        return editedFields[currency.id][field];
      }
      return currency[field];
    },
    [editedFields]
  );

  const handleSaveRow = useCallback(
    async (currency) => {
      try {
        setSavingId(currency.id);
        const edited = editedFields[currency.id] || {};
        const payload = {
          code: currency.code,
          symbol: currency.symbol,
          rateToEur: edited.rateToEur !== undefined ? parseFloat(edited.rateToEur) : currency.rateToEur,
          isEnabled: edited.isEnabled !== undefined ? edited.isEnabled : currency.isEnabled,
          sortOrder: edited.sortOrder !== undefined ? parseInt(edited.sortOrder, 10) : currency.sortOrder,
        };
        await updateCurrency(currency.id, payload);
        showSnackbar(`${currency.code} updated`);
        loadData();
      } catch (err) {
        showSnackbar(err.response?.data?.message || 'Failed to update currency', 'error');
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
        rateToEur: parseFloat(form.rateToEur),
        sortOrder: parseInt(form.sortOrder, 10) || 0,
      };
      await createCurrency(payload);
      showSnackbar('Currency created');
      setDialogOpen(false);
      setForm(EMPTY_FORM);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to create currency', 'error');
    } finally {
      setCreating(false);
    }
  }, [form, loadData, showSnackbar]);

  const hasChanges = useCallback(
    (id) => {
      return editedFields[id] && Object.keys(editedFields[id]).length > 0;
    },
    [editedFields]
  );

  return (
    <Box>
      <PageHeader
        title="Currencies"
        subtitle="Manage exchange rates"
        action={
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
            Add Currency
          </Button>
        }
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : currencies.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 6 }}>
          <CurrencyExchangeIcon sx={{ fontSize: 48, color: 'rgba(255,255,255,0.2)', mb: 1 }} />
          <Typography color="text.secondary">No currencies configured yet.</Typography>
        </Box>
      ) : (
        <Grid container spacing={2}>
          {currencies.map((currency) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={currency.id}>
              <Card
                sx={{
                  position: 'relative',
                  borderLeft: hasChanges(currency.id) ? '3px solid #ff9800' : '3px solid transparent',
                  transition: 'border-color 0.2s',
                }}
              >
                <CardContent sx={{ p: 2.5 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="h6" sx={{ fontWeight: 700 }}>
                        {currency.code}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {currency.symbol}
                      </Typography>
                    </Box>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={getFieldValue(currency, 'isEnabled')}
                          onChange={(e) => handleFieldChange(currency.id, 'isEnabled', e.target.checked)}
                          size="small"
                        />
                      }
                      label={
                        <Typography variant="caption" color="text.secondary">
                          Enabled
                        </Typography>
                      }
                      labelPlacement="start"
                      sx={{ mr: 0 }}
                    />
                  </Box>

                  <TextField
                    label="Rate to EUR"
                    fullWidth
                    size="small"
                    type="number"
                    value={getFieldValue(currency, 'rateToEur')}
                    onChange={(e) => handleFieldChange(currency.id, 'rateToEur', e.target.value)}
                    sx={{ mb: 1.5 }}
                  />

                  <TextField
                    label="Sort Order"
                    fullWidth
                    size="small"
                    type="number"
                    value={getFieldValue(currency, 'sortOrder')}
                    onChange={(e) => handleFieldChange(currency.id, 'sortOrder', e.target.value)}
                    sx={{ mb: 2 }}
                  />

                  <Button
                    variant="outlined"
                    size="small"
                    fullWidth
                    startIcon={
                      savingId === currency.id ? (
                        <CircularProgress size={16} color="inherit" />
                      ) : (
                        <SaveIcon />
                      )
                    }
                    onClick={() => handleSaveRow(currency)}
                    disabled={savingId === currency.id || !hasChanges(currency.id)}
                  >
                    {savingId === currency.id ? 'Saving...' : 'Save'}
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Create Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Add Currency</DialogTitle>
        <DialogContent>
          <TextField
            label="Code (e.g. USD)"
            fullWidth
            margin="normal"
            value={form.code}
            onChange={(e) => setForm((prev) => ({ ...prev, code: e.target.value.toUpperCase() }))}
            inputProps={{ maxLength: 3 }}
          />
          <TextField
            label="Symbol (e.g. $)"
            fullWidth
            margin="normal"
            value={form.symbol}
            onChange={(e) => setForm((prev) => ({ ...prev, symbol: e.target.value }))}
            inputProps={{ maxLength: 5 }}
          />
          <TextField
            label="Rate to EUR"
            fullWidth
            margin="normal"
            type="number"
            value={form.rateToEur}
            onChange={(e) => setForm((prev) => ({ ...prev, rateToEur: e.target.value }))}
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
                checked={form.isEnabled}
                onChange={(e) => setForm((prev) => ({ ...prev, isEnabled: e.target.checked }))}
              />
            }
            label="Enabled"
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleCreate}
            disabled={creating || !form.code || !form.symbol || !form.rateToEur}
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
