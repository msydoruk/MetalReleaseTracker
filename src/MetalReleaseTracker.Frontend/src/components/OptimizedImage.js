import React from 'react';
import { Box } from '@mui/material';

const placeholderImg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='300'%3E%3Crect width='300' height='300' fill='%23111'/%3E%3Cpath d='M162 100v66.5c-3.7-2.1-8-3.5-12.5-3.5-13.8 0-25 11.2-25 25s11.2 25 25 25 25-11.2 25-25V119h25v-19H162z' fill='%23333'/%3E%3C/svg%3E";

const OptimizedImage = ({
  imageSet,
  photoUrl,
  alt,
  sizes,
  loading = 'lazy',
  onClick,
  sx = {},
}) => {
  const src = photoUrl || placeholderImg;
  const hasWebP = imageSet?.small || imageSet?.medium || imageSet?.large;

  if (hasWebP) {
    const srcSetParts = [];
    if (imageSet.small) srcSetParts.push(`${imageSet.small} 180w`);
    if (imageSet.medium) srcSetParts.push(`${imageSet.medium} 350w`);
    if (imageSet.large) srcSetParts.push(`${imageSet.large} 500w`);
    if (imageSet.original) srcSetParts.push(`${imageSet.original} 1000w`);

    return (
      <Box
        component="picture"
        onClick={onClick}
        sx={{ display: 'block', cursor: onClick ? 'pointer' : 'default', ...sx }}
      >
        <source
          type="image/webp"
          srcSet={srcSetParts.join(', ')}
          sizes={sizes}
        />
        <Box
          component="img"
          src={src}
          alt={alt}
          loading={loading}
          sx={{
            width: '100%',
            height: '100%',
            objectFit: 'contain',
            display: 'block',
          }}
        />
      </Box>
    );
  }

  return (
    <Box
      component="img"
      src={src}
      alt={alt}
      loading={loading}
      onClick={onClick}
      sx={{
        objectFit: 'contain',
        cursor: onClick ? 'pointer' : 'default',
        ...sx,
      }}
    />
  );
};

export default OptimizedImage;
