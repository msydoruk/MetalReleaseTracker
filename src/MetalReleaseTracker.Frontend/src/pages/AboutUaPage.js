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
import usePageMeta from '../hooks/usePageMeta';

const features = [
  {
    icon: <SearchIcon sx={{ fontSize: 40 }} />,
    title: 'Знаходь',
    description: 'Шукай фізичні релізи українських метал-гуртів, які продаються у закордонних дистриб\'юторів та лейблів.'
  },
  {
    icon: <PublicIcon sx={{ fontSize: 40 }} />,
    title: 'Весь світ',
    description: 'Ми збираємо каталоги дистриб\'юторів і лейблів з усієї Європи та світу в одному місці.'
  },
  {
    icon: <LocalShippingIcon sx={{ fontSize: 40 }} />,
    title: 'Замовляй напряму',
    description: 'Переходь прямо на сторінку товару у дистриб\'ютора та замовляй фізичні релізи без посередників.'
  },
  {
    icon: <TrackChangesIcon sx={{ fontSize: 40 }} />,
    title: 'Будь в курсі',
    description: 'Наші автоматичні парсери постійно сканують каталоги дистриб\'юторів - ти не пропустиш новинку, рестки чи передзамовлення.'
  },
  {
    icon: <LibraryMusicIcon sx={{ fontSize: 40 }} />,
    title: 'Всі формати',
    description: 'Вініл, CD, касети - переглядай релізи у всіх фізичних форматах в єдиному каталозі.'
  },
  {
    icon: <GroupsIcon sx={{ fontSize: 40 }} />,
    title: 'Для спільноти',
    description: 'Створено українськими металхедами для українських металхедів. Підтримуємо сцену, роблячи її музику доступнішою.'
  }
];

const AboutUaPage = () => {
  usePageMeta('Про сервіс - Трекер релізів українського металу', 'Metal Release Tracker збирає релізи українських метал-гуртів із закордонних дистриб\'юторів та лейблів в один каталог. Вініл, CD, касети - замовляй напряму.');

  return (
    <Container maxWidth="lg" sx={{ py: 6 }}>
      {/* Hero */}
      <Box sx={{ textAlign: 'center', mb: 6 }}>
        <Typography variant="h3" component="h1" sx={{ fontWeight: 800, mb: 2 }}>
          Metal Release Tracker {'\uD83C\uDDFA\uD83C\uDDE6'}
        </Typography>
        <Typography variant="h5" color="text.secondary" sx={{ mb: 3, maxWidth: 750, mx: 'auto', lineHeight: 1.6 }}>
          Єдиний хаб для відстеження релізів українського металу, що продаються закордонними дистриб'юторами та лейблами.
        </Typography>
        <Divider sx={{ maxWidth: 100, mx: 'auto', borderColor: 'primary.main', borderWidth: 2 }} />
      </Box>

      {/* Problem & Solution */}
      <Paper sx={{ p: 4, mb: 6, borderLeft: '4px solid', borderColor: 'primary.main' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Проблема
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3, lineHeight: 1.8 }}>
          Українські метал-гурти випускають потужну музику, але їхні фізичні релізи (вініл, CD, касети) часто
          розповсюджуються виключно через закордонні лейбли та дістро, розкидані по всій Європі. Щоб знайти
          де купити конкретний реліз, потрібно вручну перевіряти десятки онлайн-магазинів, багато з яких
          не мають фільтрів для українських гуртів. Релізи з'являються і зникають - і поки ти дізнаєшся про них,
          часто вже все розпродано.
        </Typography>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Рішення
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8 }}>
          Metal Release Tracker автоматично сканує каталоги закордонних дистриб'юторів та лейблів, знаходить
          кожен український метал-реліз і збирає їх у єдиний каталог з пошуком. Кожен реліз веде прямо на
          сторінку товару в магазині дистриб'ютора, де можна одразу зробити замовлення. Новинки, рестоки та
          передзамовлення відстежуються безперервно - ти завжди знатимеш, що доступно і де це дістати.
        </Typography>
      </Paper>

      {/* Features Grid */}
      <Typography variant="h4" sx={{ fontWeight: 700, mb: 4, textAlign: 'center' }}>
        Як це працює
      </Typography>
      <Grid container spacing={3} sx={{ mb: 6 }}>
        {features.map((feature, index) => (
          <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}>
            <Paper sx={{
              p: 3,
              height: '100%',
              textAlign: 'center',
              transition: 'transform 0.2s',
              '&:hover': { transform: 'translateY(-4px)' }
            }}>
              <Box sx={{ color: 'primary.main', mb: 2 }}>
                {feature.icon}
              </Box>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                {feature.title}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>
                {feature.description}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>

      {/* Currently Tracking */}
      <Paper sx={{ p: 4, mb: 6, textAlign: 'center' }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Зростаюча мережа дистриб'юторів
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 700, mx: 'auto' }}>
          Ми постійно додаємо нових закордонних дистриб'юторів та лейблів, які мають у каталозі українські
          метал-релізи. Наша автоматизована система моніторить їхні каталоги цілодобово, забезпечуючи
          найактуальнішу інформацію про наявність, ціни та нові надходження. Чим більше дістро ми відстежуємо -
          тим менше тобі треба шукати самому.
        </Typography>
      </Paper>

      {/* Call to Action */}
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h5" sx={{ fontWeight: 700, mb: 2 }}>
          Підтримай український метал
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ lineHeight: 1.8, maxWidth: 600, mx: 'auto' }}>
          Кожна покупка у легітимного дистриб'ютора підтримує українських артистів та глобальну метал-спільноту.
          Переглядай каталог, знаходь щось важке та замовляй напряму.
        </Typography>
      </Box>
    </Container>
  );
};

export default AboutUaPage;
