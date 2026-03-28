import React, { useRef, useEffect, useCallback } from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { useLanguage } from '../i18n/LanguageContext';

const InfiniteScroll = ({ onLoadMore, hasMore, loading, children }) => {
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

  return (
    <Box>
      {children}
      <Box ref={sentinelRef} sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
        {loading && <CircularProgress size={32} />}
        {!loading && !hasMore && (
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
