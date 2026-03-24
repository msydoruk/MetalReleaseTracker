import React, { useState, useEffect, useCallback } from 'react';
import { IconButton, Tooltip } from '@mui/material';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone';
import { useNavigate } from 'react-router-dom';
import { watchAlbum, unwatchAlbum, checkWatchingAlbum } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

const WatchButton = ({ albumId, isLoggedIn, size = 'medium' }) => {
  const [watching, setWatching] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { t } = useLanguage();

  const checkStatus = useCallback(async () => {
    if (!isLoggedIn || !albumId) return;
    try {
      const response = await checkWatchingAlbum(albumId);
      setWatching(response.data);
    } catch {
      // ignore
    }
  }, [albumId, isLoggedIn]);

  useEffect(() => {
    checkStatus();
  }, [checkStatus]);

  const handleToggle = async (event) => {
    event.stopPropagation();
    event.preventDefault();

    if (!isLoggedIn) {
      navigate('/login');
      return;
    }

    if (loading) return;
    setLoading(true);

    try {
      if (watching) {
        await unwatchAlbum(albumId);
        setWatching(false);
      } else {
        await watchAlbum(albumId);
        setWatching(true);
      }
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  };

  return (
    <Tooltip title={watching ? t('watch.unwatchTooltip') : t('watch.watchTooltip')}>
      <IconButton
        onClick={handleToggle}
        disabled={loading}
        size={size}
        sx={{
          color: watching ? 'warning.main' : 'text.secondary',
          '&:hover': { color: 'warning.main' },
        }}
      >
        {watching ? <NotificationsActiveIcon /> : <NotificationsNoneIcon />}
      </IconButton>
    </Tooltip>
  );
};

export default WatchButton;
