import React from 'react';
import { Pagination as MuiPagination, Box, Typography, FormControl, Select, MenuItem } from '@mui/material';

const Pagination = ({ 
  currentPage, 
  totalPages, 
  totalItems, 
  pageSize, 
  onPageChange, 
  onPageSizeChange 
}) => {
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

  return (
    <Box sx={{ 
      display: 'flex', 
      flexDirection: { xs: 'column', sm: 'row' }, 
      justifyContent: 'space-between',
      alignItems: { xs: 'center', sm: 'flex-end' },
      mb: 2
    }}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: { xs: 2, sm: 0 } }}>
        Showing {startItem}-{endItem} of {totalItems} items
      </Typography>
      
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="body2" color="text.secondary" mr={1}>
            Items per page:
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
        
        <MuiPagination 
          count={totalPages} 
          page={currentPage} 
          onChange={handlePageChange} 
          color="primary" 
          size="medium"
          showFirstButton 
          showLastButton
        />
      </Box>
    </Box>
  );
};

export default Pagination; 