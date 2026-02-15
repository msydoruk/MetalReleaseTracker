import React, { useState, useEffect } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Box,
  Avatar,
  Container,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Tooltip,
  Chip
} from '@mui/material';
import { 
  Menu as MenuIcon, 
  Person as PersonIcon,
  Home as HomeIcon,
  Album as AlbumIcon, 
  MusicNote as MusicNoteIcon,
  Store as StoreIcon,
  Logout as LogoutIcon,
  AccountCircle as AccountCircleIcon,
  Email as EmailIcon,
  Login as LoginIcon,
  AppRegistration as RegisterIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';
import authService from '../services/auth';

const Header = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [anchorEl, setAnchorEl] = useState(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  
  const navigate = useNavigate();
  
  const checkUserStatus = async () => {
    try {
      const currentUser = await authService.getUser();
      
      setUser(currentUser);
      setLoading(false);
      
      return !!currentUser;
    } catch (error) {
      console.error('Error checking user status:', error);
      setLoading(false);
      return false;
    }
  };
  
  useEffect(() => {
    checkUserStatus();
    
    const intervalId = setInterval(() => {
      checkUserStatus();
    }, 60000);
    
    const handleAuthStateChange = () => {
      console.log('Auth state changed event received');
      checkUserStatus();
    };
    
    const handleStorageChange = (event) => {
      if (event.key === 'auth_token' || event.key === 'user_data') {
        console.log('Auth storage changed, checking authentication status');
        checkUserStatus();
      }
    };
    
    window.addEventListener('auth_state_changed', handleAuthStateChange);
    window.addEventListener('storage', handleStorageChange);
    
    return () => {
      clearInterval(intervalId);
      window.removeEventListener('auth_state_changed', handleAuthStateChange);
      window.removeEventListener('storage', handleStorageChange);
    };
  }, []);
  
  const handleProfileMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };
  
  const handleMenuClose = () => {
    setAnchorEl(null);
  };
  
  const handleLogin = () => {
    navigate('/login');
  };
  
  const handleRegister = () => {
    navigate('/register');
  };
  
  const handleLoginWithGoogle = async () => {
    try {
      await authService.login();
    } catch (error) {
      console.error('Login error:', error);
    }
  };
  
  const handleLogout = async () => {
    try {
      handleMenuClose();
      await authService.logout();
      setUser(null);
    } catch (error) {
      console.error('Logout error:', error);
    }
  };
  
  const toggleDrawer = (open) => (event) => {
    if (event.type === 'keydown' && (event.key === 'Tab' || event.key === 'Shift')) {
      return;
    }
    setDrawerOpen(open);
  };
  
  const navItems = [
    { title: 'Home', path: '/', icon: <HomeIcon /> },
    { title: 'Albums', path: '/albums', icon: <AlbumIcon /> },
    { title: 'Bands', path: '/bands', icon: <MusicNoteIcon /> },
    { title: 'Distributors', path: '/distributors', icon: <StoreIcon /> },
    { title: 'About', path: '/about', icon: <InfoIcon /> }
  ];
  
  const getInitials = (name) => {
    if (!name) return 'U';
    return name.split(' ').map(part => part[0]).join('').toUpperCase();
  };
  
  const getUserEmail = () => {
    if (!user) return '';
    return user.profile?.email || '';
  };
  
  const getUserName = () => {
    if (!user) return 'User';
    return user.profile?.name || 
           user.profile?.given_name || 
           user.profile?.email?.split('@')[0] || 
           'User';
  };
  
  const drawerList = (
    <Box
      sx={{ width: 250 }}
      role="presentation"
      onClick={toggleDrawer(false)}
      onKeyDown={toggleDrawer(false)}
    >
      {user && (
        <>
          <Box sx={{ px: 2, py: 3, bgcolor: 'primary.dark', color: 'white', textAlign: 'center' }}>
            <Avatar 
              sx={{ width: 60, height: 60, mx: 'auto', mb: 1, bgcolor: 'secondary.main' }}
              alt={getUserName()}
            >
              {getInitials(getUserName())}
            </Avatar>
            <Typography variant="subtitle1" component="div" noWrap>
              {getUserName()}
            </Typography>
            <Typography variant="body2" component="div" sx={{ mt: 0.5 }} noWrap>
              {getUserEmail()}
            </Typography>
          </Box>
          <Divider />
        </>
      )}
      
      <List>
        {navItems.map((item) => (
          <ListItemButton
            key={item.title}
            component={Link}
            to={item.path}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primary={item.title} />
          </ListItemButton>
        ))}
      </List>
      
      {user ? (
        <>
          <Divider />
          <List>
            <ListItemButton component={Link} to="/profile">
              <ListItemIcon><AccountCircleIcon /></ListItemIcon>
              <ListItemText primary="Profile" />
            </ListItemButton>
            <ListItemButton onClick={handleLogout}>
              <ListItemIcon><LogoutIcon color="error" /></ListItemIcon>
              <ListItemText primary="Sign Out" primaryTypographyProps={{ color: 'error' }} />
            </ListItemButton>
          </List>
        </>
      ) : (
        <>
          <Divider />
          <List>
            <ListItemButton component={Link} to="/login">
              <ListItemIcon><LoginIcon /></ListItemIcon>
              <ListItemText primary="Log In" />
            </ListItemButton>
            <ListItemButton component={Link} to="/register">
              <ListItemIcon><RegisterIcon /></ListItemIcon>
              <ListItemText primary="Sign Up" />
            </ListItemButton>
          </List>
        </>
      )}
    </Box>
  );
  
  const renderAuthButtons = () => {
    if (user) {
      return (
        <Box sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center' }}>
          <Chip 
            icon={<EmailIcon fontSize="small" />}
            label={getUserEmail()}
            variant="outlined"
            size="small"
            sx={{ 
              mr: 2, 
              color: 'white', 
              borderColor: 'rgba(255,255,255,0.5)',
              display: { xs: 'none', sm: 'flex' } 
            }}
          />
          <Tooltip title="User Profile">
            <Button
              onClick={handleProfileMenuOpen}
              color="inherit"
              startIcon={
                <Avatar 
                  sx={{ width: 32, height: 32, bgcolor: 'secondary.main' }}
                  alt={getUserName()}
                >
                  {getInitials(getUserName())}
                </Avatar>
              }
            >
              {getUserName()}
            </Button>
          </Tooltip>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
            keepMounted
            transformOrigin={{ horizontal: 'right', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
          >
            <MenuItem onClick={() => {
              handleMenuClose();
              navigate('/profile');
            }}>
              <ListItemIcon>
                <AccountCircleIcon fontSize="small" />
              </ListItemIcon>
              <ListItemText>Profile</ListItemText>
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <LogoutIcon fontSize="small" color="error" />
              </ListItemIcon>
              <ListItemText primaryTypographyProps={{ color: 'error' }}>
                Sign Out
              </ListItemText>
            </MenuItem>
          </Menu>
        </Box>
      );
    }
    
    return (
      <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
        <Button 
          color="inherit" 
          startIcon={<LoginIcon />}
          onClick={handleLogin}
          sx={{ mr: 1 }}
        >
          Login
        </Button>
        <Button 
          color="secondary" 
          variant="outlined"
          startIcon={<RegisterIcon />}
          onClick={handleRegister}
        >
          Sign Up
        </Button>
      </Box>
    );
  };
  
  return (
    <>
      <AppBar position="static">
        <Container maxWidth="xl">
          <Toolbar disableGutters>
            {/* Mobile menu button */}
            <IconButton
              size="large"
              edge="start"
              color="inherit"
              aria-label="menu"
              onClick={toggleDrawer(true)}
              sx={{ mr: 2, display: { md: 'none' } }}
            >
              <MenuIcon />
            </IconButton>
            
            {/* Logo and title */}
            <Box sx={{ display: 'flex', alignItems: 'center', mr: 2 }}>
              <Typography
                variant="h6"
                noWrap
                component={Link}
                to="/"
                sx={{
                  display: 'flex',
                  fontFamily: 'monospace',
                  fontWeight: 700,
                  letterSpacing: { xs: 0, sm: '.1rem' },
                  fontSize: { xs: '0.85rem', sm: '1.25rem' },
                  color: 'inherit',
                  textDecoration: 'none',
                }}
              >
                METAL RELEASE TRACKER
              </Typography>
              <Tooltip title="Ukrainian metal releases sold by foreign distributors">
                <Box
                  component="span"
                  sx={{
                    ml: 1.5,
                    fontSize: { xs: '1.2rem', sm: '1.5rem' },
                    lineHeight: 1,
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  {'\uD83C\uDDFA\uD83C\uDDE6'}
                </Box>
              </Tooltip>
            </Box>
            
            {/* Desktop navigation */}
            <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
              {navItems.map((item) => (
                <Button
                  key={item.title}
                  component={Link}
                  to={item.path}
                  sx={{ my: 2, color: 'white', display: 'block' }}
                >
                  {item.title}
                </Button>
              ))}
            </Box>
            
            {/* Auth buttons */}
            {!loading && (
              <Box sx={{ flexGrow: 0, display: 'flex', alignItems: 'center' }}>
                {renderAuthButtons()}
              </Box>
            )}
          </Toolbar>
        </Container>
      </AppBar>
      
      {/* Drawer for mobile navigation */}
      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={toggleDrawer(false)}
      >
        {drawerList}
      </Drawer>
    </>
  );
};

export default Header; 