import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Paper,
  Typography,
  Avatar,
  Box,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Button,
  Grid,
  Card,
  CardContent,
  CircularProgress,
  Tabs,
  Tab,
  TextField,
  Alert
} from '@mui/material';
import {
  Email as EmailIcon,
  Person as PersonIcon,
  Logout as LogoutIcon,
  Favorite as FavoriteIcon,
  Bookmark as BookmarkIcon,
  CheckCircle as CheckCircleIcon,
  FileDownload as FileDownloadIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import authService from '../services/auth';
import { fetchFavorites, removeFavorite, addFavorite, updateFavoriteStatus, exportCollection, generateTelegramToken, getTelegramStatus, unlinkTelegram, subscribeEmail, unsubscribeEmail, getEmailStatus } from '../services/api';
import TelegramIcon from '@mui/icons-material/Telegram';
import LinkOffIcon from '@mui/icons-material/LinkOff';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import AlbumCard from '../components/AlbumCard';
import Pagination from '../components/Pagination';
import { useLanguage } from '../i18n/LanguageContext';

const COLLECTION_TABS = [
  { status: null, icon: <FavoriteIcon />, translationKey: 'profile.favorites' },
  { status: 1, icon: <BookmarkIcon />, translationKey: 'profile.wishlist' },
  { status: 2, icon: <CheckCircleIcon />, translationKey: 'profile.collection' },
];

const ProfilePage = () => {
  const { t } = useLanguage();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState(0);
  const [collectionFilter, setCollectionFilter] = useState(null);
  const [favorites, setFavorites] = useState([]);
  const [favoritesLoading, setFavoritesLoading] = useState(false);
  const [favoritesTotalCount, setFavoritesTotalCount] = useState(0);
  const [favoritesPageCount, setFavoritesPageCount] = useState(0);
  const [favoritesPage, setFavoritesPage] = useState(1);
  const [favoritesPageSize, setFavoritesPageSize] = useState(12);
  const [favoriteIds, setFavoriteIds] = useState({});
  const [telegramLinked, setTelegramLinked] = useState(false);
  const [telegramToken, setTelegramToken] = useState(null);
  const [telegramBotUsername, setTelegramBotUsername] = useState(null);
  const [emailStatus, setEmailStatus] = useState(null);
  const [emailInput, setEmailInput] = useState('');
  const [emailLoading, setEmailLoading] = useState(false);
  const [emailMessage, setEmailMessage] = useState(null);
  const navigate = useNavigate();

  const userName = localStorage.getItem('user_name') || '';
  const userEmail = localStorage.getItem('user_email') || t('profile.emailNotProvided');
  const userId = localStorage.getItem('user_id') || 'Not available';
  const loginTimestamp = localStorage.getItem('login_timestamp');

  useEffect(() => {
    const checkAuthentication = async () => {
      try {
        const isLoggedIn = await authService.isLoggedIn();
        if (!isLoggedIn) {
          navigate('/login');
          return;
        }
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Error checking authentication:', error);
        navigate('/login');
      } finally {
        setLoading(false);
      }
    };

    checkAuthentication();

    const checkTelegramStatus = async () => {
      try {
        const response = await getTelegramStatus();
        setTelegramLinked(response.data.isLinked);
      } catch {
        // ignore
      }
    };

    const checkEmailStatus = async () => {
      try {
        const response = await getEmailStatus();
        setEmailStatus(response.data);
      } catch {
        // ignore
      }
    };

    checkTelegramStatus();
    checkEmailStatus();
  }, [navigate]);

  const handleGenerateTelegramToken = async () => {
    try {
      const response = await generateTelegramToken();
      setTelegramToken(response.data.token);
      setTelegramBotUsername(response.data.botUsername);
    } catch {
      // ignore
    }
  };

  const handleUnlinkTelegram = async () => {
    try {
      await unlinkTelegram();
      setTelegramLinked(false);
      setTelegramToken(null);
    } catch {
      // ignore
    }
  };

  const handleEmailSubscribe = async () => {
    setEmailLoading(true);
    setEmailMessage(null);
    try {
      await subscribeEmail(emailInput);
      const { data } = await getEmailStatus();
      setEmailStatus(data);
      setEmailInput('');
      setEmailMessage({ severity: 'success', text: t('email.subscribeSuccess') });
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data || t('email.subscribeError');
      setEmailMessage({ severity: 'error', text: message });
    } finally {
      setEmailLoading(false);
    }
  };

  const handleEmailResend = async () => {
    setEmailLoading(true);
    setEmailMessage(null);
    try {
      await subscribeEmail(emailStatus?.email);
      setEmailMessage({ severity: 'success', text: t('email.resendSuccess') });
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data || t('email.resendError');
      setEmailMessage({ severity: 'error', text: message });
    } finally {
      setEmailLoading(false);
    }
  };

  const handleEmailUnsubscribe = async () => {
    setEmailLoading(true);
    setEmailMessage(null);
    try {
      await unsubscribeEmail();
      setEmailStatus({ isSubscribed: false, isVerified: false, email: null });
      setEmailMessage({ severity: 'success', text: t('email.unsubscribeSuccess') });
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data || t('email.unsubscribeError');
      setEmailMessage({ severity: 'error', text: message });
    } finally {
      setEmailLoading(false);
    }
  };

  const loadFavorites = useCallback(async () => {
    try {
      setFavoritesLoading(true);
      const response = await fetchFavorites(favoritesPage, favoritesPageSize, collectionFilter);
      const data = response.data;
      setFavorites(data.items || []);
      setFavoritesTotalCount(data.totalCount || 0);
      setFavoritesPageCount(data.pageCount || 0);

      const ids = {};
      (data.items || []).forEach((album) => {
        ids[album.id] = collectionFilter !== null ? collectionFilter : 0;
      });
      setFavoriteIds(ids);
    } catch (error) {
      console.error('Error loading favorites:', error);
    } finally {
      setFavoritesLoading(false);
    }
  }, [favoritesPage, favoritesPageSize, collectionFilter]);

  useEffect(() => {
    if (isAuthenticated && activeTab === 0) {
      loadFavorites();
    }
  }, [isAuthenticated, activeTab, favoritesPage, favoritesPageSize, collectionFilter, loadFavorites]);

  const handleCollectionChange = async (albumId, status) => {
    try {
      if (albumId in favoriteIds) {
        await updateFavoriteStatus(albumId, status);
      } else {
        await addFavorite(albumId, status);
      }
      loadFavorites();
    } catch (error) {
      console.error('Error updating collection:', error);
    }
  };

  const handleRemoveFromCollection = async (albumId) => {
    try {
      await removeFavorite(albumId);
      setFavorites((previous) => previous.filter((album) => album.id !== albumId));
      setFavoritesTotalCount((previous) => previous - 1);
      setFavoriteIds((previous) => {
        const next = { ...previous };
        delete next[albumId];
        return next;
      });
    } catch (error) {
      console.error('Error removing from collection:', error);
    }
  };

  const handleExportCollection = async () => {
    try {
      const response = await exportCollection('csv');
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `collection-${new Date().toISOString().slice(0, 10)}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting collection:', error);
    }
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
      navigate('/');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  const getInitials = (name) => {
    if (!name) return '';
    return name.split(' ').map(part => part[0]).join('').toUpperCase();
  };

  const getFormattedDate = (timestamp) => {
    if (!timestamp) return 'Unknown';
    try {
      return new Date(parseInt(timestamp)).toLocaleString('en-US', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch (e) {
      return 'Invalid date';
    }
  };

  const getExpirationTime = (loginTimestamp) => {
    if (!loginTimestamp) return 'Unknown';
    try {
      const expirationTime = parseInt(loginTimestamp) + (24 * 60 * 60 * 1000);
      return getFormattedDate(expirationTime);
    } catch (e) {
      return 'Unknown';
    }
  };

  const getEmptyIcon = () => {
    if (collectionFilter === 1) return <BookmarkIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />;
    if (collectionFilter === 2) return <CheckCircleIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />;
    return <FavoriteIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />;
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ pt: 8, textAlign: 'center' }}>
        <CircularProgress />
        <Typography variant="h6" mt={2}>
          {t('profile.loading')}
        </Typography>
      </Container>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <Avatar
          sx={{ width: 56, height: 56, bgcolor: 'primary.main', mr: 2 }}
          alt={userName}
        >
          {getInitials(userName)}
        </Avatar>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            {userName}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {userEmail}
          </Typography>
        </Box>
      </Box>

      <Paper elevation={2} sx={{ borderRadius: 2 }}>
        <Tabs
          value={activeTab}
          onChange={(event, newValue) => setActiveTab(newValue)}
          sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
        >
          <Tab
            icon={<FavoriteIcon />}
            iconPosition="start"
            label={t('profile.favorites')}
          />
          <Tab
            icon={<PersonIcon />}
            iconPosition="start"
            label={t('profile.profileTab')}
          />
        </Tabs>

        <Box sx={{ p: 3 }}>
          {activeTab === 0 && (
            <>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
                <Tabs
                  value={COLLECTION_TABS.findIndex((tab) => tab.status === collectionFilter)}
                  onChange={(event, newValue) => {
                    setCollectionFilter(COLLECTION_TABS[newValue].status);
                    setFavoritesPage(1);
                  }}
                  variant="scrollable"
                  scrollButtons="auto"
                  sx={{ minHeight: 40, '& .MuiTab-root': { minHeight: 40, py: 0.5 } }}
                >
                  {COLLECTION_TABS.map((tab) => (
                    <Tab
                      key={tab.translationKey}
                      icon={tab.icon}
                      iconPosition="start"
                      label={t(tab.translationKey)}
                    />
                  ))}
                </Tabs>
                <Button
                  variant="outlined"
                  size="small"
                  startIcon={<FileDownloadIcon />}
                  onClick={handleExportCollection}
                  disabled={favoritesLoading}
                  sx={{ ml: 2, whiteSpace: 'nowrap' }}
                >
                  {t('profile.exportCsv')}
                </Button>
              </Box>

              {favoritesLoading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                  <CircularProgress />
                </Box>
              ) : favorites.length > 0 ? (
                <>
                  <Grid
                    container
                    spacing={3}
                    sx={{
                      display: 'grid',
                      gridTemplateColumns: {
                        xs: 'repeat(1, 1fr)',
                        sm: 'repeat(2, 1fr)',
                        md: 'repeat(3, 1fr)',
                        lg: 'repeat(4, 1fr)'
                      },
                      gap: 3,
                      alignItems: 'stretch'
                    }}
                  >
                    {favorites.map((album) => (
                      <Box key={album.id} sx={{ display: 'flex', height: '100%' }}>
                        <AlbumCard
                          album={album}
                          collectionStatus={album.id in favoriteIds ? favoriteIds[album.id] : undefined}
                          onCollectionChange={(albumId, status) => handleCollectionChange(albumId, status)}
                          onRemoveFromCollection={(albumId) => handleRemoveFromCollection(albumId)}
                          isLoggedIn={true}
                        />
                      </Box>
                    ))}
                  </Grid>

                  {favoritesPageCount > 1 && (
                    <Box sx={{ mt: 3 }}>
                      <Pagination
                        currentPage={favoritesPage}
                        totalPages={favoritesPageCount}
                        totalItems={favoritesTotalCount}
                        pageSize={favoritesPageSize}
                        onPageChange={setFavoritesPage}
                        onPageSizeChange={(newSize) => {
                          setFavoritesPageSize(newSize);
                          setFavoritesPage(1);
                        }}
                      />
                    </Box>
                  )}
                </>
              ) : (
                <Box sx={{ textAlign: 'center', py: 6 }}>
                  {getEmptyIcon()}
                  <Typography variant="h6" color="text.secondary">
                    {t('favorites.empty')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    {t('favorites.emptyHint')}
                  </Typography>
                </Box>
              )}
            </>
          )}

          {activeTab === 1 && (
            <Grid container spacing={3}>
              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>
                  {t('profile.authInfo')}
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          {t('profile.loginTime')}
                        </Typography>
                        <Typography variant="body1">
                          {getFormattedDate(loginTimestamp)}
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                          {t('profile.sessionValidUntil')}
                        </Typography>
                        <Typography variant="body1">
                          {getExpirationTime(loginTimestamp)}
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
                <Box mt={3} display="flex" justifyContent="flex-end">
                  <Button
                    variant="contained"
                    color="error"
                    startIcon={<LogoutIcon />}
                    onClick={handleLogout}
                  >
                    {t('profile.signOut')}
                  </Button>
                </Box>
              </Grid>

              <Grid item xs={12}>
                <Divider sx={{ my: 1 }} />
                <Typography variant="h6" gutterBottom>
                  {t('profile.userInfo')}
                </Typography>
                <List>
                  <ListItem>
                    <ListItemIcon>
                      <PersonIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary={t('profile.userId')}
                      secondary={userId}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon>
                      <EmailIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary={t('profile.email')}
                      secondary={userEmail}
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemIcon>
                      <PersonIcon />
                    </ListItemIcon>
                    <ListItemText
                      primary={t('profile.username')}
                      secondary={userName}
                    />
                  </ListItem>
                </List>
              </Grid>
            </Grid>
          )}
        </Box>
      </Paper>

      <Paper elevation={2} sx={{ borderRadius: 2, mt: 3, p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <TelegramIcon color="info" />
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {t('telegram.title')}
          </Typography>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {t('telegram.description')}
        </Typography>

        {telegramLinked ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="body2" color="success.main" sx={{ fontWeight: 600 }}>
              {t('telegram.linked')}
            </Typography>
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<LinkOffIcon />}
              onClick={handleUnlinkTelegram}
            >
              {t('telegram.unlink')}
            </Button>
          </Box>
        ) : telegramToken ? (
          <Box>
            <Typography variant="body2" sx={{ mb: 1 }}>
              {t('telegram.sendCommand')}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Paper variant="outlined" sx={{ px: 2, py: 1, fontFamily: 'monospace', fontSize: '0.9rem' }}>
                /start {telegramToken}
              </Paper>
              <Button
                size="small"
                startIcon={<ContentCopyIcon />}
                onClick={() => navigator.clipboard.writeText(`/start ${telegramToken}`)}
              >
                {t('telegram.copy')}
              </Button>
            </Box>
            <Button
              variant="contained"
              startIcon={<TelegramIcon />}
              href={`https://t.me/${telegramBotUsername}?start=${telegramToken}`}
              target="_blank"
              rel="noopener noreferrer"
            >
              {t('telegram.openBot')}
            </Button>
          </Box>
        ) : (
          <Button
            variant="contained"
            startIcon={<TelegramIcon />}
            onClick={handleGenerateTelegramToken}
          >
            {t('telegram.linkButton')}
          </Button>
        )}
      </Paper>

      <Paper elevation={2} sx={{ borderRadius: 2, mt: 3, p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <EmailIcon color="info" />
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            {t('email.title')}
          </Typography>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {t('email.description')}
        </Typography>

        {emailMessage && (
          <Alert severity={emailMessage.severity} sx={{ mb: 2 }} onClose={() => setEmailMessage(null)}>
            {emailMessage.text}
          </Alert>
        )}

        {emailStatus?.isSubscribed && emailStatus?.isVerified ? (
          <Box>
            <Alert severity="success" sx={{ mb: 2 }}>
              {t('email.activeFor')} {emailStatus.email}
            </Alert>
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<LinkOffIcon />}
              onClick={handleEmailUnsubscribe}
              disabled={emailLoading}
            >
              {t('email.unsubscribe')}
            </Button>
          </Box>
        ) : emailStatus?.isSubscribed && !emailStatus?.isVerified ? (
          <Box>
            <Alert severity="info" sx={{ mb: 2 }}>
              {t('email.verificationSent')} {emailStatus.email}. {t('email.checkInbox')}
            </Alert>
            <Button
              variant="outlined"
              size="small"
              onClick={handleEmailResend}
              disabled={emailLoading}
            >
              {t('email.resend')}
            </Button>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
            <TextField
              size="small"
              label={t('email.inputLabel')}
              type="email"
              value={emailInput}
              onChange={(event) => setEmailInput(event.target.value)}
              disabled={emailLoading}
              sx={{ minWidth: 280 }}
            />
            <Button
              variant="contained"
              onClick={handleEmailSubscribe}
              disabled={emailLoading || !emailInput.trim()}
            >
              {t('email.subscribe')}
            </Button>
          </Box>
        )}
      </Paper>
    </Container>
  );
};

export default ProfilePage;
