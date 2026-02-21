import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import theme from './theme';
import { AuthProvider } from './hooks/useAuth';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './components/LoginPage';
import Layout from './components/Layout';
import BandReferencesPage from './pages/BandReferencesPage';
import BandReferenceDetailPage from './pages/BandReferenceDetailPage';
import CatalogueIndexPage from './pages/CatalogueIndexPage';
import ParsingSessionsPage from './pages/ParsingSessionsPage';
import ParsingSessionDetailPage from './pages/ParsingSessionDetailPage';
import AiVerificationPage from './pages/AiVerificationPage';

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
              <Route index element={<BandReferencesPage />} />
              <Route path="band-references/:id" element={<BandReferenceDetailPage />} />
              <Route path="catalogue-index" element={<CatalogueIndexPage />} />
              <Route path="parsing-sessions" element={<ParsingSessionsPage />} />
              <Route path="parsing-sessions/:id" element={<ParsingSessionDetailPage />} />
              <Route path="ai-verification" element={<AiVerificationPage />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}
