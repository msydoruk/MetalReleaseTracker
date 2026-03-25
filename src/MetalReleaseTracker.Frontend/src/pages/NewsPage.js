import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Chip,
  Divider,
  CircularProgress
} from '@mui/material';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import BuildIcon from '@mui/icons-material/Build';
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch';
import FavoriteIcon from '@mui/icons-material/Favorite';
import TrackChangesIcon from '@mui/icons-material/TrackChanges';
import StoreIcon from '@mui/icons-material/Store';
import { useLanguage } from '../i18n/LanguageContext';
import usePageMeta from '../hooks/usePageMeta';
import { fetchPublicNews } from '../services/api';

const ICON_MAP = {
  StoreIcon: StoreIcon,
  TrackChangesIcon: TrackChangesIcon,
  FavoriteIcon: FavoriteIcon,
  RocketLaunchIcon: RocketLaunchIcon,
  BuildIcon: BuildIcon,
  NewReleasesIcon: NewReleasesIcon,
};

const NewsPage = () => {
  const { language, t } = useLanguage();
  usePageMeta(t('pageMeta.newsTitle'), t('pageMeta.newsDescription'));

  const [newsItems, setNewsItems] = useState([]);
  const [loadingNews, setLoadingNews] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setLoadingNews(true);
    fetchPublicNews()
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (Array.isArray(data)) {
          setNewsItems(data);
        }
        setLoadingNews(false);
      })
      .catch((fetchError) => {
        if (cancelled) return;
        console.error('Failed to fetch news:', fetchError);
        setLoadingNews(false);
      });
    return () => { cancelled = true; };
  }, []);

  const mapNewsItem = (item) => {
    const IconComponent = ICON_MAP[item.iconName];
    return {
      date: item.date,
      icon: IconComponent ? <IconComponent sx={{ fontSize: 28 }} /> : <NewReleasesIcon sx={{ fontSize: 28 }} />,
      chipLabel: item.chipLabel,
      chipColor: item.chipColor,
      title: language === 'ua' ? (item.titleUa || item.titleEn) : (item.titleEn || item.titleUa),
      content: language === 'ua' ? (item.contentUa || item.contentEn) : (item.contentEn || item.contentUa),
    };
  };

  if (loadingNews) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  const displayItems = newsItems.map(mapNewsItem);

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 1 }}>
          {t('news.title')}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {t('news.subtitle')}
        </Typography>
      </Box>

      {displayItems.length === 0 && (
        <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
          {t('news.empty') !== 'news.empty' ? t('news.empty') : 'No news articles yet.'}
        </Typography>
      )}

      {displayItems.map((item, index) => (
        <Paper
          key={index}
          sx={{
            p: 3,
            mb: 3,
            borderLeft: '4px solid',
            borderColor: `${item.chipColor}.main`,
            transition: 'transform 0.2s',
            '&:hover': { transform: 'translateX(4px)' },
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2, gap: 1.5 }}>
            <Box sx={{ color: `${item.chipColor}.main` }}>{item.icon}</Box>
            <Typography variant="h6" sx={{ fontWeight: 600, flexGrow: 1 }}>
              {item.title}
            </Typography>
            <Chip label={item.chipLabel} color={item.chipColor} size="small" />
          </Box>
          <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, mb: 1.5 }}>
            {item.content}
          </Typography>
          <Divider sx={{ mb: 1 }} />
          <Typography variant="caption" color="text.secondary">
            {item.date}
          </Typography>
        </Paper>
      ))}
    </Container>
  );
};

export default NewsPage;
