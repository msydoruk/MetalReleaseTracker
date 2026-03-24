import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  ToggleButton,
  ToggleButtonGroup,
  Chip,
  Paper,
  Grid,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { Link } from 'react-router-dom';
import { fetchGroupedAlbums, fetchDistributorsWithAlbumCount } from '../services/api';
import MediaTypeIcon from '../components/MediaTypeIcon';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';

const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

const CalendarPage = () => {
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();
  const [preOrders, setPreOrders] = useState([]);
  const [recentReleases, setRecentReleases] = useState([]);
  const [loading, setLoading] = useState(true);
  const [mediaFilter, setMediaFilter] = useState('');
  const [distributors, setDistributors] = useState([]);
  const [distributorFilter, setDistributorFilter] = useState('');

  usePageMeta(
    t('calendar.title'),
    t('calendar.description')
  );

  useEffect(() => {
    const loadFilters = async () => {
      try {
        const [distributorResponse] = await Promise.all([
          fetchDistributorsWithAlbumCount(),
        ]);
        setDistributors(distributorResponse.data || []);
      } catch {
        // ignore
      }
    };

    loadFilters();
  }, []);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

      const baseFilters = {
        pageSize: 50,
        page: 1,
        sortBy: 8,
        sortAscending: false,
      };

      if (mediaFilter) baseFilters.mediaType = mediaFilter;
      if (distributorFilter) baseFilters.distributorId = distributorFilter;

      const [preOrderResponse, recentResponse] = await Promise.all([
        fetchGroupedAlbums({ ...baseFilters, status: 'Preorder' }),
        fetchGroupedAlbums({ ...baseFilters, addedAfter: thirtyDaysAgo.toISOString() }),
      ]);

      setPreOrders(preOrderResponse.data?.items || []);
      setRecentReleases(recentResponse.data?.items || []);
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  }, [mediaFilter, distributorFilter]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const renderAlbumGrid = (albums) => (
    <Grid
      container
      spacing={2}
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: 'repeat(2, 1fr)',
          sm: 'repeat(3, 1fr)',
          md: 'repeat(4, 1fr)',
          lg: 'repeat(5, 1fr)',
        },
      }}
    >
      {albums.map((album, index) => (
        <Paper
          key={`${album.albumSlug}-${index}`}
          component={Link}
          to={`/albums/${album.albumSlug}`}
          sx={{
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
            src={album.photoUrl || placeholderImg}
            alt={album.albumName}
            loading="lazy"
            sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
          />
          <Box sx={{ p: 1.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 0.5 }}>
              <MediaTypeIcon mediaType={album.media} size={14} />
              {album.status === 2 && (
                <Chip label={t('calendar.preOrder')} size="small" color="warning" sx={{ height: 18, fontSize: '0.65rem' }} />
              )}
            </Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {album.albumName}
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {album.bandName}
            </Typography>
            {album.originalYear > 0 && (
              <Typography variant="caption" color="text.disabled">
                {album.originalYear}
              </Typography>
            )}
            {album.variants?.length > 0 && (
              <Typography variant="caption" color="primary" sx={{ display: 'block', fontWeight: 600 }}>
                {formatPrice(Math.min(...album.variants.map((variant) => variant.price)))}
                {album.variants.length > 1 && ` — ${album.variants.length} stores`}
              </Typography>
            )}
          </Box>
        </Paper>
      ))}
    </Grid>
  );

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <CalendarMonthIcon sx={{ fontSize: 32 }} />
        <Typography variant="h4" sx={{ fontWeight: 700 }}>
          {t('calendar.title')}
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap', alignItems: 'center' }}>
        <ToggleButtonGroup
          value={mediaFilter}
          exclusive
          onChange={(event, value) => setMediaFilter(value || '')}
          size="small"
        >
          <ToggleButton value="">{t('albumFilter.all')}</ToggleButton>
          <ToggleButton value="CD">CD</ToggleButton>
          <ToggleButton value="LP">Vinyl</ToggleButton>
          <ToggleButton value="Tape">Tape</ToggleButton>
        </ToggleButtonGroup>

        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>{t('albumFilter.distributor')}</InputLabel>
          <Select
            value={distributorFilter}
            onChange={(event) => setDistributorFilter(event.target.value)}
            label={t('albumFilter.distributor')}
          >
            <MenuItem value="">{t('albumFilter.all')}</MenuItem>
            {distributors.map((distributor) => (
              <MenuItem key={distributor.id} value={distributor.id}>{distributor.name}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {preOrders.length > 0 && (
            <Box sx={{ mb: 5 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <ShoppingCartIcon color="warning" />
                <Typography variant="h5" sx={{ fontWeight: 700 }}>
                  {t('calendar.preOrders')}
                </Typography>
                <Chip label={preOrders.length} size="small" color="warning" />
              </Box>
              {renderAlbumGrid(preOrders)}
            </Box>
          )}

          <Box sx={{ mb: 5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <NewReleasesIcon color="info" />
              <Typography variant="h5" sx={{ fontWeight: 700 }}>
                {t('calendar.recentReleases')}
              </Typography>
              <Chip label={recentReleases.length} size="small" color="info" />
            </Box>
            {recentReleases.length > 0 ? (
              renderAlbumGrid(recentReleases)
            ) : (
              <Typography variant="body2" color="text.secondary">
                {t('calendar.noReleases')}
              </Typography>
            )}
          </Box>
        </>
      )}
    </Container>
  );
};

export default CalendarPage;
