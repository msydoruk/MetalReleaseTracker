import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import theme from './theme';
import { AuthProvider } from './hooks/useAuth';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './components/LoginPage';
import Layout from './components/Layout';
import DashboardPage from './pages/DashboardPage';
import DistributorsPage from './pages/DistributorsPage';
import BandsPage from './pages/BandsPage';
import AlbumsPage from './pages/AlbumsPage';
import CurrenciesPage from './pages/CurrenciesPage';
import NavigationPage from './pages/NavigationPage';
import TranslationsPage from './pages/TranslationsPage';
import NewsPage from './pages/NewsPage';
import UsersPage from './pages/UsersPage';
import ReviewsPage from './pages/ReviewsPage';
import SettingsPage from './pages/SettingsPage';
import NotificationsPage from './pages/NotificationsPage';
import TelegramPage from './pages/TelegramPage';
import LanguagesPage from './pages/LanguagesPage';
import AnalyticsPage from './pages/AnalyticsPage';
import AuditLogPage from './pages/AuditLogPage';

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter basename="/admin">
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route
              element={
                <ProtectedRoute>
                  <Layout />
                </ProtectedRoute>
              }
            >
              <Route index element={<DashboardPage />} />
              <Route path="distributors" element={<DistributorsPage />} />
              <Route path="bands" element={<BandsPage />} />
              <Route path="albums" element={<AlbumsPage />} />
              <Route path="currencies" element={<CurrenciesPage />} />
              <Route path="navigation" element={<NavigationPage />} />
              <Route path="translations" element={<TranslationsPage />} />
              <Route path="news" element={<NewsPage />} />
              <Route path="users" element={<UsersPage />} />
              <Route path="reviews" element={<ReviewsPage />} />
              <Route path="settings" element={<SettingsPage />} />
              <Route path="notifications" element={<NotificationsPage />} />
              <Route path="telegram" element={<TelegramPage />} />
              <Route path="languages" element={<LanguagesPage />} />
              <Route path="analytics" element={<AnalyticsPage />} />
              <Route path="audit-log" element={<AuditLogPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
