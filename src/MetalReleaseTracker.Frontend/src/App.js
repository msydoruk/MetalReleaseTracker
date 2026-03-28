import React, { useEffect, useState, lazy, Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { ThemeProvider, CssBaseline, CircularProgress, Box } from '@mui/material';

import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import Header from './components/Header';
import Footer from './components/Footer';
import authService from './services/auth';
import { LanguageProvider } from './i18n/LanguageContext';
import { CurrencyProvider } from './contexts/CurrencyContext';
import { NavigationProvider } from './contexts/NavigationContext';
import { SeoProvider } from './contexts/SeoContext';
import { ThemeModeProvider, useThemeMode } from './contexts/ThemeContext';
import { CompareProvider } from './contexts/CompareContext';
import BackToTop from './components/BackToTop';
import CompareBar from './components/CompareBar';
import InstallPrompt from './components/InstallPrompt';

// Lazy-loaded route components
const AlbumsPage = lazy(() => import('./pages/AlbumsPage'));
const AlbumDetailPage = lazy(() => import('./pages/AlbumDetailPage'));
const BandsPage = lazy(() => import('./pages/BandsPage'));
const BandDetailPage = lazy(() => import('./pages/BandDetailPage'));
const DistributorsPage = lazy(() => import('./pages/DistributorsPage'));
const DistributorDetailPage = lazy(() => import('./pages/DistributorDetailPage'));
const NewsPage = lazy(() => import('./pages/NewsPage'));
const AboutPage = lazy(() => import('./pages/AboutPage'));
const ReviewsPage = lazy(() => import('./pages/ReviewsPage'));
const ChangelogPage = lazy(() => import('./pages/ChangelogPage'));
const CalendarPage = lazy(() => import('./pages/CalendarPage'));
const ProfilePage = lazy(() => import('./pages/ProfilePage'));
const LoginPage = lazy(() => import('./pages/LoginPage'));
const RegisterPage = lazy(() => import('./pages/RegisterPage'));
const GoogleCallback = lazy(() => import('./pages/GoogleCallback'));
const LoginCallback = lazy(() => import('./pages/LoginCallback'));
const EmailVerifyPage = lazy(() => import('./pages/EmailVerifyPage'));
const ComparePage = lazy(() => import('./pages/ComparePage'));
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'));

// Theme is now managed by ThemeContext

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

const AppContent = () => {
  const { theme } = useThemeMode();

  useEffect(() => {
    const checkInitialAuthStatus = async () => {
      try {
        console.log('App: Checking initial auth status...');
        const isLoggedIn = await authService.isLoggedIn();
        console.log('App: Initial auth check complete', { isLoggedIn });

        if (isLoggedIn) {
          authService.triggerAuthUpdate();
        }
      } catch (error) {
        console.error('App: Error checking initial auth status', error);
      }
    };

    checkInitialAuthStatus();

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
        <LanguageProvider>
          <CurrencyProvider>
            <NavigationProvider>
            <SeoProvider>
            <CompareProvider>
            <Router>
            <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
              <Header />
              <Box component="main" sx={{ flexGrow: 1, pt: { xs: '56px', sm: '64px' } }}>
                <Suspense fallback={
                  <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                    <CircularProgress />
                  </Box>
                }>
                <Routes>
                  {/* Public routes */}
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/register" element={<RegisterPage />} />
                  <Route path="/auth/callback" element={<GoogleCallback />} />

                  {/* Public catalog routes */}
                  <Route path="/" element={<AlbumsPage isHome />} />
                  <Route path="/albums" element={<AlbumsPage />} />
                  <Route path="/albums/:slug" element={<AlbumDetailPage />} />
                  <Route path="/bands" element={<BandsPage />} />
                  <Route path="/bands/:slug" element={<BandDetailPage />} />
                  <Route path="/distributors" element={<DistributorsPage />} />
                  <Route path="/distributors/:slug" element={<DistributorDetailPage />} />
                  <Route path="/news" element={<NewsPage />} />
                  <Route path="/about" element={<AboutPage />} />
                  <Route path="/reviews" element={<ReviewsPage />} />
                  <Route path="/calendar" element={<CalendarPage />} />
                  <Route path="/changelog" element={<ChangelogPage />} />
                  <Route path="/compare" element={<ComparePage />} />
                  <Route path="/email/verify/:token" element={<EmailVerifyPage />} />

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

                  {/* Catch-all 404 */}
                  <Route path="*" element={<NotFoundPage />} />
                </Routes>
                </Suspense>
              </Box>
              <Footer />
              <CompareBar />
              <BackToTop />
              <InstallPrompt />
            </Box>
            </Router>
            </CompareProvider>
            </SeoProvider>
            </NavigationProvider>
          </CurrencyProvider>
        </LanguageProvider>
      </LocalizationProvider>
    </ThemeProvider>
  );
};

function App() {
  return (
    <ThemeModeProvider>
      <AppContent />
    </ThemeModeProvider>
  );
}

export default App;
