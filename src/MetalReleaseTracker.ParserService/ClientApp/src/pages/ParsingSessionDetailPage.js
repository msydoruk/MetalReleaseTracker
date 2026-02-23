import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SyncIcon from '@mui/icons-material/Sync';
import EventNoteIcon from '@mui/icons-material/EventNote';
import { DataGrid } from '@mui/x-data-grid';
import StatusChip from '../components/StatusChip';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import { PARSING_STATUS } from '../constants';
import { fetchParsingSessionById } from '../api/parsingSessions';

export default function ParsingSessionDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [session, setSession] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const { data } = await fetchParsingSessionById(id);
        setSession(data);
      } catch (err) {
        console.error('Failed to load parsing session', err);
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;
  if (!session) return <Typography>Parsing session not found.</Typography>;

  const columns = [
    {
      field: 'createdDate',
      headerName: 'Created',
      width: 180,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
    {
      field: 'eventPayloadPreview',
      headerName: 'Payload Preview',
      flex: 1,
      minWidth: 300,
      renderCell: ({ value }) => (
        <Box sx={{ fontFamily: 'monospace', fontSize: '0.75rem', color: 'rgba(255, 255, 255, 0.7)', overflow: 'hidden', textOverflow: 'ellipsis' }}>
          {value}
        </Box>
      ),
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'auto' }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/parsing-sessions')}
        sx={{ mb: 2, color: 'rgba(255, 255, 255, 0.6)', '&:hover': { color: 'rgba(255, 255, 255, 0.9)' } }}
      >
        Back to Parsing Sessions
      </Button>
      <Card sx={{ mb: 3, border: '1px solid rgba(183, 28, 28, 0.2)' }}>
        <CardContent sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            <Box sx={{
              width: 48,
              height: 48,
              borderRadius: 2,
              backgroundColor: 'rgba(183, 28, 28, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}>
              <SyncIcon sx={{ color: '#e53935' }} />
            </Box>
            <Box>
              <Typography variant="h5">Parsing Session</Typography>
              <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                <DistributorChip code={session.distributorCode} />
                <StatusChip value={session.parsingStatus} statusMap={PARSING_STATUS} />
              </Box>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', mt: 2, pl: 0.5 }}>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Last Updated</Typography>
              <Typography>{new Date(session.lastUpdatedDate).toLocaleString()}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Events</Typography>
              <Typography sx={{ fontWeight: 600, color: '#e53935' }}>{session.eventCount}</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
      <PageHeader
        icon={<EventNoteIcon />}
        title="Events"
        subtitle={`${session.eventCount} parsed events`}
      />
      <DataGrid
        rows={session.events}
        columns={columns}
        pageSizeOptions={[10, 25, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
        sx={{ flexGrow: 1, minHeight: 300 }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
