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
import { useLanguage } from '../i18n/LanguageContext';

const DistributorsPage = () => {
  const { t, language } = useLanguage();

  usePageMeta(t('pageMeta.distributorsTitle'), t('pageMeta.distributorsDescription'));
  const [distributors, setDistributors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const response = await fetchDistributorsWithAlbumCount(language);
        console.log('Distributors data:', response.data);
        setDistributors(response.data || []);
      } catch (err) {
        console.error('Error fetching distributors:', err);
        setError(t('distributors.error'));
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [language, t]);

  const handleDistributorClick = (distributorSlug) => {
    navigate(`/distributors/${distributorSlug}`);
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 1 }}>
          {t('distributors.title')}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {t('distributors.subtitle')}
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
                boxShadow: '0 6px 16px rgba(0, 0, 0, 0.08)',
                borderRadius: 2,
                overflow: 'hidden',
                transition: 'all 0.25s ease-in-out',
                bgcolor: 'background.paper',
                border: '1px solid rgba(255, 255, 255, 0.1)',
                cursor: 'pointer',
                '&:hover': {
                  transform: 'translateY(-8px)',
                  boxShadow: '0 12px 20px rgba(0, 0, 0, 0.15)',
                }
              }}
              onClick={() => handleDistributorClick(distributor.slug)}
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
                {distributor.logoUrl ? (
                  <img
                    src={distributor.logoUrl}
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
                  {distributor.countryFlag && (
                    <Typography sx={{ ml: 1, fontSize: '1.2rem', lineHeight: 1 }}>
                      {distributor.countryFlag}
                    </Typography>
                  )}
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
                  {distributor.description || t('distributors.fallbackDescription')}
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
                    label={`${distributor.albumCount || 0} ${t('distributors.products')}`}
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
                    handleDistributorClick(distributor.slug);
                  }}
                  sx={{
                    borderRadius: 5,
                    px: 2,
                    fontWeight: 600,
                    textTransform: 'none'
                  }}
                >
                  {t('distributors.browseProducts')}
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
                    {t('distributors.website')}
                  </Button>
                )}
              </CardActions>
            </Card>
          ))}
        </Grid>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('distributors.noDistributors')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {t('distributors.checkBack')}
          </Typography>
        </Paper>
      )}
    </Container>
  );
};

export default DistributorsPage;