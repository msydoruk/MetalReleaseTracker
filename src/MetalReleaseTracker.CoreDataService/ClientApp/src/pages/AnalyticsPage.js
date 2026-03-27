import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Skeleton from '@mui/material/Skeleton';
import Alert from '@mui/material/Alert';
import ButtonGroup from '@mui/material/ButtonGroup';
import Button from '@mui/material/Button';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import {
  AreaChart, Area, BarChart, Bar, LineChart, Line,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import PageHeader from '../components/PageHeader';
import {
  fetchAlbumsPerWeek,
  fetchUserGrowth,
  fetchPopularGenres,
  fetchTopDistributors,
  fetchTopWatchedAlbums,
} from '../api/analytics';

const RANGE_OPTIONS = [
  { label: '30d', days: 30 },
  { label: '90d', days: 90 },
  { label: '1y', days: 365 },
  { label: 'All', days: null },
];

const formatDate = (dateStr) => {
  const date = new Date(dateStr);
  return `${date.getMonth() + 1}/${date.getDate()}`;
};

const ChartCard = ({ title, loading, error, children, minHeight = 300 }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Typography variant="subtitle2" sx={{ mb: 2, color: 'rgba(255,255,255,0.7)', textTransform: 'uppercase', fontSize: '0.75rem', letterSpacing: '0.05em' }}>
        {title}
      </Typography>
      {error && <Alert severity="error" sx={{ mb: 1 }}>{error}</Alert>}
      {loading ? (
        <Skeleton variant="rectangular" height={minHeight - 40} sx={{ borderRadius: 1 }} />
      ) : (
        <Box sx={{ width: '100%', minHeight: minHeight - 40 }}>
          {children}
        </Box>
      )}
    </CardContent>
  </Card>
);

export default function AnalyticsPage() {
  const [range, setRange] = useState(90);
  const [albumsData, setAlbumsData] = useState([]);
  const [userGrowthData, setUserGrowthData] = useState([]);
  const [genresData, setGenresData] = useState([]);
  const [distributorsData, setDistributorsData] = useState([]);
  const [watchedData, setWatchedData] = useState([]);
  const [loading, setLoading] = useState({ albums: true, users: true, genres: true, distributors: true, watched: true });
  const [errors, setErrors] = useState({});

  const getDateParams = useCallback(() => {
    if (!range) return {};
    const to = new Date().toISOString();
    const from = new Date(Date.now() - range * 24 * 60 * 60 * 1000).toISOString();
    return { from, to };
  }, [range]);

  const loadAlbums = useCallback(async () => {
    try {
      setLoading((prev) => ({ ...prev, albums: true }));
      setErrors((prev) => ({ ...prev, albums: null }));
      const { data } = await fetchAlbumsPerWeek(getDateParams());
      setAlbumsData(data);
    } catch (err) {
      setErrors((prev) => ({ ...prev, albums: 'Failed to load' }));
    } finally {
      setLoading((prev) => ({ ...prev, albums: false }));
    }
  }, [getDateParams]);

  const loadUserGrowth = useCallback(async () => {
    try {
      setLoading((prev) => ({ ...prev, users: true }));
      setErrors((prev) => ({ ...prev, users: null }));
      const { data } = await fetchUserGrowth(getDateParams());
      setUserGrowthData(data);
    } catch (err) {
      setErrors((prev) => ({ ...prev, users: 'Failed to load' }));
    } finally {
      setLoading((prev) => ({ ...prev, users: false }));
    }
  }, [getDateParams]);

  const loadGenres = useCallback(async () => {
    try {
      setLoading((prev) => ({ ...prev, genres: true }));
      setErrors((prev) => ({ ...prev, genres: null }));
      const { data } = await fetchPopularGenres(getDateParams());
      setGenresData(data);
    } catch (err) {
      setErrors((prev) => ({ ...prev, genres: 'Failed to load' }));
    } finally {
      setLoading((prev) => ({ ...prev, genres: false }));
    }
  }, [getDateParams]);

  const loadDistributors = useCallback(async () => {
    try {
      setLoading((prev) => ({ ...prev, distributors: true }));
      setErrors((prev) => ({ ...prev, distributors: null }));
      const { data } = await fetchTopDistributors(getDateParams());
      setDistributorsData(data);
    } catch (err) {
      setErrors((prev) => ({ ...prev, distributors: 'Failed to load' }));
    } finally {
      setLoading((prev) => ({ ...prev, distributors: false }));
    }
  }, [getDateParams]);

  const loadWatched = useCallback(async () => {
    try {
      setLoading((prev) => ({ ...prev, watched: true }));
      setErrors((prev) => ({ ...prev, watched: null }));
      const { data } = await fetchTopWatchedAlbums();
      setWatchedData(data);
    } catch (err) {
      setErrors((prev) => ({ ...prev, watched: 'Failed to load' }));
    } finally {
      setLoading((prev) => ({ ...prev, watched: false }));
    }
  }, []);

  useEffect(() => {
    loadAlbums();
    loadUserGrowth();
    loadGenres();
    loadDistributors();
    loadWatched();
  }, [loadAlbums, loadUserGrowth, loadGenres, loadDistributors, loadWatched]);

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2, mb: 3 }}>
        <PageHeader title="Analytics" subtitle="Trend data and insights" sx={{ mb: 0 }} />
        <ButtonGroup size="small" variant="outlined">
          {RANGE_OPTIONS.map((opt) => (
            <Button
              key={opt.label}
              onClick={() => setRange(opt.days)}
              variant={range === opt.days ? 'contained' : 'outlined'}
            >
              {opt.label}
            </Button>
          ))}
        </ButtonGroup>
      </Box>

      <Grid container spacing={2.5}>
        {/* Albums Per Week */}
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="Albums Added Per Week" loading={loading.albums} error={errors.albums}>
            <ResponsiveContainer width="100%" height={260}>
              <AreaChart data={albumsData}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis dataKey="date" tickFormatter={formatDate} stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <YAxis stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <Tooltip
                  contentStyle={{ backgroundColor: '#1e1e1e', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 8 }}
                  labelFormatter={(label) => new Date(label).toLocaleDateString()}
                />
                <Area type="monotone" dataKey="count" stroke="#e53935" fill="#e5393533" strokeWidth={2} name="Albums" />
              </AreaChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>

        {/* User Growth */}
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="User Registrations Per Week" loading={loading.users} error={errors.users}>
            <ResponsiveContainer width="100%" height={260}>
              <LineChart data={userGrowthData}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis dataKey="date" tickFormatter={formatDate} stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <YAxis stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <Tooltip
                  contentStyle={{ backgroundColor: '#1e1e1e', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 8 }}
                  labelFormatter={(label) => new Date(label).toLocaleDateString()}
                />
                <Line type="monotone" dataKey="count" stroke="#4caf50" strokeWidth={2} dot={{ r: 3 }} name="Users" />
              </LineChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>

        {/* Popular Genres */}
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="Popular Genres" loading={loading.genres} error={errors.genres} minHeight={360}>
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={genresData} layout="vertical" margin={{ left: 20 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis type="number" stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <YAxis type="category" dataKey="genre" stroke="rgba(255,255,255,0.5)" fontSize={11} width={120} />
                <Tooltip contentStyle={{ backgroundColor: '#1e1e1e', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 8 }} />
                <Bar dataKey="count" fill="#ff9800" radius={[0, 4, 4, 0]} name="Albums" />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>

        {/* Top Distributors */}
        <Grid size={{ xs: 12, md: 6 }}>
          <ChartCard title="Top Distributors by Album Count" loading={loading.distributors} error={errors.distributors} minHeight={360}>
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={distributorsData}>
                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                <XAxis dataKey="distributorName" stroke="rgba(255,255,255,0.5)" fontSize={11} angle={-30} textAnchor="end" height={60} />
                <YAxis stroke="rgba(255,255,255,0.5)" fontSize={12} />
                <Tooltip contentStyle={{ backgroundColor: '#1e1e1e', border: '1px solid rgba(255,255,255,0.1)', borderRadius: 8 }} />
                <Bar dataKey="albumCount" fill="#2196f3" radius={[4, 4, 0, 0]} name="Albums" />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>

        {/* Top Watched Albums */}
        <Grid size={{ xs: 12 }}>
          <ChartCard title="Top Watched Albums" loading={loading.watched} error={errors.watched} minHeight={200}>
            {watchedData.length === 0 ? (
              <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.5)', py: 4, textAlign: 'center' }}>
                No watched albums yet
              </Typography>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>#</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Band</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Album</TableCell>
                      <TableCell sx={{ fontWeight: 600 }} align="right">Watchers</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {watchedData.map((row, index) => (
                      <TableRow key={`${row.bandName}-${row.canonicalTitle}`}>
                        <TableCell>{index + 1}</TableCell>
                        <TableCell>{row.bandName}</TableCell>
                        <TableCell>{row.canonicalTitle}</TableCell>
                        <TableCell align="right">{row.watchCount}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </ChartCard>
        </Grid>
      </Grid>
    </Box>
  );
}
