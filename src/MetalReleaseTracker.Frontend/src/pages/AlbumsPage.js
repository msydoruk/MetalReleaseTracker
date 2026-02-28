import React, { useState, useEffect, useMemo, useRef } from 'react';
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
  FormControl,
  FormControlLabel,
  Checkbox,
  Select,
  MenuItem,
  TextField,
  InputAdornment,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useSearchParams, Link } from 'react-router-dom';
import FilterListIcon from '@mui/icons-material/FilterList';
import SearchIcon from '@mui/icons-material/Search';
import AlbumCard from '../components/AlbumCard';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import AlbumFilter from '../components/AlbumFilter';
import Pagination from '../components/Pagination';
import { fetchAlbums, fetchGroupedAlbums, fetchDistributors, fetchFavoriteIds, addFavorite, removeFavorite } from '../services/api';
import authService from '../services/auth';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';

const DEFAULTS = {
  page: 1,
  pageSize: 20,
  sortBy: ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
  sortAscending: false,
  minPrice: 0,
  maxPrice: 200,
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
  const [error, setError] = useState(null);
  const [albums, setAlbums] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [distributors, setDistributors] = useState([]);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isGrouped, setIsGrouped] = useState(() => localStorage.getItem('albumsGrouped') !== 'false');
  const [groupedAlbums, setGroupedAlbums] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const albumListRef = useRef(null);
  const searchTimerRef = useRef(null);

  const filters = useMemo(() => parseFiltersFromUrl(searchParams), [searchParams]);

  useEffect(() => {
    setSearchQuery(filters.name || '');
  }, [filters.name]);

  useEffect(() => {
    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, []);

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
          setFavoriteIds(new Set(response.data));
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

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        if (isGrouped) {
          const response = await fetchGroupedAlbums(filters);
          if (response.data) {
            setGroupedAlbums(response.data.items || []);
            setAlbums([]);
            setTotalCount(response.data.totalCount || 0);
            setPageCount(response.data.pageCount || 0);
          }
        } else {
          const response = await fetchAlbums(filters);
          if (response.data) {
            setAlbums(response.data.items || []);
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
      }
    };

    fetchData();
  }, [filters, isGrouped]);

  const handleFilterChange = (newFilters) => {
    updateFilters({ ...newFilters, page: 1 });
    setIsFilterOpen(false);
  };

  const handlePageChange = (newPage) => {
    updateFilters({ ...filters, page: newPage });
    albumListRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  const handlePageSizeChange = (newPageSize) => {
    updateFilters({ ...filters, pageSize: newPageSize, page: 1 });
  };

  const toggleFilterDrawer = () => {
    setIsFilterOpen(!isFilterOpen);
  };

  const handleToggleFavorite = async (albumId) => {
    try {
      if (favoriteIds.has(albumId)) {
        await removeFavorite(albumId);
        setFavoriteIds((previous) => {
          const next = new Set(previous);
          next.delete(albumId);
          return next;
        });
      } else {
        await addFavorite(albumId);
        setFavoriteIds((previous) => new Set(previous).add(albumId));
      }
    } catch (error) {
      console.error('Error toggling favorite:', error);
    }
  };

  const handleDistributorSelect = (distributorId) => {
    updateFilters({ ...filters, distributorId: distributorId || '', page: 1 });
  };

  const handleSearchChange = (event) => {
    const value = event.target.value;
    setSearchQuery(value);
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      updateFilters({ ...filters, name: value, page: 1 });
    }, 400);
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
      <Box sx={{
        display: 'flex',
        flexDirection: { xs: 'column', sm: 'row' },
        justifyContent: 'space-between',
        alignItems: { xs: 'flex-start', sm: 'center' },
        gap: { xs: 1.5, sm: 0 },
        mb: 3
      }}>
        <Typography variant="h4" component={isHome ? 'h2' : 'h1'}>
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

      <TextField
        fullWidth
        size="small"
        placeholder={t('albums.searchPlaceholder')}
        value={searchQuery}
        onChange={handleSearchChange}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon sx={{ color: 'rgba(255, 255, 255, 0.5)' }} />
            </InputAdornment>
          ),
        }}
        sx={{
          mb: 3,
          maxWidth: 400,
          backgroundColor: 'rgba(255, 255, 255, 0.05)',
          borderRadius: 1,
          '& .MuiOutlinedInput-root': {
            '& fieldset': {
              borderColor: 'rgba(255, 255, 255, 0.3)',
            },
            '&:hover fieldset': {
              borderColor: 'rgba(255, 255, 255, 0.5)',
            },
            '&.Mui-focused fieldset': {
              borderColor: 'primary.main',
            },
          },
          '& .MuiInputBase-input': {
            color: 'white',
          },
        }}
      />

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
        <>
          <Box ref={albumListRef} sx={{ mb: 1 }}>
            <Pagination
              currentPage={filters.page}
              totalPages={pageCount}
              totalItems={totalCount}
              pageSize={filters.pageSize}
              onPageChange={handlePageChange}
              onPageSizeChange={handlePageSizeChange}
              compact
            />
          </Box>

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
                albums.map((album) => (
                  <Box
                    key={album.id}
                    sx={{
                      display: 'flex',
                      height: '100%'
                    }}
                  >
                    <AlbumCard
                      album={album}
                      isFavorited={favoriteIds.has(album.id)}
                      onToggleFavorite={handleToggleFavorite}
                      isLoggedIn={isLoggedIn}
                    />
                  </Box>
                ))
              )}
            </Box>
          </Box>

          <Box sx={{ mt: 3, mb: 4 }}>
            <Pagination
              currentPage={filters.page}
              totalPages={pageCount}
              totalItems={totalCount}
              pageSize={filters.pageSize}
              onPageChange={handlePageChange}
              onPageSizeChange={handlePageSizeChange}
            />
          </Box>
        </>
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
            overflow: 'hidden'
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