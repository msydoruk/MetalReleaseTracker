import React from 'react';
import { Pagination as MuiPagination, Box, Typography, FormControl, Select, MenuItem, useMediaQuery, useTheme } from '@mui/material';
import { useLanguage } from '../i18n/LanguageContext';

const Pagination = ({
  currentPage,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
  onPageSizeChange
}) => {
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const handlePageChange = (event, page) => {
    if (onPageChange) {
      onPageChange(page);
    }
  };

  const handlePageSizeChange = (event) => {
    if (onPageSizeChange) {
      onPageSizeChange(event.target.value);
    }
  };

  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalItems);

  const showingText = t('pagination.showing')
    .replace('{start}', startItem)
    .replace('{end}', endItem)
    .replace('{total}', totalItems);

  return (
    <Box sx={{
      display: 'flex',
      flexDirection: { xs: 'column', sm: 'row' },
      alignItems: 'center',
      justifyContent: 'center',
      gap: { xs: 1.5, sm: 2 },
      mb: 2
    }}>
      <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
        {showingText}
      </Typography>

      <MuiPagination
        count={totalPages}
        page={currentPage}
        onChange={handlePageChange}
        color="primary"
        size={isMobile ? 'small' : 'medium'}
        showFirstButton
        showLastButton
      />

      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography variant="body2" color="text.secondary" mr={1} sx={{ whiteSpace: 'nowrap' }}>
          {t('pagination.itemsPerPage')}
        </Typography>
        <FormControl size="small" sx={{ minWidth: 70 }}>
          <Select
            value={pageSize}
            onChange={handlePageSizeChange}
            variant="outlined"
            sx={{ height: 32 }}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={20}>20</MenuItem>
            <MenuItem value={50}>50</MenuItem>
          </Select>
        </FormControl>
      </Box>
    </Box>
  );
};

export default Pagination;
