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
  Link,
  Chip,
  Avatar,
  CardActions,
  Paper
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import StoreIcon from '@mui/icons-material/Store';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import LaunchIcon from '@mui/icons-material/Launch';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import { fetchDistributorsWithAlbumCount } from '../services/api';
import DefaultDistributorImage from '../components/DefaultDistributorImage';
import usePageMeta from '../hooks/usePageMeta';

const distributorLogos = {
  'osmose productions': '/logos/osmose.png',
  'drakkar': '/logos/drakkar.png',
  'black metal vendor': '/logos/black-metal-vendor.png',
  'black-metal-vendor.com': '/logos/black-metal-vendor.png',
};

const getDistributorLogo = (distributor) => {
  if (distributor.logoUrl) return distributor.logoUrl;
  const name = (distributor.name || '').toLowerCase();
  for (const [key, logo] of Object.entries(distributorLogos)) {
    if (name.includes(key)) return logo;
  }
  return null;
};

const DistributorsPage = () => {
  usePageMeta('Distributors - Foreign Metal Labels & Shops', 'Foreign distributors and labels selling Ukrainian metal releases. Osmose Productions, Drakkar, Black Metal Vendor and more.');
  const [distributors, setDistributors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const response = await fetchDistributorsWithAlbumCount();
        console.log('Distributors data:', response.data);
        setDistributors(response.data || []);
      } catch (err) {
        console.error('Error fetching distributors:', err);
        setError('Failed to load distributors. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleDistributorClick = (distributorId) => {
    navigate(`/albums?distributorId=${distributorId}`);
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 1 }}>
          Metal Distributors
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Browse our collection of metal music distributors and shops
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
      ) : distributors.length > 0 ? (
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
          {distributors.map((distributor) => (
            <Card 
              key={distributor.id}
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
              onClick={() => handleDistributorClick(distributor.id)}
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
                {getDistributorLogo(distributor) ? (
                  <img
                    src={getDistributorLogo(distributor)}
                    alt={distributor.name}
                    style={{
                      maxWidth: '80%',
                      maxHeight: '80%',
                      objectFit: 'contain'
                    }}
                  />
                ) : (
                  <DefaultDistributorImage />
                )}
              </CardMedia>
              <CardContent sx={{ flexGrow: 1, p: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                  <StoreIcon sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h6" component="h2" gutterBottom sx={{ 
                    fontWeight: 'bold',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}>
                    {distributor.name}
                  </Typography>
                </Box>
                
                {distributor.location && (
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1, color: 'text.secondary' }}>
                    <LocationOnIcon fontSize="small" sx={{ mr: 0.5 }} />
                    <Typography variant="body2">
                      {distributor.location}
                    </Typography>
                  </Box>
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
                  {distributor.description || 'Metal music distributor and shop.'}
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
                    icon={<ShoppingCartIcon />}
                    label={`${distributor.albumCount || 0} Products`}
                    variant="outlined"
                    size="small"
                  />
                </Box>
              </CardContent>
              <CardActions sx={{ p: 2, pt: 0 }}>
                <Button 
                  size="small" 
                  variant="contained" 
                  color="primary"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleDistributorClick(distributor.id);
                  }}
                  sx={{ 
                    borderRadius: 5,
                    px: 2,
                    fontWeight: 'bold',
                    textTransform: 'none'
                  }}
                >
                  Browse Products
                </Button>
                {distributor.websiteUrl && (
                  <Button
                    size="small"
                    variant="outlined"
                    color="secondary"
                    component="a"
                    href={distributor.websiteUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    endIcon={<LaunchIcon />}
                    sx={{ 
                      ml: 'auto',
                      borderRadius: 5,
                      textTransform: 'none'
                    }}
                  >
                    Website
                  </Button>
                )}
              </CardActions>
            </Card>
          ))}
        </Grid>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            No distributors found.
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Please check back later for updates.
          </Typography>
        </Paper>
      )}
    </Container>
  );
};

export default DistributorsPage; 