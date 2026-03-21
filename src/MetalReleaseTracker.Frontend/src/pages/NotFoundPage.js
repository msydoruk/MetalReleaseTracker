import React from 'react';
import { Box, Container, Typography, Button, Stack } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';
import HomeIcon from '@mui/icons-material/Home';
import AlbumIcon from '@mui/icons-material/Album';
import { useLanguage } from '../i18n/LanguageContext';
import usePageMeta from '../hooks/usePageMeta';

const NotFoundPage = () => {
  const { t } = useLanguage();
  usePageMeta(t('pageMeta.notFoundTitle'));

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '60vh',
          textAlign: 'center',
          py: 8,
        }}
      >
        <Typography
          variant="h1"
          sx={{
            fontSize: { xs: '6rem', sm: '8rem' },
            fontWeight: 700,
            color: 'primary.main',
            lineHeight: 1,
            mb: 2,
          }}
        >
          {t('notFound.title')}
        </Typography>
        <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>
          {t('notFound.heading')}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          {t('notFound.message')}
        </Typography>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <Button
            component={RouterLink}
            to="/"
            variant="contained"
            color="primary"
            startIcon={<HomeIcon />}
            size="large"
          >
            {t('notFound.backHome')}
          </Button>
          <Button
            component={RouterLink}
            to="/albums"
            variant="outlined"
            color="secondary"
            startIcon={<AlbumIcon />}
            size="large"
          >
            {t('notFound.backAlbums')}
          </Button>
        </Stack>
      </Box>
    </Container>
  );
};

export default NotFoundPage;
