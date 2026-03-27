import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Paper,
  Chip,
  Button,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useParams, Link } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import StoreIcon from '@mui/icons-material/Store';
import LaunchIcon from '@mui/icons-material/Launch';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import Pagination from '../components/Pagination';
import DefaultDistributorImage from '../components/DefaultDistributorImage';
import ShareButton from '../components/ShareButton';
import { fetchDistributorBySlug, fetchGroupedAlbums } from '../services/api';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';

const DistributorDetailPage = () => {
  const { slug } = useParams();
  const { t, language } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [distributor, setDistributor] = useState(null);
  const [albums, setAlbums] = useState(null);
  const [loading, setLoading] = useState(true);
  const [albumsLoading, setAlbumsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const metaDescription = (() => {
    if (!distributor) return '';
    const parts = [`${distributor.name} - metal music distributor`];
    if (distributor.country) parts[0] += ` from ${distributor.country}`;
    if (distributor.albumCount > 0) {
      parts.push(`${distributor.albumCount} Ukrainian metal releases available`);
    }
    return parts.join('. ');
  })();

  usePageMeta(
    distributor ? distributor.name : null,
    metaDescription,
    distributor?.logoUrl
  );

  useEffect(() => {
    if (!distributor) return;
    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'Organization',
      name: distributor.name,
      ...(distributor.websiteUrl && { url: distributor.websiteUrl }),
      ...(distributor.logoUrl && { logo: distributor.logoUrl }),
      ...(distributor.country && { address: { '@type': 'PostalAddress', addressCountry: distributor.country } }),
      ...(distributor.description && { description: distributor.description }),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.textContent = JSON.stringify(jsonLd);
    script.id = 'distributor-jsonld';
    const existing = document.getElementById('distributor-jsonld');
    if (existing) existing.remove();
    document.head.appendChild(script);
    return () => { script.remove(); };
  }, [distributor]);

  useEffect(() => {
    const loadDistributor = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetchDistributorBySlug(slug, language);
        setDistributor(response.data);
      } catch {
        setError(t('distributorDetail.notFound'));
      } finally {
        setLoading(false);
      }
    };

    loadDistributor();
  }, [slug, language, t]);

  const loadAlbums = useCallback(async () => {
    if (!distributor) return;
    try {
      setAlbumsLoading(true);
      const response = await fetchGroupedAlbums({
        distributorId: distributor.id,
        page,
        pageSize,
        sortBy: ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
        sortAscending: false,
      });
      setAlbums(response.data);
    } catch {
      setAlbums(null);
    } finally {
      setAlbumsLoading(false);
    }
  }, [distributor, page, pageSize]);

  useEffect(() => {
    loadAlbums();
  }, [loadAlbums]);

  if (loading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !distributor) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{error || t('distributorDetail.notFound')}</Alert>
        <Button component={Link} to="/distributors" startIcon={<ArrowBackIcon />} sx={{ textTransform: 'none' }}>
          {t('distributorDetail.backToDistributors')}
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Button
        component={Link}
        to="/distributors"
        startIcon={<ArrowBackIcon />}
        sx={{ textTransform: 'none', mb: 3 }}
      >
        {t('distributorDetail.backToDistributors')}
      </Button>

      <Box sx={{
        display: 'flex',
        flexDirection: isMobile ? 'column' : 'row',
        gap: 3,
        mb: 4,
        alignItems: isMobile ? 'center' : 'flex-start',
      }}>
        <Box
          sx={{
            width: isMobile ? 200 : 250,
            height: isMobile ? 200 : 250,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            borderRadius: 2,
            backgroundColor: '#333',
            flexShrink: 0,
            overflow: 'hidden',
          }}
        >
          {distributor.logoUrl ? (
            <Box
              component="img"
              src={distributor.logoUrl}
              alt={distributor.name}
              sx={{
                maxWidth: '80%',
                maxHeight: '80%',
                objectFit: 'contain',
              }}
            />
          ) : (
            <DefaultDistributorImage />
          )}
        </Box>

        <Box sx={{ textAlign: isMobile ? 'center' : 'left' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: isMobile ? 'center' : 'flex-start', mb: 1, flexWrap: 'wrap' }}>
            <StoreIcon sx={{ color: 'primary.main' }} />
            <Typography variant="h4" component="h1" sx={{ fontWeight: 800 }}>
              {distributor.name}
            </Typography>
            {distributor.countryFlag && (
              <Typography sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
                {distributor.countryFlag}
              </Typography>
            )}
            <ShareButton
              url={window.location.href}
              title={distributor.name}
              text={t('share.distributorText').replace('{distributorName}', distributor.name)}
            />
          </Box>

          <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap', justifyContent: isMobile ? 'center' : 'flex-start' }}>
            {distributor.country && (
              <Chip icon={<LocationOnIcon />} label={distributor.country} size="small" variant="outlined" />
            )}
            {albums && (
              <Chip icon={<ShoppingCartIcon />} label={`${albums.totalCount} ${t('distributorDetail.releases')}`} size="small" color="secondary" />
            )}
          </Box>

          {distributor.description && (
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 600, mb: 2 }}>
              {distributor.description}
            </Typography>
          )}

          {distributor.websiteUrl && (
            <Button
              component="a"
              href={distributor.websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
              size="small"
              variant="outlined"
              endIcon={<LaunchIcon />}
              sx={{ textTransform: 'none', borderRadius: 5 }}
            >
              {t('distributorDetail.visitWebsite')}
            </Button>
          )}
        </Box>
      </Box>

      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
        {t('distributorDetail.albumsFrom').replace('{distributorName}', distributor.name)}
      </Typography>

      {albumsLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : albums && albums.items.length > 0 ? (
        <>
          <Pagination
            currentPage={albums.currentPage}
            totalPages={albums.pageCount}
            totalItems={albums.totalCount}
            pageSize={albums.pageSize}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
            compact
          />
          <Box sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: 'repeat(1, 1fr)',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
              lg: 'repeat(4, 1fr)',
              xl: 'repeat(5, 1fr)',
            },
            gap: 3,
            alignItems: 'start',
            my: 3,
          }}>
            {albums.items.map((group, index) => (
              <Box
                key={`${group.bandName}-${group.albumName}-${index}`}
                sx={{ display: 'flex', height: '100%' }}
              >
                <GroupedAlbumCard group={group} />
              </Box>
            ))}
          </Box>
          <Pagination
            currentPage={albums.currentPage}
            totalPages={albums.pageCount}
            totalItems={albums.totalCount}
            pageSize={albums.pageSize}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
          />
        </>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('distributorDetail.noAlbums')}
          </Typography>
        </Paper>
      )}
    </Container>
  );
};

export default DistributorDetailPage;
