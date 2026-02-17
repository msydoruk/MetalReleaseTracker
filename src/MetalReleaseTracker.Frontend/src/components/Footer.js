import React from 'react';
import { Box, Container, Typography, Divider } from '@mui/material';
import { useLanguage } from '../i18n/LanguageContext';

const Footer = () => {
  const { t } = useLanguage();
  const currentYear = new Date().getFullYear();

  return (
    <Box component="footer" sx={{ mt: 'auto' }}>
      <Divider sx={{ borderColor: 'rgba(255, 255, 255, 0.1)' }} />
      <Container maxWidth="xl">
        <Box sx={{ py: 3, textAlign: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            {currentYear} Metal Release Tracker. {t('footer.rights')}
          </Typography>
        </Box>
      </Container>
    </Box>
  );
};

export default Footer;
