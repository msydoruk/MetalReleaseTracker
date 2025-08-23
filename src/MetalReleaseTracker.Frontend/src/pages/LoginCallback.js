import React, { useEffect, useState } from 'react';
import { Box, Typography, CircularProgress, Button } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import authService from '../services/auth';

const ErrorDisplay = ({ error, onRetry }) => (
  <Box 
    sx={{ 
      display: 'flex', 
      flexDirection: 'column',
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '50vh',
      p: 3 
    }}
  >
    <Typography variant="h5" color="error" gutterBottom>
      Authentication Error
    </Typography>
    <Typography variant="body1" align="center" sx={{ mb: 3 }}>
      {error}
    </Typography>
    <Button 
      variant="contained" 
      color="primary" 
      onClick={onRetry}
      sx={{ mt: 2 }}
    >
      Try Again
    </Button>
  </Box>
);

const LoadingDisplay = () => (
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

const LoginCallback = () => {
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const handleRetry = () => {
    authService.clearStaleState().then(() => {
      navigate('/', { replace: true });
    });
  };

  useEffect(() => {
    const processCallback = async () => {
      try {
        console.log('Processing authentication callback');
        
        const user = await authService.handleLoginCallback();
        
        console.log('Login completed successfully', {
          hasUser: !!user,
          email: user?.profile?.email
        });
        
        navigate('/', { replace: true });
      } catch (error) {
        console.error('Error processing authentication callback', error);
        setError(error.message || 'Authentication failed');
        setLoading(false);
      }
    };

    processCallback();
  }, [navigate]);

  if (error) {
    return <ErrorDisplay error={error} onRetry={handleRetry} />;
  }

  return <LoadingDisplay />;
};

export default LoginCallback; 