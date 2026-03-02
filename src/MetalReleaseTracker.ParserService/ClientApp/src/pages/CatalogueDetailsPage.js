import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import InputAdornment from '@mui/material/InputAdornment';
import Chip from '@mui/material/Chip';
import SearchIcon from '@mui/icons-material/Search';
import InventoryIcon from '@mui/icons-material/Inventory';
import { DataGrid } from '@mui/x-data-grid';
import StatusChip from '../components/StatusChip';
import DistributorChip from '../components/DistributorChip';
import PageHeader from '../components/PageHeader';
import FilterBar from '../components/FilterBar';
import { DISTRIBUTOR_CODES, MEDIA_TYPES, CHANGE_TYPE, PUBLICATION_STATUS } from '../constants';
import { fetchCatalogueDetails } from '../api/catalogueDetails';

const SORT_FIELD_MAP = {
  bandName: 0,
  name: 1,
  distributorCode: 2,
  changeType: 3,
  publicationStatus: 4,
  price: 5,
  updatedAt: 6,
  lastPublishedAt: 7,
};

export default function CatalogueDetailsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [rows, setRows] = useState([]);
  const [rowCount, setRowCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState(searchParams.get('search') || '');

  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '25', 10);
  const distributorCode = searchParams.get('distributorCode') || '';
  const changeType = searchParams.get('changeType') || '';
  const publicationStatus = searchParams.get('publicationStatus') || '';
  const sortBy = searchParams.get('sortBy') || '';
  const sortAscending = searchParams.get('sortAscending') !== 'false';

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (distributorCode) params.distributorCode = distributorCode;
      if (changeType !== '') params.changeType = changeType;
      if (publicationStatus !== '') params.publicationStatus = publicationStatus;
      if (sortBy && SORT_FIELD_MAP[sortBy] !== undefined) {
        params.sortBy = SORT_FIELD_MAP[sortBy];
        params.sortAscending = sortAscending;
      }
      const { data } = await fetchCatalogueDetails(params);
      setRows(data.items);
      setRowCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load catalogue details', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, distributorCode, changeType, publicationStatus, sortBy, sortAscending]);

  useEffect(() => { load(); }, [load]);

  const updateParams = (updates) => {
    const params = Object.fromEntries(searchParams);
    Object.assign(params, updates);
    Object.keys(params).forEach((k) => { if (params[k] === '' || params[k] === null || params[k] === undefined) delete params[k]; });
    setSearchParams(params);
  };

  const columns = [
    {
      field: 'distributorCode',
      headerName: 'Distributor',
      width: 180,
      renderCell: ({ value }) => <DistributorChip code={value} />,
    },
    {
      field: 'bandName',
      headerName: 'Band',
      flex: 1,
      minWidth: 120,
      renderCell: ({ value }) => (
        <Box sx={{ fontWeight: 500, color: 'rgba(255, 255, 255, 0.95)' }}>{value}</Box>
      ),
    },
    {
      field: 'name',
      headerName: 'Album',
      flex: 1,
      minWidth: 150,
    },
    {
      field: 'price',
      headerName: 'Price',
      width: 80,
      renderCell: ({ value }) => value != null ? `${value.toFixed(2)}` : '-',
    },
    {
      field: 'media',
      headerName: 'Media',
      width: 80,
      renderCell: ({ value }) => {
        const label = value !== null && value !== undefined ? (MEDIA_TYPES[value] || value) : null;
        return label ? (
          <Chip label={label} size="small" variant="outlined" sx={{ borderColor: 'rgba(255,255,255,0.15)', fontSize: '0.7rem' }} />
        ) : '-';
      },
    },
    {
      field: 'canonicalTitle',
      headerName: 'Canonical Title',
      width: 180,
      renderCell: ({ value }) => value || <Box sx={{ color: 'rgba(255,255,255,0.25)' }}>-</Box>,
    },
    {
      field: 'originalYear',
      headerName: 'Year',
      width: 70,
      renderCell: ({ value }) => value ? (
        <Chip
          label={value}
          size="small"
          sx={{
            height: 20,
            fontSize: '0.7rem',
            fontWeight: 600,
            backgroundColor: 'rgba(255, 255, 255, 0.08)',
            color: 'rgba(255, 255, 255, 0.5)',
          }}
        />
      ) : <Box sx={{ color: 'rgba(255,255,255,0.25)' }}>-</Box>,
    },
    {
      field: 'changeType',
      headerName: 'Change Type',
      width: 120,
      renderCell: ({ value }) => <StatusChip value={value} statusMap={CHANGE_TYPE} />,
    },
    {
      field: 'publicationStatus',
      headerName: 'Publication',
      width: 130,
      renderCell: ({ value }) => <StatusChip value={value} statusMap={PUBLICATION_STATUS} />,
    },
    {
      field: 'lastPublishedAt',
      headerName: 'Published At',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '-',
    },
    {
      field: 'updatedAt',
      headerName: 'Updated',
      width: 170,
      valueFormatter: (value) => value ? new Date(value).toLocaleString() : '',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flex: 1, minHeight: 0, overflow: 'hidden' }}>
      <PageHeader
        icon={<InventoryIcon />}
        title="Catalogue Details"
        subtitle={rowCount > 0 ? `${rowCount} parsed album details` : undefined}
      />
      <FilterBar>
        <TextField
          size="small"
          placeholder="Search..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') updateParams({ search, page: 1 }); }}
          sx={{ width: 220 }}
          InputProps={{
            startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 20, opacity: 0.5 }} /></InputAdornment>,
          }}
        />
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel>Distributor</InputLabel>
          <Select
            value={distributorCode}
            label="Distributor"
            onChange={(e) => updateParams({ distributorCode: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(DISTRIBUTOR_CODES).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Change Type</InputLabel>
          <Select
            value={changeType}
            label="Change Type"
            onChange={(e) => updateParams({ changeType: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(CHANGE_TYPE).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Publication</InputLabel>
          <Select
            value={publicationStatus}
            label="Publication"
            onChange={(e) => updateParams({ publicationStatus: e.target.value, page: 1 })}
          >
            <MenuItem value="">All</MenuItem>
            {Object.entries(PUBLICATION_STATUS).map(([val, cfg]) => (
              <MenuItem key={val} value={val}>{cfg.label}</MenuItem>
            ))}
          </Select>
        </FormControl>
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
        sx={{ flexGrow: 1, minHeight: 0, '& .MuiDataGrid-virtualScroller': { overflowX: 'hidden' } }}
        disableRowSelectionOnClick
      />
    </Box>
  );
}
