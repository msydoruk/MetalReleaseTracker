import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  IconButton,
  Badge,
  Popover,
  Box,
  Typography,
  Button,
  Divider,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import InventoryIcon from '@mui/icons-material/Inventory';
import RestoreIcon from '@mui/icons-material/Restore';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import DoneAllIcon from '@mui/icons-material/DoneAll';
import { useNavigate } from 'react-router-dom';
import {
  fetchUnreadNotificationCount,
  fetchNotifications,
  markNotificationRead,
  markAllNotificationsRead,
} from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

const NotificationTypeIcons = {
  PriceDrop: TrendingDownIcon,
  PriceIncrease: TrendingUpIcon,
  BackInStock: InventoryIcon,
  Restock: RestoreIcon,
  NewVariant: NewReleasesIcon,
};

const NotificationTypeColors = {
  PriceDrop: 'success.main',
  PriceIncrease: 'error.main',
  BackInStock: 'info.main',
  Restock: 'warning.main',
  NewVariant: 'secondary.main',
};

const formatTimeAgo = (dateString) => {
  const now = new Date();
  const date = new Date(dateString);
  const diffMs = now - date;
  const diffMinutes = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMinutes < 1) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 30) return `${diffDays}d ago`;
  return date.toLocaleDateString();
};

const NotificationBell = () => {
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [anchorEl, setAnchorEl] = useState(null);
  const navigate = useNavigate();
  const { t } = useLanguage();
  const intervalRef = useRef(null);

  const loadUnreadCount = useCallback(async () => {
    try {
      const response = await fetchUnreadNotificationCount();
      setUnreadCount(response.data);
    } catch {
      // ignore - user might not be authenticated
    }
  }, []);

  useEffect(() => {
    loadUnreadCount();
    intervalRef.current = setInterval(loadUnreadCount, 60000);
    return () => clearInterval(intervalRef.current);
  }, [loadUnreadCount]);

  const handleOpen = async (event) => {
    setAnchorEl(event.currentTarget);
    try {
      const response = await fetchNotifications(1, 10);
      setNotifications(response.data.items || []);
    } catch {
      // ignore
    }
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNotificationClick = async (notification) => {
    if (!notification.isRead) {
      try {
        await markNotificationRead(notification.id);
        setNotifications((previous) =>
          previous.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item))
        );
        setUnreadCount((previous) => Math.max(0, previous - 1));
      } catch {
        // ignore
      }
    }

    handleClose();
    if (notification.albumSlug) {
      navigate(`/albums/${notification.albumSlug}`);
    }
  };

  const handleMarkAllRead = async () => {
    try {
      await markAllNotificationsRead();
      setNotifications((previous) => previous.map((item) => ({ ...item, isRead: true })));
      setUnreadCount(0);
    } catch {
      // ignore
    }
  };

  const open = Boolean(anchorEl);

  return (
    <>
      <IconButton onClick={handleOpen} sx={{ color: 'text.secondary' }}>
        <Badge badgeContent={unreadCount} color="error" max={99}>
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        slotProps={{ paper: { sx: { width: 360, maxHeight: 480 } } }}
      >
        <Box sx={{ p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="subtitle1" fontWeight={700}>
            {t('notifications.title')}
          </Typography>
          {unreadCount > 0 && (
            <Button size="small" startIcon={<DoneAllIcon />} onClick={handleMarkAllRead}>
              {t('notifications.markAllRead')}
            </Button>
          )}
        </Box>
        <Divider />

        {notifications.length === 0 ? (
          <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              {t('notifications.noNotifications')}
            </Typography>
          </Box>
        ) : (
          <List disablePadding sx={{ maxHeight: 380, overflowY: 'auto' }}>
            {notifications.map((notification) => {
              const IconComponent = NotificationTypeIcons[notification.notificationType] || NewReleasesIcon;
              const iconColor = NotificationTypeColors[notification.notificationType] || 'text.secondary';

              return (
                <ListItemButton
                  key={notification.id}
                  onClick={() => handleNotificationClick(notification)}
                  sx={{
                    bgcolor: notification.isRead ? 'transparent' : 'action.hover',
                    borderLeft: notification.isRead ? 'none' : '3px solid',
                    borderColor: iconColor,
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 40 }}>
                    <IconComponent sx={{ color: iconColor, fontSize: 20 }} />
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Typography variant="body2" fontWeight={notification.isRead ? 400 : 600} noWrap>
                        {notification.title}
                      </Typography>
                    }
                    secondary={
                      <Box component="span">
                        <Typography variant="caption" color="text.secondary" noWrap component="span" display="block">
                          {notification.message}
                        </Typography>
                        <Typography variant="caption" color="text.disabled" component="span">
                          {formatTimeAgo(notification.createdDate)}
                        </Typography>
                      </Box>
                    }
                  />
                </ListItemButton>
              );
            })}
          </List>
        )}
      </Popover>
    </>
  );
};

export default NotificationBell;
