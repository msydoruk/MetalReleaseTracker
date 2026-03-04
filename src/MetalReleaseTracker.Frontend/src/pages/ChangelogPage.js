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
import usePageMeta from '../hooks/usePageMeta';
import Pagination from '../components/Pagination';

const statusConfig = {
  New: { color: 'success', translationKey: 'statusNew' },
  Updated: { color: 'warning', translationKey: 'statusUpdated' },
  Deleted: { color: 'error', translationKey: 'statusDeleted' },
};

const ChangelogPage = () => {
  const { t } = useLanguage();
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
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusChip = (changeType) => {
    const config = statusConfig[changeType] || { color: 'default', translationKey: changeType };
    const label = t(`changelog.${config.translationKey}`) || changeType;
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
        <Paper sx={{ p: 4, textAlign: 'center' }}>
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
          <TableContainer component={Paper} sx={{ mb: 3 }}>
            <Table size={isMobile ? 'small' : 'medium'}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('changelog.date')}</TableCell>
                  <TableCell>{t('changelog.band')}</TableCell>
                  <TableCell>{t('changelog.album')}</TableCell>
                  {!isMobile && <TableCell>{t('changelog.price')}</TableCell>}
                  <TableCell>{t('changelog.distributor')}</TableCell>
                  <TableCell>{t('changelog.status')}</TableCell>
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
                    {!isMobile && (
                      <TableCell>
                        {item.changeType !== 'Deleted' ? `\u20AC${item.price.toFixed(2)}` : '\u2014'}
                      </TableCell>
                    )}
                    <TableCell>{item.distributorName}</TableCell>
                    <TableCell>{getStatusChip(item.changeType)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

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
