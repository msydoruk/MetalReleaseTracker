import { useState, useEffect, useCallback, useRef } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import InputAdornment from '@mui/material/InputAdornment';
import { DataGrid } from '@mui/x-data-grid';
import SearchIcon from '@mui/icons-material/Search';
import PageHeader from '../components/PageHeader';
import LanguageTabs from '../components/LanguageTabs';
import { fetchTranslations, updateTranslation } from '../api/translations';

const CATEGORY_OPTIONS = [
  { value: '', label: 'All Categories' },
  { value: 'general', label: 'General' },
  { value: 'navigation', label: 'Navigation' },
  { value: 'albums', label: 'Albums' },
  { value: 'bands', label: 'Bands' },
  { value: 'auth', label: 'Auth' },
  { value: 'errors', label: 'Errors' },
];

export default function TranslationsPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [languageFilter, setLanguageFilter] = useState('en');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });
  const searchTimeoutRef = useRef(null);

  const showSnackbar = useCallback((message, severity = 'success') => {
    setSnackbar({ open: true, message, severity });
  }, []);

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const params = {
        page: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        language: languageFilter,
      };
      if (search) {
        params.search = search;
      }
      if (categoryFilter) {
        params.category = categoryFilter;
      }
      const { data } = await fetchTranslations(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load translations', 'error');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search, categoryFilter, languageFilter, showSnackbar]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleSearchChange = useCallback((e) => {
    const value = e.target.value;
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    searchTimeoutRef.current = setTimeout(() => {
      setSearch(value);
      setPaginationModel((prev) => ({ ...prev, page: 0 }));
    }, 400);
  }, []);

  const handleCategoryChange = useCallback((e) => {
    setCategoryFilter(e.target.value);
    setPaginationModel((prev) => ({ ...prev, page: 0 }));
  }, []);

  const handleLanguageChange = useCallback(
    (newLanguage) => {
      setLanguageFilter(newLanguage);
      setPaginationModel((prev) => ({ ...prev, page: 0 }));
    },
    []
  );

  const processRowUpdate = useCallback(
    async (newRow, oldRow) => {
      if (newRow.value === oldRow.value) {
        return oldRow;
      }
      try {
        await updateTranslation(newRow.id, { value: newRow.value });
        showSnackbar('Translation updated');
        return newRow;
      } catch (err) {
        showSnackbar(err.response?.data?.message || 'Failed to update translation', 'error');
        return oldRow;
      }
    },
    [showSnackbar]
  );

  const handleProcessRowUpdateError = useCallback(
    (err) => {
      showSnackbar(err.message || 'Failed to update translation', 'error');
    },
    [showSnackbar]
  );

  const columns = [
    { field: 'key', headerName: 'Key', flex: 1, minWidth: 250 },
    { field: 'language', headerName: 'Language', width: 100, align: 'center', headerAlign: 'center' },
    {
      field: 'value',
      headerName: 'Value',
      flex: 1.5,
      minWidth: 300,
      editable: true,
    },
    { field: 'category', headerName: 'Category', width: 140 },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader title="Translations" subtitle="Manage translations - click a cell to edit" />

      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        <TextField
          placeholder="Search by key..."
          size="small"
          onChange={handleSearchChange}
          sx={{ width: 280 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" sx={{ color: 'rgba(255,255,255,0.4)' }} />
                </InputAdornment>
              ),
            },
          }}
        />
        <TextField
          select
          size="small"
          value={categoryFilter}
          onChange={handleCategoryChange}
          sx={{ width: 180 }}
          label="Category"
        >
          {CATEGORY_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {opt.label}
            </MenuItem>
          ))}
        </TextField>
        <LanguageTabs value={languageFilter} onChange={handleLanguageChange} />
      </Box>

      <Box sx={{ flexGrow: 1, minHeight: 0 }}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          rowCount={totalRows}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 25, 50]}
          processRowUpdate={processRowUpdate}
          onProcessRowUpdateError={handleProcessRowUpdateError}
          disableRowSelectionOnClick
          autoHeight={false}
          sx={{ height: '100%' }}
        />
      </Box>

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
