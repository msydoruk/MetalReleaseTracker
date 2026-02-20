import React, { useState, useEffect } from 'react';
import {
  Container,
  Grid,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Paper,
  Drawer,
  IconButton,
  Button,
  Divider,
  Chip,
  FormControl,
  FormControlLabel,
  Checkbox,
  Select,
  MenuItem,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useLocation, Link } from 'react-router-dom';
import FilterListIcon from '@mui/icons-material/FilterList';
import CloseIcon from '@mui/icons-material/Close';
import AlbumCard from '../components/AlbumCard';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import AlbumFilter from '../components/AlbumFilter';
import Pagination from '../components/Pagination';
import { fetchAlbums, fetchGroupedAlbums, fetchDistributors, fetchFavoriteIds, addFavorite, removeFavorite } from '../services/api';
import authService from '../services/auth';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';

const AlbumsPage = ({ isHome = false }) => {
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  usePageMeta(
    isHome
      ? 'Metal Release Tracker - Ukrainian Metal Releases from Foreign Distributors'
      : 'Albums - Ukrainian Metal Releases',
    'Browse Ukrainian metal releases available from foreign distributors. Filter by band, format, price. Vinyl, CD, cassette - order directly from the source.'
  );
  const location = useLocation();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [albums, setAlbums] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageCount, setPageCount] = useState(0);
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [distributors, setDistributors] = useState([]);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isGrouped, setIsGrouped] = useState(false);
  const [groupedAlbums, setGroupedAlbums] = useState([]);

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

  const getInitialFilters = () => {
    const searchParams = new URLSearchParams(location.search);
    const bandId = searchParams.get('bandId');
    const distributorId = searchParams.get('distributorId');

    return {
      page: 1,
      pageSize: 20,
      sortBy: ALBUM_SORT_FIELDS.RELEASE_DATE,
      sortAscending: false,
      ...(bandId && { bandId }),
      ...(distributorId && { distributorId })
    };
  };

  const [filters, setFilters] = useState(getInitialFilters);

  useEffect(() => {
    setFilters(getInitialFilters());
  }, [location.search]);

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
    // Reset to page 1 when filters change
    setFilters({
      ...newFilters,
      page: 1
    });
    // Close filter drawer after applying
    setIsFilterOpen(false);
  };

  const handlePageChange = (newPage) => {
    setFilters({
      ...filters,
      page: newPage
    });
  };

  const handlePageSizeChange = (newPageSize) => {
    setFilters({
      ...filters,
      pageSize: newPageSize,
      page: 1 // Reset to first page when changing page size
    });
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
    setFilters({
      ...filters,
      distributorId: distributorId || '',
      page: 1
    });
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
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component={isHome ? 'h2' : 'h1'}>
          {t('albums.metalReleases')}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={isGrouped}
                onChange={(event) => {
                  setIsGrouped(event.target.checked);
                  setFilters({ ...filters, page: 1 });
                }}
                size="small"
              />
            }
            label={t('albums.groupDuplicates')}
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

      {distributors.length > 0 && (
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
          <Box sx={{ width: '100%', mb: 4 }}>
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
                gap: 3,
                alignItems: 'stretch'
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
            </Grid>
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
            boxShadow: '-4px 0 20px rgba(0,0,0,0.2)'
          },
        }}
      >
        <Box sx={{ position: 'relative', p: 1 }}>
          <IconButton
            onClick={toggleFilterDrawer}
            sx={{ position: 'absolute', right: 8, top: 8, zIndex: 5 }}
          >
            <CloseIcon />
          </IconButton>
          <AlbumFilter
            onFilterChange={handleFilterChange}
            initialFilters={filters}
          />
        </Box>
      </Drawer>
    </Container>
  );
};

export default AlbumsPage;