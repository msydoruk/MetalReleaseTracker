import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline, CircularProgress, Box } from '@mui/material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import Header from './components/Header';
import AlbumsPage from './pages/AlbumsPage';
import LoginCallback from './pages/LoginCallback';
import GoogleCallback from './pages/GoogleCallback';
import ProfilePage from './pages/ProfilePage';
import BandsPage from './pages/BandsPage';
import DistributorsPage from './pages/DistributorsPage';
import AboutPage from './pages/AboutPage';
import AboutUaPage from './pages/AboutUaPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import authService from './services/auth';

// Create a dark theme for the metal music theme
const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#b71c1c', // Deep red
    },
    secondary: {
      main: '#9e9e9e', // Silver/metallic
    },
    background: {
      default: '#121212',
      paper: '#1e1e1e',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontWeight: 700,
    },
    h2: {
      fontWeight: 600,
    },
    h3: {
      fontWeight: 600,
    },
    h4: {
      fontWeight: 600,
    },
    h5: {
      fontWeight: 500,
    },
    h6: {
      fontWeight: 500,
    },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#000000',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          borderRadius: 4,
        },
        contained: {
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.25)',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundColor: '#1e1e1e',
          borderRadius: 8,
        },
      },
    },
  },
});

// Protected route component
const ProtectedRoute = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(null);
  const location = useLocation();

  useEffect(() => {
    const checkAuth = async () => {
      const loggedIn = await authService.isLoggedIn();
      setIsAuthenticated(loggedIn);
    };

    checkAuth();
  }, []);

  if (isAuthenticated === null) {
    // Show loading spinner while checking authentication
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    // Redirect to login page if not authenticated
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Show protected content if authenticated
  return children;
};

function App() {
  useEffect(() => {
    const checkInitialAuthStatus = async () => {
      try {
        console.log('App: Checking initial auth status...');
        const isLoggedIn = await authService.isLoggedIn();
        console.log('App: Initial auth check complete', { isLoggedIn });
        
        // Trigger auth event to make sure all components are in sync
        if (isLoggedIn) {
          authService.triggerAuthUpdate();
        }
      } catch (error) {
        console.error('App: Error checking initial auth status', error);
      }
    };
    
    checkInitialAuthStatus();
    
    // Listen for auth errors that might happen during the lifetime of the app
    const handleAuthError = (event) => {
      if (event.detail?.type === 'auth_error') {
        console.error('App: Auth error event received', event.detail);
      }
    };
    
    window.addEventListener('auth_state_changed', handleAuthError);
    
    return () => {
      window.removeEventListener('auth_state_changed', handleAuthError);
    };
  }, []);
  
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <Router>
          <Header />
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/auth/callback" element={<GoogleCallback />} />
            
            {/* Public catalog routes */}
            <Route path="/" element={<AlbumsPage isHome />} />
            <Route path="/albums" element={<AlbumsPage />} />
            <Route path="/bands" element={<BandsPage />} />
            <Route path="/distributors" element={<DistributorsPage />} />
            <Route path="/about" element={<AboutPage />} />
            <Route path="/about/ua" element={<AboutUaPage />} />

            {/* Protected routes */}
            <Route
              path="/profile"
              element={
                <ProtectedRoute>
                  <ProfilePage />
                </ProtectedRoute>
              }
            />
            <Route path="/signin-callback" element={<LoginCallback />} />
          </Routes>
        </Router>
      </LocalizationProvider>
    </ThemeProvider>
  );
}

export default App;
