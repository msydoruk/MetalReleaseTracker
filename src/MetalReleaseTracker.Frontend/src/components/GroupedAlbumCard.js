import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
  Chip,
  Dialog,
  IconButton,
  Divider
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import { useLanguage } from '../i18n/LanguageContext';

const GroupedAlbumCard = ({ group }) => {
  const { t } = useLanguage();
  const [lightboxOpen, setLightboxOpen] = useState(false);

  const mediaTypeLabels = {
    0: t('albumCard.mediaCD'),
    1: t('albumCard.mediaVinyl'),
    2: t('albumCard.mediaCassette')
  };

  const statusLabels = {
    0: { label: t('albumCard.statusNew'), color: 'success' },
    1: { label: t('albumCard.statusRestock'), color: 'info' },
    2: { label: t('albumCard.statusPreOrder'), color: 'warning' }
  };

  const statusInfo = statusLabels[group.status];
  const mediaLabel = mediaTypeLabels[group.media] || t('albumCard.mediaUnknown');

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' });
  };

  const minPrice = Math.min(...group.variants.map((v) => v.price));
  const maxPrice = Math.max(...group.variants.map((v) => v.price));
  const priceDisplay = minPrice === maxPrice
    ? `€${minPrice.toFixed(2)}`
    : `€${minPrice.toFixed(2)} – €${maxPrice.toFixed(2)}`;

  return (
    <>
      <Card sx={{
        display: 'flex',
        flexDirection: 'column',
        width: '100%',
        height: '100%',
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: '0 8px 25px rgba(0,0,0,0.3)'
        }
      }}>
        <Box sx={{ position: 'relative' }}>
          <CardMedia
            component="img"
            height="220"
            image={group.photoUrl || '/placeholder-album.png'}
            alt={`${group.bandName} - ${group.albumName}`}
            sx={{
              objectFit: 'cover',
              cursor: 'pointer'
            }}
            onClick={() => setLightboxOpen(true)}
          />
          {group.variants.length > 1 && (
            <Chip
              label={`${group.variants.length} ${t('grouped.stores')}`}
              size="small"
              color="primary"
              sx={{
                position: 'absolute',
                top: 8,
                right: 8,
                fontWeight: 'bold',
                fontSize: '0.7rem'
              }}
            />
          )}
        </Box>

        <CardContent sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column', p: 2, '&:last-child': { pb: 2 } }}>
          <Typography variant="subtitle2" color="text.secondary" noWrap>
            {group.bandName}
          </Typography>
          <Typography variant="body1" sx={{ fontWeight: 600, mb: 1 }} noWrap>
            {group.albumName}
          </Typography>

          <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 1 }}>
            {statusInfo && (
              <Chip label={statusInfo.label} color={statusInfo.color} size="small" variant="outlined" />
            )}
            <Chip label={mediaLabel} size="small" variant="outlined" />
            {group.releaseDate && (
              <Chip label={formatDate(group.releaseDate)} size="small" variant="outlined" />
            )}
          </Box>

          <Typography variant="h6" color="primary" sx={{ fontWeight: 700, mb: 1 }}>
            {priceDisplay}
          </Typography>

          <Divider sx={{ mb: 1 }} />

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, mt: 'auto' }}>
            {group.variants.map((variant) => (
              <Box
                key={variant.albumId}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  py: 0.5,
                  px: 1,
                  borderRadius: 1,
                  bgcolor: 'action.hover',
                  '&:hover': { bgcolor: 'action.selected' }
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 0, flex: 1 }}>
                  <Typography variant="body2" noWrap sx={{ flex: 1 }}>
                    {variant.distributorName}
                  </Typography>
                  <Typography variant="body2" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>
                    €{variant.price.toFixed(2)}
                  </Typography>
                </Box>
                <IconButton
                  size="small"
                  href={variant.purchaseUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ ml: 0.5 }}
                >
                  <OpenInNewIcon fontSize="small" />
                </IconButton>
              </Box>
            ))}
          </Box>
        </CardContent>
      </Card>

      <Dialog
        open={lightboxOpen}
        onClose={() => setLightboxOpen(false)}
        maxWidth="md"
        fullWidth
        slotProps={{
          backdrop: {
            sx: { backgroundColor: 'rgba(0,0,0,0.95)' }
          }
        }}
        PaperProps={{
          sx: { backgroundColor: 'transparent', boxShadow: 'none', overflow: 'visible' }
        }}
      >
        <IconButton
          onClick={() => setLightboxOpen(false)}
          sx={{
            position: 'absolute', top: -40, right: 0,
            color: 'white', bgcolor: 'rgba(255,255,255,0.1)',
            '&:hover': { bgcolor: 'rgba(255,255,255,0.2)' }
          }}
        >
          <CloseIcon />
        </IconButton>
        <Box
          component="img"
          src={group.photoUrl || '/placeholder-album.png'}
          alt={`${group.bandName} - ${group.albumName}`}
          sx={{ width: '100%', maxHeight: '90vh', objectFit: 'contain' }}
        />
      </Dialog>
    </>
  );
};

export default GroupedAlbumCard;
