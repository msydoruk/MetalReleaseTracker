import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import LinearProgress from '@mui/material/LinearProgress';
import Tooltip from '@mui/material/Tooltip';
import IconButton from '@mui/material/IconButton';
import CircularProgress from '@mui/material/CircularProgress';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import SmartToyIcon from '@mui/icons-material/SmartToy';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import { DataGrid } from '@mui/x-data-grid';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import FilterBar from '../components/FilterBar';
import { DISTRIBUTOR_CODES } from '../constants';
import DialogContentText from '@mui/material/DialogContentText';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import RemoveDoneIcon from '@mui/icons-material/RemoveDone';
import { fetchAiVerifications, runVerificationStream, setDecision, batchSetDecision, bulkSetDecision } from '../api/aiVerification';

export default function AiVerificationPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [selectedIds, setSelectedIds] = useState([]);
  const [runDialogOpen, setRunDialogOpen] = useState(false);
  const [runDistributor, setRunDistributor] = useState('');
  const [running, setRunning] = useState(false);
  const [runProgress, setRunProgress] = useState({ processed: 0, total: 0, failed: 0, current: '' });
  const [runFinished, setRunFinished] = useState(false);
  const [runError, setRunError] = useState(null);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false);
  const [bulkDecision, setBulkDecision] = useState(null);

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);
  const distributorCode = searchParams.get('distributorCode') || '';
  const verifiedOnly = searchParams.get('verifiedOnly') || '';
  const isUkrainian = searchParams.get('isUkrainian') || '';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize, sortAscending: false };
      if (distributorCode) params.distributorCode = distributorCode;
      if (verifiedOnly !== '') params.verifiedOnly = verifiedOnly === 'true';
      if (isUkrainian !== '') params.isUkrainian = isUkrainian === 'true';
      const { data } = await fetchAiVerifications(params);
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load AI verifications', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, distributorCode, verifiedOnly, isUkrainian]);

  useEffect(() => { load(); }, [load]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => { if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k]; });
    setSearchParams(params);
  };

  const handleDecision = async (verificationId, decision) => {
    try {
      await setDecision(verificationId, decision);
      setSnackbar({ open: true, message: `Marked as ${decision === 0 ? 'Confirmed' : 'Rejected'}`, severity: 'success' });
      load();
    } catch {
      setSnackbar({ open: true, message: 'Failed to set decision', severity: 'error' });
    }
  };

  const handleBatchDecision = async (decision) => {
    const verificationIds = rows
      .filter((row) => selectedIds.includes(row.id) && row.verificationId)
      .map((row) => row.verificationId);
    if (!verificationIds.length) return;
    try {
      await batchSetDecision(verificationIds, decision);
      setSnackbar({ open: true, message: `Updated ${verificationIds.length} entries`, severity: 'success' });
      setSelectedIds([]);
      load();
    } catch {
      setSnackbar({ open: true, message: 'Batch update failed', severity: 'error' });
    }
  };

  const handleRunVerification = async () => {
    setRunning(true);
    setRunFinished(false);
    setRunError(null);
    setRunProgress({ processed: 0, total: 0, failed: 0, current: '' });
    try {
      await runVerificationStream(runDistributor || null, (event) => {
        if (event.type === 'started') {
          setRunProgress((prev) => ({ ...prev, total: event.total }));
        } else if (event.type === 'progress') {
          setRunProgress({ processed: event.processed, total: event.total, failed: event.failed, current: event.current || '' });
        } else if (event.type === 'completed') {
          setRunProgress((prev) => ({ ...prev, processed: event.processed, failed: event.failed }));
          setRunFinished(true);
        } else if (event.type === 'error') {
          setRunError(event.message || 'Unknown error');
          setRunFinished(true);
        }
      });
      if (!runError) {
        setRunFinished(true);
      }
    } catch (err) {
      setRunError(err.message || 'Verification run failed');
      setRunFinished(true);
    } finally {
      setRunning(false);
    }
  };

  const handleCloseRunDialog = () => {
    setRunDialogOpen(false);
    setRunFinished(false);
    setRunError(null);
    setRunProgress({ processed: 0, total: 0, failed: 0, current: '' });
    if (runFinished) {
      load();
    }
  };

  const handleBulkDecision = async () => {
    try {
      const { data } = await bulkSetDecision(distributorCode, isUkrainian, bulkDecision);
      setSnackbar({ open: true, message: `Updated ${data.count} entries`, severity: 'success' });
      setBulkDialogOpen(false);
      setBulkDecision(null);
      setSelectedIds([]);
      load();
    } catch {
      setSnackbar({ open: true, message: 'Bulk update failed', severity: 'error' });
    }
  };

  const selectedWithVerification = rows.filter((row) => selectedIds.includes(row.id) && row.verificationId).length;
  const verifiedRowCount = rows.filter((row) => row.verificationId != null).length;

  const columns = [
    {
      field: 'distributorCode',
      headerName: 'Distributor',
      width: 130,
      renderCell: ({ value }) => value != null ? <DistributorChip code={value} /> : '-',
    },
    {
      field: 'bandName',
      headerName: 'Band',
      flex: 1,
      minWidth: 100,
      renderCell: ({ value }) => (
        <Box sx={{ fontWeight: 500, color: 'rgba(255, 255, 255, 0.95)' }}>{value}</Box>
      ),
    },
    {
      field: 'albumTitle',
      headerName: 'Album',
      flex: 1,
      minWidth: 100,
    },
    {
      field: 'matchedAlbumTitle',
      headerName: 'Matched Album',
      flex: 1,
      minWidth: 100,
      renderCell: ({ row }) => {
        if (!row.matchedAlbumTitle) {
          return <Box sx={{ color: 'rgba(255, 255, 255, 0.2)' }}>-</Box>;
        }
        const isDifferent = row.matchedAlbumTitle !== row.albumTitle;
        return (
          <Tooltip title={`${row.matchedAlbumTitle} / ${row.matchedAlbumType || '?'} / ${row.matchedAlbumYear || '?'}`} placement="bottom-start">
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, overflow: 'hidden' }}>
              <Box sx={{
                color: isDifferent ? '#66bb6a' : 'rgba(255, 255, 255, 0.7)',
                fontWeight: isDifferent ? 500 : 400,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
              }}>
                {row.matchedAlbumTitle}
              </Box>
              {row.matchedAlbumYear && (
                <Chip
                  label={row.matchedAlbumYear}
                  size="small"
                  sx={{
                    height: 20,
                    fontSize: '0.65rem',
                    fontWeight: 600,
                    backgroundColor: 'rgba(255, 255, 255, 0.08)',
                    color: 'rgba(255, 255, 255, 0.5)',
                    flexShrink: 0,
                  }}
                />
              )}
            </Box>
          </Tooltip>
        );
      },
    },
    {
      field: 'isUkrainian',
      headerName: 'UA',
      width: 70,
      renderCell: ({ row }) => {
        if (row.verificationId == null) {
          return (
            <Chip
              label="Pending"
              size="small"
              variant="outlined"
              sx={{ borderColor: 'rgba(255, 255, 255, 0.15)', color: 'rgba(255, 255, 255, 0.35)', fontSize: '0.7rem' }}
            />
          );
        }
        return (
          <Chip
            label={row.isUkrainian ? 'Yes' : 'No'}
            size="small"
            sx={{
              backgroundColor: row.isUkrainian ? 'rgba(76, 175, 80, 0.15)' : 'rgba(244, 67, 54, 0.15)',
              color: row.isUkrainian ? '#66bb6a' : '#ef5350',
              border: `1px solid ${row.isUkrainian ? 'rgba(76, 175, 80, 0.4)' : 'rgba(244, 67, 54, 0.4)'}`,
              fontWeight: 600,
            }}
          />
        );
      },
    },
    {
      field: 'confidenceScore',
      headerName: 'Confidence',
      width: 100,
      renderCell: ({ row }) => {
        if (row.verificationId == null || row.confidenceScore == null) {
          return <Box sx={{ color: 'rgba(255, 255, 255, 0.2)' }}>-</Box>;
        }
        const pct = row.confidenceScore * 100;
        const color = pct > 70 ? 'success' : pct > 40 ? 'warning' : 'error';
        return (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
            <LinearProgress
              variant="determinate"
              value={pct}
              sx={{ flexGrow: 1, height: 6, borderRadius: 3 }}
              color={color}
            />
            <Typography variant="caption" sx={{ fontWeight: 600, minWidth: 32, textAlign: 'right' }}>
              {pct.toFixed(0)}%
            </Typography>
          </Box>
        );
      },
    },
    {
      field: 'actions',
      headerName: '',
      width: 80,
      sortable: false,
      renderCell: ({ row }) => {
        if (row.verificationId == null) {
          return (
            <Tooltip title="Run verification first">
              <HelpOutlineIcon sx={{ fontSize: 18, opacity: 0.25 }} />
            </Tooltip>
          );
        }
        return (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Confirm (Ukrainian)">
              <IconButton
                size="small"
                onClick={(e) => { e.stopPropagation(); handleDecision(row.verificationId, 0); }}
                sx={{
                  color: '#66bb6a',
                  backgroundColor: 'rgba(76, 175, 80, 0.08)',
                  '&:hover': { backgroundColor: 'rgba(76, 175, 80, 0.2)' },
                }}
              >
                <CheckCircleIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Reject (Not Ukrainian)">
              <IconButton
                size="small"
                onClick={(e) => { e.stopPropagation(); handleDecision(row.verificationId, 1); }}
                sx={{
                  color: '#ef5350',
                  backgroundColor: 'rgba(244, 67, 54, 0.08)',
                  '&:hover': { backgroundColor: 'rgba(244, 67, 54, 0.2)' },
                }}
              >
                <CancelIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        );
      },
    },
    {
      field: 'aiAnalysis',
      headerName: 'AI Analysis',
      flex: 1,
      minWidth: 80,
      renderCell: ({ row }) => {
        if (row.verificationId == null || !row.aiAnalysis) {
          return <Box sx={{ color: 'rgba(255, 255, 255, 0.2)' }}>-</Box>;
        }
        return (
          <Tooltip title={row.aiAnalysis} placement="bottom-start">
            <Box sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              fontSize: '0.8rem',
              color: 'rgba(255, 255, 255, 0.6)',
            }}>
              {row.aiAnalysis}
            </Box>
          </Tooltip>
        );
      },
    },
    {
      field: 'verifiedAt',
      headerName: 'Verified',
      width: 120,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <PageHeader
        icon={<SmartToyIcon />}
        title="AI Verification"
        subtitle={rowCount > 0 ? `${rowCount} relevant entries awaiting review` : undefined}
        action={
          <Button
            variant="contained"
            startIcon={<PlayArrowIcon />}
            onClick={() => setRunDialogOpen(true)}
            sx={{ px: 3 }}
          >
            Run Verification
          </Button>
        }
      />
      <FilterBar>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Distributor</InputLabel>
          <Select
            value={distributorCode}
            label="Distributor"
            onChange={(e) => updateParams({ distributorCode: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(DISTRIBUTOR_CODES).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Verification</InputLabel>
          <Select
            value={verifiedOnly}
            label="Verification"
            onChange={(e) => updateParams({ verifiedOnly: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="true">Verified Only</MenuItem>
            <MenuItem value="false">Not Verified</MenuItem>
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 130 }}>
          <InputLabel>Ukrainian</InputLabel>
          <Select
            value={isUkrainian}
            label="Ukrainian"
            onChange={(e) => updateParams({ isUkrainian: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="true">Yes</MenuItem>
            <MenuItem value="false">No</MenuItem>
          </Select>
        </FormControl>
        {selectedWithVerification > 0 && (
          <>
            <Button
              variant="outlined"
              color="success"
              size="small"
              startIcon={<CheckCircleIcon />}
              onClick={() => handleBatchDecision(0)}
              sx={{ borderWidth: 1.5 }}
            >
              Confirm {selectedWithVerification}
            </Button>
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<CancelIcon />}
              onClick={() => handleBatchDecision(1)}
              sx={{ borderWidth: 1.5 }}
            >
              Reject {selectedWithVerification}
            </Button>
          </>
        )}
        {verifiedRowCount > 0 && (
          <>
            <Button
              variant="outlined"
              color="success"
              size="small"
              startIcon={<DoneAllIcon />}
              onClick={() => { setBulkDecision(0); setBulkDialogOpen(true); }}
              sx={{ borderWidth: 1.5, ml: 'auto' }}
            >
              Confirm All
            </Button>
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<RemoveDoneIcon />}
              onClick={() => { setBulkDecision(1); setBulkDialogOpen(true); }}
              sx={{ borderWidth: 1.5 }}
            >
              Reject All
            </Button>
          </>
        )}
      </FilterBar>
      <DataGrid
        rows={rows}
        columns={columns}
        rowCount={rowCount}
        loading={loading}
        paginationMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(m) => updateParams({ page: m.page + 1, pageSize: m.pageSize })}
        pageSizeOptions={[10, 25, 50, 100]}
        checkboxSelection
        isRowSelectable={(params) => params.row.verificationId != null}
        rowSelectionModel={selectedIds}
        onRowSelectionModelChange={(ids) => setSelectedIds(ids)}
        sx={{ flexGrow: 1, minHeight: 0, '& .MuiDataGrid-virtualScroller': { overflowX: 'hidden' } }}
        disableRowSelectionOnClick
      />

      <Dialog open={runDialogOpen} onClose={() => !running && handleCloseRunDialog()} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ pb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <SmartToyIcon sx={{ color: '#e53935' }} />
            Run AI Verification
          </Box>
        </DialogTitle>
        <DialogContent>
          {!running && !runFinished ? (
            <>
              <Typography color="text.secondary" sx={{ mb: 2.5, fontSize: '0.875rem' }}>
                Send all "Relevant" catalogue entries to Claude API for Ukrainian band verification.
                Existing pending verifications will be replaced with fresh results.
              </Typography>
              <FormControl fullWidth size="small">
                <InputLabel>Distributor (optional)</InputLabel>
                <Select
                  value={runDistributor}
                  label="Distributor (optional)"
                  onChange={(e) => setRunDistributor(e.target.value)}
                >
                  <MenuItem value="">All Distributors</MenuItem>
                  {Object.entries(DISTRIBUTOR_CODES).map(([val, cfg]) => (
                    <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </>
          ) : (
            <Box sx={{ py: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
              {runError ? (
                <Alert severity="error" sx={{ whiteSpace: 'pre-wrap' }}>
                  {runError}
                </Alert>
              ) : runFinished ? (
                <Alert severity="success">
                  Verification complete: {runProgress.processed - runProgress.failed} verified
                  {runProgress.failed > 0 && `, ${runProgress.failed} failed`}
                </Alert>
              ) : null}

              {!runFinished && (
                <>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <CircularProgress size={24} sx={{ color: '#e53935' }} />
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      Verified {runProgress.processed} / {runProgress.total}
                      {runProgress.failed > 0 && (
                        <Typography component="span" sx={{ color: '#ef5350', ml: 1, fontSize: '0.875rem' }}>
                          ({runProgress.failed} failed)
                        </Typography>
                      )}
                    </Typography>
                  </Box>
                  {runProgress.current && (
                    <Typography
                      color="text.secondary"
                      sx={{
                        fontSize: '0.8rem',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap',
                      }}
                    >
                      Analyzing: {runProgress.current}
                    </Typography>
                  )}
                </>
              )}

              <LinearProgress
                variant={runProgress.total > 0 ? 'determinate' : 'indeterminate'}
                value={runProgress.total > 0 ? (runProgress.processed / runProgress.total) * 100 : 0}
                sx={{ width: '100%', height: 6, borderRadius: 3 }}
                color={runError ? 'error' : runFinished ? 'success' : 'primary'}
              />

              {runFinished && runProgress.total > 0 && (
                <Typography color="text.secondary" sx={{ fontSize: '0.8rem', textAlign: 'center' }}>
                  {runProgress.processed} of {runProgress.total} entries processed
                </Typography>
              )}
            </Box>
          )}
        </DialogContent>
        {!running && (
          <DialogActions sx={{ px: 3, pb: 2 }}>
            {runFinished ? (
              <Button variant="contained" onClick={handleCloseRunDialog}>Close</Button>
            ) : (
              <>
                <Button onClick={handleCloseRunDialog}>Cancel</Button>
                <Button variant="contained" onClick={handleRunVerification} startIcon={<PlayArrowIcon />}>
                  Start Verification
                </Button>
              </>
            )}
          </DialogActions>
        )}
      </Dialog>

      <Dialog open={bulkDialogOpen} onClose={() => setBulkDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {bulkDecision === 0 ? 'Confirm All Matching Entries' : 'Reject All Matching Entries'}
        </DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 2 }}>
            Are you sure you want to {bulkDecision === 0 ? 'confirm' : 'reject'} all pending verifications matching the current filters?
          </DialogContentText>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
            {distributorCode && (
              <Typography variant="body2" color="text.secondary">
                Distributor: <strong>{DISTRIBUTOR_CODES[distributorCode]?.label || distributorCode}</strong>
              </Typography>
            )}
            {isUkrainian !== '' && (
              <Typography variant="body2" color="text.secondary">
                Ukrainian: <strong>{isUkrainian === 'true' ? 'Yes' : 'No'}</strong>
              </Typography>
            )}
            {!distributorCode && isUkrainian === '' && (
              <Typography variant="body2" color="text.secondary">
                No filters applied — this will affect <strong>all</strong> pending verifications.
              </Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setBulkDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color={bulkDecision === 0 ? 'success' : 'error'}
            onClick={handleBulkDecision}
          >
            {bulkDecision === 0 ? 'Confirm All' : 'Reject All'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
      >
        <Alert severity={snackbar.severity} variant="filled">{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
