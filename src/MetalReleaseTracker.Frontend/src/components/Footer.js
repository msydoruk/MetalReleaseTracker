import React from 'react';
import { Box, Container, Typography, Divider, Link as MuiLink, Stack } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { useLanguage } from '../i18n/LanguageContext';

const Footer = () => {
  const { t } = useLanguage();
  const currentYear = new Date().getFullYear();

  return (
    <Box component="footer" sx={{ mt: 'auto' }}>
      <Divider sx={{ borderColor: 'rgba(255, 255, 255, 0.1)' }} />
      <Container maxWidth="xl">
        <Box sx={{ py: 3, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1.5 }}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={{ xs: 1, sm: 3 }}
            alignItems="center"
          >
            <MuiLink
              component={RouterLink}
              to="/albums"
              color="text.secondary"
              underline="hover"
              variant="body2"
            >
              {t('nav.albums')}
            </MuiLink>
            <MuiLink
              component={RouterLink}
              to="/bands"
              color="text.secondary"
              underline="hover"
              variant="body2"
            >
              {t('nav.bands')}
            </MuiLink>
            <MuiLink
              component={RouterLink}
              to="/distributors"
              color="text.secondary"
              underline="hover"
              variant="body2"
            >
              {t('nav.distributors')}
            </MuiLink>
            <MuiLink
              component={RouterLink}
              to="/about"
              color="text.secondary"
              underline="hover"
              variant="body2"
            >
              {t('nav.about')}
            </MuiLink>
            <MuiLink
              href="mailto:metal.release.tracker@gmail.com?subject=Distributor Suggestion"
              color="text.secondary"
              underline="hover"
              variant="body2"
            >
              {t('footer.suggestDistributor')}
            </MuiLink>
          </Stack>
          <Typography variant="body2" color="text.secondary">
            {currentYear} Metal Release Tracker. {t('footer.rights')}
          </Typography>
        </Box>
      </Container>
    </Box>
  );
};

export default Footer;
