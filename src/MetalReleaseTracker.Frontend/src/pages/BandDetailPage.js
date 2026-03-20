import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Paper,
  Chip,
  Button,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useParams, Link } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MusicNoteIcon from '@mui/icons-material/MusicNote';
import AlbumCard from '../components/AlbumCard';
import Pagination from '../components/Pagination';
import { fetchBandById, fetchAlbums, fetchFavoriteIds, addFavorite, removeFavorite } from '../services/api';
import authService from '../services/auth';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';

const BandDetailPage = () => {
  const { id } = useParams();
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [band, setBand] = useState(null);
  const [albums, setAlbums] = useState(null);
  const [loading, setLoading] = useState(true);
  const [albumsLoading, setAlbumsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  usePageMeta(
    band ? `${band.name} - Metal Release Tracker` : 'Metal Release Tracker',
    band ? `${band.name} releases from foreign distributors` : ''
  );

  useEffect(() => {
    const loadBand = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetchBandById(id);
        setBand(response.data);
      } catch {
        setError(t('bandDetail.notFound'));
      } finally {
        setLoading(false);
      }
    };

    loadBand();
  }, [id, t]);

  const loadAlbums = useCallback(async () => {
    try {
      setAlbumsLoading(true);
      const response = await fetchAlbums({
        bandId: id,
        page,
        pageSize,
        sortBy: ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
        sortAscending: false,
      });
      setAlbums(response.data);
    } catch {
      setAlbums(null);
    } finally {
      setAlbumsLoading(false);
    }
  }, [id, page, pageSize]);

  useEffect(() => {
    loadAlbums();
  }, [loadAlbums]);

  useEffect(() => {
    const loadAuth = async () => {
      const loggedIn = await authService.isLoggedIn();
      setIsLoggedIn(loggedIn);
      if (loggedIn) {
        try {
          const response = await fetchFavoriteIds();
          setFavoriteIds(new Set(response.data));
        } catch {
          // ignore
        }
      }
    };

    loadAuth();
  }, []);

  const handleToggleFavorite = async (albumId) => {
    try {
      if (favoriteIds.has(albumId)) {
        await removeFavorite(albumId);
        setFavoriteIds((prev) => { const next = new Set(prev); next.delete(albumId); return next; });
      } else {
        await addFavorite(albumId);
        setFavoriteIds((prev) => new Set(prev).add(albumId));
      }
    } catch {
      // ignore
    }
  };

  const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

  if (loading) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !band) {
    return (
      <Container maxWidth="xl" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{error || t('bandDetail.notFound')}</Alert>
        <Button component={Link} to="/bands" startIcon={<ArrowBackIcon />} sx={{ textTransform: 'none' }}>
          {t('bandDetail.backToBands')}
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Button
        component={Link}
        to="/bands"
        startIcon={<ArrowBackIcon />}
        sx={{ textTransform: 'none', mb: 3 }}
      >
        {t('bandDetail.backToBands')}
      </Button>

      <Box sx={{
        display: 'flex',
        flexDirection: isMobile ? 'column' : 'row',
        gap: 3,
        mb: 4,
        alignItems: isMobile ? 'center' : 'flex-start',
      }}>
        <Box
          component="img"
          src={band.photoUrl || placeholderImg}
          alt={band.name}
          sx={{
            width: isMobile ? 200 : 250,
            height: isMobile ? 200 : 250,
            objectFit: 'contain',
            borderRadius: 2,
            backgroundColor: '#111',
            flexShrink: 0,
          }}
        />
        <Box sx={{ textAlign: isMobile ? 'center' : 'left' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: isMobile ? 'center' : 'flex-start', mb: 1 }}>
            <MusicNoteIcon sx={{ color: 'primary.main' }} />
            <Typography variant="h4" component="h1" sx={{ fontWeight: 800 }}>
              {band.name}
            </Typography>
          </Box>
          {band.genre && (
            <Chip label={band.genre} size="small" color="secondary" sx={{ mb: 2 }} />
          )}
          {band.description && (
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 600 }}>
              {band.description}
            </Typography>
          )}
        </Box>
      </Box>

      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 3 }}>
        {t('bandDetail.albumsBy').replace('{bandName}', band.name)}
      </Typography>

      {albumsLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : albums && albums.items.length > 0 ? (
        <>
          <Pagination
            currentPage={albums.currentPage}
            totalPages={albums.pageCount}
            totalItems={albums.totalCount}
            pageSize={albums.pageSize}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
            compact
          />
          <Box sx={{
            display: 'grid',
            gridTemplateColumns: {
              xs: 'repeat(1, 1fr)',
              sm: 'repeat(2, 1fr)',
              md: 'repeat(3, 1fr)',
              lg: 'repeat(4, 1fr)',
              xl: 'repeat(5, 1fr)',
            },
            gap: 3,
            my: 3,
          }}>
            {albums.items.map((album) => (
              <AlbumCard
                key={album.id}
                album={album}
                isFavorited={favoriteIds.has(album.id)}
                onToggleFavorite={handleToggleFavorite}
                isLoggedIn={isLoggedIn}
              />
            ))}
          </Box>
          <Pagination
            currentPage={albums.currentPage}
            totalPages={albums.pageCount}
            totalItems={albums.totalCount}
            pageSize={albums.pageSize}
            onPageChange={setPage}
            onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
          />
        </>
      ) : (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('bandDetail.noAlbums')}
          </Typography>
        </Paper>
      )}
    </Container>
  );
};

export default BandDetailPage;
