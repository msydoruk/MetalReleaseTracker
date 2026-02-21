import React from 'react';
import { Container, Paper } from '@mui/material';
import RegisterForm from '../components/RegisterForm';
import usePageMeta from '../hooks/usePageMeta';
import { useLanguage } from '../i18n/LanguageContext';

const RegisterPage = () => {
  const { t } = useLanguage();
  usePageMeta(t('pageMeta.registerTitle'));

  return (
    <Container maxWidth="sm" sx={{ mt: 4 }}>
      <Paper elevation={3}>
        <RegisterForm />
      </Paper>
    </Container>
  );
};

export default RegisterPage; 