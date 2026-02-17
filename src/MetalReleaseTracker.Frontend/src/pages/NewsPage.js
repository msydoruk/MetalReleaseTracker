import React from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Chip,
  Divider
} from '@mui/material';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import BuildIcon from '@mui/icons-material/Build';
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch';
import { useLanguage } from '../i18n/LanguageContext';
import usePageMeta from '../hooks/usePageMeta';

const getNewsItems = (language) => {
  if (language === 'ua') {
    return [
      {
        date: '2026-02-17',
        icon: <RocketLaunchIcon sx={{ fontSize: 28 }} />,
        chipLabel: 'Плани',
        chipColor: 'info',
        title: 'Плануються нові можливості',
        content:
          'Ми працюємо над розширенням функціоналу: можливість підписатися на оновлення цін, нові позиції в каталозі та сповіщення про видалені товари. Слідкуйте за оновленнями!',
      },
      {
        date: '2026-02-17',
        icon: <BuildIcon sx={{ fontSize: 28 }} />,
        chipLabel: 'Тестовий режим',
        chipColor: 'warning',
        title: 'Сайт працює в тестовому режимі',
        content:
          'Metal Release Tracker наразі працює в тестовому режимі. Можливі баги та неточності в даних. Якщо ви знайшли помилку - будемо вдячні за зворотний зв\'язок.',
      },
      {
        date: '2026-02-15',
        icon: <NewReleasesIcon sx={{ fontSize: 28 }} />,
        chipLabel: 'Нове',
        chipColor: 'success',
        title: 'Підключено 4 нових дистриб\'ютори',
        content:
          'Ми додали підтримку чотирьох нових дистриб\'юторів: Napalm Records, Season of Mist, Paragon Records та Black Metal Store. Тепер каталог стає ще більшим - відстежуємо 7 дистриб\'юторів по всій Європі.',
      },
    ];
  }

  return [
    {
      date: '2026-02-17',
      icon: <RocketLaunchIcon sx={{ fontSize: 28 }} />,
      chipLabel: 'Upcoming',
      chipColor: 'info',
      title: 'New features planned',
      content:
        'We are working on expanding functionality: ability to subscribe to price updates, new catalog items, and notifications about removed products. Stay tuned!',
    },
    {
      date: '2026-02-17',
      icon: <BuildIcon sx={{ fontSize: 28 }} />,
      chipLabel: 'Test Mode',
      chipColor: 'warning',
      title: 'Site is running in test mode',
      content:
        'Metal Release Tracker is currently running in test mode. Bugs and data inaccuracies are possible. If you find an issue, we appreciate your feedback.',
    },
    {
      date: '2026-02-15',
      icon: <NewReleasesIcon sx={{ fontSize: 28 }} />,
      chipLabel: 'New',
      chipColor: 'success',
      title: '4 new distributors connected',
      content:
        'We have added support for four new distributors: Napalm Records, Season of Mist, Paragon Records, and Black Metal Store. The catalog keeps growing - we now track 7 distributors across Europe.',
    },
  ];
};

const NewsPage = () => {
  const { language, t } = useLanguage();
  usePageMeta('News - Metal Release Tracker', 'Latest news and updates from Metal Release Tracker.');

  const newsItems = getNewsItems(language);

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

      {newsItems.map((item, index) => (
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
