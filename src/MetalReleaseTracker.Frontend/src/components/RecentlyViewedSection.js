import React from 'react';
import { Box, Typography, Paper } from '@mui/material';
import { Link } from 'react-router-dom';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';

const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

const RecentlyViewedSection = ({ albums }) => {
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();

  const validAlbums = albums?.filter((album) => album.slug) || [];

  if (validAlbums.length === 0) {
    return null;
  }

  return (
    <Box sx={{ mb: 4 }}>
      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
        {t('recentlyViewed.title')}
      </Typography>
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
        {validAlbums.map((album) => (
          <Paper
            key={album.id}
            component={Link}
            to={`/albums/${album.slug}`}
            sx={{
              minWidth: 160,
              maxWidth: 160,
              flexShrink: 0,
              scrollSnapAlign: 'start',
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
              sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
            />
            <Box sx={{ p: 1.5 }}>
              <Typography variant="subtitle2" sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                fontWeight: 600,
              }}>
                {album.albumName}
              </Typography>
              <Typography variant="caption" color="text.secondary" sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                display: 'block',
              }}>
                {album.bandName}
              </Typography>
              {album.minPrice > 0 && (
                <Typography variant="caption" color="text.primary" sx={{ fontWeight: 600 }}>
                  {formatPrice(album.minPrice)}
                </Typography>
              )}
            </Box>
          </Paper>
        ))}
      </Box>
    </Box>
  );
};

export default RecentlyViewedSection;
