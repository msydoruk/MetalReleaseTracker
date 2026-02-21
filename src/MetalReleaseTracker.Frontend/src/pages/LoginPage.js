import React from 'react';
import { Container, Paper } from '@mui/material';
import LoginForm from '../components/LoginForm';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';

const LoginPage = () => {
  const { t } = useLanguage();
  usePageMeta(t('pageMeta.loginTitle'));

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={3}>
        <LoginForm />
      </Paper>
    </Container>
  );
};

export default LoginPage; 