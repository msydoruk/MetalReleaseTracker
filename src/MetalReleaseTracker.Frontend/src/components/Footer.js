import React from 'react';
import { Box, Container, Typography, Divider, Link as MuiLink, Stack } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import { useLanguage } from '../i18n/LanguageContext';
import { useNavigation } from '../contexts/NavigationContext';

const Footer = () => {
  const { language, t } = useLanguage();
  const { navItems } = useNavigation();
  const currentYear = new Date().getFullYear();

  const footerLinks = navItems
    .filter((item) => !item.isProtected && item.path !== '/')
    .map((item) => ({
      title: language === 'ua' ? item.titleUa : item.titleEn,
      path: item.path,
    }));

  return (
    <Box component="footer" sx={{ mt: 'auto' }}>
      <Divider sx={{ borderColor: 'rgba(255, 255, 255, 0.1)' }} />
      <Container maxWidth="xl">
        <Box sx={{ py: 3, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1.5 }}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={{ xs: 1, sm: 3 }}
            alignItems="center"
            sx={{ flexWrap: 'wrap', justifyContent: 'center' }}
          >
            {footerLinks.map((link) => (
              <MuiLink
                key={link.path}
                component={RouterLink}
                to={link.path}
                color="text.secondary"
                underline="hover"
                variant="body2"
                sx={{ minHeight: 44, display: 'flex', alignItems: 'center' }}
              >
                {link.title}
              </MuiLink>
            ))}
            <MuiLink
              href="mailto:metal.release.tracker@gmail.com?subject=Distributor Suggestion"
              color="text.secondary"
              underline="hover"
              variant="body2"
              sx={{ minHeight: 44, display: 'flex', alignItems: 'center' }}
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
