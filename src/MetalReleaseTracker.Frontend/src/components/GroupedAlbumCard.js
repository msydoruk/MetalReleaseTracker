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
  Button
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import MediaTypeIcon from './MediaTypeIcon';
import { useLanguage } from '../i18n/LanguageContext';

const GroupedAlbumCard = ({ group }) => {
  const { t } = useLanguage();
  const [lightboxOpen, setLightboxOpen] = useState(false);
  const [expanded, setExpanded] = useState(false);

  const MAX_VISIBLE_VARIANTS = 3;

  const sortedVariants = [...group.variants].sort((a, b) => a.price - b.price);
  const minPrice = Math.min(...group.variants.map((v) => v.price));
  const maxPrice = Math.max(...group.variants.map((v) => v.price));
  const priceDisplay = minPrice === maxPrice
    ? `\u20AC${minPrice.toFixed(2)}`
    : `\u20AC${minPrice.toFixed(2)} \u2013 \u20AC${maxPrice.toFixed(2)}`;

  return (
    <>
      <Card sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        boxShadow: '0 6px 16px rgba(0, 0, 0, 0.08)',
        borderRadius: 2,
        overflow: expanded ? 'visible' : 'hidden',
        zIndex: expanded ? 10 : 'auto',
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
            image={group.photoUrl || '/placeholder-album.png'}
            alt={`${group.bandName} - ${group.albumName}`}
            onClick={() => setLightboxOpen(true)}
            sx={{
              aspectRatio: '1 / 1',
              objectFit: 'contain',
              backgroundColor: '#111',
              cursor: 'pointer',
              transition: 'transform 0.3s ease',
              '&:hover': {
                transform: 'scale(1.05)'
              }
            }}
          />
          {group.variants.length > 1 && (
            <Chip
              label={`${group.variants.length} ${t('grouped.stores')}`}
              size="small"
              sx={{
                position: 'absolute',
                top: 8,
                right: 8,
                fontWeight: 'bold',
                fontSize: '0.7rem',
                bgcolor: 'rgba(0,0,0,0.5)',
                color: 'white'
              }}
            />
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
              <MediaTypeIcon mediaType={group.media} />
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
            }} title={group.albumName}>
              {group.albumName}
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              mb: 0.5,
              fontWeight: 500
            }}>
              {group.bandName}
            </Typography>
          </Box>

          <Box sx={{ mt: 'auto' }}>
            <Box sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              pt: 1,
              mb: 1,
              borderTop: '1px solid rgba(0,0,0,0.05)'
            }}>
              <Typography variant="body1" color="text.primary" sx={{ fontWeight: 'bold' }}>
                {priceDisplay}
              </Typography>
            </Box>

            <Box sx={{ position: 'relative' }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                {sortedVariants.slice(0, MAX_VISIBLE_VARIANTS).map((variant) => (
                  <Button
                    key={variant.albumId}
                    component="a"
                    href={variant.purchaseUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    size="small"
                    variant="outlined"
                    endIcon={<OpenInNewIcon sx={{ fontSize: 14, color: '#f44336' }} />}
                    sx={{
                      justifyContent: 'space-between',
                      textTransform: 'none',
                      borderRadius: 5,
                      px: 1.5,
                      py: 0.4,
                      fontSize: '0.8rem',
                      fontWeight: 500,
                      borderColor: 'rgba(255,255,255,0.12)',
                      bgcolor: 'rgba(255,255,255,0.03)',
                      '&:hover': {
                        borderColor: 'rgba(255,255,255,0.3)',
                        bgcolor: 'rgba(255,255,255,0.07)'
                      }
                    }}
                  >
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%', mr: 1 }}>
                      <Typography variant="body2" sx={{ fontWeight: 700, whiteSpace: 'nowrap', fontSize: '0.8rem', color: 'white' }}>
                        {'\u20AC'}{variant.price.toFixed(2)}
                      </Typography>
                      <Typography variant="body2" noWrap sx={{ fontSize: '0.8rem', color: 'text.secondary', ml: 1 }}>
                        {variant.distributorName}
                      </Typography>
                    </Box>
                  </Button>
                ))}
              </Box>
              {sortedVariants.length > MAX_VISIBLE_VARIANTS && (
                <Button
                  size="small"
                  onClick={() => setExpanded(!expanded)}
                  endIcon={expanded ? <KeyboardArrowUpIcon sx={{ fontSize: 16 }} /> : <KeyboardArrowDownIcon sx={{ fontSize: 16 }} />}
                  sx={{
                    width: '100%',
                    mt: 0.5,
                    textTransform: 'none',
                    fontSize: '0.75rem',
                    color: 'text.secondary',
                    py: 0.25,
                    minHeight: 'unset',
                    borderRadius: 5,
                    '&:hover': {
                      bgcolor: 'rgba(255,255,255,0.05)'
                    }
                  }}
                >
                  {expanded ? t('grouped.showLess') : `+${sortedVariants.length - MAX_VISIBLE_VARIANTS} ${t('grouped.moreStores')}`}
                </Button>
              )}
              {expanded && sortedVariants.length > MAX_VISIBLE_VARIANTS && (
                <Box sx={{
                  position: 'absolute',
                  top: '100%',
                  left: -16,
                  right: -16,
                  zIndex: 10,
                  bgcolor: 'background.paper',
                  borderRadius: '0 0 8px 8px',
                  boxShadow: '0 8px 24px rgba(0,0,0,0.4)',
                  px: 2,
                  py: 1,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 0.5,
                  borderTop: '1px solid rgba(255,255,255,0.08)',
                }}>
                  {sortedVariants.slice(MAX_VISIBLE_VARIANTS).map((variant) => (
                    <Button
                      key={variant.albumId}
                      component="a"
                      href={variant.purchaseUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      size="small"
                      variant="outlined"
                      endIcon={<OpenInNewIcon sx={{ fontSize: 14, color: '#f44336' }} />}
                      sx={{
                        justifyContent: 'space-between',
                        textTransform: 'none',
                        borderRadius: 5,
                        px: 1.5,
                        py: 0.4,
                        fontSize: '0.8rem',
                        fontWeight: 500,
                        borderColor: 'rgba(255,255,255,0.12)',
                        bgcolor: 'rgba(255,255,255,0.03)',
                        '&:hover': {
                          borderColor: 'rgba(255,255,255,0.3)',
                          bgcolor: 'rgba(255,255,255,0.07)'
                        }
                      }}
                    >
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%', mr: 1 }}>
                        <Typography variant="body2" sx={{ fontWeight: 700, whiteSpace: 'nowrap', fontSize: '0.8rem', color: 'white' }}>
                          {'\u20AC'}{variant.price.toFixed(2)}
                        </Typography>
                        <Typography variant="body2" noWrap sx={{ fontSize: '0.8rem', color: 'text.secondary', ml: 1 }}>
                          {variant.distributorName}
                        </Typography>
                      </Box>
                    </Button>
                  ))}
                </Box>
              )}
            </Box>
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
