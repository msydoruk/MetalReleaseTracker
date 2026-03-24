import { useState, useEffect, useCallback, useRef } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import { DataGrid } from '@mui/x-data-grid';
import SearchIcon from '@mui/icons-material/Search';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import LockIcon from '@mui/icons-material/Lock';
import LockOpenIcon from '@mui/icons-material/LockOpen';
import PageHeader from '../components/PageHeader';
import { fetchUsers, updateUserRole, lockUser, unlockUser } from '../api/users';

export default function UsersPage() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalRows, setTotalRows] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 25 });
  const [search, setSearch] = useState('');
  const [lockDialogOpen, setLockDialogOpen] = useState(false);
  const [lockingUser, setLockingUser] = useState(null);
  const [saving, setSaving] = useState(false);
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
      };
      if (search) {
        params.search = search;
      }
      const { data } = await fetchUsers(params);
      setRows(data.items || []);
      setTotalRows(data.totalCount || 0);
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to load users', 'error');
    } finally {
      setLoading(false);
    }
  }, [paginationModel, search, showSnackbar]);

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

  const handleToggleAdmin = useCallback(async (row) => {
    try {
      setSaving(true);
      const isAdmin = (row.roles || []).includes('Admin');
      await updateUserRole(row.id, { role: 'Admin', assign: !isAdmin });
      showSnackbar(isAdmin ? 'Admin role removed' : 'Admin role assigned');
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to update role', 'error');
    } finally {
      setSaving(false);
    }
  }, [loadData, showSnackbar]);

  const handleOpenLockDialog = useCallback((row) => {
    setLockingUser(row);
    setLockDialogOpen(true);
  }, []);

  const handleConfirmLockToggle = useCallback(async () => {
    if (!lockingUser) return;
    try {
      setSaving(true);
      const isLocked = lockingUser.isLocked;
      if (isLocked) {
        await unlockUser(lockingUser.id);
        showSnackbar('User unlocked');
      } else {
        await lockUser(lockingUser.id, { reason: 'Locked by admin' });
        showSnackbar('User locked');
      }
      setLockDialogOpen(false);
      setLockingUser(null);
      loadData();
    } catch (err) {
      showSnackbar(err.response?.data?.message || 'Failed to update lock status', 'error');
    } finally {
      setSaving(false);
    }
  }, [lockingUser, loadData, showSnackbar]);

  const columns = [
    { field: 'email', headerName: 'Email', flex: 1, minWidth: 220 },
    { field: 'userName', headerName: 'UserName', flex: 0.8, minWidth: 150 },
    {
      field: 'roles',
      headerName: 'Roles',
      flex: 0.7,
      minWidth: 160,
      sortable: false,
      renderCell: (params) => {
        const roles = params.value || [];
        return (
          <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', alignItems: 'center', height: '100%' }}>
            {roles.length > 0 ? (
              roles.map((role) => (
                <Chip
                  key={role}
                  label={role}
                  size="small"
                  color={role === 'Admin' ? 'warning' : 'default'}
                  variant="outlined"
                />
              ))
            ) : (
              <Chip label="User" size="small" variant="outlined" />
            )}
          </Box>
        );
      },
    },
    {
      field: 'isLocked',
      headerName: 'Status',
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <Chip
          label={params.value ? 'Locked' : 'Active'}
          size="small"
          color={params.value ? 'error' : 'success'}
          variant="outlined"
        />
      ),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 140,
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => {
        const isAdmin = (params.row.roles || []).includes('Admin');
        const isLocked = params.row.isLocked;
        return (
          <Box>
            <Tooltip title={isAdmin ? 'Remove Admin' : 'Make Admin'}>
              <IconButton
                size="small"
                color={isAdmin ? 'warning' : 'default'}
                onClick={() => handleToggleAdmin(params.row)}
                disabled={saving}
              >
                <AdminPanelSettingsIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title={isLocked ? 'Unlock User' : 'Lock User'}>
              <IconButton
                size="small"
                color={isLocked ? 'success' : 'error'}
                onClick={() => handleOpenLockDialog(params.row)}
                disabled={saving}
              >
                {isLocked ? <LockOpenIcon fontSize="small" /> : <LockIcon fontSize="small" />}
              </IconButton>
            </Tooltip>
          </Box>
        );
      },
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <PageHeader title="Users" subtitle="Manage users" />

      <Box sx={{ mb: 2 }}>
        <TextField
          placeholder="Search users..."
          size="small"
          onChange={handleSearchChange}
          sx={{ width: 320 }}
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
          disableRowSelectionOnClick
          autoHeight={false}
          sx={{ height: '100%' }}
        />
      </Box>

      {/* Lock/Unlock Confirmation Dialog */}
      <Dialog open={lockDialogOpen} onClose={() => setLockDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>{lockingUser?.isLocked ? 'Unlock User' : 'Lock User'}</DialogTitle>
        <DialogContent>
          {lockingUser?.isLocked
            ? <>Are you sure you want to unlock <strong>{lockingUser?.email}</strong>? They will regain access to the platform.</>
            : <>Are you sure you want to lock <strong>{lockingUser?.email}</strong>? They will be unable to log in.</>}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setLockDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color={lockingUser?.isLocked ? 'success' : 'error'}
            onClick={handleConfirmLockToggle}
            disabled={saving}
          >
            {saving ? 'Processing...' : lockingUser?.isLocked ? 'Unlock' : 'Lock'}
          </Button>
        </DialogActions>
      </Dialog>

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
