import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { Box, Typography, useMediaQuery, useTheme } from '@mui/material';
import { Link } from 'react-router-dom';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import GroupedAlbumCard from './GroupedAlbumCard';
import { fetchGroupedAlbums } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';

const DAYS_LOOKBACK = 14;
const MAX_DISPLAY = 5;

const isPlaceholderImage = (url) => !url || url.startsWith('data:');

const NewArrivalsSection = () => {
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [albums, setAlbums] = useState([]);
  const [loaded, setLoaded] = useState(false);

  const loadNewArrivals = useCallback(async () => {
    try {
      const cutoff = new Date(Date.now() - DAYS_LOOKBACK * 24 * 60 * 60 * 1000);
      const response = await fetchGroupedAlbums({
        addedAfter: cutoff.toISOString(),
        sortBy: ALBUM_SORT_FIELDS.DATE_ADDED,
        sortAscending: false,
        pageSize: 20,
        page: 1,
      });
      setAlbums(response.data.items || []);
    } catch {
      setAlbums([]);
    } finally {
      setLoaded(true);
    }
  }, []);

  useEffect(() => {
    loadNewArrivals();
  }, [loadNewArrivals]);

  const displayAlbums = useMemo(() => {
    return albums.filter((album) => !isPlaceholderImage(album.photoUrl)).slice(0, MAX_DISPLAY);
  }, [albums]);

  if (!loaded || albums.length === 0) {
    return null;
  }

  return (
    <Box sx={{ mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Box>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 700 }}>
            {t('newArrivals.title')}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('newArrivals.subtitle')}
          </Typography>
        </Box>
        <Typography
          component={Link}
          to={`/albums?sortBy=${ALBUM_SORT_FIELDS.DATE_ADDED}&sortAscending=false`}
          variant="body2"
          color="primary"
          sx={{ display: 'flex', alignItems: 'center', gap: 0.5, textDecoration: 'none', whiteSpace: 'nowrap' }}
        >
          {t('newArrivals.viewAll')}
          <ArrowForwardIcon sx={{ fontSize: 16 }} />
        </Typography>
      </Box>

      {isMobile ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: '1fr',
            gap: 2,
            alignItems: 'start',
          }}
        >
          {displayAlbums.slice(0, 3).map((group, index) => (
            <GroupedAlbumCard
              key={`${group.bandName}-${group.albumName}-${index}`}
              group={group}
            />
          ))}
        </Box>
      ) : (
        <Box
          sx={{
            display: 'flex',
            gap: 2,
            overflowX: 'auto',
            scrollSnapType: 'x mandatory',
            pb: 1,
            mx: -1,
            px: 1,
            '&::-webkit-scrollbar': { height: 6 },
            '&::-webkit-scrollbar-thumb': { bgcolor: 'rgba(255,255,255,0.2)', borderRadius: 3 },
          }}
        >
          {displayAlbums.map((group, index) => (
            <Box
              key={`${group.bandName}-${group.albumName}-${index}`}
              sx={{
                minWidth: 280,
                maxWidth: 280,
                flexShrink: 0,
                scrollSnapAlign: 'start',
              }}
            >
              <GroupedAlbumCard group={group} />
            </Box>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default NewArrivalsSection;
