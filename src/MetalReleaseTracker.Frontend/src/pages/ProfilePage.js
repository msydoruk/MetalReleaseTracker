import React, { useState, useEffect } from 'react';
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
  Tab
} from '@mui/material';
import {
  Email as EmailIcon,
  Person as PersonIcon,
  Logout as LogoutIcon,
  Favorite as FavoriteIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import authService from '../services/auth';
import { fetchFavorites, removeFavorite } from '../services/api';
import AlbumCard from '../components/AlbumCard';
import Pagination from '../components/Pagination';
import { useLanguage } from '../i18n/LanguageContext';

const ProfilePage = () => {
  const { t } = useLanguage();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState(0);
  const [favorites, setFavorites] = useState([]);
  const [favoritesLoading, setFavoritesLoading] = useState(false);
  const [favoritesTotalCount, setFavoritesTotalCount] = useState(0);
  const [favoritesPageCount, setFavoritesPageCount] = useState(0);
  const [favoritesPage, setFavoritesPage] = useState(1);
  const [favoritesPageSize, setFavoritesPageSize] = useState(12);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
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
  }, [navigate]);

  useEffect(() => {
    if (isAuthenticated && activeTab === 0) {
      loadFavorites();
    }
  }, [isAuthenticated, activeTab, favoritesPage, favoritesPageSize]);

  const loadFavorites = async () => {
    try {
      setFavoritesLoading(true);
      const response = await fetchFavorites(favoritesPage, favoritesPageSize);
      const data = response.data;
      setFavorites(data.items || []);
      setFavoritesTotalCount(data.totalCount || 0);
      setFavoritesPageCount(data.pageCount || 0);
      setFavoriteIds(new Set((data.items || []).map((album) => album.id)));
    } catch (error) {
      console.error('Error loading favorites:', error);
    } finally {
      setFavoritesLoading(false);
    }
  };

  const handleToggleFavorite = async (albumId) => {
    try {
      await removeFavorite(albumId);
      setFavorites((previous) => previous.filter((album) => album.id !== albumId));
      setFavoritesTotalCount((previous) => previous - 1);
      setFavoriteIds((previous) => {
        const next = new Set(previous);
        next.delete(albumId);
        return next;
      });
    } catch (error) {
      console.error('Error removing favorite:', error);
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
                          isFavorited={favoriteIds.has(album.id)}
                          onToggleFavorite={handleToggleFavorite}
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
                  <FavoriteIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
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
    </Container>
  );
};

export default ProfilePage;
