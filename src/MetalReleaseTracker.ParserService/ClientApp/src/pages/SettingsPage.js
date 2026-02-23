import { useState } from 'react';
import Box from '@mui/material/Box';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import SettingsIcon from '@mui/icons-material/Settings';
import PageHeader from '../components/PageHeader';
import AiAgentsTab from '../components/settings/AiAgentsTab';
import ParsingSourcesTab from '../components/settings/ParsingSourcesTab';
import BandReferenceTab from '../components/settings/BandReferenceTab';

function TabPanel({ children, value, index }) {
  return value === index ? <Box sx={{ pt: 2 }}>{children}</Box> : null;
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
    </Box>
  );
}
