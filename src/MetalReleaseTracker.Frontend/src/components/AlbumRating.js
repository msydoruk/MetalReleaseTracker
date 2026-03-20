import React, { useState, useEffect, useCallback } from 'react';
import { Box, Typography, Rating as MuiRating } from '@mui/material';
import StarIcon from '@mui/icons-material/Star';
import { fetchAlbumRating, submitAlbumRating, deleteAlbumRating } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

const AlbumRating = ({ albumId, isLoggedIn }) => {
  const { t } = useLanguage();
  const [averageRating, setAverageRating] = useState(null);
  const [ratingCount, setRatingCount] = useState(0);
  const [userRating, setUserRating] = useState(null);

  const loadRating = useCallback(async () => {
    try {
      const response = await fetchAlbumRating(albumId);
      const data = response.data;
      setAverageRating(data.averageRating);
      setRatingCount(data.ratingCount);
      setUserRating(data.userRating);
    } catch {
      // ignore
    }
  }, [albumId]);

  useEffect(() => {
    loadRating();
  }, [loadRating]);

  const handleRatingChange = async (event, newValue) => {
    if (!isLoggedIn) return;
    try {
      if (newValue === null) {
        await deleteAlbumRating(albumId);
        setUserRating(null);
      } else {
        await submitAlbumRating(albumId, newValue);
        setUserRating(newValue);
      }
      await loadRating();
    } catch {
      // ignore
    }
  };

  const ratingText = ratingCount === 1
    ? t('rating.averageOne')
    : t('rating.average').replace('{count}', ratingCount);

  return (
    <Box sx={{ mt: 2, mb: 1 }}>
      {averageRating != null && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <MuiRating
            value={averageRating}
            precision={0.1}
            readOnly
            size="small"
            emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
          />
          <Typography variant="body2" color="text.secondary">
            {averageRating.toFixed(1)} ({ratingText})
          </Typography>
        </Box>
      )}

      {isLoggedIn ? (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="body2" color="text.secondary">
            {t('rating.yourRating')}:
          </Typography>
          <MuiRating
            value={userRating}
            onChange={handleRatingChange}
            size="medium"
            emptyIcon={<StarIcon style={{ opacity: 0.3 }} fontSize="inherit" />}
          />
        </Box>
      ) : (
        <Typography variant="body2" color="text.secondary">
          {t('rating.loginToRate')}
        </Typography>
      )}
    </Box>
  );
};

export default AlbumRating;
