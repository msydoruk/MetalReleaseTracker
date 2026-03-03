import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import LinearProgress from '@mui/material/LinearProgress';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import { DataGrid } from '@mui/x-data-grid';
import MonitorHeartIcon from '@mui/icons-material/MonitorHeart';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import { DISTRIBUTOR_CODES } from '../constants';
import { fetchParsingRuns, subscribeToLiveWithAuth } from '../api/parsingMonitor';

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

function formatDuration(startedAt, completedAt) {
  const start = new Date(startedAt);
  const end = completedAt ? new Date(completedAt) : new Date();
  const seconds = Math.floor((end - start) / 1000);
  if (seconds < 60) return `${seconds}s`;
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${minutes}m ${remainingSeconds}s`;
}

function CounterChips({ counters, size = 'small' }) {
  if (!counters || Object.keys(counters).length === 0) return null;

  return (
    <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
      {Object.entries(counters).map(([key, value]) => {
        const display = COUNTER_DISPLAY[key];
        if (!display || value === 0) return null;
        return (
          <Chip
            key={key}
            label={`${value} ${display.label}`}
            size={size}
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
  );
}

function ActiveRunCard({ run }) {
  const isIndeterminate = run.total === 0;
  const progress = !isIndeterminate && run.total > 0 ? (run.processed / run.total) * 100 : 0;

  return (
    <Card sx={{
      backgroundColor: 'rgba(255, 255, 255, 0.03)',
      border: '1px solid rgba(255, 255, 255, 0.08)',
      minWidth: 320,
    }}>
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CircularProgress size={16} sx={{ color: '#e53935' }} />
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
              {JOB_TYPES[run.jobType] || 'Unknown'}
            </Typography>
          </Box>
          <DistributorChip code={run.distributorCode} />
        </Box>

        <Box sx={{ mb: 1.5 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
            <Typography variant="caption" color="text.secondary">
              {isIndeterminate
                ? `${run.processed} items processed`
                : `${run.processed} / ${run.total} items`}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {run.failed > 0 && (
                <Box component="span" sx={{ color: '#ef5350', mr: 1 }}>
                  {run.failed} failed
                </Box>
              )}
              {formatDuration(run.startedAt)}
            </Typography>
          </Box>
          <LinearProgress
            variant={isIndeterminate ? 'indeterminate' : 'determinate'}
            value={progress}
            sx={{ height: 6, borderRadius: 3 }}
          />
        </Box>

        <CounterChips counters={run.counters} />

        {run.currentItem && (
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{
              display: 'block',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              mt: run.counters ? 1 : 0,
            }}
          >
            Current: {run.currentItem}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

export default function ParsingMonitorPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeRuns, setActiveRuns] = useState({});
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [connected, setConnected] = useState(false);
  const disconnectRef = useRef(null);
  const navigate = useNavigate();

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);

  const loadHistory = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await fetchParsingRuns({ page, pageSize });
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load parsing runs', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => { loadHistory(); }, [loadHistory]);

  useEffect(() => {
    const disconnect = subscribeToLiveWithAuth(
      (event) => {
        setConnected(true);
        if (event.type === 'started') {
          setActiveRuns((prev) => ({
            ...prev,
            [event.runId]: {
              runId: event.runId,
              jobType: event.jobType,
              distributorCode: event.distributorCode,
              processed: event.processed,
              total: event.total,
              failed: event.failed,
              currentItem: event.currentItem,
              counters: event.counters,
              startedAt: event.timestamp,
            },
          }));
        } else if (event.type === 'progress') {
          setActiveRuns((prev) => {
            const existing = prev[event.runId];
            if (!existing) return prev;
            return {
              ...prev,
              [event.runId]: {
                ...existing,
                processed: event.processed,
                failed: event.failed,
                currentItem: event.currentItem,
                counters: event.counters || existing.counters,
              },
            };
          });
        } else if (event.type === 'error') {
          setActiveRuns((prev) => {
            const existing = prev[event.runId];
            if (!existing) return prev;
            return {
              ...prev,
              [event.runId]: {
                ...existing,
                failed: event.failed,
                currentItem: event.currentItem,
                counters: event.counters || existing.counters,
              },
            };
          });
        } else if (event.type === 'completed') {
          setActiveRuns((prev) => {
            const next = { ...prev };
            delete next[event.runId];
            return next;
          });
          loadHistory();
        }
      },
      () => {
        setConnected(false);
      },
    );

    disconnectRef.current = disconnect;

    return () => {
      if (typeof disconnect === 'function') disconnect();
    };
  }, [loadHistory]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => {
      if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k];
    });
    setSearchParams(params);
  };

  const activeRunList = Object.values(activeRuns);

  const columns = [
    {
      field: 'jobType',
      headerName: 'Job Type',
      width: 140,
      renderCell: ({ value }) => (
        <Chip
          label={JOB_TYPES[value] || 'Unknown'}
          size="small"
          sx={{
            backgroundColor: 'rgba(255, 255, 255, 0.06)',
            color: 'rgba(255, 255, 255, 0.8)',
            fontWeight: 500,
          }}
        />
      ),
    },
    {
      field: 'distributorCode',
      headerName: 'Distributor',
      width: 160,
      renderCell: ({ value }) => value != null ? <DistributorChip code={value} /> : '-',
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: ({ value }) => {
        const cfg = RUN_STATUS[value] || { label: 'Unknown', color: 'default' };
        return <Chip label={cfg.label} size="small" color={cfg.color} variant="outlined" />;
      },
    },
    {
      field: 'totalItems',
      headerName: 'Total',
      width: 80,
      type: 'number',
      renderCell: ({ value }) => value === 0 ? '-' : value,
    },
    {
      field: 'processedItems',
      headerName: 'Processed',
      width: 100,
      type: 'number',
    },
    {
      field: 'failedItems',
      headerName: 'Failed',
      width: 80,
      type: 'number',
      renderCell: ({ value }) => (
        <Typography
          variant="body2"
          sx={{ color: value > 0 ? '#ef5350' : 'rgba(255, 255, 255, 0.5)' }}
        >
          {value}
        </Typography>
      ),
    },
    {
      field: 'counters',
      headerName: 'Breakdown',
      flex: 1,
      minWidth: 200,
      sortable: false,
      renderCell: ({ row }) => <CounterChips counters={row.counters} />,
    },
    {
      field: 'startedAt',
      headerName: 'Started',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
    {
      field: 'duration',
      headerName: 'Duration',
      width: 100,
      valueGetter: (value, row) => formatDuration(row.startedAt, row.completedAt),
    },
    {
      field: 'errorMessage',
      headerName: 'Error',
      width: 150,
      renderCell: ({ value }) =>
        value ? (
          <Typography
            variant="body2"
            sx={{
              color: '#ef5350',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
            title={value}
          >
            {value}
          </Typography>
        ) : (
          <Box sx={{ color: 'rgba(255, 255, 255, 0.2)' }}>-</Box>
        ),
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <PageHeader
        icon={<MonitorHeartIcon />}
        title="Parsing Monitor"
        subtitle={connected ? 'Live connected' : 'Connecting...'}
        action={
          <Chip
            label={connected ? 'Live' : 'Disconnected'}
            size="small"
            color={connected ? 'success' : 'default'}
            variant="outlined"
            sx={{ fontWeight: 600 }}
          />
        }
      />

      {activeRunList.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle2" sx={{ mb: 1.5, color: 'rgba(255, 255, 255, 0.6)', textTransform: 'uppercase', fontSize: '0.7rem', letterSpacing: 1 }}>
            Active Runs
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            {activeRunList.map((run) => (
              <ActiveRunCard key={run.runId} run={run} />
            ))}
          </Box>
        </Box>
      )}

      {activeRunList.length === 0 && !loading && rows.length === 0 && (
        <Alert severity="info" sx={{ mb: 2 }}>
          No parsing runs yet. Runs will appear here when parsing jobs are triggered via TickerQ.
        </Alert>
      )}

      <Typography variant="subtitle2" sx={{ mb: 1.5, color: 'rgba(255, 255, 255, 0.6)', textTransform: 'uppercase', fontSize: '0.7rem', letterSpacing: 1 }}>
        Run History
      </Typography>

      <DataGrid
        rows={rows}
        columns={columns}
        rowCount={rowCount}
        loading={loading}
        paginationMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(m) => updateParams({ page: m.page + 1, pageSize: m.pageSize })}
        pageSizeOptions={[10, 25, 50]}
        sx={{
          flexGrow: 1,
          minHeight: 0,
          '& .MuiDataGrid-virtualScroller': { overflowX: 'hidden' },
          '& .MuiDataGrid-row': { cursor: 'pointer' },
        }}
        disableRowSelectionOnClick
        onRowClick={(params) => navigate(`/parsing-monitor/${params.id}`)}
      />
    </Box>
  );
}
