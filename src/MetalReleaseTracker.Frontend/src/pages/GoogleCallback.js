import React, { useEffect } from 'react';
import { Box, Typography, CircularProgress } from '@mui/material';
import authService from '../services/auth';

const GoogleCallback = () => {
  useEffect(() => {
    authService.handleGoogleCallback();
  }, []);

  return (
    <Box 
      sx={{ 
        display: 'flex', 
        flexDirection: 'column',
        alignItems: 'center', 
        justifyContent: 'center', 
        minHeight: '50vh' 
      }}
    >
      <CircularProgress size={60} thickness={4} />
      <Typography variant="h6" sx={{ mt: 4 }}>
        Completing Google authentication...
      </Typography>
    </Box>
  );
};

export default GoogleCallback; 