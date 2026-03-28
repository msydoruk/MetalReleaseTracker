import { useMemo } from 'react';
import Container from '@mui/material/Container';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Card from '@mui/material/Card';
import CardMedia from '@mui/material/CardMedia';
import IconButton from '@mui/material/IconButton';
import Chip from '@mui/material/Chip';
import Button from '@mui/material/Button';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import CloseIcon from '@mui/icons-material/Close';
import CompareArrowsIcon from '@mui/icons-material/CompareArrows';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import { Link } from 'react-router-dom';
import { useCompare } from '../contexts/CompareContext';
import { useCurrency } from '../contexts/CurrencyContext';
import { useLanguage } from '../i18n/LanguageContext';
import usePageMeta from '../hooks/usePageMeta';
import BreadcrumbNav from '../components/BreadcrumbNav';

const STATUS_MAP = { 0: 'New', 1: 'Restock', 2: 'Pre-Order' };
const MEDIA_MAP = { 0: 'CD', 1: 'LP', 2: 'Tape' };

export default function ComparePage() {
  const { compareItems, removeFromCompare, clearCompare } = useCompare();
  const { format: formatPrice } = useCurrency();
  const { t } = useLanguage();
  usePageMeta(t('compare.title') || 'Compare Albums', t('compare.description') || 'Compare album prices and details side by side');

  const lowestPrice = useMemo(() => {
    if (compareItems.length === 0) return null;
    return Math.min(...compareItems.map((item) => item.price || Infinity));
  }, [compareItems]);

  if (compareItems.length === 0) {
    return (
      <Container maxWidth="lg" sx={{ py: 6, textAlign: 'center' }}>
        <BreadcrumbNav items={[{ label: t('compare.title') || 'Compare' }]} />
        <CompareArrowsIcon sx={{ fontSize: 80, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h5" gutterBottom>{t('compare.empty') || 'No albums to compare'}</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          {t('compare.emptyHint') || 'Add albums to compare from the album catalog'}
        </Typography>
        <Button component={Link} to="/albums" variant="contained">{t('compare.browseAlbums') || 'Browse Albums'}</Button>
      </Container>
    );
  }

  const rows = [
    { label: t('compare.band') || 'Band', render: (item) => (
      <Typography component={Link} to={`/bands/${item.bandSlug}`} variant="body2" sx={{ color: 'primary.main', textDecoration: 'none' }}>
        {item.bandName}
      </Typography>
    )},
    { label: t('compare.media') || 'Media', render: (item) => MEDIA_MAP[item.media] || '--' },
    { label: t('compare.year') || 'Year', render: (item) => item.originalYear || '--' },
    { label: t('compare.price') || 'Price', render: (item) => (
      <Typography variant="body2" sx={{ fontWeight: 700, color: item.price === lowestPrice ? 'success.main' : 'text.primary' }}>
        {item.price ? formatPrice(item.price) : '--'}
      </Typography>
    )},
    { label: t('compare.distributor') || 'Distributor', render: (item) => item.distributorName || '--' },
    { label: t('compare.status') || 'Status', render: (item) => (
      item.status !== undefined && item.status !== null
        ? <Chip label={STATUS_MAP[item.status] || item.status} size="small" />
        : '--'
    )},
    { label: t('compare.buy') || 'Buy', render: (item) => (
      item.purchaseUrl ? (
        <IconButton size="small" component="a" href={item.purchaseUrl} target="_blank" rel="noopener noreferrer">
          <OpenInNewIcon fontSize="small" />
        </IconButton>
      ) : '--'
    )},
  ];

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <BreadcrumbNav items={[{ label: t('compare.title') || 'Compare Albums' }]} />

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5" sx={{ fontWeight: 700 }}>
          {t('compare.title') || 'Compare Albums'} ({compareItems.length})
        </Typography>
        <Button size="small" variant="outlined" onClick={clearCompare} sx={{ textTransform: 'none' }}>
          {t('compare.clearAll') || 'Clear All'}
        </Button>
      </Box>

      <TableContainer component={Card} sx={{ overflowX: 'auto' }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ minWidth: 100, fontWeight: 600 }} />
              {compareItems.map((item) => (
                <TableCell key={item.id} align="center" sx={{ minWidth: 180 }}>
                  <Box sx={{ position: 'relative', display: 'inline-block' }}>
                    <CardMedia
                      component="img"
                      image={item.photoUrl}
                      alt={item.name}
                      sx={{ width: 120, height: 120, objectFit: 'cover', borderRadius: 1, mx: 'auto' }}
                    />
                    <IconButton
                      size="small"
                      onClick={() => removeFromCompare(item.id)}
                      sx={{
                        position: 'absolute', top: -8, right: -8,
                        bgcolor: 'error.main', color: 'white',
                        width: 22, height: 22, '&:hover': { bgcolor: 'error.dark' },
                      }}
                    >
                      <CloseIcon sx={{ fontSize: 14 }} />
                    </IconButton>
                  </Box>
                  <Typography
                    component={Link}
                    to={`/albums/${item.slug}`}
                    variant="subtitle2"
                    sx={{ display: 'block', mt: 1, color: 'text.primary', textDecoration: 'none', fontWeight: 600, '&:hover': { color: 'primary.main' } }}
                  >
                    {item.name}
                  </Typography>
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((row) => (
              <TableRow key={row.label}>
                <TableCell sx={{ fontWeight: 600, color: 'text.secondary' }}>{row.label}</TableCell>
                {compareItems.map((item) => (
                  <TableCell key={item.id} align="center">{row.render(item)}</TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Container>
  );
}
