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
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import SmartToyIcon from '@mui/icons-material/SmartToy';
import { fetchAiAgents, createAiAgent, updateAiAgent, deleteAiAgent } from '../../api/settings';

const EMPTY_AGENT = {
  name: '',
  description: '',
  systemPrompt: '',
  model: 'claude-sonnet-4-20250514',
  maxTokens: 1024,
  maxConcurrentRequests: 5,
  delayBetweenBatchesMs: 1000,
  apiKey: '',
  isActive: false,
};

export default function AiAgentsTab() {
  const [agents, setAgents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editingAgent, setEditingAgent] = useState(null);
  const [deletingAgent, setDeletingAgent] = useState(null);
  const [form, setForm] = useState(EMPTY_AGENT);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await fetchAiAgents();
      setAgents(data);
    } catch {
      setSnackbar({ open: true, message: 'Failed to load AI agents', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleCreate = () => {
    setEditingAgent(null);
    setForm(EMPTY_AGENT);
    setDialogOpen(true);
  };

  const handleEdit = (agent) => {
    setEditingAgent(agent);
    setForm({
      name: agent.name,
      description: agent.description || '',
      systemPrompt: agent.systemPrompt,
      model: agent.model,
      maxTokens: agent.maxTokens,
      maxConcurrentRequests: agent.maxConcurrentRequests,
      delayBetweenBatchesMs: agent.delayBetweenBatchesMs,
      apiKey: '',
      isActive: agent.isActive,
    });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editingAgent) {
        await updateAiAgent(editingAgent.id, {
          ...form,
          apiKey: form.apiKey || null,
        });
        setSnackbar({ open: true, message: 'Agent updated', severity: 'success' });
      } else {
        await createAiAgent(form);
        setSnackbar({ open: true, message: 'Agent created', severity: 'success' });
      }
      setDialogOpen(false);
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to save agent', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    try {
      await deleteAiAgent(deletingAgent.id);
      setSnackbar({ open: true, message: 'Agent deleted', severity: 'success' });
      setDeleteDialogOpen(false);
      setDeletingAgent(null);
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to delete agent', severity: 'error' });
    }
  };

  const updateForm = (field, value) => setForm((prev) => ({ ...prev, [field]: value }));

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="body2" color="text.secondary">
          Manage AI agents used for album verification. Only one agent can be active at a time.
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreate} size="small">
          Add Agent
        </Button>
      </Box>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        {agents.map((agent) => (
          <Card
            key={agent.id}
            sx={{
              border: agent.isActive ? '1px solid rgba(76, 175, 80, 0.5)' : '1px solid rgba(255, 255, 255, 0.08)',
              backgroundColor: agent.isActive ? 'rgba(76, 175, 80, 0.04)' : 'transparent',
            }}
          >
            <CardContent sx={{ pb: '16px !important' }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                  <SmartToyIcon sx={{ color: agent.isActive ? '#66bb6a' : 'rgba(255, 255, 255, 0.3)', fontSize: 28 }} />
                  <Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>{agent.name}</Typography>
                      {agent.isActive && (
                        <Chip label="Active" size="small" color="success" variant="outlined" sx={{ height: 20, fontSize: '0.7rem' }} />
                      )}
                    </Box>
                    {agent.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                        {agent.description}
                      </Typography>
                    )}
                  </Box>
                </Box>
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                  <IconButton size="small" onClick={() => handleEdit(agent)}>
                    <EditIcon fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => { setDeletingAgent(agent); setDeleteDialogOpen(true); }}
                    sx={{ color: '#ef5350' }}
                  >
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Box>
              </Box>
              <Box sx={{ display: 'flex', gap: 2, mt: 1.5, flexWrap: 'wrap' }}>
                <Typography variant="caption" color="text.secondary">
                  Model: <strong>{agent.model}</strong>
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Max Tokens: <strong>{agent.maxTokens}</strong>
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Concurrency: <strong>{agent.maxConcurrentRequests}</strong>
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Batch Delay: <strong>{agent.delayBetweenBatchesMs}ms</strong>
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  API Key: <strong>{agent.hasApiKey ? 'Set' : 'Not Set'}</strong>
                </Typography>
              </Box>
            </CardContent>
          </Card>
        ))}
        {agents.length === 0 && (
          <Typography color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            No AI agents configured. Create one to enable AI verification.
          </Typography>
        )}
      </Box>

      <Dialog open={dialogOpen} onClose={() => !saving && setDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>{editingAgent ? 'Edit AI Agent' : 'Create AI Agent'}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
          <TextField
            label="Name"
            value={form.name}
            onChange={(e) => updateForm('name', e.target.value)}
            size="small"
            fullWidth
            required
          />
          <TextField
            label="Description"
            value={form.description}
            onChange={(e) => updateForm('description', e.target.value)}
            size="small"
            fullWidth
          />
          <TextField
            label="System Prompt"
            value={form.systemPrompt}
            onChange={(e) => updateForm('systemPrompt', e.target.value)}
            size="small"
            fullWidth
            multiline
            minRows={6}
            maxRows={14}
            helperText="Template variables: {{bandName}}, {{albumTitle}}, {{discography}}"
          />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Model"
              value={form.model}
              onChange={(e) => updateForm('model', e.target.value)}
              size="small"
              fullWidth
            />
            <TextField
              label="Max Tokens"
              type="number"
              value={form.maxTokens}
              onChange={(e) => updateForm('maxTokens', parseInt(e.target.value, 10) || 0)}
              size="small"
              sx={{ minWidth: 130 }}
            />
          </Box>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Max Concurrent Requests"
              type="number"
              value={form.maxConcurrentRequests}
              onChange={(e) => updateForm('maxConcurrentRequests', parseInt(e.target.value, 10) || 0)}
              size="small"
              fullWidth
            />
            <TextField
              label="Delay Between Batches (ms)"
              type="number"
              value={form.delayBetweenBatchesMs}
              onChange={(e) => updateForm('delayBetweenBatchesMs', parseInt(e.target.value, 10) || 0)}
              size="small"
              fullWidth
            />
          </Box>
          <TextField
            label={editingAgent ? 'API Key (leave blank to keep existing)' : 'API Key'}
            value={form.apiKey}
            onChange={(e) => updateForm('apiKey', e.target.value)}
            size="small"
            fullWidth
            type="password"
          />
          <FormControlLabel
            control={<Switch checked={form.isActive} onChange={(e) => updateForm('isActive', e.target.checked)} />}
            label="Active (only one agent can be active)"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)} disabled={saving}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving || !form.name || !form.systemPrompt || (!editingAgent && !form.apiKey)}
          >
            {saving ? <CircularProgress size={20} /> : (editingAgent ? 'Save' : 'Create')}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="sm">
        <DialogTitle>Delete AI Agent</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete <strong>{deletingAgent?.name}</strong>?
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" color="error" onClick={handleDelete}>Delete</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
        <Alert severity={snackbar.severity} variant="filled">{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
