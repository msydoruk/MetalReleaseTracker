import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Skeleton from '@mui/material/Skeleton';
import NotificationsIcon from '@mui/icons-material/Notifications';
import MarkEmailUnreadIcon from '@mui/icons-material/MarkEmailUnread';
import SendIcon from '@mui/icons-material/Send';
import PageHeader from '../components/PageHeader';
import { fetchNotificationStats, sendBroadcast } from '../api/notifications';

const NOTIFICATION_TYPES = [
  { value: 'PriceDrop', label: 'Price Drop' },
  { value: 'BackInStock', label: 'Back In Stock' },
  { value: 'Restock', label: 'Restock' },
  { value: 'NewVariant', label: 'New Variant' },
];

const STAT_COLORS = {
  totalCount: '#2196f3',
  unreadCount: '#ff9800',
  PriceDrop: '#4caf50',
  BackInStock: '#e91e63',
  Restock: '#9c27b0',
  NewVariant: '#00bcd4',
};

export default function NotificationsPage() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [content, setContent] = useState('');
  const [notificationType, setNotificationType] = useState('PriceDrop');
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [sending, setSending] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadStats = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await fetchNotificationStats();
      setStats(data);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load notification stats', 'error');
    } finally {
      setLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadStats();
  }, [loadStats]);

  const handleOpenConfirm = useCallback(() => {
    if (!content.trim()) {
      showSnackbar('Please enter notification content', 'warning');
      return;
    }
    setConfirmDialogOpen(true);
  }, [content, showSnackbar]);

  const handleSendBroadcast = useCallback(async () => {
    try {
      setSending(true);
      const { data } = await sendBroadcast({
        content: content.trim(),
        notificationType,
      });
      const sentCount = data?.sentCount ?? 0;
      showSnackbar(`Broadcast sent to ${sentCount} user${sentCount !== 1 ? 's' : ''}`);
      setConfirmDialogOpen(false);
      setContent('');
      loadStats();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to send broadcast', 'error');
    } finally {
      setSending(false);
    }
  }, [content, notificationType, loadStats, showSnackbar]);

  const byType = stats?.byType || {};

  return (
    <Box>
      <PageHeader title="Notifications" subtitle="Notification statistics and broadcasting" />

      {/* Stats Section */}
      <Grid container spacing={2.5} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card
            sx={{
              position: 'relative',
              overflow: 'hidden',
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': {
                transform: 'translateY(-2px)',
                boxShadow: `0 4px 20px ${STAT_COLORS.totalCount}33`,
              },
            }}
          >
            <CardContent sx={{ p: 2.5 }}>
              <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                <Box>
                  <Typography
                    variant="body2"
                    sx={{
                      color: 'rgba(255, 255, 255, 0.5)',
                      fontWeight: 500,
                      fontSize: '0.75rem',
                      textTransform: 'uppercase',
                      letterSpacing: '0.05em',
                      mb: 0.5,
                    }}
                  >
                    Total Notifications
                  </Typography>
                  {loading ? (
                    <Skeleton variant="text" width={60} height={42} />
                  ) : (
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#fff', lineHeight: 1.2 }}>
                      {stats?.totalCount != null ? stats.totalCount.toLocaleString() : '--'}
                    </Typography>
                  )}
                </Box>
                <Box
                  sx={{
                    width: 44,
                    height: 44,
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: `${STAT_COLORS.totalCount}20`,
                  }}
                >
                  <NotificationsIcon sx={{ color: STAT_COLORS.totalCount, fontSize: 24 }} />
                </Box>
              </Box>
            </CardContent>
            <Box
              sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 3,
                backgroundColor: STAT_COLORS.totalCount,
                opacity: 0.6,
              }}
            />
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card
            sx={{
              position: 'relative',
              overflow: 'hidden',
              transition: 'transform 0.2s, box-shadow 0.2s',
              '&:hover': {
                transform: 'translateY(-2px)',
                boxShadow: `0 4px 20px ${STAT_COLORS.unreadCount}33`,
              },
            }}
          >
            <CardContent sx={{ p: 2.5 }}>
              <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                <Box>
                  <Typography
                    variant="body2"
                    sx={{
                      color: 'rgba(255, 255, 255, 0.5)',
                      fontWeight: 500,
                      fontSize: '0.75rem',
                      textTransform: 'uppercase',
                      letterSpacing: '0.05em',
                      mb: 0.5,
                    }}
                  >
                    Unread
                  </Typography>
                  {loading ? (
                    <Skeleton variant="text" width={60} height={42} />
                  ) : (
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#fff', lineHeight: 1.2 }}>
                      {stats?.unreadCount != null ? stats.unreadCount.toLocaleString() : '--'}
                    </Typography>
                  )}
                </Box>
                <Box
                  sx={{
                    width: 44,
                    height: 44,
                    borderRadius: 2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    backgroundColor: `${STAT_COLORS.unreadCount}20`,
                  }}
                >
                  <MarkEmailUnreadIcon sx={{ color: STAT_COLORS.unreadCount, fontSize: 24 }} />
                </Box>
              </Box>
            </CardContent>
            <Box
              sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 3,
                backgroundColor: STAT_COLORS.unreadCount,
                opacity: 0.6,
              }}
            />
          </Card>
        </Grid>

        {NOTIFICATION_TYPES.map((type) => {
          const color = STAT_COLORS[type.value] || '#607d8b';
          const count = byType[type.value];

          return (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={type.value}>
              <Card
                sx={{
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: `0 4px 20px ${color}33`,
                  },
                }}
              >
                <CardContent sx={{ p: 2.5 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
                    <Box>
                      <Typography
                        variant="body2"
                        sx={{
                          color: 'rgba(255, 255, 255, 0.5)',
                          fontWeight: 500,
                          fontSize: '0.75rem',
                          textTransform: 'uppercase',
                          letterSpacing: '0.05em',
                          mb: 0.5,
                        }}
                      >
                        {type.label}
                      </Typography>
                      {loading ? (
                        <Skeleton variant="text" width={60} height={42} />
                      ) : (
                        <Typography variant="h4" sx={{ fontWeight: 700, color: '#fff', lineHeight: 1.2 }}>
                          {count != null ? count.toLocaleString() : '--'}
                        </Typography>
                      )}
                    </Box>
                    <Box
                      sx={{
                        width: 44,
                        height: 44,
                        borderRadius: 2,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        backgroundColor: `${color}20`,
                      }}
                    >
                      <NotificationsIcon sx={{ color, fontSize: 24 }} />
                    </Box>
                  </Box>
                </CardContent>
                <Box
                  sx={{
                    position: 'absolute',
                    bottom: 0,
                    left: 0,
                    right: 0,
                    height: 3,
                    backgroundColor: color,
                    opacity: 0.6,
                  }}
                />
              </Card>
            </Grid>
          );
        })}
      </Grid>

      {/* Broadcast Section */}
      <Card>
        <CardContent sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Send Broadcast
          </Typography>
          <TextField
            label="Content"
            fullWidth
            multiline
            rows={4}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Enter notification message..."
            sx={{ mb: 2 }}
          />
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', flexWrap: 'wrap' }}>
            <TextField
              select
              label="Notification Type"
              value={notificationType}
              onChange={(e) => setNotificationType(e.target.value)}
              size="small"
              sx={{ width: 200 }}
            >
              {NOTIFICATION_TYPES.map((type) => (
                <MenuItem key={type.value} value={type.value}>
                  {type.label}
                </MenuItem>
              ))}
            </TextField>
            <Button
              variant="contained"
              startIcon={<SendIcon />}
              onClick={handleOpenConfirm}
              disabled={sending || !content.trim()}
            >
              Send to All Users
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Confirmation Dialog */}
      <Dialog open={confirmDialogOpen} onClose={() => setConfirmDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Confirm Broadcast</DialogTitle>
        <DialogContent>
          Are you sure you want to send this <strong>{NOTIFICATION_TYPES.find((t) => t.value === notificationType)?.label}</strong> notification to all users?
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setConfirmDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSendBroadcast} disabled={sending}>
            {sending ? 'Sending...' : 'Confirm Send'}
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
