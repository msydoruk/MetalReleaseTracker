import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Button,
  Box,
  Dialog,
  IconButton
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import OptimizedImage from './OptimizedImage';
import { IMAGE_SIZES } from '../constants/imageSizes';
import Chip from '@mui/material/Chip';
import { useNavigate, Link } from 'react-router-dom';
import MediaTypeIcon from './MediaTypeIcon';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';
import { getDistributorCountry } from '../utils/distributorCountries';

const COLLECTION_STATUS_FAVORITE = 0;

const AlbumCard = ({ album, collectionStatus, onCollectionChange, onRemoveFromCollection, isLoggedIn = false }) => {
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();
  const navigate = useNavigate();
  const [lightboxOpen, setLightboxOpen] = useState(false);

  const imageUrl = album.photoUrl || "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

  const isFavorited = collectionStatus !== undefined && collectionStatus !== null;

  const handleFavoriteToggle = (event) => {
    event.stopPropagation();
    event.preventDefault();
    if (!isLoggedIn) {
      navigate('/login');
      return;
    }
    if (isFavorited) {
      if (onRemoveFromCollection) {
        onRemoveFromCollection(album.id);
      }
    } else {
      if (onCollectionChange) {
        onCollectionChange(album.id, COLLECTION_STATUS_FAVORITE);
      }
    }
  };

  return (
    <>
      <Card sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        boxShadow: '0 6px 16px rgba(0, 0, 0, 0.08)',
        borderRadius: 2,
        overflow: 'hidden',
        transition: 'all 0.25s ease-in-out',
        bgcolor: 'background.paper',
        border: '1px solid rgba(255, 255, 255, 0.1)',
        '&:hover': {
          transform: 'translateY(-8px)',
          boxShadow: '0 12px 20px rgba(0, 0, 0, 0.15)',
        }
      }}>
        <Box sx={{ position: 'relative', overflow: 'hidden' }}>
          <OptimizedImage
            imageSet={album.imageSet}
            photoUrl={imageUrl}
            alt={album.name}
            sizes={IMAGE_SIZES.card}
            loading="lazy"
            onClick={() => setLightboxOpen(true)}
            sx={{
              aspectRatio: '1 / 1',
              backgroundColor: '#111',
              transition: 'transform 0.3s ease',
              '&:hover': {
                transform: 'scale(1.05)'
              }
            }}
          />
          {album.status != null && (
            <Chip
              label={
                album.status === 2 ? t('albumCard.statusPreOrder')
                : album.status === 0 ? t('albumCard.statusNew')
                : album.status === 1 ? t('albumCard.statusRestock')
                : null
              }
              size="small"
              sx={{
                position: 'absolute',
                top: 8,
                left: 8,
                fontWeight: 700,
                fontSize: '0.7rem',
                bgcolor: album.status === 2 ? '#ff6f00' : album.status === 0 ? '#2e7d32' : '#1565c0',
                color: 'white',
              }}
            />
          )}
          {onCollectionChange && (
            <IconButton
              onClick={handleFavoriteToggle}
              size="small"
              sx={{
                position: 'absolute',
                top: 8,
                right: 8,
                bgcolor: 'rgba(0,0,0,0.5)',
                '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
                width: 44,
                height: 44,
              }}
            >
              {isFavorited ? (
                <FavoriteIcon sx={{ color: '#f44336', fontSize: 22 }} />
              ) : (
                <FavoriteBorderIcon sx={{ color: 'white', fontSize: 22 }} />
              )}
            </IconButton>
          )}
        </Box>
        <CardContent sx={{
          flex: '1 0 auto',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          p: 2,
          pt: 1.5,
          pb: 1
        }}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <MediaTypeIcon mediaType={album.media} />
            </Box>
            <Typography
              component={Link}
              to={`/albums/${album.slug}`}
              gutterBottom
              variant="h6"
              onClick={(event) => event.stopPropagation()}
              sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                display: '-webkit-box',
                WebkitLineClamp: 2,
                WebkitBoxOrient: 'vertical',
                mb: 0.5,
                fontWeight: 600,
                fontSize: '1.1rem',
                lineHeight: 1.3,
                height: '2.8rem',
                textDecoration: 'none',
                color: 'inherit',
                '&:hover': { textDecoration: 'underline' },
              }}
              title={album.name}
            >
              {album.name}
            </Typography>
            <Typography
              component={Link}
              to={`/bands/${album.bandSlug}`}
              variant="body2"
              color="text.secondary"
              gutterBottom
              onClick={(event) => event.stopPropagation()}
              sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                mb: 0.5,
                fontWeight: 500,
                display: 'block',
                textDecoration: 'none',
                '&:hover': { textDecoration: 'underline' },
              }}
            >
              {album.bandName}
            </Typography>
            {album.originalYear > 0 && (
              <Typography variant="caption" color="text.secondary" sx={{
                display: 'block',
                opacity: 0.6,
                fontSize: '0.7rem',
                mb: 0.5
              }}>
                {album.originalYear}
              </Typography>
            )}
            {album.distributorName && (
              <Typography variant="caption" color="text.secondary" sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                display: 'block',
                opacity: 0.7,
                fontSize: '0.7rem'
              }}>
                {getDistributorCountry(album.distributorName)} {album.distributorName}
              </Typography>
            )}
          </Box>
          <Box sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            mt: 'auto',
            pt: 1,
            borderTop: '1px solid rgba(0,0,0,0.05)'
          }}>
            <Typography variant="body1" color="text.primary" sx={{ fontWeight: 'bold' }}>
              {formatPrice(album.price)}
            </Typography>
            <Button
              size="small"
              component="a"
              href={album.purchaseUrl}
              target="_blank"
              rel="noopener noreferrer"
              variant="contained"
              color="primary"
              endIcon={<OpenInNewIcon sx={{ fontSize: 14 }} />}
              sx={{
                borderRadius: 5,
                px: 2,
                fontWeight: 600,
                textTransform: 'none',
                whiteSpace: 'nowrap'
              }}
            >
              {t('albumCard.viewInStore')}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <Dialog
        open={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
        maxWidth="md"
        fullScreen={false}
        slotProps={{
          backdrop: {
            sx: { backgroundColor: 'rgba(0,0,0,0.95)' }
          }
        }}
        PaperProps={{
          sx: {
            bgcolor: 'transparent',
            boxShadow: 'none',
            maxHeight: '90vh',
            m: 1
          }
        }}
      >
        <IconButton
          onClick={() => setLightboxOpen(false)}
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
            color: 'white',
            bgcolor: 'rgba(0,0,0,0.5)',
            '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' },
            zIndex: 1
          }}
        >
          <CloseIcon />
        </IconButton>
        <Box
          component="img"
          src={imageUrl}
          alt={album.name}
          sx={{
            maxHeight: '90vh',
            width: '100%',
            objectFit: 'contain'
          }}
        />
      </Dialog>
    </>
  );
};

export default AlbumCard;
