import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Skeleton from '@mui/material/Skeleton';
import Alert from '@mui/material/Alert';
import AlbumIcon from '@mui/icons-material/Album';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import StoreIcon from '@mui/icons-material/Store';
import PeopleIcon from '@mui/icons-material/People';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import LanguageIcon from '@mui/icons-material/Language';
import NewspaperIcon from '@mui/icons-material/Newspaper';
import PageHeader from '../components/PageHeader';
import { fetchDashboardStats } from '../api/dashboard';

const STAT_CARDS = [
  { key: 'totalAlbums', label: 'Total Albums', icon: AlbumIcon, color: '#b71c1c' },
  { key: 'totalBands', label: 'Total Bands', icon: MusicNoteIcon, color: '#4caf50' },
  { key: 'totalDistributors', label: 'Distributors', icon: StoreIcon, color: '#2196f3' },
  { key: 'totalUsers', label: 'Users', icon: PeopleIcon, color: '#ff9800' },
  { key: 'newAlbumsThisMonth', label: 'New This Month', icon: NewReleasesIcon, color: '#e91e63' },
  { key: 'preOrders', label: 'Pre-Orders', icon: TrendingUpIcon, color: '#9c27b0' },
  { key: 'totalTranslations', label: 'Translations', icon: LanguageIcon, color: '#00bcd4' },
  { key: 'publishedNews', label: 'Published News', icon: NewspaperIcon, color: '#ff5722' },
];

export default function DashboardPage() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadStats = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const { data } = await fetchDashboardStats();
      setStats(data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load dashboard stats');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStats();
  }, [loadStats]);

  return (
    <Box>
      <PageHeader title="Dashboard" subtitle="Overview of the system" />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Grid container spacing={2.5}>
        {STAT_CARDS.map((card) => {
          const Icon = card.icon;
          const value = stats?.[card.key];

          return (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={card.key}>
              <Card
                sx={{
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: `0 4px 20px ${card.color}33`,
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
                        {card.label}
                      </Typography>
                      {loading ? (
                        <Skeleton variant="text" width={60} height={42} />
                      ) : (
                        <Typography
                          variant="h4"
                          sx={{
                            fontWeight: 700,
                            color: '#fff',
                            lineHeight: 1.2,
                          }}
                        >
                          {value != null ? value.toLocaleString() : '--'}
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
                        backgroundColor: `${card.color}20`,
                      }}
                    >
                      <Icon sx={{ color: card.color, fontSize: 24 }} />
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
                    backgroundColor: card.color,
                    opacity: 0.6,
                  }}
                />
              </Card>
            </Grid>
          );
        })}
      </Grid>
    </Box>
  );
}
