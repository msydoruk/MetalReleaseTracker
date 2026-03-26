import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Container, Typography, CircularProgress, Alert, Button, Box } from '@mui/material';
import { verifyEmail } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

export default function EmailVerifyPage() {
  const { token } = useParams();
  const { t } = useLanguage();
  const [status, setStatus] = useState('loading');

  useEffect(() => {
    verifyEmail(token)
      .then(() => setStatus('success'))
      .catch(() => setStatus('error'));
  }, [token]);

  return (
    <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
      {status === 'loading' && (
        <Box>
          <CircularProgress />
          <Typography variant="h6" sx={{ mt: 2 }}>
            {t('email.verifying')}
          </Typography>
        </Box>
      )}
      {status === 'success' && (
        <Box>
          <Alert severity="success" sx={{ mb: 3 }}>
            {t('email.verifySuccess')}
          </Alert>
          <Button variant="contained" component={Link} to="/profile">
            {t('email.goToProfile')}
          </Button>
        </Box>
      )}
      {status === 'error' && (
        <Box>
          <Alert severity="error" sx={{ mb: 3 }}>
            {t('email.verifyError')}
          </Alert>
          <Button variant="outlined" component={Link} to="/profile">
            {t('email.goToProfile')}
          </Button>
        </Box>
      )}
    </Container>
  );
}
