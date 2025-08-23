import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, Typography, CircularProgress } from '@mui/material';
import authService from '../services/auth';

const LoginCallback = () => {
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const processLoginCallback = async () => {
      try {
        console.log('Processing login callback...');
        await authService.handleLoginCallback();
        navigate('/');
      } catch (e) {
        console.error('Login callback error:', e);
        setError(e.message || 'Authentication failed');
      }
    };

    processLoginCallback();
  }, [navigate]);

  if (error) {
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
        <Typography variant="h5" color="error" gutterBottom>
          Authentication Error
        </Typography>
        <Typography variant="body1">{error}</Typography>
      </Box>
    );
  }

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
        Completing login process...
      </Typography>
    </Box>
  );
};

export default LoginCallback; 