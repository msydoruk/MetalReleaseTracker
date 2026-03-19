import { useState } from 'react';
import Box from '@mui/material/Box';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import PhotoCameraIcon from '@mui/icons-material/PhotoCamera';
import SettingsIcon from '@mui/icons-material/Settings';
import PageHeader from '../components/PageHeader';
import AiAgentsTab from '../components/settings/AiAgentsTab';
import ParsingSourcesTab from '../components/settings/ParsingSourcesTab';
import BandReferenceTab from '../components/settings/BandReferenceTab';
import { syncBandPhotos } from '../api/bandPhotos';

function TabPanel({ children, value, index }) {
  return value === index ? <Box sx={{ pt: 2 }}>{children}</Box> : null;
}

function OperationsTab() {
  const [loading, setLoading] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const handleSyncPhotos = async () => {
    setLoading(true);
    try {
      await syncBandPhotos();
      setSnackbar({ open: true, message: 'Sync started. Track progress in Parsing Monitor.', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to start band photo sync.', severity: 'error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box>
      <Card sx={{ backgroundColor: 'rgba(255, 255, 255, 0.03)', border: '1px solid rgba(255, 255, 255, 0.08)' }}>
        <CardContent sx={{ p: 3 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
            Band Photo Sync
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Download band photos from Metal Archives and upload to MinIO storage. Only processes bands that appear in the catalogue. Skips bands whose photos are already uploaded.
          </Typography>
          <Button
            variant="contained"
            startIcon={<PhotoCameraIcon />}
            onClick={handleSyncPhotos}
            disabled={loading}
          >
            {loading ? 'Starting...' : 'Sync Band Photos'}
          </Button>
        </CardContent>
      </Card>
      <Snackbar
        open={snackbar.open}
        autoHideDuration={5000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
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

export default function SettingsPage() {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'auto' }}>
      <PageHeader
        icon={<SettingsIcon />}
        title="Settings"
        subtitle="Manage AI agents, parsing sources, and service configuration"
      />
      <Tabs
        value={tab}
        onChange={(_, newValue) => setTab(newValue)}
        sx={{
          borderBottom: 1,
          borderColor: 'rgba(255, 255, 255, 0.08)',
          '& .MuiTab-root': { textTransform: 'none', fontWeight: 500 },
        }}
      >
        <Tab label="AI Agents" />
        <Tab label="Parsing Sources" />
        <Tab label="Band Reference" />
        <Tab label="Operations" />
      </Tabs>
      <TabPanel value={tab} index={0}>
        <AiAgentsTab />
      </TabPanel>
      <TabPanel value={tab} index={1}>
        <ParsingSourcesTab />
      </TabPanel>
      <TabPanel value={tab} index={2}>
        <BandReferenceTab />
      </TabPanel>
      <TabPanel value={tab} index={3}>
        <OperationsTab />
      </TabPanel>
    </Box>
  );
}
