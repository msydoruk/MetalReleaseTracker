import React from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Grid,
  Divider
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import PublicIcon from '@mui/icons-material/Public';
import LocalShippingIcon from '@mui/icons-material/LocalShipping';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import TrackChangesIcon from '@mui/icons-material/TrackChanges';
import GroupsIcon from '@mui/icons-material/Groups';
import { useLanguage } from '../i18n/LanguageContext';
import usePageMeta from '../hooks/usePageMeta';

const featureIcons = [
  <SearchIcon sx={{ fontSize: 40 }} />,
  <PublicIcon sx={{ fontSize: 40 }} />,
  <LocalShippingIcon sx={{ fontSize: 40 }} />,
  <TrackChangesIcon sx={{ fontSize: 40 }} />,
  <LibraryMusicIcon sx={{ fontSize: 40 }} />,
  <GroupsIcon sx={{ fontSize: 40 }} />,
];

const featureKeys = ['discover', 'globalReach', 'orderDirect', 'stayUpdated', 'allFormats', 'forCommunity'];

const AboutPage = () => {
  const { t } = useLanguage();
  usePageMeta('About - Ukrainian Metal Release Tracker', 'Metal Release Tracker aggregates Ukrainian metal releases from foreign distributors and labels into one searchable catalog.');

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      {/* Hero */}
      <Box sx={{ textAlign: 'center', mb: 6 }}>
        <Typography variant="h3" component="h1" sx={{ fontWeight: 800, mb: 2 }}>
          {t('about.title')} {'\uD83C\uDDFA\uD83C\uDDE6'}
        </Typography>
        <Typography variant="h5" color="text.secondary" sx={{ mb: 3, maxWidth: 700, mx: 'auto', lineHeight: 1.6 }}>
          {t('about.heroSubtitle')}
        </Typography>
        <Divider sx={{ maxWidth: 100, mx: 'auto', borderColor: 'primary.main', borderWidth: 2 }} />
      </Box>

      {/* Problem & Solution */}
      <Paper sx={{ p: 4, mb: 6, borderLeft: '4px solid', borderColor: 'primary.main' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          {t('about.problemTitle')}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3, lineHeight: 1.8 }}>
          {t('about.problemText')}
        </Typography>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          {t('about.solutionTitle')}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8 }}>
          {t('about.solutionText')}
        </Typography>
      </Paper>

      {/* Features Grid */}
      <Typography variant="h4" sx={{ fontWeight: 700, mb: 4, textAlign: 'center' }}>
        {t('about.howItWorks')}
      </Typography>
      <Grid container spacing={3} sx={{ mb: 6 }}>
        {featureKeys.map((key, index) => (
          <Grid key={key} size={{ xs: 12, sm: 6, md: 4 }}>
            <Paper sx={{
              p: 3,
              height: '100%',
              textAlign: 'center',
              transition: 'transform 0.2s',
              '&:hover': { transform: 'translateY(-4px)' }
            }}>
              <Box sx={{ color: 'primary.main', mb: 2 }}>
                {featureIcons[index]}
              </Box>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                {t(`about.features.${key}.title`)}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>
                {t(`about.features.${key}.description`)}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>

      {/* Currently Tracking */}
      <Paper sx={{ p: 4, mb: 6, textAlign: 'center' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          {t('about.networkTitle')}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 700, mx: 'auto' }}>
          {t('about.networkText')}
        </Typography>
      </Paper>

      {/* Call to Action */}
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          {t('about.supportTitle')}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 600, mx: 'auto' }}>
          {t('about.supportText')}
        </Typography>
      </Box>
    </Container>
  );
};

export default AboutPage;
