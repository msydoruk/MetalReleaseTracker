import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Link from '@mui/material/Link';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import AlbumIcon from '@mui/icons-material/Album';
import { DataGrid } from '@mui/x-data-grid';
import PageHeader from '../components/PageHeader';
import { fetchBandReferenceById } from '../api/bandReferences';

export default function BandReferenceDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [band, setBand] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const { data } = await fetchBandReferenceById(id);
        setBand(data);
      } catch (err) {
        console.error('Failed to load band reference', err);
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}><CircularProgress /></Box>;
  if (!band) return <Typography>Band reference not found.</Typography>;

  const columns = [
    {
      field: 'albumTitle',
      headerName: 'Album Title',
      flex: 1,
      minWidth: 250,
      renderCell: ({ value }) => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AlbumIcon sx={{ fontSize: 18, opacity: 0.4 }} />
          <Box sx={{ fontWeight: 500 }}>{value}</Box>
        </Box>
      ),
    },
    {
      field: 'albumType',
      headerName: 'Type',
      width: 150,
      renderCell: ({ value }) => value ? (
        <Chip label={value} size="small" variant="outlined" sx={{ borderColor: 'rgba(255,255,255,0.15)' }} />
      ) : '-',
    },
    { field: 'year', headerName: 'Year', width: 100, type: 'number', valueFormatter: (value) => value ?? '' },
  ];

  return (
    <Box>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/')}
        sx={{ mb: 2, color: 'rgba(255, 255, 255, 0.6)', '&:hover': { color: 'rgba(255, 255, 255, 0.9)' } }}
      >
        Back to Band References
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
              <LibraryMusicIcon sx={{ color: '#e53935' }} />
            </Box>
            <Box>
              <Typography variant="h5">{band.bandName}</Typography>
              {band.genre && (
                <Chip
                  label={band.genre}
                  size="small"
                  sx={{ mt: 0.5, backgroundColor: 'rgba(255,255,255,0.06)', fontSize: '0.75rem' }}
                />
              )}
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', mt: 2, pl: 0.5 }}>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Metal Archives</Typography>
              <Typography>
                <Link href={`https://www.metal-archives.com/bands/_/${band.metalArchivesId}`} target="_blank" rel="noopener" sx={{ color: '#e53935' }}>
                  {band.metalArchivesId}
                </Link>
              </Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Last Synced</Typography>
              <Typography>{new Date(band.lastSyncedAt).toLocaleString()}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: '0.08em', fontSize: '0.65rem' }}>Albums</Typography>
              <Typography sx={{ fontWeight: 600, color: '#e53935' }}>{band.discographyCount}</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
      <PageHeader
        icon={<AlbumIcon />}
        title="Discography"
        subtitle={`${band.discographyCount} releases`}
      />
      <DataGrid
        rows={band.discography}
        columns={columns}
        pageSizeOptions={[10, 25, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
        sx={{ height: 'calc(100vh - 460px)' }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
