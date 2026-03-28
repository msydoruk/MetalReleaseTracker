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
  Fab,
  Drawer,
  FormControlLabel,
  Checkbox,
  useMediaQuery,
  useTheme
} from '@mui/material';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import FilterListIcon from '@mui/icons-material/FilterList';
import { useParams, Link } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import StoreIcon from '@mui/icons-material/Store';
import LaunchIcon from '@mui/icons-material/Launch';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import AlbumCard from '../components/AlbumCard';
import AlbumFilter from '../components/AlbumFilter';
import InfiniteScroll from '../components/InfiniteScroll';
import DefaultDistributorImage from '../components/DefaultDistributorImage';
import ShareButton from '../components/ShareButton';
import BreadcrumbNav from '../components/BreadcrumbNav';
import { fetchDistributorBySlug, fetchGroupedAlbums, fetchAlbums } from '../services/api';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';

const DistributorDetailPage = () => {
  const { slug } = useParams();
  const { t, language } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [distributor, setDistributor] = useState(null);
  const [allAlbums, setAllAlbums] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [scrollPage, setScrollPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [albumsLoading, setAlbumsLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState(null);
  const [showScrollTop, setShowScrollTop] = useState(false);
  const [isGrouped, setIsGrouped] = useState(() => localStorage.getItem('albumsGrouped') !== 'false');
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [albumFilters, setAlbumFilters] = useState({});
  const pageSize = 20;

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

  // Reset scroll when filters or grouping change
  useEffect(() => {
    setScrollPage(1);
    setAllAlbums([]);
  }, [isGrouped, albumFilters]);

  const loadAlbums = useCallback(async () => {
    if (!distributor) return;
    try {
      const isAppending = scrollPage > 1;
      if (isAppending) {
        setLoadingMore(true);
      } else {
        setAlbumsLoading(true);
      }

      const params = {
        ...albumFilters,
        distributorId: distributor.id,
        page: scrollPage,
        pageSize,
        sortBy: albumFilters.sortBy || ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
        sortAscending: albumFilters.sortAscending ?? false,
      };

      const fetchFn = isGrouped ? fetchGroupedAlbums : fetchAlbums;
      const response = await fetchFn(params);

      if (response.data) {
        setAllAlbums((previous) => isAppending
          ? [...previous, ...(response.data.items || [])]
          : (response.data.items || []));
        setTotalCount(response.data.totalCount || 0);
        setPageCount(response.data.pageCount || 0);
      }
    } catch {
      if (scrollPage === 1) setAllAlbums([]);
    } finally {
      setAlbumsLoading(false);
      setLoadingMore(false);
    }
  }, [distributor, scrollPage, pageSize, isGrouped, albumFilters]);

  useEffect(() => {
    loadAlbums();
  }, [loadAlbums]);

  const handleLoadMore = useCallback(() => {
    if (!loadingMore && scrollPage < pageCount) {
      setScrollPage((previous) => previous + 1);
    }
  }, [loadingMore, scrollPage, pageCount]);

  useEffect(() => {
    const handleScroll = () => setShowScrollTop(window.scrollY > 600);
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

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
      <BreadcrumbNav items={[
        { label: t('nav.distributors'), to: '/distributors' },
        { label: distributor.name },
      ]} />

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
            {totalCount > 0 && (
              <Chip icon={<ShoppingCartIcon />} label={`${totalCount} ${t('distributorDetail.releases')}`} size="small" color="secondary" />
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

      <Box sx={{
        display: 'flex',
        flexDirection: { xs: 'column', sm: 'row' },
        justifyContent: 'space-between',
        alignItems: { xs: 'flex-start', sm: 'center' },
        gap: { xs: 1, sm: 0 },
        mb: 2,
      }}>
        <Typography
          variant="h5"
          component="h2"
          sx={{
            fontWeight: 700,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: { xs: 'nowrap', sm: 'normal' },
          }}
        >
          {t('distributorDetail.albumsFrom').replace('{distributorName}', distributor.name)}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={isGrouped}
                onChange={(event) => {
                  const checked = event.target.checked;
                  setIsGrouped(checked);
                  localStorage.setItem('albumsGrouped', String(checked));
                }}
                size="small"
              />
            }
            label={t('albums.comparePrices')}
            sx={{ mr: 0, '& .MuiFormControlLabel-label': { fontSize: '0.875rem' } }}
          />
          <Button
            variant="contained"
            color="primary"
            startIcon={<FilterListIcon />}
            onClick={() => setIsFilterOpen(true)}
            sx={{ fontWeight: 'bold' }}
          >
            {t('albums.filters')}
          </Button>
        </Box>
      </Box>

      {totalCount > 0 && !albumsLoading && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {allAlbums.length} / {totalCount}
        </Typography>
      )}

      {albumsLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : allAlbums.length > 0 ? (
        <InfiniteScroll
          onLoadMore={handleLoadMore}
          hasMore={scrollPage < pageCount}
          loading={loadingMore}
        >
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
            {isGrouped ? (
              allAlbums.map((group, index) => (
                <Box
                  key={`${group.bandName}-${group.albumName}-${index}`}
                  sx={{ display: 'flex', height: '100%' }}
                >
                  <GroupedAlbumCard group={group} />
                </Box>
              ))
            ) : (
              allAlbums.map((album, index) => (
                <Box
                  key={`${album.id}-${index}`}
                  sx={{ display: 'flex', height: '100%' }}
                >
                  <AlbumCard album={album} />
                </Box>
              ))
            )}
          </Box>
        </InfiniteScroll>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('distributorDetail.noAlbums')}
          </Typography>
        </Paper>
      )}

      {showScrollTop && (
        <Fab
          size="small"
          color="primary"
          onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
          sx={{ position: 'fixed', bottom: 24, right: 24, zIndex: 1200 }}
        >
          <KeyboardArrowUpIcon />
        </Fab>
      )}

      <Drawer
        anchor="right"
        open={isFilterOpen}
        onClose={() => setIsFilterOpen(false)}
        sx={{
          '& .MuiDrawer-paper': {
            width: { xs: '100%', sm: 400 },
            boxSizing: 'border-box',
            backgroundColor: 'background.paper',
            borderTopLeftRadius: { xs: 0, sm: 8 },
            borderBottomLeftRadius: { xs: 0, sm: 8 },
            boxShadow: '-4px 0 20px rgba(0,0,0,0.2)',
            overflow: 'hidden'
          },
        }}
      >
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', p: 1 }}>
          <AlbumFilter
            onFilterChange={(newFilters) => {
              setAlbumFilters(newFilters);
              setIsFilterOpen(false);
            }}
            onClose={() => setIsFilterOpen(false)}
            initialFilters={{ ...albumFilters, distributorId: distributor?.id }}
          />
        </Box>
      </Drawer>
    </Container>
  );
};

export default DistributorDetailPage;
