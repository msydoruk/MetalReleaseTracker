import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Skeleton from '@mui/material/Skeleton';
import { DataGrid } from '@mui/x-data-grid';
import TelegramIcon from '@mui/icons-material/Telegram';
import PeopleIcon from '@mui/icons-material/People';
import PageHeader from '../components/PageHeader';
import { fetchTelegramStats, fetchLinkedUsers } from '../api/telegram';

const STAT_COLORS = {
  linkedUsers: '#2196f3',
};

export default function TelegramPage() {
  const [stats, setStats] = useState(null);
  const [users, setUsers] = useState([]);
  const [statsLoading, setStatsLoading] = useState(true);
  const [usersLoading, setUsersLoading] = useState(true);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadStats = useCallback(async () => {
    try {
      setStatsLoading(true);
      const { data } = await fetchTelegramStats();
      setStats(data);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load Telegram stats', 'error');
    } finally {
      setStatsLoading(false);
    }
  }, [showSnackbar]);

  const loadUsers = useCallback(async () => {
    try {
      setUsersLoading(true);
      const { data } = await fetchLinkedUsers();
      const items = Array.isArray(data) ? data : data.items || [];
      setUsers(items.map((user, index) => ({ id: user.id || index, ...user })));
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load linked users', 'error');
    } finally {
      setUsersLoading(false);
    }
  }, [showSnackbar]);

  useEffect(() => {
    loadStats();
    loadUsers();
  }, [loadStats, loadUsers]);

  const columns = [
    { field: 'userName', headerName: 'UserName', flex: 1, minWidth: 160 },
    { field: 'email', headerName: 'Email', flex: 1.2, minWidth: 220 },
    { field: 'chatId', headerName: 'Chat ID', flex: 0.8, minWidth: 140 },
  ];

  return (
    <Box>
      <PageHeader title="Telegram" subtitle="Telegram bot management" />

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
                boxShadow: `0 4px 20px ${STAT_COLORS.linkedUsers}33`,
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
                    Linked Users
                  </Typography>
                  {statsLoading ? (
                    <Skeleton variant="text" width={60} height={42} />
                  ) : (
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#fff', lineHeight: 1.2 }}>
                      {stats?.linkedUsersCount != null ? stats.linkedUsersCount.toLocaleString() : '--'}
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
                    backgroundColor: `${STAT_COLORS.linkedUsers}20`,
                  }}
                >
                  <PeopleIcon sx={{ color: STAT_COLORS.linkedUsers, fontSize: 24 }} />
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
                backgroundColor: STAT_COLORS.linkedUsers,
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
                boxShadow: '0 4px 20px #00968833',
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
                    Bot Status
                  </Typography>
                  {statsLoading ? (
                    <Skeleton variant="text" width={60} height={42} />
                  ) : (
                    <Typography variant="h4" sx={{ fontWeight: 700, color: '#fff', lineHeight: 1.2 }}>
                      {stats?.botActive ? 'Active' : 'Inactive'}
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
                    backgroundColor: '#00968820',
                  }}
                >
                  <TelegramIcon sx={{ color: '#009688', fontSize: 24 }} />
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
                backgroundColor: '#009688',
                opacity: 0.6,
              }}
            />
          </Card>
        </Grid>
      </Grid>

      {/* Linked Users Table */}
      <Card>
        <CardContent sx={{ p: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            Linked Users
          </Typography>
          <Box sx={{ width: '100%' }}>
            <DataGrid
              rows={users}
              columns={columns}
              loading={usersLoading}
              pageSizeOptions={[10, 25]}
              initialState={{
                pagination: { paginationModel: { page: 0, pageSize: 10 } },
              }}
              disableRowSelectionOnClick
              autoHeight
            />
          </Box>
        </CardContent>
      </Card>

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
