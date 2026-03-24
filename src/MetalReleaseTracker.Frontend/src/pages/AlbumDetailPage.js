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
  Dialog,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useParams, Link } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import CloseIcon from '@mui/icons-material/Close';
import HeadphonesIcon from '@mui/icons-material/Headphones';
import { IconButton } from '@mui/material';
import MediaTypeIcon from '../components/MediaTypeIcon';
import AlbumRating from '../components/AlbumRating';
import CollectionStatusMenu from '../components/CollectionStatusMenu';
import PriceHistoryChart from '../components/PriceHistoryChart';
import { fetchAlbumDetailBySlug, fetchFavoriteIds, addFavorite, removeFavorite, updateFavoriteStatus } from '../services/api';
import authService from '../services/auth';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';
import { getDistributorCountry, getDistributorCountryName } from '../utils/distributorCountries';
import useRecentlyViewed from '../hooks/useRecentlyViewed';
import WatchButton from '../components/WatchButton';

const AlbumDetailPage = () => {
  const { slug } = useParams();
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const [album, setAlbum] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lightboxOpen, setLightboxOpen] = useState(false);
  const [favoriteIds, setFavoriteIds] = useState({});
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const { addRecentlyViewed } = useRecentlyViewed();

  const albumMetaDescription = (() => {
    if (!album) return '';
    const parts = [`Buy ${album.albumName} by ${album.bandName}`];
    if (album.originalYear > 0) parts[0] += ` (${album.originalYear})`;
    if (album.genre) parts.push(album.genre);
    const prices = album.variants.map((variant) => variant.price);
    if (prices.length > 0) {
      const min = Math.min(...prices).toFixed(2);
      const max = Math.max(...prices).toFixed(2);
      const priceText = min === max ? `\u20AC${min}` : `\u20AC${min} to \u20AC${max}`;
      parts.push(`${priceText} at ${album.variants.length} store${album.variants.length > 1 ? 's' : ''}`);
    }
    return parts.join('. ');
  })();

  usePageMeta(
    album ? `${album.albumName} - ${album.bandName}` : null,
    albumMetaDescription,
    album?.photoUrl
  );

  useEffect(() => {
    if (!album) return;
    const jsonLd = {
      '@context': 'https://schema.org',
      '@type': 'MusicAlbum',
      name: album.albumName,
      byArtist: { '@type': 'MusicGroup', name: album.bandName },
      ...(album.originalYear > 0 && { datePublished: String(album.originalYear) }),
      ...(album.genre && { genre: album.genre }),
      ...(album.photoUrl && { image: album.photoUrl }),
      offers: album.variants.map((variant) => ({
        '@type': 'Offer',
        price: variant.price.toFixed(2),
        priceCurrency: 'EUR',
        url: variant.purchaseUrl,
        seller: { '@type': 'Organization', name: variant.distributorName },
        availability: 'https://schema.org/InStock',
      })),
    };
    const script = document.createElement('script');
    script.type = 'application/ld+json';
    script.textContent = JSON.stringify(jsonLd);
    script.id = 'album-jsonld';
    const existing = document.getElementById('album-jsonld');
    if (existing) existing.remove();
    document.head.appendChild(script);
    return () => { script.remove(); };
  }, [album]);

  useEffect(() => {
    const loadAlbum = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetchAlbumDetailBySlug(slug);
        setAlbum(response.data);
        if (response.data) {
          addRecentlyViewed(response.data);
        }
      } catch {
        setError(t('albumDetail.notFound'));
      } finally {
        setLoading(false);
      }
    };

    loadAlbum();
  }, [slug, t, addRecentlyViewed]);

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

  const handleCollectionChange = useCallback(async (status) => {
    if (!isLoggedIn || !album) return;
    const albumId = album.primaryAlbumId;
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
  }, [isLoggedIn, album, favoriteIds]);

  const handleCollectionRemove = useCallback(async () => {
    if (!isLoggedIn || !album) return;
    const albumId = album.primaryAlbumId;
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
  }, [isLoggedIn, album]);

  const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";


  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !album) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{error || t('albumDetail.notFound')}</Alert>
        <Button component={Link} to="/albums" startIcon={<ArrowBackIcon />} sx={{ textTransform: 'none' }}>
          {t('albumDetail.backToAlbums')}
        </Button>
      </Container>
    );
  }

  const currentStatus = album.primaryAlbumId in favoriteIds ? favoriteIds[album.primaryAlbumId] : undefined;
  const storeCount = album.variants.length;
  const storesText = storeCount === 1
    ? t('albumDetail.availableAtOne')
    : t('albumDetail.availableAt').replace('{count}', storeCount);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Button
        component={Link}
        to="/albums"
        startIcon={<ArrowBackIcon />}
        sx={{ textTransform: 'none', mb: 3 }}
      >
        {t('albumDetail.backToAlbums')}
      </Button>

      <Box sx={{
        display: 'flex',
        flexDirection: isMobile ? 'column' : 'row',
        gap: 4,
        mb: 4,
        alignItems: isMobile ? 'center' : 'flex-start',
      }}>
        <Box
          component="img"
          src={album.photoUrl || placeholderImg}
          alt={`${album.bandName} - ${album.albumName}`}
          onClick={() => setLightboxOpen(true)}
          sx={{
            width: isMobile ? '100%' : 350,
            maxWidth: 350,
            aspectRatio: '1 / 1',
            objectFit: 'contain',
            borderRadius: 2,
            backgroundColor: '#111',
            flexShrink: 0,
            cursor: 'pointer',
          }}
        />
        <Box sx={{ flex: 1, textAlign: isMobile ? 'center' : 'left' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: isMobile ? 'center' : 'flex-start', mb: 1 }}>
            {album.media != null && <MediaTypeIcon mediaType={album.media} size={28} />}
            <Typography variant="h4" component="h1" sx={{ fontWeight: 800 }}>
              {album.albumName}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: isMobile ? 'center' : 'flex-start', mb: 1 }}>
            <Typography
              component={Link}
              to={`/bands/${album.bandSlug}`}
              variant="h6"
              color="text.secondary"
              sx={{ textDecoration: 'none', '&:hover': { textDecoration: 'underline' }, display: 'inline-block' }}
            >
              {album.bandName}
            </Typography>
            {album.bandMetalArchivesUrl && (
              <IconButton
                component="a"
                href={album.bandMetalArchivesUrl}
                target="_blank"
                rel="noopener noreferrer"
                size="small"
                title={t('bandDetail.viewOnMetalArchives')}
                sx={{ color: 'text.secondary' }}
              >
                <OpenInNewIcon fontSize="small" />
              </IconButton>
            )}
          </Box>

          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', justifyContent: isMobile ? 'center' : 'flex-start', mb: 2 }}>
            {album.originalYear > 0 && (
              <Chip label={album.originalYear} size="small" variant="outlined" />
            )}
            {album.genre && (
              <Chip label={album.genre} size="small" color="secondary" />
            )}
          </Box>

          <AlbumRating albumId={album.primaryAlbumId} isLoggedIn={isLoggedIn} />

          {album.label && (
            <Typography variant="body2" color="text.secondary">
              {t('albumDetail.label')}: {album.label}
            </Typography>
          )}
          {album.press && (
            <Typography variant="body2" color="text.secondary">
              {t('albumDetail.press')}: {album.press}
            </Typography>
          )}
          {album.description && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {album.description}
            </Typography>
          )}

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, justifyContent: isMobile ? 'center' : 'flex-start' }}>
            {isLoggedIn && (
              <>
                <CollectionStatusMenu
                  currentStatus={currentStatus}
                  onSelect={(status) => handleCollectionChange(status)}
                  onRemove={handleCollectionRemove}
                  sx={{ color: 'text.secondary' }}
                />
                <WatchButton albumId={album.primaryAlbumId} isLoggedIn={isLoggedIn} />
              </>
            )}
            <Button
              variant="outlined"
              startIcon={<HeadphonesIcon />}
              href={`https://bandcamp.com/search?q=${encodeURIComponent(album.bandName + ' ' + album.albumName)}`}
              target="_blank"
              rel="noopener noreferrer"
              sx={{ textTransform: 'none', borderRadius: 5, fontWeight: 600 }}
            >
              {t('albumDetail.listenOnBandcamp')}
            </Button>
          </Box>
        </Box>
      </Box>

      {album.bandcampUrl && (
        <Box sx={{ mb: 4 }}>
          <iframe
            title="Bandcamp Player"
            style={{ border: 0, width: '100%', height: 120 }}
            src={`${album.bandcampUrl}?size=large&bgcol=333333&linkcol=e32c14`}
            seamless
            loading="lazy"
          />
        </Box>
      )}

      <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
        {storesText}
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 4 }}>
        {[...album.variants]
        .sort((variantA, variantB) => {
          const order = { InStock: 0, PreOrder: 1, Unknown: 2, OutOfStock: 3 };
          return (order[variantA.stockStatus] ?? 2) - (order[variantB.stockStatus] ?? 2);
        })
        .map((variant) => {
          const flag = getDistributorCountry(variant.distributorName);
          const countryName = getDistributorCountryName(variant.distributorName);
          const isOutOfStock = variant.stockStatus === 'OutOfStock';
          const isPreOrder = variant.stockStatus === 'PreOrder';
          return (
            <Paper
              key={variant.albumId}
              component={isOutOfStock ? 'div' : 'a'}
              href={isOutOfStock ? undefined : variant.purchaseUrl}
              target={isOutOfStock ? undefined : '_blank'}
              rel={isOutOfStock ? undefined : 'noopener noreferrer'}
              sx={{
                p: 2,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                textDecoration: 'none',
                color: 'inherit',
                opacity: isOutOfStock ? 0.5 : 1,
                transition: 'background-color 0.2s',
                '&:hover': isOutOfStock ? {} : { backgroundColor: 'rgba(255,255,255,0.05)' },
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                <Typography variant="h6" sx={{ fontWeight: 700, minWidth: 80 }}>
                  {formatPrice(variant.price)}
                </Typography>
                <Typography variant="body1">
                  {flag}{countryName ? ` ${t('albumDetail.shipsFrom')} ${countryName} \u00B7 ` : ' '}{variant.distributorName}
                </Typography>
                {isOutOfStock && (
                  <Chip label="Out of Stock" size="small" color="error" variant="outlined" />
                )}
                {isPreOrder && (
                  <Chip label="Pre-Order" size="small" color="warning" variant="outlined" />
                )}
              </Box>
              <Button
                variant="contained"
                color={isPreOrder ? 'warning' : 'primary'}
                size="small"
                disabled={isOutOfStock}
                endIcon={!isOutOfStock ? <OpenInNewIcon sx={{ fontSize: 14 }} /> : null}
                sx={{ textTransform: 'none', borderRadius: 5, fontWeight: 600, flexShrink: 0 }}
              >
                {isOutOfStock ? 'Out of Stock' : isPreOrder ? 'Pre-Order' : t('albumDetail.buy')}
              </Button>
            </Paper>
          );
        })}
      </Box>

      {album.formatGroups && album.formatGroups.length > 0 && (
        <Box sx={{ mb: 4 }}>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
            {t('albumDetail.alsoAvailableAs')}
          </Typography>
          {album.formatGroups.map((group) => {
            const groupMediaLabel = group.media != null
              ? { 0: 'CD', 1: 'Vinyl', 2: 'Tape' }[group.media] || ''
              : '';
            return (
              <Box key={group.media} sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                  <MediaTypeIcon mediaType={group.media} size={22} />
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    {groupMediaLabel}
                  </Typography>
                </Box>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                  {[...group.variants]
                    .sort((variantA, variantB) => {
                      const order = { InStock: 0, PreOrder: 1, Unknown: 2, OutOfStock: 3 };
                      return (order[variantA.stockStatus] ?? 2) - (order[variantB.stockStatus] ?? 2);
                    })
                    .map((variant) => {
                      const flag = getDistributorCountry(variant.distributorName);
                      const countryName = getDistributorCountryName(variant.distributorName);
                      const isOutOfStock = variant.stockStatus === 'OutOfStock';
                      const isPreOrder = variant.stockStatus === 'PreOrder';
                      return (
                        <Paper
                          key={variant.albumId}
                          component={isOutOfStock ? 'div' : 'a'}
                          href={isOutOfStock ? undefined : variant.purchaseUrl}
                          target={isOutOfStock ? undefined : '_blank'}
                          rel={isOutOfStock ? undefined : 'noopener noreferrer'}
                          sx={{
                            p: 2,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            textDecoration: 'none',
                            color: 'inherit',
                            opacity: isOutOfStock ? 0.5 : 1,
                            transition: 'background-color 0.2s',
                            '&:hover': isOutOfStock ? {} : { backgroundColor: 'rgba(255,255,255,0.05)' },
                          }}
                        >
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                            <Typography variant="h6" sx={{ fontWeight: 700, minWidth: 80 }}>
                              {formatPrice(variant.price)}
                            </Typography>
                            <Typography variant="body1">
                              {flag}{countryName ? ` ${t('albumDetail.shipsFrom')} ${countryName} \u00B7 ` : ' '}{variant.distributorName}
                            </Typography>
                            {isOutOfStock && (
                              <Chip label="Out of Stock" size="small" color="error" variant="outlined" />
                            )}
                            {isPreOrder && (
                              <Chip label="Pre-Order" size="small" color="warning" variant="outlined" />
                            )}
                          </Box>
                          <Button
                            variant="contained"
                            color={isPreOrder ? 'warning' : 'primary'}
                            size="small"
                            disabled={isOutOfStock}
                            endIcon={!isOutOfStock ? <OpenInNewIcon sx={{ fontSize: 14 }} /> : null}
                            sx={{ textTransform: 'none', borderRadius: 5, fontWeight: 600, flexShrink: 0 }}
                          >
                            {isOutOfStock ? 'Out of Stock' : isPreOrder ? 'Pre-Order' : t('albumDetail.buy')}
                          </Button>
                        </Paper>
                      );
                    })}
                </Box>
              </Box>
            );
          })}
        </Box>
      )}

      <PriceHistoryChart albumName={album.albumName} bandName={album.bandName} />

      {album.relatedReleases.length > 0 && (
        <>
          <Typography variant="h5" component="h2" sx={{ fontWeight: 700, mb: 2 }}>
            {t('albumDetail.moreByBand').replace('{bandName}', album.bandName)}
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
            {album.relatedReleases.map((related) => (
              <Paper
                key={related.albumId}
                component={Link}
                to={`/albums/${related.albumSlug}`}
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
                  src={related.photoUrl || placeholderImg}
                  alt={related.albumName}
                  sx={{ width: '100%', aspectRatio: '1 / 1', objectFit: 'contain', backgroundColor: '#111' }}
                />
                <Box sx={{ p: 1.5 }}>
                  <Typography variant="subtitle2" sx={{
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}>
                    {related.albumName}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {related.originalYear > 0 && `${related.originalYear} \u00B7 `}{formatPrice(related.minPrice)}
                  </Typography>
                </Box>
              </Paper>
            ))}
          </Box>
        </>
      )}

      <Dialog open={lightboxOpen} onClose={() => setLightboxOpen(false)} maxWidth="lg">
        <IconButton
          onClick={() => setLightboxOpen(false)}
          sx={{ position: 'absolute', top: 8, right: 8, zIndex: 1, bgcolor: 'rgba(0,0,0,0.5)' }}
        >
          <CloseIcon />
        </IconButton>
        <Box
          component="img"
          src={album.photoUrl || placeholderImg}
          alt={`${album.bandName} - ${album.albumName}`}
          sx={{ width: '100%', maxHeight: '90vh', objectFit: 'contain' }}
        />
      </Dialog>
    </Container>
  );
};

export default AlbumDetailPage;
