import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';
import { DataGrid } from '@mui/x-data-grid';
import PageHeader from '../components/PageHeader';
import DistributorChip from '../components/DistributorChip';
import { fetchParsingRunById, fetchParsingRunItems } from '../api/parsingMonitor';

const JOB_TYPES = { 0: 'Detail Parsing', 1: 'Catalogue Index' };
const RUN_STATUS = {
  0: { label: 'Running', color: 'info' },
  1: { label: 'Completed', color: 'success' },
  2: { label: 'Failed', color: 'error' },
  3: { label: 'Cancelled', color: 'warning' },
};

const COUNTER_DISPLAY = {
  new: { label: 'New', color: '#4caf50' },
  updated: { label: 'Updated', color: '#ff9800' },
  active: { label: 'Active', color: '#90a4ae' },
  deleted: { label: 'Deleted', color: '#ef5350' },
  newEntry: { label: 'New', color: '#4caf50' },
  existingEntry: { label: 'Existing', color: '#90a4ae' },
  relevant: { label: 'Relevant', color: '#42a5f5' },
  notRelevant: { label: 'Not Relevant', color: '#78909c' },
};

const CATEGORY_DISPLAY = {
  new: { label: 'New', color: '#4caf50' },
  updated: { label: 'Updated', color: '#ff9800' },
  active: { label: 'Active', color: '#90a4ae' },
  deleted: { label: 'Deleted', color: '#ef5350' },
  newEntry: { label: 'New Entry', color: '#4caf50' },
  existingEntry: { label: 'Existing', color: '#90a4ae' },
  relevant: { label: 'Relevant', color: '#42a5f5' },
  notRelevant: { label: 'Not Relevant', color: '#78909c' },
};

function formatDuration(startedAt, completedAt) {
  const start = new Date(startedAt);
  const end = completedAt ? new Date(completedAt) : new Date();
  const seconds = Math.floor((end - start) / 1000);
  if (seconds < 60) return `${seconds}s`;
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${minutes}m ${remainingSeconds}s`;
}

export default function ParsingRunDetailPage() {
  const { runId } = useParams();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [run, setRun] = useState(null);
  const [loading, setLoading] = useState(true);
  const [items, setItems] = useState([]);
  const [itemCount, setItemCount] = useState(0);
  const [itemsLoading, setItemsLoading] = useState(false);

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '50', 10);

  useEffect(() => {
    (async () => {
      try {
        const { data } = await fetchParsingRunById(runId);
        setRun(data);
      } catch (err) {
        console.error('Failed to load parsing run', err);
      } finally {
        setLoading(false);
      }
    })();
  }, [runId]);

  const loadItems = useCallback(async () => {
    setItemsLoading(true);
    try {
      const { data } = await fetchParsingRunItems(runId, { page, pageSize });
      setItems(data.items);
      setItemCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load run items', err);
    } finally {
      setItemsLoading(false);
    }
  }, [runId, page, pageSize]);

  useEffect(() => { loadItems(); }, [loadItems]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => {
      if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k];
    });
    setSearchParams(params);
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;
  if (!run) return <Typography>Parsing run not found.</Typography>;

  const statusCfg = RUN_STATUS[run.status] || { label: 'Unknown', color: 'default' };

  const columns = [
    {
      field: 'itemDescription',
      headerName: 'Item',
      flex: 1,
      minWidth: 250,
      renderCell: ({ value }) => (
        <Typography variant="body2" sx={{ fontWeight: 500 }}>{value}</Typography>
      ),
    },
    {
      field: 'categories',
      headerName: 'Categories',
      width: 250,
      sortable: false,
      renderCell: ({ value }) => (
        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
          {(value || []).map((cat) => {
            const display = CATEGORY_DISPLAY[cat] || { label: cat, color: '#9e9e9e' };
            return (
              <Chip
                key={cat}
                label={display.label}
                size="small"
                sx={{
                  backgroundColor: `${display.color}20`,
                  color: display.color,
                  fontWeight: 600,
                  fontSize: '0.7rem',
                  height: 22,
                }}
              />
            );
          })}
        </Box>
      ),
    },
    {
      field: 'isSuccess',
      headerName: 'Status',
      width: 100,
      renderCell: ({ value }) => (
        <Chip
          label={value ? 'Success' : 'Failed'}
          size="small"
          color={value ? 'success' : 'error'}
          variant="outlined"
        />
      ),
    },
    {
      field: 'errorMessage',
      headerName: 'Error',
      width: 200,
      renderCell: ({ value }) =>
        value ? (
          <Typography
            variant="body2"
            sx={{ color: '#ef5350', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
            title={value}
          >
            {value}
          </Typography>
        ) : (
          <Box sx={{ color: 'rgba(255, 255, 255, 0.2)' }}>-</Box>
        ),
    },
    {
      field: 'processedAt',
      headerName: 'Time',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/parsing-monitor')}
        sx={{ mb: 2, color: 'rgba(255, 255, 255, 0.6)', '&:hover': { color: 'rgba(255, 255, 255, 0.9)' }, alignSelf: 'flex-start' }}
      >
        Back to Parsing Monitor
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
              <MonitorHeartIcon sx={{ color: '#e53935' }} />
            </Box>
            <Box>
              <Typography variant="h5">{JOB_TYPES[run.jobType] || 'Unknown Job'}</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                <DistributorChip code={run.distributorCode} />
                <Chip label={statusCfg.label} size="small" color={statusCfg.color} variant="outlined" />
              </Box>
            </Box>
          </Box>

          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', mt: 2, pl: 0.5 }}>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Started</Typography>
              <Typography>{new Date(run.startedAt).toLocaleString()}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Duration</Typography>
              <Typography>{formatDuration(run.startedAt, run.completedAt)}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Total</Typography>
              <Typography sx={{ fontWeight: 600 }}>{run.totalItems || '-'}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Processed</Typography>
              <Typography sx={{ fontWeight: 600, color: '#e53935' }}>{run.processedItems}</Typography>
            </Box>
            {run.failedItems > 0 && (
              <Box>
                <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Failed</Typography>
                <Typography sx={{ fontWeight: 600, color: '#ef5350' }}>{run.failedItems}</Typography>
              </Box>
            )}
          </Box>

          {run.counters && Object.keys(run.counters).length > 0 && (
            <Box sx={{ mt: 2, display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
              {Object.entries(run.counters).map(([key, value]) => {
                const display = COUNTER_DISPLAY[key];
                if (!display || value === 0) return null;
                return (
                  <Chip
                    key={key}
                    label={`${value} ${display.label}`}
                    size="small"
                    sx={{
                      backgroundColor: `${display.color}20`,
                      color: display.color,
                      fontWeight: 600,
                      fontSize: '0.75rem',
                    }}
                  />
                );
              })}
            </Box>
          )}

          {run.errorMessage && (
            <Box sx={{ mt: 2, p: 1.5, borderRadius: 1, backgroundColor: 'rgba(239, 83, 80, 0.08)', border: '1px solid rgba(239, 83, 80, 0.2)' }}>
              <Typography variant="body2" sx={{ color: '#ef5350' }}>{run.errorMessage}</Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      <PageHeader
        icon={<MonitorHeartIcon />}
        title="Processed Items"
        subtitle={`${itemCount} items`}
      />

      <DataGrid
        rows={items}
        columns={columns}
        rowCount={itemCount}
        loading={itemsLoading}
        paginationMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(m) => updateParams({ page: m.page + 1, pageSize: m.pageSize })}
        pageSizeOptions={[25, 50, 100]}
        sx={{ flexGrow: 1, minHeight: 0, '& .MuiDataGrid-virtualScroller': { overflowX: 'hidden' } }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
