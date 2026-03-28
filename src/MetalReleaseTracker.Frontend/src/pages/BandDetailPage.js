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
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import AlbumCard from '../components/AlbumCard';
import AlbumFilter from '../components/AlbumFilter';
import InfiniteScroll from '../components/InfiniteScroll';
import { fetchBandBySlug, fetchGroupedAlbums, fetchAlbums, fetchSimilarBands, followBand, unfollowBand, checkFollowingBand, fetchBandFollowerCount } from '../services/api';
import authService from '../services/auth';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';
import ShareButton from '../components/ShareButton';
import BreadcrumbNav from '../components/BreadcrumbNav';

const BandDetailPage = () => {
  const { slug } = useParams();
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [band, setBand] = useState(null);
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
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [similarBands, setSimilarBands] = useState([]);
  const [isFollowing, setIsFollowing] = useState(false);
  const [followerCount, setFollowerCount] = useState(0);
  const [followLoading, setFollowLoading] = useState(false);

  const bandMetaDescription = (() => {
    if (!band) return '';
    const parts = [band.name];
    if (band.genre) parts[0] += ` - ${band.genre}`;
    parts[0] += ' from Ukraine';
    if (totalCount > 0) {
      parts.push(`${totalCount} release${totalCount > 1 ? 's' : ''} available from foreign distributors`);
    }
    return parts.join('. ');
  })();

  usePageMeta(
    band ? band.name : null,
    bandMetaDescription,
    band?.photoUrl
  );

  useEffect(() => {
    if (!band) return;
    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'MusicGroup',
      name: band.name,
      ...(band.genre && { genre: band.genre }),
      ...(band.photoUrl && { image: band.photoUrl }),
      ...(band.description && { description: band.description }),
      ...(band.metalArchivesUrl && { sameAs: [band.metalArchivesUrl] }),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.textContent = JSON.stringify(jsonLd);
    script.id = 'band-jsonld';
    const existing = document.getElementById('band-jsonld');
    if (existing) existing.remove();
    document.head.appendChild(script);
    return () => { script.remove(); };
  }, [band]);

  useEffect(() => {
    const loadBand = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetchBandBySlug(slug);
        setBand(response.data);
      } catch {
        setError(t('bandDetail.notFound'));
      } finally {
        setLoading(false);
      }
    };

    loadBand();
  }, [slug, t]);

  useEffect(() => {
    if (!band) return;
    const loadSimilarBands = async () => {
      try {
        const response = await fetchSimilarBands(band.id);
        setSimilarBands(response.data);
      } catch {
        setSimilarBands([]);
      }
    };

    loadSimilarBands();
  }, [band]);

  useEffect(() => {
    if (!band) return;

    const loadFollowData = async () => {
      try {
        const countResponse = await fetchBandFollowerCount(band.id);
        setFollowerCount(countResponse.data);
      } catch {
        // ignore
      }

      const loggedIn = await authService.isLoggedIn();
      if (loggedIn) {
        try {
          const followResponse = await checkFollowingBand(band.id);
          setIsFollowing(followResponse.data);
        } catch {
          // ignore
        }
      }
    };

    loadFollowData();
  }, [band]);

  // Reset scroll when filters or grouping change
  useEffect(() => {
    setScrollPage(1);
    setAllAlbums([]);
  }, [isGrouped, albumFilters]);

  const loadAlbums = useCallback(async () => {
    if (!band) return;
    try {
      const isAppending = scrollPage > 1;
      if (isAppending) {
        setLoadingMore(true);
      } else {
        setAlbumsLoading(true);
      }

      const params = {
        ...albumFilters,
        bandId: band.id,
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
  }, [band, scrollPage, pageSize, isGrouped, albumFilters]);

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

  useEffect(() => {
    const loadAuth = async () => {
      const loggedIn = await authService.isLoggedIn();
      setIsLoggedIn(loggedIn);
    };

    loadAuth();
  }, []);

  const handleFollowToggle = async () => {
    setFollowLoading(true);
    try {
      if (isFollowing) {
        await unfollowBand(band.id);
        setIsFollowing(false);
        setFollowerCount((prev) => Math.max(0, prev - 1));
      } else {
        await followBand(band.id);
        setIsFollowing(true);
        setFollowerCount((prev) => prev + 1);
      }
    } catch {
      // ignore
    } finally {
      setFollowLoading(false);
    }
  };

  const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

  if (loading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !band) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{error || t('bandDetail.notFound')}</Alert>
        <Button component={Link} to="/bands" startIcon={<ArrowBackIcon />} sx={{ textTransform: 'none' }}>
          {t('bandDetail.backToBands')}
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <BreadcrumbNav items={[
        { label: t('nav.bands'), to: '/bands' },
        { label: band.name },
      ]} />

      <Box sx={{
        display: 'flex',
        flexDirection: isMobile ? 'column' : 'row',
        gap: 3,
        mb: 4,
        alignItems: isMobile ? 'center' : 'flex-start',
      }}>
        <Box
          component="img"
          src={band.photoUrl || placeholderImg}
          alt={band.name}
          sx={{
            width: isMobile ? 200 : 250,
            height: isMobile ? 200 : 250,
            objectFit: 'contain',
            borderRadius: 2,
            backgroundColor: '#111',
            flexShrink: 0,
          }}
        />
        <Box sx={{ textAlign: isMobile ? 'center' : 'left' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: isMobile ? 'center' : 'flex-start', mb: 1, flexWrap: 'wrap' }}>
            <MusicNoteIcon sx={{ color: 'primary.main' }} />
            <Typography variant="h4" component="h1" sx={{ fontWeight: 800 }}>
              {band.name}
            </Typography>
            <ShareButton
              url={window.location.href}
              title={band.name}
              text={t('share.bandText').replace('{bandName}', band.name)}
            />
            {isLoggedIn && (
              <Button
                variant={isFollowing ? "outlined" : "contained"}
                color={isFollowing ? "inherit" : "primary"}
                onClick={handleFollowToggle}
                disabled={followLoading}
                size="small"
                sx={{ borderRadius: 5, ml: 1 }}
              >
                {isFollowing ? t('bandDetail.following') : t('bandDetail.follow')}
              </Button>
            )}
            {followerCount > 0 && (
              <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
                {followerCount} {followerCount === 1 ? t('bandDetail.follower') : t('bandDetail.followers')}
              </Typography>
            )}
          </Box>
          <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap', justifyContent: isMobile ? 'center' : 'flex-start' }}>
            {band.genre && (
              <Chip label={band.genre} size="small" color="secondary" />
            )}
            {band.formationYear > 0 && (
              <Chip label={`${t('bandDetail.formedIn')} ${band.formationYear}`} size="small" variant="outlined" />
            )}
          </Box>
          {band.description && (
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 600 }}>
              {band.description}
            </Typography>
          )}
          {band.metalArchivesUrl && (
            <Button
              component="a"
              href={band.metalArchivesUrl}
              target="_blank"
              rel="noopener noreferrer"
              size="small"
              startIcon={<OpenInNewIcon />}
              sx={{ textTransform: 'none', mt: 1 }}
            >
              {t('bandDetail.viewOnMetalArchives')}
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
          {t('bandDetail.albumsBy').replace('{bandName}', band.name)}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={isGrouped}
                onChange={(event) => {
                  const checked = event.target.checked;
                  setIsGrouped(checked);
                  setAllAlbums([]);
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

      {albumsLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : allAlbums.length > 0 ? (
        <InfiniteScroll
          onLoadMore={handleLoadMore}
          hasMore={scrollPage < pageCount}
          loading={loadingMore}
          loadedCount={allAlbums.length}
          totalCount={totalCount}
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
            {t('bandDetail.noAlbums')}
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

      {similarBands.length > 0 && (
        <>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2, mt: 4 }}>
            {t('bandDetail.similarBands')}
          </Typography>
          <Box
            sx={{
              display: 'flex',
              gap: 2,
              overflowX: 'auto',
              pb: 1,
              mx: -1,
              px: 1,
              '&::-webkit-scrollbar': { height: 6 },
              '&::-webkit-scrollbar-thumb': { bgcolor: 'rgba(255,255,255,0.2)', borderRadius: 3 },
            }}
          >
            {similarBands.map((similarBand) => (
              <Paper
                key={similarBand.id}
                component={Link}
                to={`/bands/${similarBand.slug}`}
                sx={{
                  minWidth: 180,
                  maxWidth: 180,
                  flexShrink: 0,
                  textDecoration: 'none',
                  color: 'inherit',
                  overflow: 'hidden',
                  borderRadius: 2,
                  transition: 'transform 0.2s',
                  '&:hover': { transform: 'translateY(-4px)' },
                }}
              >
                <Box
                  component="img"
                  src={similarBand.photoUrl || placeholderImg}
                  alt={similarBand.name}
                  loading="lazy"
                  sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
                />
                <Box sx={{ p: 1.5 }}>
                  <Typography variant="subtitle2" sx={{
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}>
                    {similarBand.name}
                  </Typography>
                  {similarBand.genre && (
                    <Chip label={similarBand.genre} size="small" color="secondary" sx={{ mt: 0.5 }} />
                  )}
                </Box>
              </Paper>
            ))}
          </Box>
        </>
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
            initialFilters={{ ...albumFilters, bandId: band?.id }}
            hideBandFilter
          />
        </Box>
      </Drawer>
    </Container>
  );
};

export default BandDetailPage;
