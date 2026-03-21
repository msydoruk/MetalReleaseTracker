import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { Box, Typography, Paper, useMediaQuery, useTheme } from '@mui/material';
import { Link } from 'react-router-dom';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import AlbumCard from './AlbumCard';
import { fetchAlbums } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';

const DAYS_LOOKBACK = 14;
const MAX_DISPLAY = 10;

const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

const isPlaceholderImage = (url) => !url || url.startsWith('data:');

const NewArrivalsSection = ({ favoriteIds, onCollectionChange, onRemoveFromCollection, isLoggedIn }) => {
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const [albums, setAlbums] = useState([]);
  const [loaded, setLoaded] = useState(false);

  const loadNewArrivals = useCallback(async () => {
    try {
      const cutoff = new Date(Date.now() - DAYS_LOOKBACK * 24 * 60 * 60 * 1000);
      const response = await fetchAlbums({
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
    const withCovers = albums.filter((album) => !isPlaceholderImage(album.photoUrl));
    const withoutCovers = albums.filter((album) => isPlaceholderImage(album.photoUrl));
    return [...withCovers, ...withoutCovers].slice(0, MAX_DISPLAY);
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
            gridTemplateColumns: 'repeat(2, 1fr)',
            gap: 1.5,
          }}
        >
          {displayAlbums.map((album) => (
            <Paper
              key={album.id}
              component={Link}
              to={`/albums/${album.id}`}
              sx={{
                textDecoration: 'none',
                color: 'inherit',
                overflow: 'hidden',
                borderRadius: 2,
                transition: 'transform 0.2s',
                '&:hover': { transform: 'translateY(-2px)' },
              }}
            >
              <Box
                component="img"
                src={album.photoUrl || placeholderImg}
                alt={album.name}
                sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
              />
              <Box sx={{ p: 1 }}>
                <Typography variant="subtitle2" sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                  fontWeight: 600,
                  fontSize: '0.8rem',
                }}>
                  {album.name}
                </Typography>
                <Typography variant="caption" color="text.secondary" sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                  display: 'block',
                  fontSize: '0.7rem',
                }}>
                  {album.bandName}
                </Typography>
                <Typography variant="caption" color="text.primary" sx={{ fontWeight: 700, fontSize: '0.75rem' }}>
                  {formatPrice(album.price)}
                </Typography>
              </Box>
            </Paper>
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
          {displayAlbums.map((album) => (
            <Box
              key={album.id}
              sx={{
                minWidth: 280,
                maxWidth: 280,
                flexShrink: 0,
                scrollSnapAlign: 'start',
              }}
            >
              <AlbumCard
                album={album}
                collectionStatus={album.id in favoriteIds ? favoriteIds[album.id] : undefined}
                onCollectionChange={(albumId, status) => onCollectionChange(albumId, status)}
                onRemoveFromCollection={(albumId) => onRemoveFromCollection(albumId)}
                isLoggedIn={isLoggedIn}
              />
            </Box>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default NewArrivalsSection;
