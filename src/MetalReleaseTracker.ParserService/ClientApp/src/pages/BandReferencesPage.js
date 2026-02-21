import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Link from '@mui/material/Link';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import SearchIcon from '@mui/icons-material/Search';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import { DataGrid } from '@mui/x-data-grid';
import PageHeader from '../components/PageHeader';
import FilterBar from '../components/FilterBar';
import { fetchBandReferences } from '../api/bandReferences';

const SORT_FIELD_MAP = { bandName: 0, genre: 1, lastSyncedAt: 2, discographyCount: 3 };

export default function BandReferencesPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState(searchParams.get('search') || '');

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);
  const sortBy = searchParams.get('sortBy') || '';
  const sortAscending = searchParams.get('sortAscending') !== 'false';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (sortBy && SORT_FIELD_MAP[sortBy] !== undefined) {
        params.sortBy = SORT_FIELD_MAP[sortBy];
        params.sortAscending = sortAscending;
      }
      const { data } = await fetchBandReferences(params);
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load band references', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, sortBy, sortAscending]);

  useEffect(() => { load(); }, [load]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => { if (!params[k] && params[k] !== 0) delete params[k]; });
    setSearchParams(params);
  };

  const columns = [
    {
      field: 'bandName',
      headerName: 'Band Name',
      flex: 1,
      minWidth: 200,
      renderCell: ({ value }) => (
        <Box sx={{ fontWeight: 500, color: 'rgba(255, 255, 255, 0.95)' }}>{value}</Box>
      ),
    },
    {
      field: 'metalArchivesId',
      headerName: 'Metal Archives',
      width: 130,
      renderCell: ({ value }) => (
        <Link
          href={`https://www.metal-archives.com/bands/_/${value}`}
          target="_blank"
          rel="noopener"
          onClick={(e) => e.stopPropagation()}
          sx={{ color: '#e53935', '&:hover': { color: '#ff5252' } }}
        >
          {value}
        </Link>
      ),
    },
    {
      field: 'genre',
      headerName: 'Genre',
      flex: 1,
      minWidth: 150,
      renderCell: ({ value }) => value ? (
        <Chip
          label={value}
          size="small"
          variant="outlined"
          sx={{ borderColor: 'rgba(255, 255, 255, 0.15)', color: 'rgba(255, 255, 255, 0.7)', fontSize: '0.75rem' }}
        />
      ) : '-',
    },
    {
      field: 'lastSyncedAt',
      headerName: 'Last Synced',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
    {
      field: 'discographyCount',
      headerName: 'Albums',
      width: 100,
      type: 'number',
      renderCell: ({ value }) => (
        <Chip
          label={value}
          size="small"
          sx={{ backgroundColor: 'rgba(183, 28, 28, 0.15)', color: '#e53935', fontWeight: 600, minWidth: 32 }}
        />
      ),
    },
  ];

  return (
    <Box>
      <PageHeader
        icon={<LibraryMusicIcon />}
        title="Band References"
        subtitle={rowCount > 0 ? `${rowCount} Ukrainian metal bands from Metal Archives` : undefined}
      />
      <FilterBar>
        <TextField
          size="small"
          placeholder="Search bands..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') updateParams({ search, page: 1 }); }}
          sx={{ width: 300 }}
          InputProps={{
            startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 20, opacity: 0.5 }} /></InputAdornment>,
          }}
        />
      </FilterBar>
      <DataGrid
        rows={rows}
        columns={columns}
        rowCount={rowCount}
        loading={loading}
        paginationMode="server"
        sortingMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(m) => updateParams({ page: m.page + 1, pageSize: m.pageSize })}
        onSortModelChange={(m) => {
          if (m.length) updateParams({ sortBy: m[0].field, sortAscending: m[0].sort === 'asc' });
          else updateParams({ sortBy: '', sortAscending: '' });
        }}
        pageSizeOptions={[10, 25, 50, 100]}
        onRowClick={(params) => navigate(`/band-references/${params.id}`)}
        sx={{ height: 'calc(100vh - 240px)', cursor: 'pointer' }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
