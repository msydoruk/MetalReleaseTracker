import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Button,
  Box,
  Chip,
  Dialog,
  IconButton
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import { useNavigate } from 'react-router-dom';
import { useLanguage } from '../i18n/LanguageContext';

const AlbumCard = ({ album, isFavorited = false, onToggleFavorite, isLoggedIn = false }) => {
  const { t } = useLanguage();
  const navigate = useNavigate();
  const [lightboxOpen, setLightboxOpen] = useState(false);

  // Mapping for media type labels
  const mediaTypeLabels = {
    0: t('albumCard.mediaCD'),
    1: t('albumCard.mediaVinyl'),
    2: t('albumCard.mediaCassette')
  };

  // Get media type label
  const mediaTypeLabel = mediaTypeLabels[album.media] || t('albumCard.mediaUnknown');

  const imageUrl = album.photoUrl || 'https://via.placeholder.com/300x300?text=No+Image';

  const handleFavoriteClick = (event) => {
    event.stopPropagation();
    if (!isLoggedIn) {
      navigate('/login');
      return;
    }
    if (onToggleFavorite) {
      onToggleFavorite(album.id);
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
          <CardMedia
            component="img"
            height="220"
            image={imageUrl}
            alt={album.name}
            onClick={() => setLightboxOpen(true)}
            sx={{
              objectFit: 'cover',
              cursor: 'pointer',
              transition: 'transform 0.3s ease',
              '&:hover': {
                transform: 'scale(1.05)'
              }
            }}
          />
          {onToggleFavorite && (
            <IconButton
              onClick={handleFavoriteClick}
              size="small"
              sx={{
                position: 'absolute',
                top: 8,
                right: 8,
                bgcolor: 'rgba(0,0,0,0.5)',
                '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' }
              }}
            >
              {isFavorited ? (
                <FavoriteIcon sx={{ color: '#f44336', fontSize: 20 }} />
              ) : (
                <FavoriteBorderIcon sx={{ color: 'white', fontSize: 20 }} />
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
              <Chip
                label={mediaTypeLabel}
                size="small"
                color="primary"
                variant="outlined"
              />
            </Box>
            <Typography gutterBottom variant="h6" component="div" sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              mb: 0.5,
              fontWeight: 600,
              fontSize: '1.1rem',
              lineHeight: 1.3,
              height: '2.8rem'
            }} title={album.name}>
              {album.name}
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              mb: 0.5,
              fontWeight: 500
            }}>
              {album.bandName}
            </Typography>
            {album.distributorName && (
              <Typography variant="caption" color="text.secondary" sx={{
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                display: 'block',
                opacity: 0.7,
                fontSize: '0.7rem'
              }}>
                {album.distributorName}
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
              {'\u20AC'}{album.price.toFixed(2)}
            </Typography>
            <Button
              size="small"
              component="a"
              href={album.purchaseUrl}
              target="_blank"
              rel="noopener noreferrer"
              variant="contained"
              color="primary"
              sx={{
                borderRadius: 5,
                px: 2,
                fontWeight: 'bold',
                textTransform: 'none'
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
