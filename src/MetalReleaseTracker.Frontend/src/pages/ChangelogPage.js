import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  CircularProgress,
  Alert,
  Link,
  useMediaQuery,
  useTheme
} from '@mui/material';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import { fetchChangelog } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';
import { useCurrency } from '../contexts/CurrencyContext';
import usePageMeta from '../hooks/usePageMeta';
import Pagination from '../components/Pagination';

const statusConfig = {
  New: { color: 'success', translationKey: 'statusNew' },
  Updated: { color: 'warning', translationKey: 'statusUpdated' },
  Deleted: { color: 'error', translationKey: 'statusDeleted' },
};

const getChangeReasons = (item) => {
  if (item.changeType !== 'Updated') return null;

  if (item.changeReason) {
    if (item.changeReason === 'PriceAndStatusChange') {
      const priceDir = item.oldPrice != null && item.price < item.oldPrice ? 'priceDown' : 'priceUp';
      return [priceDir, 'statusChange'];
    }

    if (item.changeReason === 'PriceChange') {
      return [item.oldPrice != null && item.price < item.oldPrice ? 'priceDown' : 'priceUp'];
    }

    if (item.changeReason === 'StatusChange') {
      return ['statusChange'];
    }
  }

  const reasons = [];

  if (item.oldPrice != null && item.oldPrice !== item.price) {
    reasons.push(item.price < item.oldPrice ? 'priceDown' : 'priceUp');
  }

  if (item.oldStockStatus && item.stockStatus && item.oldStockStatus !== item.stockStatus) {
    reasons.push('statusChange');
  }

  return reasons.length > 0 ? reasons : null;
};

const reasonConfig = {
  priceDown: { color: 'success', translationKey: 'reasonPriceDown' },
  priceUp: { color: 'warning', translationKey: 'reasonPriceUp' },
  statusChange: { color: 'info', translationKey: 'reasonStatusChange' },
};

const ChangelogPage = () => {
  const { t } = useLanguage();
  const { format: formatPrice } = useCurrency();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  usePageMeta(t('pageMeta.changelogTitle'), t('pageMeta.changelogDescription'));

  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const loadChangelog = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await fetchChangelog(page, pageSize);
      setData(response.data);
    } catch (err) {
      setError(t('changelog.error'));
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, t]);

  useEffect(() => {
    loadChangelog();
  }, [loadChangelog]);

  const handlePageChange = (newPage) => {
    setPage(newPage);
  };

  const handlePageSizeChange = (newPageSize) => {
    setPageSize(newPageSize);
    setPage(1);
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatChangeDetail = (item) => {
    if (item.changeType === 'Deleted') return '\u2014';

    const parts = [];

    if (item.changeType === 'Updated' && item.oldPrice != null && item.oldPrice !== item.price) {
      parts.push(`${formatPrice(item.oldPrice)} \u2192 ${formatPrice(item.price)}`);
    } else if (item.price > 0) {
      parts.push(formatPrice(item.price));
    }

    if (item.changeType === 'Updated' && item.oldStockStatus && item.stockStatus
        && item.oldStockStatus !== item.stockStatus) {
      parts.push(`${item.oldStockStatus} \u2192 ${item.stockStatus}`);
    }

    return parts.length > 0 ? parts.join(' | ') : formatPrice(item.price);
  };

  const getStatusChips = (item) => {
    const reasons = getChangeReasons(item);

    if (reasons) {
      return (
        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
          {reasons.map((reason) => {
            const config = reasonConfig[reason];
            const label = t(`changelog.${config.translationKey}`) || reason;
            return <Chip key={reason} label={label} color={config.color} size="small" />;
          })}
        </Box>
      );
    }

    const config = statusConfig[item.changeType] || { color: 'default', translationKey: item.changeType };
    const label = t(`changelog.${config.translationKey}`) || item.changeType;
    return <Chip label={label} color={config.color} size="small" />;
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" sx={{ mb: 1 }}>
          {t('changelog.title')}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {t('changelog.subtitle')}
        </Typography>
      </Box>

      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {!loading && !error && data && data.items.length === 0 && (
        <Paper sx={{ p: 4, my: 3, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary">
            {t('changelog.empty')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {t('changelog.emptyHint')}
          </Typography>
        </Paper>
      )}

      {!loading && !error && data && data.items.length > 0 && (
        <>
          {isMobile ? (
            <Box sx={{ mb: 3, display: 'flex', flexDirection: 'column', gap: 1 }}>
              {data.items.map((item) => (
                <Paper key={item.id} sx={{ p: 1.5 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                    <Typography variant="caption" color="text.secondary">
                      {formatDate(item.changedAt)}
                    </Typography>
                    {getStatusChips(item)}
                  </Box>
                  <Typography variant="subtitle2">{item.bandName}</Typography>
                  <Typography variant="body2">
                    {item.purchaseUrl && item.changeType !== 'Deleted' ? (
                      <Link
                        href={item.purchaseUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        color="primary"
                        underline="hover"
                        sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5 }}
                      >
                        {item.albumName}
                        <OpenInNewIcon sx={{ fontSize: 14 }} />
                      </Link>
                    ) : (
                      item.albumName
                    )}
                  </Typography>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 0.5 }}>
                    <Typography variant="caption" color="text.secondary">
                      {item.distributorName}
                    </Typography>
                    {item.changeType !== 'Deleted' && (
                      <Typography variant="caption" color="text.secondary">
                        {formatChangeDetail(item)}
                      </Typography>
                    )}
                  </Box>
                </Paper>
              ))}
            </Box>
          ) : (
            <TableContainer component={Paper} sx={{ mb: 3 }}>
              <Table size="medium">
                <TableHead>
                  <TableRow>
                    <TableCell>{t('changelog.date')}</TableCell>
                    <TableCell>{t('changelog.band')}</TableCell>
                    <TableCell>{t('changelog.album')}</TableCell>
                    <TableCell>{t('changelog.price')}</TableCell>
                    <TableCell>{t('changelog.distributor')}</TableCell>
                    <TableCell>{t('changelog.change')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.items.map((item) => (
                    <TableRow key={item.id} hover>
                      <TableCell sx={{ whiteSpace: 'nowrap' }}>
                        {formatDate(item.changedAt)}
                      </TableCell>
                      <TableCell>{item.bandName}</TableCell>
                      <TableCell>
                        {item.purchaseUrl && item.changeType !== 'Deleted' ? (
                          <Link
                            href={item.purchaseUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            color="primary"
                            underline="hover"
                            sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5 }}
                          >
                            {item.albumName}
                            <OpenInNewIcon sx={{ fontSize: 14 }} />
                          </Link>
                        ) : (
                          item.albumName
                        )}
                      </TableCell>
                      <TableCell>
                        {formatChangeDetail(item)}
                      </TableCell>
                      <TableCell>{item.distributorName}</TableCell>
                      <TableCell>{getStatusChips(item)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          <Pagination
            currentPage={data.currentPage}
            totalPages={data.pageCount}
            totalItems={data.totalCount}
            pageSize={data.pageSize}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
          />
        </>
      )}
    </Container>
  );
};

export default ChangelogPage;
