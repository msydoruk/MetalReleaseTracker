import React, { useState, useEffect, useMemo } from 'react';
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
  Tooltip
} from '@mui/material';
import NotificationBell from './NotificationBell';
import {
  Menu as MenuIcon,
  Home as HomeIcon,
  Album as AlbumIcon,
  MusicNote as MusicNoteIcon,
  Store as StoreIcon,
  Logout as LogoutIcon,
  AccountCircle as AccountCircleIcon,
  Login as LoginIcon,
  AppRegistration as RegisterIcon,
  Info as InfoIcon,
  Newspaper as NewspaperIcon,
  Language as LanguageIcon,
  History as HistoryIcon,
  CalendarMonth as CalendarMonthIcon,
  RateReview as RateReviewIcon,
  Brightness4 as Brightness4Icon,
  Brightness7 as Brightness7Icon,
  CurrencyExchange as CurrencyExchangeIcon
} from '@mui/icons-material';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import authService from '../services/auth';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';
import { useNavigation } from '../contexts/NavigationContext';
import { useSeoConfig } from '../contexts/SeoContext';
import { useThemeMode } from '../contexts/ThemeContext';
import HeaderSearch from './HeaderSearch';

const ICON_MAP = {
  HomeIcon: HomeIcon,
  AlbumIcon: AlbumIcon,
  MusicNoteIcon: MusicNoteIcon,
  StoreIcon: StoreIcon,
  CalendarMonthIcon: CalendarMonthIcon,
  NewspaperIcon: NewspaperIcon,
  InfoIcon: InfoIcon,
  HistoryIcon: HistoryIcon,
  RateReviewIcon: RateReviewIcon,
};

const Header = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [anchorEl, setAnchorEl] = useState(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const [currencyAnchorEl, setCurrencyAnchorEl] = useState(null);

  const navigate = useNavigate();
  const location = useLocation();
  const { language, changeLanguage, availableLanguages, t } = useLanguage();
  const [languageAnchorEl, setLanguageAnchorEl] = useState(null);
  const { currency, changeCurrency, availableCurrencies } = useCurrency();
  const { navItems: apiNavItems } = useNavigation();
  const seoConfig = useSeoConfig();
  const { themeMode, toggleThemeMode } = useThemeMode();

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

  const navItems = useMemo(() => {
    return apiNavItems
      .filter((item) => !item.isProtected)
      .map((item) => {
        const IconComponent = ICON_MAP[item.iconName];
        return {
          title: item.title,
          path: item.path,
          icon: IconComponent ? <IconComponent /> : <HomeIcon />,
        };
      });
  }, [apiNavItems]);

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

  const getInitials = (name) => {
    if (!name) return 'U';
    return name.split(' ').map(part => part[0]).join('').toUpperCase();
  };

  const getUserName = () => {
    if (!user) return 'User';
    return user.claims?.username ||
           user.claims?.email?.split('@')[0] ||
           'User';
  };

  const handleDrawerNavClick = () => {
    setDrawerOpen(false);
  };

  const drawerList = (
    <Box
      sx={{ width: 250 }}
      role="presentation"
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
          </Box>
          <Divider />
        </>
      )}

      <List>
        {navItems.map((item) => {
          const isActive = item.path === '/'
            ? location.pathname === '/'
            : location.pathname.startsWith(item.path);
          return (
            <ListItemButton
              key={item.title}
              component={Link}
              to={item.path}
              selected={isActive}
              onClick={handleDrawerNavClick}
              sx={{
                borderLeft: isActive ? '3px solid' : '3px solid transparent',
                borderColor: isActive ? 'primary.main' : 'transparent',
                '&.Mui-selected': {
                  bgcolor: 'rgba(144, 202, 249, 0.08)'
                },
                '&.Mui-selected:hover': {
                  bgcolor: 'rgba(144, 202, 249, 0.12)'
                }
              }}
            >
              <ListItemIcon sx={{ color: isActive ? 'primary.main' : 'inherit' }}>
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.title}
                primaryTypographyProps={{
                  fontWeight: isActive ? 600 : 400,
                  color: isActive ? 'primary.main' : 'inherit'
                }}
              />
            </ListItemButton>
          );
        })}
      </List>

      <Divider />
      <List>
        <ListItemButton onClick={() => { toggleThemeMode(); handleDrawerNavClick(); }}>
          <ListItemIcon>
            {themeMode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
          </ListItemIcon>
          <ListItemText primary={themeMode === 'dark' ? 'Light mode' : 'Dark mode'} />
        </ListItemButton>
        <ListItemButton onClick={(event) => { event.stopPropagation(); setCurrencyAnchorEl(event.currentTarget); }}>
          <ListItemIcon><CurrencyExchangeIcon /></ListItemIcon>
          <ListItemText primary={`Currency: ${currency}`} />
        </ListItemButton>
        <ListItemButton onClick={(event) => { event.stopPropagation(); setLanguageAnchorEl(event.currentTarget); }}>
          <ListItemIcon><LanguageIcon /></ListItemIcon>
          <ListItemText primary={`Language: ${language.toUpperCase()}`} />
        </ListItemButton>
      </List>

      <Divider />
      <List>
        {user ? (
          <>
            <ListItemButton component={Link} to="/profile" onClick={handleDrawerNavClick}>
              <ListItemIcon><AccountCircleIcon /></ListItemIcon>
              <ListItemText primary={t('nav.profile')} />
            </ListItemButton>
            <ListItemButton onClick={() => { handleLogout(); handleDrawerNavClick(); }}>
              <ListItemIcon><LogoutIcon color="error" /></ListItemIcon>
              <ListItemText primary={t('nav.signOut')} primaryTypographyProps={{ color: 'error' }} />
            </ListItemButton>
          </>
        ) : (
          <>
            <ListItemButton component={Link} to="/login" onClick={handleDrawerNavClick}>
              <ListItemIcon><LoginIcon /></ListItemIcon>
              <ListItemText primary={t('nav.login')} />
            </ListItemButton>
            <ListItemButton component={Link} to="/register" onClick={handleDrawerNavClick}>
              <ListItemIcon><RegisterIcon /></ListItemIcon>
              <ListItemText primary={t('nav.signUp')} />
            </ListItemButton>
          </>
        )}
      </List>
    </Box>
  );

  const renderAuthButtons = () => {
    if (user) {
      return (
        <Box sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center' }}>
          <NotificationBell />
          <Tooltip title={t('nav.profile')}>
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
              <ListItemText>{t('nav.profile')}</ListItemText>
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleLogout}>
              <ListItemIcon>
                <LogoutIcon fontSize="small" color="error" />
              </ListItemIcon>
              <ListItemText primaryTypographyProps={{ color: 'error' }}>
                {t('nav.signOut')}
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
          {t('nav.login')}
        </Button>
        <Button
          color="secondary"
          variant="outlined"
          startIcon={<RegisterIcon />}
          onClick={handleRegister}
        >
          {t('nav.signUp')}
        </Button>
      </Box>
    );
  };

  return (
    <>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
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
            <Box sx={{ display: 'flex', alignItems: 'center', mr: 2, flexGrow: { xs: 1, md: 0 } }}>
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
                {(seoConfig.SiteName || 'METAL RELEASE TRACKER').toUpperCase()}
              </Typography>
              <Tooltip title={t('header.flagTooltip')}>
                <Box
                  component="span"
                  sx={{
                    ml: 1.5,
                    lineHeight: 1,
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  <svg width="24" height="16" viewBox="0 0 24 16" style={{ borderRadius: 2 }}>
                    <rect width="24" height="8" fill="#005BBB" />
                    <rect y="8" width="24" height="8" fill="#FFD500" />
                  </svg>
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

            {/* Search + Currency + Language toggle + Auth buttons */}
            {!loading && (
              <Box sx={{ flexGrow: 0, display: 'flex', alignItems: 'center' }}>
                <HeaderSearch />
                <Tooltip title={t('header.currencyTooltip')}>
                  <Button
                    color="inherit"
                    onClick={(event) => setCurrencyAnchorEl(event.currentTarget)}
                    sx={{ minWidth: 'auto', px: 1, mr: 0.5, fontSize: '0.85rem', display: { xs: 'none', md: 'inline-flex' } }}
                  >
                    {currency}
                  </Button>
                </Tooltip>
                <Menu
                  anchorEl={currencyAnchorEl}
                  open={Boolean(currencyAnchorEl)}
                  onClose={() => setCurrencyAnchorEl(null)}
                  keepMounted
                  transformOrigin={{ horizontal: 'center', vertical: 'top' }}
                  anchorOrigin={{ horizontal: 'center', vertical: 'bottom' }}
                >
                  {availableCurrencies.map((option) => (
                    <MenuItem
                      key={option}
                      selected={option === currency}
                      onClick={() => {
                        changeCurrency(option);
                        setCurrencyAnchorEl(null);
                      }}
                    >
                      {option}
                    </MenuItem>
                  ))}
                </Menu>
                <Tooltip title={t('header.themeTooltip') || (themeMode === 'dark' ? 'Light mode' : 'Dark mode')}>
                  <IconButton
                    color="inherit"
                    onClick={toggleThemeMode}
                    sx={{ mr: 0.5, display: { xs: 'none', md: 'inline-flex' } }}
                  >
                    {themeMode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
                  </IconButton>
                </Tooltip>
                <Tooltip title={t('header.languageTooltip')}>
                  <Button
                    color="inherit"
                    onClick={(event) => setLanguageAnchorEl(event.currentTarget)}
                    sx={{ minWidth: 'auto', px: 1, mr: 1, display: { xs: 'none', md: 'inline-flex' } }}
                    startIcon={<LanguageIcon />}
                  >
                    {language.toUpperCase()}
                  </Button>
                </Tooltip>
                <Menu
                  anchorEl={languageAnchorEl}
                  open={Boolean(languageAnchorEl)}
                  onClose={() => setLanguageAnchorEl(null)}
                  keepMounted
                  transformOrigin={{ horizontal: 'center', vertical: 'top' }}
                  anchorOrigin={{ horizontal: 'center', vertical: 'bottom' }}
                >
                  {availableLanguages.map((lang) => (
                    <MenuItem
                      key={lang.code}
                      selected={lang.code === language}
                      onClick={() => {
                        changeLanguage(lang.code);
                        setLanguageAnchorEl(null);
                      }}
                    >
                      {lang.nativeName}
                    </MenuItem>
                  ))}
                </Menu>
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
