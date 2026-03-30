import React, { useState, useEffect, useMemo, useRef, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Paper,
  Drawer,
  Button,
  Divider,
  Chip,
  Fab,
  FormControl,
  FormControlLabel,
  Checkbox,
  Select,
  MenuItem,
  useMediaQuery,
  useTheme
} from '@mui/material';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { useSearchParams, Link } from 'react-router-dom';
import FilterListIcon from '@mui/icons-material/FilterList';
import AlbumCard from '../components/AlbumCard';
import NewArrivalsSection from '../components/NewArrivalsSection';
import RecentlyViewedSection from '../components/RecentlyViewedSection';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import AlbumFilter from '../components/AlbumFilter';
import InfiniteScroll from '../components/InfiniteScroll';
import { fetchAlbums, fetchGroupedAlbums, fetchDistributors, fetchFavoriteIds, addFavorite, removeFavorite, updateFavoriteStatus } from '../services/api';
import authService from '../services/auth';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';
import usePageMeta from '../hooks/usePageMeta';
import useRecentlyViewed from '../hooks/useRecentlyViewed';
import { useLanguage } from '../i18n/LanguageContext';

const DEFAULTS = {
  page: 1,
  pageSize: 20,
  sortBy: ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
  sortAscending: false,
  minPrice: 0,
  maxPrice: 200,
};

const FILTER_STORAGE_KEY = 'albumFilters';

const saveFilters = (filters) => {
  localStorage.setItem(FILTER_STORAGE_KEY, JSON.stringify(filters));
};

const loadFilters = () => {
  try {
    const stored = localStorage.getItem(FILTER_STORAGE_KEY);
    return stored ? JSON.parse(stored) : null;
  } catch {
    return null;
  }
};

const parseIntParam = (value, defaultValue) => {
  const parsed = parseInt(value, 10);
  return isNaN(parsed) ? defaultValue : parsed;
};

const parseFiltersFromUrl = (searchParams) => ({
  page: parseIntParam(searchParams.get('page'), DEFAULTS.page),
  pageSize: parseIntParam(searchParams.get('pageSize'), DEFAULTS.pageSize),
  sortBy: parseIntParam(searchParams.get('sortBy'), DEFAULTS.sortBy),
  sortAscending: searchParams.has('sortAscending') ? searchParams.get('sortAscending') !== 'false' : DEFAULTS.sortAscending,
  bandId: searchParams.get('bandId') || '',
  distributorId: searchParams.get('distributorId') || '',
  name: searchParams.get('name') || '',
  mediaType: searchParams.get('mediaType') || '',
  stockStatus: searchParams.get('stockStatus') || '',
  genre: searchParams.get('genre') || '',
  minPrice: parseIntParam(searchParams.get('minPrice'), DEFAULTS.minPrice),
  maxPrice: parseIntParam(searchParams.get('maxPrice'), DEFAULTS.maxPrice),
  minYear: searchParams.get('minYear') ? parseIntParam(searchParams.get('minYear'), null) : null,
  maxYear: searchParams.get('maxYear') ? parseIntParam(searchParams.get('maxYear'), null) : null,
});

const filtersToSearchParams = (filters) => {
  const params = new URLSearchParams();
  if (filters.page > DEFAULTS.page) params.set('page', filters.page);
  if (filters.pageSize !== DEFAULTS.pageSize) params.set('pageSize', filters.pageSize);
  if (filters.sortBy !== DEFAULTS.sortBy) params.set('sortBy', filters.sortBy);
  if (filters.sortAscending !== DEFAULTS.sortAscending) params.set('sortAscending', String(filters.sortAscending));
  if (filters.bandId) params.set('bandId', filters.bandId);
  if (filters.distributorId) params.set('distributorId', filters.distributorId);
  if (filters.name) params.set('name', filters.name);
  if (filters.mediaType) params.set('mediaType', filters.mediaType);
  if (filters.stockStatus) params.set('stockStatus', filters.stockStatus);
  if (filters.genre) params.set('genre', filters.genre);
  if (filters.minPrice > DEFAULTS.minPrice) params.set('minPrice', filters.minPrice);
  if (filters.maxPrice < DEFAULTS.maxPrice) params.set('maxPrice', filters.maxPrice);
  if (filters.minYear) params.set('minYear', filters.minYear);
  if (filters.maxYear) params.set('maxYear', filters.maxYear);
  return params;
};

const AlbumsPage = ({ isHome = false }) => {
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const { recentAlbums } = useRecentlyViewed();

  usePageMeta(
    isHome
      ? t('pageMeta.homeTitle')
      : t('pageMeta.albumsTitle'),
    isHome
      ? t('pageMeta.homeDescription')
      : t('pageMeta.albumsDescription')
  );
  const [searchParams, setSearchParams] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState(null);
  const [albums, setAlbums] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [scrollPage, setScrollPage] = useState(1);
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [distributors, setDistributors] = useState([]);
  const [favoriteIds, setFavoriteIds] = useState({});
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isGrouped, setIsGrouped] = useState(() => localStorage.getItem('albumsGrouped') !== 'false');
  const [groupedAlbums, setGroupedAlbums] = useState([]);
  const [showScrollTop, setShowScrollTop] = useState(false);
  const restoredFromStorage = useRef(false);

  // On initial load, if no URL params, restore filters from localStorage
  useEffect(() => {
    if (restoredFromStorage.current) return;
    restoredFromStorage.current = true;

    const hasUrlParams = searchParams.toString().length > 0;
    if (!hasUrlParams) {
      const savedFilters = loadFilters();
      if (savedFilters) {
        const params = filtersToSearchParams(savedFilters);
        if (params.toString().length > 0) {
          setSearchParams(params, { replace: true });
        }
      }
    }
  }, [searchParams, setSearchParams]);

  const filters = useMemo(() => parseFiltersFromUrl(searchParams), [searchParams]);

  // Save filters to localStorage whenever they change
  useEffect(() => {
    saveFilters(filters);
  }, [filters]);

  const updateFilters = (newFilters) => {
    setSearchParams(filtersToSearchParams(newFilters), { replace: true });
  };

  useEffect(() => {
    const checkAuthAndLoadFavorites = async () => {
      try {
        const loggedIn = await authService.isLoggedIn();
        setIsLoggedIn(loggedIn);
        if (loggedIn) {
          const response = await fetchFavoriteIds();
          setFavoriteIds(response.data || {});
        }
      } catch (error) {
        console.error('Error loading favorites:', error);
      }
    };

    checkAuthAndLoadFavorites();
  }, []);

  useEffect(() => {
    const loadDistributors = async () => {
      try {
        const response = await fetchDistributors();
        setDistributors(response.data || []);
      } catch (error) {
        console.error('Error fetching distributors:', error);
      }
    };

    loadDistributors();
  }, []);

  // Reset scroll page when filters change
  const filtersKey = useMemo(() => {
    const { page, ...rest } = filters;
    return JSON.stringify(rest);
  }, [filters]);

  useEffect(() => {
    setScrollPage(1);
    setAlbums([]);
    setGroupedAlbums([]);
  }, [filtersKey, isGrouped]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const isAppending = scrollPage > 1;
        if (isAppending) {
          setLoadingMore(true);
        } else {
          setLoading(true);
        }

        const fetchParams = { ...filters, page: scrollPage };
        if (isGrouped) {
          const response = await fetchGroupedAlbums(fetchParams);
          if (response.data) {
            setGroupedAlbums((previous) => isAppending
              ? [...previous, ...(response.data.items || [])]
              : (response.data.items || []));
            setAlbums([]);
            setTotalCount(response.data.totalCount || 0);
            setPageCount(response.data.pageCount || 0);
          }
        } else {
          const response = await fetchAlbums(fetchParams);
          if (response.data) {
            setAlbums((previous) => isAppending
              ? [...previous, ...(response.data.items || [])]
              : (response.data.items || []));
            setGroupedAlbums([]);
            setTotalCount(response.data.totalCount || 0);
            setPageCount(response.data.pageCount || 0);
          }
        }
      } catch (err) {
        console.error('Error fetching albums:', err);
        setError(t('albums.error'));
      } finally {
        setLoading(false);
        setLoadingMore(false);
      }
    };

    fetchData();
  }, [scrollPage, filtersKey, isGrouped, filters, t]);

  const handleLoadMore = useCallback(() => {
    if (!loadingMore && scrollPage < pageCount) {
      setScrollPage((previous) => previous + 1);
    }
  }, [loadingMore, scrollPage, pageCount]);

  // Show scroll-to-top button on scroll
  useEffect(() => {
    const handleScroll = () => {
      setShowScrollTop(window.scrollY > 600);
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleFilterChange = (newFilters) => {
    updateFilters({ ...newFilters, page: 1 });
    setIsFilterOpen(false);
  };

  const toggleFilterDrawer = () => {
    setIsFilterOpen(!isFilterOpen);
  };

  const handleCollectionChange = async (albumId, status) => {
    try {
      if (albumId in favoriteIds) {
        await updateFavoriteStatus(albumId, status);
      } else {
        await addFavorite(albumId, status);
      }
      setFavoriteIds((previous) => ({ ...previous, [albumId]: status }));
    } catch (error) {
      console.error('Error updating collection:', error);
    }
  };

  const handleRemoveFromCollection = async (albumId) => {
    try {
      await removeFavorite(albumId);
      setFavoriteIds((previous) => {
        const next = { ...previous };
        delete next[albumId];
        return next;
      });
    } catch (error) {
      console.error('Error removing from collection:', error);
    }
  };

  const handleDistributorSelect = (distributorId) => {
    updateFilters({ ...filters, distributorId: distributorId || '', page: 1 });
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {isHome && (
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Typography variant="h3" component="h1" sx={{ fontWeight: 800, mb: 1.5 }}>
            {t('albums.heroTitle')}{' '}
            <Box component="span" sx={{ display: 'inline-flex', verticalAlign: 'middle', ml: 1 }}>
              <svg width="36" height="24" viewBox="0 0 36 24" style={{ borderRadius: 3 }}>
                <rect width="36" height="12" fill="#005BBB" />
                <rect y="12" width="36" height="12" fill="#FFD500" />
              </svg>
            </Box>
          </Typography>
          <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 700, mx: 'auto', mb: 2, lineHeight: 1.6 }}>
            {t('albums.heroSubtitle')}
          </Typography>
          <Button
            component={Link}
            to="/about"
            variant="outlined"
            color="primary"
            sx={{ textTransform: 'none', fontWeight: 600, borderRadius: 5, px: 3 }}
          >
            {t('albums.learnMore')}
          </Button>
          <Divider sx={{ mt: 3 }} />
        </Box>
      )}
      {isHome && (
        <NewArrivalsSection />
      )}
      {isHome && <RecentlyViewedSection albums={recentAlbums} />}
      {isHome && <Divider sx={{ my: 3 }} />}
      <Box sx={{
        display: 'flex',
        flexDirection: { xs: 'column', sm: 'row' },
        justifyContent: 'space-between',
        alignItems: { xs: 'flex-start', sm: 'center' },
        gap: { xs: 1.5, sm: 0 },
        mb: 3
      }}>
        <Typography variant="h4" component={isHome ? 'h2' : 'h1'} sx={{ fontWeight: 800 }}>
          {t('albums.metalReleases')}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: { xs: '100%', sm: 'auto' }, justifyContent: { xs: 'space-between', sm: 'flex-start' } }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={isGrouped}
                onChange={(event) => {
                  const checked = event.target.checked;
                  setIsGrouped(checked);
                  localStorage.setItem('albumsGrouped', String(checked));
                  updateFilters({ ...filters, distributorId: checked ? '' : filters.distributorId, page: 1 });
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
            onClick={toggleFilterDrawer}
            sx={{ fontWeight: 'bold' }}
          >
            {t('albums.filters')}
          </Button>
        </Box>
      </Box>

      {distributors.length > 0 && !isGrouped && (
        isMobile ? (
          <FormControl
            fullWidth
            size="small"
            sx={{ mb: 3 }}
          >
            <Select
              value={filters.distributorId || ''}
              onChange={(event) => handleDistributorSelect(event.target.value)}
              displayEmpty
              renderValue={(selected) => {
                if (!selected) {
                  return t('albums.allDistributorsDropdown');
                }
                const dist = distributors.find(d => d.id === selected);
                return dist ? dist.name : '';
              }}
              MenuProps={{
                PaperProps: {
                  style: {
                    backgroundColor: '#222',
                    color: '#fff'
                  }
                }
              }}
              sx={{
                '& .MuiSelect-select': {
                  color: 'white',
                  fontWeight: 'medium'
                },
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.3)'
                },
                '&:hover .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.5)'
                }
              }}
            >
              <MenuItem value="">{t('albums.allDistributorsDropdown')}</MenuItem>
              {distributors.map((distributor) => (
                <MenuItem key={distributor.id} value={distributor.id}>
                  {distributor.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        ) : (
          <Box sx={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 1,
            mb: 3
          }}>
            <Chip
              label={t('albums.allDistributors')}
              variant={!filters.distributorId ? 'filled' : 'outlined'}
              color={!filters.distributorId ? 'primary' : 'default'}
              onClick={() => handleDistributorSelect('')}
              sx={{
                fontWeight: !filters.distributorId ? 'bold' : 'normal',
                borderColor: 'rgba(255, 255, 255, 0.3)',
                '&:hover': { borderColor: 'rgba(255, 255, 255, 0.6)' }
              }}
            />
            {distributors.map((distributor) => (
              <Chip
                key={distributor.id}
                label={distributor.name}
                variant={filters.distributorId === distributor.id ? 'filled' : 'outlined'}
                color={filters.distributorId === distributor.id ? 'primary' : 'default'}
                onClick={() => handleDistributorSelect(distributor.id)}
                sx={{
                  fontWeight: filters.distributorId === distributor.id ? 'bold' : 'normal',
                  borderColor: 'rgba(255, 255, 255, 0.3)',
                  '&:hover': { borderColor: 'rgba(255, 255, 255, 0.6)' }
                }}
              />
            ))}
          </Box>
        )
      )}

      {error && (
        <Alert severity="error" sx={{ my: 2 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
          <CircularProgress />
        </Box>
      ) : (isGrouped ? groupedAlbums.length > 0 : albums.length > 0) ? (
        <InfiniteScroll
          onLoadMore={handleLoadMore}
          hasMore={scrollPage < pageCount}
          loading={loadingMore}
          loadedCount={isGrouped ? groupedAlbums.length : albums.length}
          totalCount={totalCount}
        >
          <Box sx={{ width: '100%', mb: 4 }}>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: 'repeat(1, 1fr)',
                  sm: 'repeat(2, 1fr)',
                  md: 'repeat(3, 1fr)',
                  lg: 'repeat(4, 1fr)',
                  xl: 'repeat(5, 1fr)'
                },
                gap: 3,
                alignItems: 'start'
              }}
            >
              {isGrouped ? (
                groupedAlbums.map((group, index) => (
                  <Box
                    key={`${group.bandName}-${group.albumName}-${index}`}
                    sx={{ display: 'flex', height: '100%' }}
                  >
                    <GroupedAlbumCard group={group} />
                  </Box>
                ))
              ) : (
                albums.map((album, index) => (
                  <Box
                    key={`${album.id}-${index}`}
                    sx={{
                      display: 'flex',
                      height: '100%'
                    }}
                  >
                    <AlbumCard
                      album={album}
                      collectionStatus={album.id in favoriteIds ? favoriteIds[album.id] : undefined}
                      onCollectionChange={(albumId, status) => handleCollectionChange(albumId, status)}
                      onRemoveFromCollection={(albumId) => handleRemoveFromCollection(albumId)}
                      isLoggedIn={isLoggedIn}
                    />
                  </Box>
                ))
              )}
            </Box>
          </Box>
        </InfiniteScroll>
      ) : (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('albums.noAlbums')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {t('albums.tryAdjusting')}
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

      {/* Filter drawer */}
      <Drawer
        anchor="right"
        open={isFilterOpen}
        onClose={toggleFilterDrawer}
        sx={{
          '& .MuiDrawer-paper': {
            width: { xs: '100%', sm: 400 },
            boxSizing: 'border-box',
            backgroundColor: 'background.paper',
            borderTopLeftRadius: { xs: 0, sm: 8 },
            borderBottomLeftRadius: { xs: 0, sm: 8 },
            boxShadow: '-4px 0 20px rgba(0,0,0,0.2)',
            overflow: 'hidden',
            mt: { xs: '56px', sm: '64px' },
            height: { xs: 'calc(100% - 56px)', sm: 'calc(100% - 64px)' },
          },
        }}
      >
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', p: 1 }}>
          <AlbumFilter
            onFilterChange={handleFilterChange}
            onClose={toggleFilterDrawer}
            initialFilters={filters}
          />
        </Box>
      </Drawer>

    </Container>
  );
};

export default AlbumsPage;