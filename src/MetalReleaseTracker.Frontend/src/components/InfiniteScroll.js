import React, { useRef, useEffect, useCallback } from 'react';
import { Box, CircularProgress, Typography, LinearProgress } from '@mui/material';
import { useLanguage } from '../i18n/LanguageContext';

const InfiniteScroll = ({ onLoadMore, hasMore, loading, loadedCount, totalCount, children }) => {
  const { t } = useLanguage();
  const sentinelRef = useRef(null);

  const handleIntersect = useCallback((entries) => {
    const entry = entries[0];
    if (entry.isIntersecting && hasMore && !loading) {
      onLoadMore();
    }
  }, [hasMore, loading, onLoadMore]);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(handleIntersect, {
      rootMargin: '400px',
    });

    observer.observe(sentinel);

    return () => observer.disconnect();
  }, [handleIntersect]);

  const showProgress = totalCount > 0 && loadedCount > 0;
  const progressPercent = showProgress ? Math.round((loadedCount / totalCount) * 100) : 0;

  const formatText = (key, replacements) => {
    let text = t(key);
    if (text === key) return null;
    Object.entries(replacements).forEach(([token, value]) => {
      text = text.replace(`{${token}}`, value);
    });
    return text;
  };

  return (
    <Box>
      {children}
      <Box ref={sentinelRef} sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 3, gap: 1.5 }}>
        {loading && (
          <>
            <CircularProgress size={32} />
            {showProgress && (
              <Typography variant="body2" color="text.secondary">
                {formatText('pagination.showingOf', { loaded: loadedCount, total: totalCount })
                  || `${loadedCount} / ${totalCount}`}
              </Typography>
            )}
          </>
        )}
        {!loading && hasMore && showProgress && (
          <Box sx={{ width: '100%', maxWidth: 300, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
              {formatText('pagination.showingOf', { loaded: loadedCount, total: totalCount })
                || `${loadedCount} / ${totalCount}`}
            </Typography>
            <LinearProgress
              variant="determinate"
              value={progressPercent}
              sx={{
                height: 4,
                borderRadius: 2,
                backgroundColor: 'rgba(255,255,255,0.08)',
                '& .MuiLinearProgress-bar': {
                  borderRadius: 2,
                },
              }}
            />
          </Box>
        )}
        {!loading && !hasMore && showProgress && (
          <Typography variant="body2" color="text.secondary">
            {formatText('pagination.allLoaded', { total: totalCount })
              || `${totalCount} / ${totalCount}`}
          </Typography>
        )}
        {!loading && !hasMore && !showProgress && (
          <Typography variant="body2" color="text.secondary">
            {t('pagination.endOfList') !== 'pagination.endOfList'
              ? t('pagination.endOfList')
              : 'No more items to load'}
          </Typography>
        )}
      </Box>
    </Box>
  );
};

export default InfiniteScroll;
