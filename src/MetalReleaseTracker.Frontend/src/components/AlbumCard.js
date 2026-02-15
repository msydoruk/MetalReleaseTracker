import React from 'react';
import { 
  Card, 
  CardContent, 
  CardMedia, 
  Typography, 
  CardActions, 
  Button, 
  Box, 
  Chip 
} from '@mui/material';
import { Link } from 'react-router-dom';

// Mapping for status colors
const statusColors = {
  0: 'success', // New
  1: 'info',    // Restock
  2: 'warning', // PreOrder
  Unavailable: 'error'
};

// Mapping for status labels
const statusLabels = {
  0: 'New',
  1: 'Restock',
  2: 'Pre-Order'
};

// Mapping for media type labels
const mediaTypeLabels = {
  0: 'CD',
  1: 'Vinyl',
  2: 'Cassette'
};

const AlbumCard = ({ album }) => {
  // Get status label (skip rendering if not in known statuses)
  const statusLabel = statusLabels[album.status];

  // Get media type label
  const mediaTypeLabel = mediaTypeLabels[album.media] || 'Unknown';
  
  return (
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
      <Box sx={{ position: 'relative' }}>
        <CardMedia
          component="img"
          height="220"
          image={album.photoUrl || 'https://via.placeholder.com/300x300?text=No+Image'}
          alt={album.name}
          sx={{ 
            objectFit: 'cover',
            transition: 'transform 0.3s ease',
            '&:hover': {
              transform: 'scale(1.05)'
            }
          }}
        />
        {statusLabel && (
          <Chip
            label={statusLabel}
            size="small"
            color={statusColors[album.status] || 'default'}
            sx={{
              position: 'absolute',
              top: 10,
              right: 10,
              fontWeight: 'bold',
              boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
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
            View in Store
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};

export default AlbumCard; 