import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  ToggleButton,
  ToggleButtonGroup,
  Chip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import { fetchGroupedAlbums, fetchDistributorsWithAlbumCount } from '../services/api';
import GroupedAlbumCard from '../components/GroupedAlbumCard';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';

const CalendarPage = () => {
  const { t } = useLanguage();
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
    <Box
      sx={{
        display: 'grid',
        gap: 3,
        gridTemplateColumns: {
          xs: 'repeat(1, 1fr)',
          sm: 'repeat(2, 1fr)',
          md: 'repeat(3, 1fr)',
          lg: 'repeat(4, 1fr)',
          xl: 'repeat(5, 1fr)',
        },
        alignItems: 'start',
      }}
    >
      {albums.map((group, index) => (
        <Box
          key={`${group.bandName}-${group.albumName}-${index}`}
          sx={{ display: 'flex', height: '100%' }}
        >
          <GroupedAlbumCard group={group} />
        </Box>
      ))}
    </Box>
  );

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Typography variant="h5" component="h1" sx={{ fontWeight: 700, mb: 3 }}>
        {t('calendar.title')}
      </Typography>

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
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
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
              <Typography variant="h6" sx={{ fontWeight: 700 }}>
                {t('calendar.recentReleases')}
              </Typography>
              <Chip label={recentReleases.length} size="small" color="info" />
            </Box>
            {recentReleases.length > 0 ? (
              renderAlbumGrid(recentReleases)
            ) : (
              <Typography variant="h6" color="text.secondary">
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
