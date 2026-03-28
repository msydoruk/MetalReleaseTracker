import React, { useState, useEffect } from 'react';
import {
  MenuItem,
  FormControl,
  Select,
  Button,
  Paper,
  Box,
  Typography,
  Slider,
  Divider,
  ToggleButton,
  ToggleButtonGroup,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormLabel,
  IconButton,
  TextField
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { fetchBands, fetchDistributors } from '../services/api';
import { ALBUM_SORT_FIELDS } from '../constants/albumSortFields';
import { useLanguage } from '../i18n/LanguageContext';

const AlbumFilter = ({ onFilterChange, onClose, initialFilters = {}, hideBandFilter = false, hideDistributorFilter = false }) => {
  const { t } = useLanguage();

  const [filters, setFilters] = useState({
    name: initialFilters.name || '',
    bandId: initialFilters.bandId || '',
    distributorId: initialFilters.distributorId || '',
    genre: initialFilters.genre || '',
    mediaType: initialFilters.mediaType || '',
    minPrice: initialFilters.minPrice || 0,
    maxPrice: initialFilters.maxPrice || 200,
    minYear: initialFilters.minYear || null,
    maxYear: initialFilters.maxYear || null,
    sortBy: initialFilters.sortBy ?? ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
    sortAscending: initialFilters.sortAscending ?? false,
    pageSize: initialFilters.pageSize || 20,
    ...initialFilters
  });

  const [bands, setBands] = useState([]);
  const [distributors, setDistributors] = useState([]);
  const [priceRange, setPriceRange] = useState([
    filters.minPrice || 0,
    filters.maxPrice || 200
  ]);

  // Fetch dropdown data
  useEffect(() => {
    const fetchFilterData = async () => {
      try {
        const [bandsResponse, distributorsResponse] = await Promise.all([
          fetchBands(),
          fetchDistributors(),
        ]);
        setBands(bandsResponse.data || []);
        setDistributors(distributorsResponse.data || []);
      } catch (error) {
        console.error('Error fetching filter data:', error);
      }
    };

    fetchFilterData();
  }, []);

  useEffect(() => {
    setFilters({
      name: initialFilters.name || '',
      bandId: initialFilters.bandId || '',
      distributorId: initialFilters.distributorId || '',
      genre: initialFilters.genre || '',
      mediaType: initialFilters.mediaType || '',
      minPrice: initialFilters.minPrice || 0,
      maxPrice: initialFilters.maxPrice || 200,
      minYear: initialFilters.minYear || null,
      maxYear: initialFilters.maxYear || null,
      sortBy: initialFilters.sortBy ?? ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
      sortAscending: initialFilters.sortAscending ?? false,
      pageSize: initialFilters.pageSize || 20,
      ...initialFilters
    });

    setPriceRange([
      initialFilters.minPrice || 0,
      initialFilters.maxPrice || 200
    ]);
  }, [initialFilters]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFilters({
      ...filters,
      [name]: value
    });
  };

  const handlePriceRangeChange = (event, newValue) => {
    setPriceRange(newValue);
  };

  const handlePriceRangeChangeCommitted = (event, newValue) => {
    setFilters({
      ...filters,
      minPrice: newValue[0],
      maxPrice: newValue[1]
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (onFilterChange) {
      onFilterChange({
        ...filters,
        minPrice: priceRange[0],
        maxPrice: priceRange[1]
      });
    }
  };

  const handleReset = () => {
    const resetFilters = {
      name: '',
      bandId: initialFilters.bandId || '',
      distributorId: initialFilters.distributorId || '',
      genre: '',
      mediaType: '',
      stockStatus: '',
      minPrice: 0,
      maxPrice: 200,
      minYear: null,
      maxYear: null,
      sortBy: ALBUM_SORT_FIELDS.ORIGINAL_YEAR,
      sortAscending: false,
      pageSize: 20
    };

    setFilters(resetFilters);
    setPriceRange([0, 200]);

    if (onFilterChange) {
      onFilterChange(resetFilters);
    }
  };

  const sortOptions = [
    { value: ALBUM_SORT_FIELDS.ORIGINAL_YEAR, label: t('albumFilter.sortYear') },
    { value: ALBUM_SORT_FIELDS.NAME, label: t('albumFilter.sortName') },
    { value: ALBUM_SORT_FIELDS.BAND, label: t('albumFilter.sortBand') },
    { value: ALBUM_SORT_FIELDS.PRICE, label: t('albumFilter.sortPrice') },
    { value: ALBUM_SORT_FIELDS.DATE_ADDED, label: t('albumFilter.sortDate') },
    { value: ALBUM_SORT_FIELDS.STORE_COUNT, label: t('albumFilter.sortStores') }
  ];

  return (
    <Paper sx={{
      height: '100%',
      mb: 0,
      borderRadius: 2,
      bgcolor: 'background.paper',
      boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)',
      border: 1,
      borderColor: 'divider',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column'
    }}>
      <Box sx={{ p: 3, pb: 0 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" component="h2" sx={{ color: 'text.primary', fontWeight: 'bold', whiteSpace: 'nowrap', fontSize: '1.1rem' }}>
            {t('albumFilter.filterAlbums')}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Button
              variant="outlined"
              size="small"
              onClick={handleReset}
              color="secondary"
              sx={{
                borderColor: 'divider',
                color: 'text.secondary',
                whiteSpace: 'nowrap',
                fontSize: '0.75rem',
                px: 1.5,
                '&:hover': {
                  borderColor: 'text.primary',
                  backgroundColor: 'action.hover'
                }
              }}
            >
              {t('albumFilter.resetFilters')}
            </Button>
            {onClose && (
              <IconButton onClick={onClose} sx={{ color: 'text.primary' }}>
                <CloseIcon />
              </IconButton>
            )}
          </Box>
        </Box>
        <Divider />
      </Box>

      <form onSubmit={handleSubmit} style={{ flex: 1, overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
        <Box sx={{ flex: 1, overflowY: 'auto', overflowX: 'hidden', px: 3, py: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Media type filter - using ToggleButtonGroup */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'text.primary',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.mediaType')}
            </FormLabel>
            <ToggleButtonGroup
              value={filters.mediaType}
              exclusive
              onChange={(e, newValue) => {
                if (newValue !== null) {
                  handleInputChange({
                    target: { name: 'mediaType', value: newValue }
                  });
                }
              }}
              aria-label="media type"
              fullWidth
              sx={{
                display: 'flex',
                '& .MuiToggleButton-root': {
                  color: 'text.primary',
                  borderColor: 'divider',
                  height: '40px',
                  fontSize: '0.85rem',
                  '&.Mui-selected': {
                    backgroundColor: 'primary.main',
                    color: 'primary.contrastText',
                    fontWeight: 'bold'
                  },
                  '&:hover': {
                    backgroundColor: 'action.hover'
                  }
                }
              }}
            >
              <ToggleButton value="" aria-label="all types">
                {t('albumFilter.all')}
              </ToggleButton>
              <ToggleButton value="CD" aria-label="cd">
                {t('albumFilter.cd')}
              </ToggleButton>
              <ToggleButton value="LP" aria-label="vinyl">
                {t('albumFilter.vinyl')}
              </ToggleButton>
              <ToggleButton value="Tape" aria-label="cassette">
                {t('albumFilter.cassette')}
              </ToggleButton>
            </ToggleButtonGroup>
          </Box>

          {/* Availability filter */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'text.secondary',
                fontSize: '0.875rem',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              Availability
            </FormLabel>
            <ToggleButtonGroup
              value={filters.stockStatus || ''}
              exclusive
              onChange={(e, newValue) => {
                handleInputChange({
                  target: { name: 'stockStatus', value: newValue ?? '' }
                });
              }}
              aria-label="availability"
              fullWidth
              sx={{
                display: 'flex',
                '& .MuiToggleButton-root': {
                  color: 'text.primary',
                  borderColor: 'divider',
                  height: '40px',
                  fontSize: '0.85rem',
                  '&.Mui-selected': {
                    backgroundColor: 'primary.main',
                    color: 'primary.contrastText',
                    fontWeight: 'bold'
                  },
                  '&:hover': {
                    backgroundColor: 'action.hover'
                  }
                }
              }}
            >
              <ToggleButton value="" aria-label="all">All</ToggleButton>
              <ToggleButton value="InStock" aria-label="in stock">In Stock</ToggleButton>
              <ToggleButton value="PreOrder" aria-label="pre-order">Pre-Order</ToggleButton>
            </ToggleButtonGroup>
          </Box>

          {/* Band filter - Simplified with autocomplete look */}
          {!hideBandFilter && (
            <Box>
              <FormLabel
                component="legend"
                sx={{
                  color: 'text.primary',
                  mb: 1,
                  fontWeight: 'medium'
                }}
              >
                {t('albumFilter.band')}
              </FormLabel>
              <FormControl
                fullWidth
                size="small"
                variant="outlined"
                sx={{
                  backgroundColor: 'action.hover',
                  width: '100%'
                }}
              >
                <Select
                  name="bandId"
                  value={filters.bandId}
                  onChange={handleInputChange}
                  displayEmpty
                  renderValue={(selected) => {
                    if (!selected) {
                      return <em style={{ opacity: 0.7 }}>{t('albumFilter.allBands')}</em>;
                    }
                    const selectedBand = bands.find(b => b.id === selected);
                    return selectedBand ? selectedBand.name : '';
                  }}
                  MenuProps={{
                    PaperProps: {
                      style: {
                        maxHeight: 300
                      }
                    }
                  }}
                  sx={{
                    '& .MuiSelect-select': {
                      py: 1,
                      color: 'text.primary',
                      fontWeight: 'medium',
                      height: '20px',
                      display: 'flex',
                      alignItems: 'center'
                    },
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'divider'
                    },
                    '&:hover .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'text.secondary'
                    },
                    height: '40px'
                  }}
                >
                  <MenuItem value="">{t('albumFilter.allBands')}</MenuItem>
                  {bands.map((band) => (
                    <MenuItem key={band.id} value={band.id}>
                      {band.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>
          )}

          {/* Distributor filter */}
          {!hideDistributorFilter && (
            <Box>
              <FormLabel
                component="legend"
                sx={{
                  color: 'text.primary',
                  mb: 1,
                  fontWeight: 'medium'
                }}
              >
                {t('albumFilter.distributor')}
              </FormLabel>
              <FormControl
                fullWidth
                size="small"
                variant="outlined"
                sx={{
                  backgroundColor: 'action.hover',
                  width: '100%'
                }}
              >
                <Select
                  name="distributorId"
                  value={filters.distributorId}
                  onChange={handleInputChange}
                  displayEmpty
                  renderValue={(selected) => {
                    if (!selected) {
                      return <em style={{ opacity: 0.7 }}>{t('albumFilter.allDistributors')}</em>;
                    }
                    const selectedDistributor = distributors.find(d => d.id === selected);
                    return selectedDistributor ? selectedDistributor.name : '';
                  }}
                  MenuProps={{
                    PaperProps: {
                      style: {
                        maxHeight: 300
                      }
                    }
                  }}
                  sx={{
                    '& .MuiSelect-select': {
                      py: 1,
                      color: 'text.primary',
                      fontWeight: 'medium',
                      height: '20px',
                      display: 'flex',
                      alignItems: 'center'
                    },
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'divider'
                    },
                    '&:hover .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'text.secondary'
                    },
                    height: '40px'
                  }}
                >
                  <MenuItem value="">{t('albumFilter.allDistributors')}</MenuItem>
                  {distributors.map((distributor) => (
                    <MenuItem key={distributor.id} value={distributor.id}>
                      {distributor.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>
          )}

          {/* Sort options - using RadioGroup for better UI */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'text.primary',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.sortBy')}
            </FormLabel>
            <Box sx={{
              display: 'flex',
              gap: 1,
              alignItems: 'stretch',
              flexWrap: 'wrap',
              flexDirection: { xs: 'column', sm: 'row' }
            }}>
              <RadioGroup
                row
                name="sortBy"
                value={filters.sortBy}
                onChange={handleInputChange}
                sx={{
                  flexGrow: 1,
                  flexShrink: 1,
                  minWidth: 0,
                  backgroundColor: 'action.hover',
                  border: 1,
                  borderColor: 'divider',
                  borderRadius: 1,
                  height: '40px',
                  px: 1.5,
                  flexWrap: 'nowrap',
                  alignItems: 'center',
                  justifyContent: 'space-around',
                  '& .MuiFormControlLabel-root': {
                    margin: 0,
                  },
                  '& .MuiRadio-root': {
                    color: 'text.secondary',
                    padding: '4px',
                    '&.Mui-checked': {
                      color: 'text.primary',
                    }
                  },
                  '& .MuiTypography-root': {
                    fontSize: '0.85rem',
                    color: 'text.primary',
                  }
                }}
              >
                {sortOptions.map(option => (
                  <FormControlLabel
                    key={option.value}
                    value={option.value}
                    control={<Radio size="small" />}
                    label={option.label}
                  />
                ))}
              </RadioGroup>
              <ToggleButtonGroup
                exclusive
                size="small"
                value={filters.sortAscending.toString()}
                onChange={(e, newValue) => {
                  if (newValue !== null) {
                    handleInputChange({
                      target: { name: 'sortAscending', value: newValue === 'true' }
                    });
                  }
                }}
                aria-label="sort order"
                fullWidth
                sx={{
                  flexShrink: 0,
                  '& .MuiToggleButton-root': {
                    color: 'text.primary',
                    borderColor: 'divider',
                    height: '40px',
                    fontSize: '0.85rem',
                    '&.Mui-selected': {
                      backgroundColor: 'rgba(25, 118, 210, 0.5)',
                      color: 'text.primary'
                    }
                  }
                }}
              >
                <ToggleButton value="false" aria-label="descending">
                  {'\u2193'} {t('albumFilter.desc')}
                </ToggleButton>
                <ToggleButton value="true" aria-label="ascending">
                  {'\u2191'} {t('albumFilter.asc')}
                </ToggleButton>
              </ToggleButtonGroup>
            </Box>
          </Box>

          {/* Year range filter */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'text.primary',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.yearRange')}
            </FormLabel>
            <Box sx={{ display: 'flex', gap: 1.5 }}>
              <TextField
                size="small"
                type="number"
                placeholder={t('albumFilter.minYear')}
                value={filters.minYear || ''}
                onChange={(event) => {
                  const value = event.target.value ? parseInt(event.target.value, 10) : null;
                  setFilters({ ...filters, minYear: value });
                }}
                inputProps={{ min: 1970, max: 2030 }}
                sx={{
                  flex: 1,
                  backgroundColor: 'action.hover',
                  '& .MuiOutlinedInput-root': {
                    height: '40px',
                    '& fieldset': { borderColor: 'divider' },
                    '&:hover fieldset': { borderColor: 'text.secondary' },
                  },
                  '& .MuiInputBase-input': { color: 'text.primary', fontSize: '0.85rem' },
                }}
              />
              <TextField
                size="small"
                type="number"
                placeholder={t('albumFilter.maxYear')}
                value={filters.maxYear || ''}
                onChange={(event) => {
                  const value = event.target.value ? parseInt(event.target.value, 10) : null;
                  setFilters({ ...filters, maxYear: value });
                }}
                inputProps={{ min: 1970, max: 2030 }}
                sx={{
                  flex: 1,
                  backgroundColor: 'action.hover',
                  '& .MuiOutlinedInput-root': {
                    height: '40px',
                    '& fieldset': { borderColor: 'divider' },
                    '&:hover fieldset': { borderColor: 'text.secondary' },
                  },
                  '& .MuiInputBase-input': { color: 'text.primary', fontSize: '0.85rem' },
                }}
              />
            </Box>
          </Box>

          {/* Price range filter */}
          <Box>
            <Typography sx={{ color: 'text.primary', mb: 1, fontWeight: 'medium' }}>
              {t('albumFilter.priceRange')}: {'\u20AC'}{priceRange[0]} - {'\u20AC'}{priceRange[1]}
            </Typography>
            <Slider
              value={priceRange}
              onChange={handlePriceRangeChange}
              onChangeCommitted={handlePriceRangeChangeCommitted}
              valueLabelDisplay="auto"
              min={0}
              max={200}
              sx={{
                mt: 1,
                width: '100%',
                color: 'primary.main',
                '& .MuiSlider-thumb': {
                  height: 20,
                  width: 20,
                  backgroundColor: '#fff',
                  boxShadow: '0 0 10px rgba(0, 0, 0, 0.3)',
                  '&:hover, &.Mui-active': {
                    boxShadow: '0 0 0 8px rgba(255, 255, 255, 0.16)',
                  }
                },
                '& .MuiSlider-rail': {
                  opacity: 0.5,
                }
              }}
            />
          </Box>
        </Box>

        <Box sx={{ p: 3, borderTop: 1, borderColor: 'divider' }}>
          <Button
            variant="contained"
            color="primary"
            type="submit"
            fullWidth
            sx={{
              fontWeight: 'bold',
              boxShadow: 2,
              '&:hover': {
                boxShadow: 4
              }
            }}
          >
            {t('albumFilter.applyFilters')}
          </Button>
        </Box>
      </form>
    </Paper>
  );
};

export default AlbumFilter;