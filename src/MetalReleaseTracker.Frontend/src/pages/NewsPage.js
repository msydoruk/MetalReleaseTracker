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

const FALLBACK_NEWS_EN = [
  {
    date: '2026-03-17',
    iconName: 'StoreIcon',
    chipLabel: 'New',
    chipColor: 'success',
    titleEn: '2 new distributors connected',
    contentEn:
      'We have added support for two new distributors: Werewolf (Poland) and Avantgarde Music / Sound Cave (Italy). The catalog now covers 9 distributors across Europe.',
  },
  {
    date: '2026-03-17',
    iconName: 'TrackChangesIcon',
    chipLabel: 'New',
    chipColor: 'success',
    titleEn: 'Catalog changelog page added',
    contentEn:
      'You can now track all catalog changes in real time on the "Changelog" page: new releases, price updates, and removed items. Access it through the navigation menu.',
  },
  {
    date: '2026-02-20',
    iconName: 'FavoriteIcon',
    chipLabel: 'New',
    chipColor: 'success',
    titleEn: 'Favorites feature added',
    contentEn:
      'You can now save your favorite albums! Sign in with Google, click the heart icon on any album card — and it will appear in your profile under the "Favorites" tab. We also added full-size cover image viewing and a feedback page.',
  },
  {
    date: '2026-02-17',
    iconName: 'RocketLaunchIcon',
    chipLabel: 'Upcoming',
    chipColor: 'info',
    titleEn: 'New features planned',
    contentEn:
      'We are working on expanding functionality: ability to subscribe to price updates, new catalog items, and notifications about removed products. Stay tuned!',
  },
  {
    date: '2026-02-17',
    iconName: 'BuildIcon',
    chipLabel: 'Test Mode',
    chipColor: 'warning',
    titleEn: 'Site is running in test mode',
    contentEn:
      'Metal Release Tracker is currently running in test mode. Bugs and data inaccuracies are possible. If you find an issue, we appreciate your feedback.',
  },
  {
    date: '2026-02-15',
    iconName: 'NewReleasesIcon',
    chipLabel: 'New',
    chipColor: 'success',
    titleEn: '4 new distributors connected',
    contentEn:
      'We have added support for four new distributors: Napalm Records, Season of Mist, Paragon Records, and Black Metal Store. The catalog keeps growing - we now track 7 distributors across Europe.',
  },
];

const FALLBACK_NEWS_UA = [
  {
    date: '2026-03-17',
    iconName: 'StoreIcon',
    chipLabel: 'Нове',
    chipColor: 'success',
    titleUa: 'Підключено 2 нових дистриб\'ютори',
    contentUa:
      'Додано підтримку двох нових дистриб\'юторів: Werewolf (Польща) та Avantgarde Music / Sound Cave (Італія). Тепер каталог охоплює 9 дистриб\'юторів по всій Європі.',
  },
  {
    date: '2026-03-17',
    iconName: 'TrackChangesIcon',
    chipLabel: 'Нове',
    chipColor: 'success',
    titleUa: 'Додано сторінку оновлень каталогу',
    contentUa:
      'Тепер ви можете відстежувати всі зміни в каталозі в реальному часі на сторінці "Журнал змін": нові релізи, оновлення цін та видалені позиції. Сторінка доступна через навігаційне меню.',
  },
  {
    date: '2026-02-20',
    iconName: 'FavoriteIcon',
    chipLabel: 'Нове',
    chipColor: 'success',
    titleUa: 'Додано функціонал "Вибране"',
    contentUa:
      'Тепер ви можете зберігати улюблені альбоми! Увійдіть через Google, натисніть на серце на картці альбому — і він з\'явиться у вашому кабінеті на вкладці "Вибране". Також додано перегляд обкладинок у повному розмірі та сторінку зворотного зв\'язку.',
  },
  {
    date: '2026-02-17',
    iconName: 'RocketLaunchIcon',
    chipLabel: 'Плани',
    chipColor: 'info',
    titleUa: 'Плануються нові можливості',
    contentUa:
      'Ми працюємо над розширенням функціоналу: можливість підписатися на оновлення цін, нові позиції в каталозі та сповіщення про видалені товари. Слідкуйте за оновленнями!',
  },
  {
    date: '2026-02-17',
    iconName: 'BuildIcon',
    chipLabel: 'Тестовий режим',
    chipColor: 'warning',
    titleUa: 'Сайт працює в тестовому режимі',
    contentUa:
      'Metal Release Tracker наразі працює в тестовому режимі. Можливі баги та неточності в даних. Якщо ви знайшли помилку - будемо вдячні за зворотний зв\'язок.',
  },
  {
    date: '2026-02-15',
    iconName: 'NewReleasesIcon',
    chipLabel: 'Нове',
    chipColor: 'success',
    titleUa: 'Підключено 4 нових дистриб\'ютори',
    contentUa:
      'Ми додали підтримку чотирьох нових дистриб\'юторів: Napalm Records, Season of Mist, Paragon Records та Black Metal Store. Тепер каталог стає ще більшим - відстежуємо 7 дистриб\'юторів по всій Європі.',
  },
];

const NewsPage = () => {
  const { language, t } = useLanguage();
  usePageMeta(t('pageMeta.newsTitle'), t('pageMeta.newsDescription'));

  const [newsItems, setNewsItems] = useState(null);
  const [loadingNews, setLoadingNews] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoadingNews(true);
    fetchPublicNews()
      .then((response) => {
        if (cancelled) return;
        const data = response.data;
        if (Array.isArray(data) && data.length > 0) {
          setNewsItems(data);
        } else {
          setNewsItems(null);
        }
        setLoadingNews(false);
      })
      .catch((fetchError) => {
        if (cancelled) return;
        console.error('Failed to fetch news, using fallback defaults:', fetchError);
        setError(fetchError);
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

  const getDisplayItems = () => {
    if (newsItems) {
      return newsItems.map(mapNewsItem);
    }
    const fallback = language === 'ua' ? FALLBACK_NEWS_UA : FALLBACK_NEWS_EN;
    return fallback.map(mapNewsItem);
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

  if (error && !newsItems) {
    // Fallback to hardcoded data on error - no visible error to user
  }

  const displayItems = getDisplayItems();

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
