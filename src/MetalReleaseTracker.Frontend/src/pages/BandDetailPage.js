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
import { fetchBandById, fetchAlbums, fetchFavoriteIds, addFavorite, removeFavorite, updateFavoriteStatus, fetchSimilarBands } from '../services/api';
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
  const [favoriteIds, setFavoriteIds] = useState({});
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [similarBands, setSimilarBands] = useState([]);

  const bandMetaDescription = (() => {
    if (!band) return '';
    const parts = [band.name];
    if (band.genre) parts[0] += ` - ${band.genre}`;
    parts[0] += ' from Ukraine';
    if (albums?.totalCount > 0) {
      parts.push(`${albums.totalCount} release${albums.totalCount > 1 ? 's' : ''} available from foreign distributors`);
    }
    return parts.join('. ');
  })();

  usePageMeta(
    band ? band.name : null,
    bandMetaDescription,
    band?.photoUrl
  );

  useEffect(() => {
    if (!band) return;
    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'MusicGroup',
      name: band.name,
      ...(band.genre && { genre: band.genre }),
      ...(band.photoUrl && { image: band.photoUrl }),
      ...(band.description && { description: band.description }),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.textContent = JSON.stringify(jsonLd);
    script.id = 'band-jsonld';
    const existing = document.getElementById('band-jsonld');
    if (existing) existing.remove();
    document.head.appendChild(script);
    return () => { script.remove(); };
  }, [band]);

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

  useEffect(() => {
    const loadSimilarBands = async () => {
      try {
        const response = await fetchSimilarBands(id);
        setSimilarBands(response.data);
      } catch {
        setSimilarBands([]);
      }
    };

    loadSimilarBands();
  }, [id]);

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
          setFavoriteIds(response.data || {});
        } catch {
          // ignore
        }
      }
    };

    loadAuth();
  }, []);

  const handleCollectionChange = async (albumId, status) => {
    try {
      if (albumId in favoriteIds) {
        await updateFavoriteStatus(albumId, status);
      } else {
        await addFavorite(albumId, status);
      }
      setFavoriteIds((prev) => ({ ...prev, [albumId]: status }));
    } catch {
      // ignore
    }
  };

  const handleRemoveFromCollection = async (albumId) => {
    try {
      await removeFavorite(albumId);
      setFavoriteIds((prev) => {
        const next = { ...prev };
        delete next[albumId];
        return next;
      });
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

      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
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
                collectionStatus={album.id in favoriteIds ? favoriteIds[album.id] : undefined}
                onCollectionChange={(albumId, status) => handleCollectionChange(albumId, status)}
                onRemoveFromCollection={(albumId) => handleRemoveFromCollection(albumId)}
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
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('bandDetail.noAlbums')}
          </Typography>
        </Paper>
      )}

      {similarBands.length > 0 && (
        <>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2, mt: 4 }}>
            {t('bandDetail.similarBands')}
          </Typography>
          <Box
            sx={{
              display: 'flex',
              gap: 2,
              overflowX: 'auto',
              pb: 1,
              mx: -1,
              px: 1,
              '&::-webkit-scrollbar': { height: 6 },
              '&::-webkit-scrollbar-thumb': { bgcolor: 'rgba(255,255,255,0.2)', borderRadius: 3 },
            }}
          >
            {similarBands.map((similarBand) => (
              <Paper
                key={similarBand.id}
                component={Link}
                to={`/bands/${similarBand.id}`}
                sx={{
                  minWidth: 180,
                  maxWidth: 180,
                  flexShrink: 0,
                  textDecoration: 'none',
                  color: 'inherit',
                  overflow: 'hidden',
                  borderRadius: 2,
                  transition: 'transform 0.2s',
                  '&:hover': { transform: 'translateY(-4px)' },
                }}
              >
                <Box
                  component="img"
                  src={similarBand.photoUrl || placeholderImg}
                  alt={similarBand.name}
                  sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
                />
                <Box sx={{ p: 1.5 }}>
                  <Typography variant="subtitle2" sx={{
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}>
                    {similarBand.name}
                  </Typography>
                  {similarBand.genre && (
                    <Chip label={similarBand.genre} size="small" color="secondary" sx={{ mt: 0.5 }} />
                  )}
                </Box>
              </Paper>
            ))}
          </Box>
        </>
      )}
    </Container>
  );
};

export default BandDetailPage;
