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
  AppBar,
  Toolbar,
  IconButton
} from '@mui/material';
import { 
  Email as EmailIcon, 
  Person as PersonIcon,
  Logout as LogoutIcon,
  AccountCircle as AccountCircleIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import authService from '../services/auth';

const ProfilePage = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  // Get user data directly from local storage
  const userName = localStorage.getItem('user_name') || '';
  const userEmail = localStorage.getItem('user_email') || 'Email not provided';
  const userId = localStorage.getItem('user_id') || 'Not available';
  const loginTimestamp = localStorage.getItem('login_timestamp');

  useEffect(() => {
    const checkAuthentication = async () => {
      try {
        const isLoggedIn = await authService.isLoggedIn();
        if (!isLoggedIn) {
          // If user is not authenticated, redirect to login
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

  // Format date for display
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
  
  // Calculate token expiration time (24 hours from login time)
  const getExpirationTime = (loginTimestamp) => {
    if (!loginTimestamp) return 'Unknown';
    try {
      const expirationTime = parseInt(loginTimestamp) + (24 * 60 * 60 * 1000); // 24 hours
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
          Loading profile...
        </Typography>
      </Container>
    );
  }

  if (!isAuthenticated) {
    return null; // User will be redirected in useEffect
  }
  
  return (
    <>
      <AppBar position="static" color="default" elevation={0}>
        <Toolbar sx={{ justifyContent: 'flex-end' }}>
          <IconButton
            size="large"
            edge="end"
            color="inherit"
            aria-label="profile"
            sx={{ ml: 2 }}
          >
            <AccountCircleIcon />
          </IconButton>
        </Toolbar>
      </AppBar>
      
      <Container maxWidth="md" sx={{ py: 6 }}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Paper elevation={2} sx={{ py: 3, px: 4, mb: 3, borderRadius: 2 }}>
              <Box display="flex" alignItems="center" mb={3}>
                <Avatar 
                  sx={{ width: 80, height: 80, bgcolor: 'primary.main', mr: 3 }}
                  alt={userName}
                >
                  {getInitials(userName)}
                </Avatar>
                <Box>
                  <Typography variant="h5" gutterBottom>
                    {userName}
                  </Typography>
                  <Typography variant="body1" color="textSecondary">
                    {userEmail}
                  </Typography>
                </Box>
              </Box>
              <Divider sx={{ my: 2 }} />
              <Typography variant="h6" gutterBottom>
                Authentication Information
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="subtitle2" color="textSecondary" gutterBottom>
                        Login Time
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
                        Session Valid Until
                      </Typography>
                      <Typography variant="body1">
                        {getExpirationTime(loginTimestamp)}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>
              <Box mt={4} display="flex" justifyContent="flex-end">
                <Button 
                  variant="contained" 
                  color="error" 
                  startIcon={<LogoutIcon />}
                  onClick={handleLogout}
                >
                  Sign Out
                </Button>
              </Box>
            </Paper>
          </Grid>

          <Grid item xs={12}>
            <Paper elevation={2} sx={{ py: 3, px: 4, borderRadius: 2 }}>
              <Typography variant="h6" gutterBottom>
                User Information
              </Typography>
              <List>
                <ListItem>
                  <ListItemIcon>
                    <PersonIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="User ID"
                    secondary={userId}
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <EmailIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="Email"
                    secondary={userEmail}
                  />
                </ListItem>
                <ListItem>
                  <ListItemIcon>
                    <PersonIcon />
                  </ListItemIcon>
                  <ListItemText
                    primary="Username"
                    secondary={userName}
                  />
                </ListItem>
              </List>
            </Paper>
          </Grid>
        </Grid>
      </Container>
    </>
  );
};

export default ProfilePage; 