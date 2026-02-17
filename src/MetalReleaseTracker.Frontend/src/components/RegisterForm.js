import React, { useState } from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Divider,
  Link,
  Alert
} from '@mui/material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import authService from '../services/auth';
import GoogleLoginButton from './GoogleLoginButton';
import { useLanguage } from '../i18n/LanguageContext';

const RegisterForm = () => {
  const { t } = useLanguage();
  const [email, setEmail] = useState('');
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    if (!email || !password || !confirmPassword) {
      setError(t('register.validationRequired'));
      setLoading(false);
      return;
    }

    if (password !== confirmPassword) {
      setError(t('register.validationMismatch'));
      setLoading(false);
      return;
    }

    try {
      await authService.register(email, password, confirmPassword, userName);
      navigate('/');
    } catch (error) {
      setError(error.toString());
      setLoading(false);
    }
  };

  const handleGoogleSuccess = () => {
    navigate('/');
  };

  const handleGoogleError = (error) => {
    setError(error);
  };

  return (
    <Box sx={{ maxWidth: 400, mx: 'auto', p: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom align="center">
        {t('register.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <GoogleLoginButton
        onSuccess={handleGoogleSuccess}
        onError={handleGoogleError}
      />

      <Divider sx={{ my: 3 }}>
        <Typography variant="body2" color="text.secondary">
          {t('register.or')}
        </Typography>
      </Divider>

      <Box component="form" onSubmit={handleRegister} noValidate sx={{ mt: 1 }}>
        <TextField
          margin="normal"
          required
          fullWidth
          id="email"
          label={t('register.emailLabel')}
          name="email"
          autoComplete="email"
          autoFocus
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={loading}
        />
        <TextField
          margin="normal"
          fullWidth
          id="userName"
          label={t('register.displayName')}
          name="userName"
          autoComplete="name"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          disabled={loading}
          helperText={t('register.displayNameHelper')}
        />
        <TextField
          margin="normal"
          required
          fullWidth
          name="password"
          label={t('register.passwordLabel')}
          type="password"
          id="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={loading}
        />
        <TextField
          margin="normal"
          required
          fullWidth
          name="confirmPassword"
          label={t('register.confirmPassword')}
          type="password"
          id="confirmPassword"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={loading}
        />
        <Button
          type="submit"
          fullWidth
          variant="contained"
          sx={{ mt: 3, mb: 2 }}
          disabled={loading}
        >
          {t('register.submit')}
        </Button>

        <Box sx={{ mt: 2, textAlign: 'center' }}>
          <Typography variant="body2">
            {t('register.hasAccount')}{' '}
            <Link component={RouterLink} to="/login" variant="body2">
              {t('register.signIn')}
            </Link>
          </Typography>
        </Box>
      </Box>
    </Box>
  );
};

export default RegisterForm;