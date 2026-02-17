import React, { useState, useEffect } from 'react';
import {
  Grid,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Button,
  Paper,
  Box,
  Typography,
  Slider,
  Divider,
  Chip,
  ToggleButton,
  ToggleButtonGroup,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormLabel
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { fetchBands, fetchDistributors } from '../services/api';
import { ALBUM_SORT_FIELDS, SORT_FIELD_NAMES } from '../constants/albumSortFields';
import { useLanguage } from '../i18n/LanguageContext';

const AlbumFilter = ({ onFilterChange, initialFilters = {} }) => {
  const { t } = useLanguage();

  const [filters, setFilters] = useState({
    name: initialFilters.name || '',
    bandId: initialFilters.bandId || '',
    distributorId: initialFilters.distributorId || '',
    mediaType: initialFilters.mediaType || '',
    status: initialFilters.status || '',
    minPrice: initialFilters.minPrice || 0,
    maxPrice: initialFilters.maxPrice || 200,
    sortBy: initialFilters.sortBy ?? ALBUM_SORT_FIELDS.RELEASE_DATE,
    sortAscending: initialFilters.sortAscending || false,
    pageSize: initialFilters.pageSize || 20,
    releaseDateFrom: initialFilters.releaseDateFrom || null,
    releaseDateTo: initialFilters.releaseDateTo || null,
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
          fetchDistributors()
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
      mediaType: initialFilters.mediaType || '',
      status: initialFilters.status || '',
      minPrice: initialFilters.minPrice || 0,
      maxPrice: initialFilters.maxPrice || 200,
      sortBy: initialFilters.sortBy ?? ALBUM_SORT_FIELDS.RELEASE_DATE,
      sortAscending: initialFilters.sortAscending || false,
      pageSize: initialFilters.pageSize || 20,
      releaseDateFrom: initialFilters.releaseDateFrom || null,
      releaseDateTo: initialFilters.releaseDateTo || null,
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

  const handleDateChange = (name, date) => {
    setFilters({
      ...filters,
      [name]: date
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
      bandId: '',
      distributorId: '',
      mediaType: '',
      status: '',
      minPrice: 0,
      maxPrice: 200,
      sortBy: ALBUM_SORT_FIELDS.RELEASE_DATE,
      sortAscending: false,
      pageSize: 20,
      releaseDateFrom: null,
      releaseDateTo: null
    };

    setFilters(resetFilters);
    setPriceRange([0, 200]);

    if (onFilterChange) {
      onFilterChange(resetFilters);
    }
  };

  const sortOptions = [
    { value: ALBUM_SORT_FIELDS.RELEASE_DATE, label: t('albumFilter.sortDate') },
    { value: ALBUM_SORT_FIELDS.NAME, label: t('albumFilter.sortName') },
    { value: ALBUM_SORT_FIELDS.PRICE, label: t('albumFilter.sortPrice') },
    { value: ALBUM_SORT_FIELDS.BAND, label: t('albumFilter.sortBand') },
    { value: ALBUM_SORT_FIELDS.DISTRIBUTOR, label: t('albumFilter.sortDistributor') }
  ];

  return (
    <Paper sx={{
      p: 3,
      mb: 0,
      borderRadius: 2,
      bgcolor: 'background.paper',
      boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)',
      border: '1px solid rgba(255, 255, 255, 0.1)'
    }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6" component="h2" sx={{ color: 'white', fontWeight: 'bold' }}>
          {t('albumFilter.filterAlbums')}
        </Typography>
        <Button
          variant="outlined"
          size="small"
          onClick={handleReset}
          color="secondary"
          sx={{
            borderColor: 'rgba(255, 255, 255, 0.3)',
            color: '#fff',
            '&:hover': {
              borderColor: '#fff',
              backgroundColor: 'rgba(255, 255, 255, 0.05)'
            }
          }}
        >
          {t('albumFilter.resetFilters')}
        </Button>
      </Box>

      <Divider sx={{ mb: 3 }} />

      <form onSubmit={handleSubmit}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Search by name */}
          <TextField
            fullWidth
            label={t('albumFilter.albumName')}
            name="name"
            value={filters.name}
            onChange={handleInputChange}
            variant="outlined"
            size="small"
            placeholder={t('albumFilter.searchPlaceholder')}
            sx={{
              backgroundColor: 'rgba(255, 255, 255, 0.05)',
              borderRadius: 1,
              width: '100%',
              '& .MuiOutlinedInput-root': {
                '& fieldset': {
                  borderColor: 'rgba(255, 255, 255, 0.3)',
                },
                '&:hover fieldset': {
                  borderColor: 'rgba(255, 255, 255, 0.5)',
                },
                '&.Mui-focused fieldset': {
                  borderColor: 'primary.main',
                },
                height: '40px'
              },
              '& .MuiInputLabel-root': {
                color: 'white',
                fontWeight: 'medium',
              },
              '& .MuiInputBase-input': {
                py: 1,
                color: 'white',
              }
            }}
          />

          {/* Release Date Range - updated component */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.releaseDateRange')}
            </FormLabel>
            <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
              <DatePicker
                label={t('albumFilter.from')}
                value={filters.releaseDateFrom}
                onChange={(date) => handleDateChange('releaseDateFrom', date)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    size="small"
                    fullWidth
                    sx={{
                      backgroundColor: 'rgba(255, 255, 255, 0.05)',
                      borderRadius: 1,
                      '& .MuiOutlinedInput-root': {
                        '& fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.3)',
                        },
                        '&:hover fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.5)',
                        },
                        '&.Mui-focused fieldset': {
                          borderColor: 'primary.main',
                        },
                        height: '40px'
                      },
                      '& .MuiInputLabel-root': {
                        color: 'white',
                        fontWeight: 'medium',
                      },
                      '& .MuiInputBase-input': {
                        py: 1,
                        color: 'white',
                      }
                    }}
                  />
                )}
              />
              <DatePicker
                label={t('albumFilter.to')}
                value={filters.releaseDateTo}
                onChange={(date) => handleDateChange('releaseDateTo', date)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    size="small"
                    fullWidth
                    sx={{
                      backgroundColor: 'rgba(255, 255, 255, 0.05)',
                      borderRadius: 1,
                      '& .MuiOutlinedInput-root': {
                        '& fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.3)',
                        },
                        '&:hover fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.5)',
                        },
                        '&.Mui-focused fieldset': {
                          borderColor: 'primary.main',
                        },
                        height: '40px'
                      },
                      '& .MuiInputLabel-root': {
                        color: 'white',
                        fontWeight: 'medium',
                      },
                      '& .MuiInputBase-input': {
                        py: 1,
                        color: 'white',
                      }
                    }}
                  />
                )}
              />
            </Box>
          </Box>

          {/* Media type filter - using ToggleButtonGroup */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
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
                  color: 'white',
                  borderColor: 'rgba(255, 255, 255, 0.3)',
                  '&.Mui-selected': {
                    backgroundColor: 'rgba(25, 118, 210, 0.5)',
                    color: 'white',
                    fontWeight: 'bold'
                  },
                  '&:hover': {
                    backgroundColor: 'rgba(255, 255, 255, 0.1)'
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

          {/* Status filter - using ToggleButtonGroup */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.status')}
            </FormLabel>
            <ToggleButtonGroup
              value={filters.status}
              exclusive
              onChange={(e, newValue) => {
                if (newValue !== null) {
                  handleInputChange({
                    target: { name: 'status', value: newValue }
                  });
                }
              }}
              aria-label="status"
              fullWidth
              sx={{
                display: 'flex',
                '& .MuiToggleButton-root': {
                  color: 'white',
                  borderColor: 'rgba(255, 255, 255, 0.3)',
                  '&.Mui-selected': {
                    backgroundColor: 'rgba(25, 118, 210, 0.5)',
                    color: 'white',
                    fontWeight: 'bold'
                  },
                  '&:hover': {
                    backgroundColor: 'rgba(255, 255, 255, 0.1)'
                  }
                }
              }}
            >
              <ToggleButton value="" aria-label="all statuses">
                {t('albumFilter.all')}
              </ToggleButton>
              <ToggleButton value="New" aria-label="new">
                {t('albumFilter.statusNew')}
              </ToggleButton>
              <ToggleButton value="Restock" aria-label="restock">
                {t('albumFilter.statusRestock')}
              </ToggleButton>
              <ToggleButton value="Preorder" aria-label="preorder">
                {t('albumFilter.statusPreorder')}
              </ToggleButton>
            </ToggleButtonGroup>
          </Box>

          {/* Band filter - Simplified with autocomplete look */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
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
                backgroundColor: 'rgba(255, 255, 255, 0.05)',
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
                      maxHeight: 300,
                      backgroundColor: '#222',
                      color: '#fff'
                    }
                  }
                }}
                sx={{
                  '& .MuiSelect-select': {
                    py: 1,
                    color: 'white',
                    fontWeight: 'medium',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center'
                  },
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255, 255, 255, 0.3)'
                  },
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255, 255, 255, 0.5)'
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

          {/* Distributor filter - Simplified with autocomplete look */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
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
                backgroundColor: 'rgba(255, 255, 255, 0.05)',
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
                  const selectedDist = distributors.find(d => d.id === selected);
                  return selectedDist ? selectedDist.name : '';
                }}
                MenuProps={{
                  PaperProps: {
                    style: {
                      maxHeight: 300,
                      backgroundColor: '#222',
                      color: '#fff'
                    }
                  }
                }}
                sx={{
                  '& .MuiSelect-select': {
                    py: 1,
                    color: 'white',
                    fontWeight: 'medium',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center'
                  },
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255, 255, 255, 0.3)'
                  },
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: 'rgba(255, 255, 255, 0.5)'
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

          {/* Sort options - using RadioGroup for better UI */}
          <Box>
            <FormLabel
              component="legend"
              sx={{
                color: 'white',
                mb: 1,
                fontWeight: 'medium'
              }}
            >
              {t('albumFilter.sortBy')}
            </FormLabel>
            <Box sx={{
              display: 'flex',
              gap: 2,
              alignItems: { xs: 'stretch', sm: 'center' },
              flexDirection: { xs: 'column', sm: 'row' }
            }}>
              <Box sx={{
                backgroundColor: 'rgba(255, 255, 255, 0.05)',
                p: 1.5,
                borderRadius: 1,
                flexGrow: 1
              }}>
                <RadioGroup
                  row
                  name="sortBy"
                  value={filters.sortBy}
                  onChange={handleInputChange}
                  sx={{
                    justifyContent: { xs: 'flex-start', sm: 'space-between' },
                    flexWrap: 'wrap',
                    gap: { xs: 0.5, sm: 0 },
                    '& .MuiFormControlLabel-root': {
                      margin: 0,
                    },
                    '& .MuiRadio-root': {
                      color: 'rgba(255, 255, 255, 0.5)',
                      padding: '4px',
                      '&.Mui-checked': {
                        color: 'white',
                      }
                    },
                    '& .MuiTypography-root': {
                      fontSize: '0.85rem',
                      color: 'white',
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
              </Box>
              <Box sx={{
                backgroundColor: 'rgba(255, 255, 255, 0.05)',
                p: 1,
                borderRadius: 1,
                minWidth: '100px'
              }}>
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
                    '& .MuiToggleButton-root': {
                      color: 'white',
                      borderColor: 'rgba(255, 255, 255, 0.3)',
                      padding: '4px 8px',
                      '&.Mui-selected': {
                        backgroundColor: 'rgba(25, 118, 210, 0.5)',
                        color: 'white'
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
          </Box>

          {/* Price range filter */}
          <Box>
            <Typography sx={{ color: 'white', mb: 1, fontWeight: 'medium' }}>
              {t('albumFilter.priceRange')}: ${priceRange[0]} - ${priceRange[1]}
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

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
            <Button
              variant="contained"
              color="primary"
              type="submit"
              sx={{
                minWidth: 120,
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
        </Box>
      </form>
    </Paper>
  );
};

export default AlbumFilter;