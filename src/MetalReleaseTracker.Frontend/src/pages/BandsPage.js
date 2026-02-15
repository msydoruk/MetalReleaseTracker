import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Typography, 
  Box, 
  Grid, 
  Card, 
  CardContent, 
  CardMedia, 
  Button, 
  CircularProgress, 
  Alert,
  Chip,
  Avatar,
  Divider,
  Paper
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { fetchBandsWithAlbumCount } from '../services/api';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import AlbumIcon from '@mui/icons-material/Album';
import DefaultBandImage from '../components/DefaultBandImage';
import usePageMeta from '../hooks/usePageMeta';

const BandsPage = () => {
  usePageMeta('Bands - Ukrainian Metal Bands', 'Explore Ukrainian metal bands whose physical releases are sold by foreign distributors and labels worldwide.');
  const [bands, setBands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const response = await fetchBandsWithAlbumCount();
        console.log('Bands data:', response.data);
        setBands(response.data || []);
      } catch (err) {
        console.error('Error fetching bands:', err);
        setError('Failed to load bands. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleBandClick = (bandId) => {
    navigate(`/albums?bandId=${bandId}`);
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 1 }}>
          Metal Bands
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Browse our collection of metal bands and discover their releases
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ my: 2 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
          <CircularProgress />
        </Box>
      ) : bands.length > 0 ? (
        <Grid 
          container 
          spacing={3}
          sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: 'repeat(1, 1fr)',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
              lg: 'repeat(4, 1fr)',
              xl: 'repeat(5, 1fr)'
            },
            gap: 3
          }}
        >
          {bands.map((band) => (
            <Card 
              key={band.id}
              sx={{ 
                height: '100%', 
                display: 'flex',
                flexDirection: 'column',
                transition: 'transform 0.3s, box-shadow 0.3s',
                borderRadius: 2,
                overflow: 'hidden',
                cursor: 'pointer',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 12px 20px rgba(0, 0, 0, 0.15)',
                }
              }}
              onClick={() => handleBandClick(band.id)}
            >
              <CardMedia
                component="div"
                sx={{
                  height: 200,
                  position: 'relative',
                  backgroundColor: '#333333',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}
              >
                {band.imageUrl ? (
                  <img
                    src={band.imageUrl}
                    alt={band.name}
                    style={{
                      width: '100%',
                      height: '100%',
                      objectFit: 'cover'
                    }}
                  />
                ) : (
                  <DefaultBandImage />
                )}
              </CardMedia>
              <CardContent sx={{ flexGrow: 1, p: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <MusicNoteIcon sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h6" component="h2" gutterBottom sx={{ 
                    fontWeight: 'bold',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}>
                    {band.name}
                  </Typography>
                </Box>
                
                {band.genre && (
                  <Chip 
                    label={band.genre} 
                    size="small" 
                    color="secondary" 
                    sx={{ mb: 2 }}
                  />
                )}
                
                <Typography variant="body2" color="text.secondary" sx={{ 
                  mb: 2,
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  display: '-webkit-box',
                  WebkitLineClamp: 3,
                  WebkitBoxOrient: 'vertical',
                  height: '4.5em'
                }}>
                  {band.description || 'No description available.'}
                </Typography>
                
                <Box sx={{ 
                  display: 'flex', 
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  mt: 'auto',
                  pt: 1,
                  borderTop: '1px solid rgba(255, 255, 255, 0.1)'
                }}>
                  <Chip 
                    icon={<AlbumIcon />}
                    label={`${band.albumCount || 0} Albums`}
                    variant="outlined"
                    size="small"
                  />
                  <Button 
                    size="small" 
                    variant="contained" 
                    color="primary"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleBandClick(band.id);
                    }}
                    sx={{ 
                      borderRadius: 5,
                      px: 2,
                      fontWeight: 'bold',
                      textTransform: 'none'
                    }}
                  >
                    View Albums
                  </Button>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Grid>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            No bands found.
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Please check back later for updates.
          </Typography>
        </Paper>
      )}
    </Container>
  );
};

export default BandsPage; 