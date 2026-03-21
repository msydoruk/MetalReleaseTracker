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
import { getDistributorCountry } from '../utils/distributorCountries';

const distributorLogos = {
  'osmose productions': '/logos/osmose.png',
  'drakkar': '/logos/drakkar.png',
  'black metal vendor': '/logos/black-metal-vendor.png',
  'black-metal-vendor.com': '/logos/black-metal-vendor.png',
  'paragon': '/logos/paragon-records.jpg',
  'season of mist': '/logos/season-of-mist.png',
  'napalm': '/logos/napalm-records.png',
  'blackmetalstore': '/logos/black-metal-store.webp',
  'black metal store': '/logos/black-metal-store.webp',
  'werewolf': '/logos/werewolf.gif',
  'avantgarde': '/logos/avantgarde-music.jpg',
};

const distributorInfo = {
  en: {
    'osmose productions': {
      country: '\uD83C\uDDEB\uD83C\uDDF7',
      description: 'French underground metal label founded in 1991 by Herv\u00E9 Herbaut. Specializes in black and death metal. Based in Lorraine, France.',
    },
    'drakkar records': {
      country: '\uD83C\uDDE9\uD83C\uDDEA',
      description: 'German heavy metal label founded in 1999 in Witten. Part of Drakkar Entertainment GmbH. Distributes metal releases across Europe.',
    },
    'black metal vendor': {
      country: '\uD83C\uDDE9\uD83C\uDDEA',
      description: 'German black metal mailorder shop based in Berlin. Operated by fascination media UG. Offers CDs, vinyl, tapes and merchandise.',
    },
    'black metal store': {
      country: '\uD83C\uDDE7\uD83C\uDDF7',
      description: 'Brazilian black metal mailorder founded in 1997. Based in S\u00E3o Paulo. Over 5000 titles specializing in black and pagan metal worldwide shipping.',
    },
    'napalm records': {
      country: '\uD83C\uDDE6\uD83C\uDDF9',
      description: 'Major Austrian metal label founded in 1992 by Markus Riedler in Eisenerz. One of the largest independent metal labels in the world.',
    },
    'season of mist': {
      country: '\uD83C\uDDEB\uD83C\uDDF7',
      description: 'French metal label founded in 1996 by Michael S. Berberian in Marseille. Covers black, death, avant-garde and progressive metal.',
    },
    'paragon records': {
      country: '\uD83C\uDDFA\uD83C\uDDF8',
      description: 'American metal label and distributor founded in 2000. Based in New York. Specializes in black, death and doom metal.',
    },
    'werewolf': {
      country: '\uD83C\uDDF5\uD83C\uDDF1',
      description: 'Polish underground label and distro founded in 2007. Werewolf Promotion — Slavonic Forge and Armoury of Underground Heathen Art. Black, pagan and folk metal.',
    },
    'avantgarde music': {
      country: '\uD83C\uDDEE\uD83C\uDDF9',
      description: 'Italian extreme metal label founded in 1993 by Roberto Mammarella. Based in Milan. Webshop operated via Sound Cave. Black, death and avant-garde metal.',
    },
  },
  ua: {
    'osmose productions': {
      country: '\uD83C\uDDEB\uD83C\uDDF7',
      description: '\u0424\u0440\u0430\u043D\u0446\u0443\u0437\u044C\u043A\u0438\u0439 \u0430\u043D\u0434\u0435\u0440\u0433\u0440\u0430\u0443\u043D\u0434-\u043B\u0435\u0439\u0431\u043B, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1991 \u0440\u043E\u0446\u0456 \u0415\u0440\u0432\u0435 \u0415\u0440\u0431\u043E. \u0421\u043F\u0435\u0446\u0456\u0430\u043B\u0456\u0437\u0443\u0454\u0442\u044C\u0441\u044F \u043D\u0430 \u0431\u043B\u0435\u043A- \u0442\u0430 \u0434\u0435\u0442-\u043C\u0435\u0442\u0430\u043B\u0456. \u0411\u0430\u0437\u0443\u0454\u0442\u044C\u0441\u044F \u0432 \u041B\u043E\u0442\u0430\u0440\u0438\u043D\u0433\u0456\u0457, \u0424\u0440\u0430\u043D\u0446\u0456\u044F.',
    },
    'drakkar records': {
      country: '\uD83C\uDDE9\uD83C\uDDEA',
      description: '\u041D\u0456\u043C\u0435\u0446\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1999 \u0440\u043E\u0446\u0456 \u0432 \u0412\u0456\u0442\u0442\u0435\u043D\u0456. \u0427\u0430\u0441\u0442\u0438\u043D\u0430 Drakkar Entertainment GmbH. \u0414\u0438\u0441\u0442\u0440\u0438\u0431\u0443\u0446\u0456\u044F \u043C\u0435\u0442\u0430\u043B-\u0440\u0435\u043B\u0456\u0437\u0456\u0432 \u043F\u043E \u0404\u0432\u0440\u043E\u043F\u0456.',
    },
    'black metal vendor': {
      country: '\uD83C\uDDE9\uD83C\uDDEA',
      description: '\u041D\u0456\u043C\u0435\u0446\u044C\u043A\u0438\u0439 \u043C\u0435\u0439\u043B\u043E\u0440\u0434\u0435\u0440-\u043C\u0430\u0433\u0430\u0437\u0438\u043D \u0431\u043B\u0435\u043A-\u043C\u0435\u0442\u0430\u043B\u0443 \u0437 \u0411\u0435\u0440\u043B\u0456\u043D\u0430. fascination media UG. \u041F\u0440\u043E\u043F\u043E\u043D\u0443\u0454 CD, \u0432\u0456\u043D\u0456\u043B, \u043A\u0430\u0441\u0435\u0442\u0438 \u0442\u0430 \u043C\u0435\u0440\u0447.',
    },
    'black metal store': {
      country: '\uD83C\uDDE7\uD83C\uDDF7',
      description: '\u0411\u0440\u0430\u0437\u0438\u043B\u044C\u0441\u044C\u043A\u0438\u0439 \u043C\u0435\u0439\u043B\u043E\u0440\u0434\u0435\u0440 \u0431\u043B\u0435\u043A-\u043C\u0435\u0442\u0430\u043B\u0443, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1997 \u0440\u043E\u0446\u0456. \u0421\u0430\u043D-\u041F\u0430\u0443\u043B\u0443. \u041F\u043E\u043D\u0430\u0434 5000 \u043F\u043E\u0437\u0438\u0446\u0456\u0439, \u0441\u043F\u0435\u0446\u0456\u0430\u043B\u0456\u0437\u0430\u0446\u0456\u044F \u043D\u0430 \u0431\u043B\u0435\u043A \u0442\u0430 \u043F\u0430\u0433\u0430\u043D-\u043C\u0435\u0442\u0430\u043B\u0456.',
    },
    'napalm records': {
      country: '\uD83C\uDDE6\uD83C\uDDF9',
      description: '\u0412\u0435\u043B\u0438\u043A\u0438\u0439 \u0430\u0432\u0441\u0442\u0440\u0456\u0439\u0441\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1992 \u0440\u043E\u0446\u0456 \u041C\u0430\u0440\u043A\u0443\u0441\u043E\u043C \u0420\u0456\u0434\u043B\u0435\u0440\u043E\u043C \u0432 \u0410\u0439\u0437\u0435\u043D\u0435\u0440\u0446\u0456. \u041E\u0434\u0438\u043D \u0437 \u043D\u0430\u0439\u0431\u0456\u043B\u044C\u0448\u0438\u0445 \u043D\u0435\u0437\u0430\u043B\u0435\u0436\u043D\u0438\u0445 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B\u0456\u0432 \u0443 \u0441\u0432\u0456\u0442\u0456.',
    },
    'season of mist': {
      country: '\uD83C\uDDEB\uD83C\uDDF7',
      description: '\u0424\u0440\u0430\u043D\u0446\u0443\u0437\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1996 \u0440\u043E\u0446\u0456 \u041C\u0430\u0439\u043A\u043B\u043E\u043C \u0411\u0435\u0440\u0431\u0435\u0440\u044F\u043D\u043E\u043C \u0443 \u041C\u0430\u0440\u0441\u0435\u043B\u0456. \u0411\u043B\u0435\u043A, \u0434\u0435\u0442, \u0430\u0432\u0430\u043D\u0433\u0430\u0440\u0434 \u0442\u0430 \u043F\u0440\u043E\u0433\u0440\u0435\u0441\u0438\u0432\u043D\u0438\u0439 \u043C\u0435\u0442\u0430\u043B.',
    },
    'paragon records': {
      country: '\uD83C\uDDFA\uD83C\uDDF8',
      description: '\u0410\u043C\u0435\u0440\u0438\u043A\u0430\u043D\u0441\u044C\u043A\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B \u0442\u0430 \u0434\u0438\u0441\u0442\u0440\u0438\u0431\u2019\u044E\u0442\u043E\u0440, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 2000 \u0440\u043E\u0446\u0456. \u041D\u044C\u044E-\u0419\u043E\u0440\u043A. \u0421\u043F\u0435\u0446\u0456\u0430\u043B\u0456\u0437\u0443\u0454\u0442\u044C\u0441\u044F \u043D\u0430 \u0431\u043B\u0435\u043A, \u0434\u0435\u0442 \u0442\u0430 \u0434\u0443\u043C-\u043C\u0435\u0442\u0430\u043B\u0456.',
    },
    'werewolf': {
      country: '\uD83C\uDDF5\uD83C\uDDF1',
      description: '\u041F\u043E\u043B\u044C\u0441\u044C\u043A\u0438\u0439 \u0430\u043D\u0434\u0435\u0440\u0433\u0440\u0430\u0443\u043D\u0434-\u043B\u0435\u0439\u0431\u043B \u0442\u0430 \u0434\u0438\u0441\u0442\u0440\u043E, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 2007 \u0440\u043E\u0446\u0456. Werewolf Promotion \u2014 Slavonic Forge. \u0411\u043B\u0435\u043A, \u043F\u0430\u0433\u0430\u043D \u0442\u0430 \u0444\u043E\u043B\u043A-\u043C\u0435\u0442\u0430\u043B.',
    },
    'avantgarde music': {
      country: '\uD83C\uDDEE\uD83C\uDDF9',
      description: '\u0406\u0442\u0430\u043B\u0456\u0439\u0441\u044C\u043A\u0438\u0439 \u0435\u043A\u0441\u0442\u0440\u0435\u043C\u0430\u043B\u044C\u043D\u0438\u0439 \u043C\u0435\u0442\u0430\u043B-\u043B\u0435\u0439\u0431\u043B, \u0437\u0430\u0441\u043D\u043E\u0432\u0430\u043D\u0438\u0439 \u0443 1993 \u0440\u043E\u0446\u0456. \u041C\u0456\u043B\u0430\u043D. \u0412\u0435\u0431-\u043C\u0430\u0433\u0430\u0437\u0438\u043D Sound Cave. \u0411\u043B\u0435\u043A, \u0434\u0435\u0442 \u0442\u0430 \u0430\u0432\u0430\u043D\u0433\u0430\u0440\u0434\u043D\u0438\u0439 \u043C\u0435\u0442\u0430\u043B.',
    },
  },
};

const getDistributorInfo = (distributorName, language) => {
  const name = (distributorName || '').toLowerCase();
  const langInfo = distributorInfo[language] || distributorInfo.en;
  for (const [key, info] of Object.entries(langInfo)) {
    if (name.includes(key) || key.includes(name)) return info;
  }
  return null;
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
        const response = await fetchDistributorsWithAlbumCount();
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
  }, [t]);

  const handleDistributorClick = (distributorId) => {
    navigate(`/albums?distributorId=${distributorId}`);
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
                  {getDistributorCountry(distributor.name) && (
                    <Typography sx={{ ml: 1, fontSize: '1.2rem', lineHeight: 1 }}>
                      {getDistributorCountry(distributor.name)}
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
                  {getDistributorInfo(distributor.name, language)?.description || distributor.description || t('distributors.fallbackDescription')}
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
                    handleDistributorClick(distributor.id);
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