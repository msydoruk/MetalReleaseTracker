import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Skeleton from '@mui/material/Skeleton';
import Alert from '@mui/material/Alert';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TablePagination from '@mui/material/TablePagination';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import BrokenImageIcon from '@mui/icons-material/BrokenImage';
import MusicOffIcon from '@mui/icons-material/MusicOff';
import HideImageIcon from '@mui/icons-material/HideImage';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import PageHeader from '../components/PageHeader';
import {
  fetchDataQualitySummary,
  fetchAlbumsMissingCovers,
  fetchBandsMissingGenre,
  fetchBandsMissingPhoto,
  fetchPotentialDuplicateBands,
  hideAlbum,
  mergeBands,
} from '../api/dataQuality';

const CATEGORIES = [
  { key: 'albumsMissingCovers', label: 'Albums Missing Covers', icon: BrokenImageIcon, color: '#e53935' },
  { key: 'bandsMissingGenre', label: 'Bands Missing Genre', icon: MusicOffIcon, color: '#ff9800' },
  { key: 'bandsMissingPhoto', label: 'Bands Missing Photo', icon: HideImageIcon, color: '#9c27b0' },
  { key: 'potentialDuplicateBands', label: 'Potential Duplicate Bands', icon: ContentCopyIcon, color: '#2196f3' },
];

export default function DataQualityPage() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selected, setSelected] = useState(null);
  const [drillData, setDrillData] = useState({ items: [], totalCount: 0 });
  const [drillLoading, setDrillLoading] = useState(false);
  const [page, setPage] = useState(0);
  const [pageSize] = useState(20);
  const [mergeDialog, setMergeDialog] = useState({ open: false, group: null });
  const [mergeTarget, setMergeTarget] = useState('');
  const [actionLoading, setActionLoading] = useState(false);

  const loadSummary = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const { data } = await fetchDataQualitySummary();
      setSummary(data);
    } catch {
      setError('Failed to load data quality summary');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadSummary(); }, [loadSummary]);

  const loadDrillDown = useCallback(async (category, pageNum) => {
    setDrillLoading(true);
    try {
      const params = { page: pageNum + 1, pageSize };
      let response;
      if (category === 'albumsMissingCovers') response = await fetchAlbumsMissingCovers(params);
      else if (category === 'bandsMissingGenre') response = await fetchBandsMissingGenre(params);
      else if (category === 'bandsMissingPhoto') response = await fetchBandsMissingPhoto(params);
      else if (category === 'potentialDuplicateBands') response = await fetchPotentialDuplicateBands(params);
      setDrillData(response.data);
    } catch {
      setDrillData({ items: [], totalCount: 0 });
    } finally {
      setDrillLoading(false);
    }
  }, [pageSize]);

  const handleCardClick = (key) => {
    setSelected(key);
    setPage(0);
    loadDrillDown(key, 0);
  };

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
    loadDrillDown(selected, newPage);
  };

  const handleHideAlbum = async (id) => {
    setActionLoading(true);
    try {
      await hideAlbum(id);
      loadDrillDown(selected, page);
      loadSummary();
    } catch { /* ignore */ }
    finally { setActionLoading(false); }
  };

  const handleMerge = async () => {
    if (!mergeTarget || !mergeDialog.group) return;
    const sourceBands = mergeDialog.group.bands.filter((b) => b.id !== mergeTarget);
    if (sourceBands.length === 0) return;
    setActionLoading(true);
    try {
      for (const source of sourceBands) {
        await mergeBands({ targetBandId: mergeTarget, sourceBandId: source.id });
      }
      setMergeDialog({ open: false, group: null });
      setMergeTarget('');
      loadDrillDown(selected, page);
      loadSummary();
    } catch { /* ignore */ }
    finally { setActionLoading(false); }
  };

  const renderAlbumsTable = () => (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>SKU</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Band</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Distributor</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Created</TableCell>
            <TableCell sx={{ fontWeight: 600 }} align="right">Action</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {drillData.items?.map((row) => (
            <TableRow key={row.id}>
              <TableCell>{row.name}</TableCell>
              <TableCell><Typography variant="caption" sx={{ fontFamily: 'monospace' }}>{row.sku}</Typography></TableCell>
              <TableCell>{row.bandName}</TableCell>
              <TableCell>{row.distributorName}</TableCell>
              <TableCell><Chip label={row.status} size="small" /></TableCell>
              <TableCell>{new Date(row.createdDate).toLocaleDateString()}</TableCell>
              <TableCell align="right">
                <Button size="small" color="error" variant="outlined" disabled={actionLoading} onClick={() => handleHideAlbum(row.id)}>
                  Hide
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );

  const renderBandsTable = () => (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Genre</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Photo</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Albums</TableCell>
            <TableCell sx={{ fontWeight: 600 }}>Visible</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {drillData.items?.map((row) => (
            <TableRow key={row.id}>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.genre || <Typography variant="caption" color="text.secondary">--</Typography>}</TableCell>
              <TableCell>{row.photoUrl ? <Chip label="Yes" size="small" color="success" /> : <Chip label="No" size="small" color="error" />}</TableCell>
              <TableCell>{row.albumCount}</TableCell>
              <TableCell>{row.isVisible ? 'Yes' : 'No'}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );

  const renderDuplicatesTable = () => (
    <Box>
      {drillData.items?.map((group) => (
        <Card key={group.normalizedName} sx={{ mb: 2 }}>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                "{group.normalizedName}" ({group.bands.length} bands)
              </Typography>
              <Button
                size="small"
                variant="outlined"
                color="warning"
                disabled={actionLoading}
                onClick={() => { setMergeDialog({ open: true, group }); setMergeTarget(group.bands[0]?.id || ''); }}
              >
                Merge
              </Button>
            </Box>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Genre</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Albums</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Visible</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {group.bands.map((band) => (
                  <TableRow key={band.id}>
                    <TableCell>{band.name}</TableCell>
                    <TableCell>{band.genre || '--'}</TableCell>
                    <TableCell>{band.albumCount}</TableCell>
                    <TableCell>{band.isVisible ? 'Yes' : 'No'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ))}
    </Box>
  );

  const renderDrillDown = () => {
    if (!selected) return null;
    if (drillLoading) return <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 1 }} />;

    return (
      <Card sx={{ mt: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 2, color: 'rgba(255,255,255,0.7)', textTransform: 'uppercase', fontSize: '0.75rem', letterSpacing: '0.05em' }}>
            {CATEGORIES.find((c) => c.key === selected)?.label}
          </Typography>
          {drillData.items?.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
              No issues found
            </Typography>
          ) : (
            <>
              {selected === 'albumsMissingCovers' && renderAlbumsTable()}
              {(selected === 'bandsMissingGenre' || selected === 'bandsMissingPhoto') && renderBandsTable()}
              {selected === 'potentialDuplicateBands' && renderDuplicatesTable()}
              <TablePagination
                component="div"
                count={drillData.totalCount || 0}
                page={page}
                onPageChange={handlePageChange}
                rowsPerPage={pageSize}
                rowsPerPageOptions={[pageSize]}
              />
            </>
          )}
        </CardContent>
      </Card>
    );
  };

  return (
    <Box>
      <PageHeader title="Data Quality" subtitle="Find and fix data issues" />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Grid container spacing={2}>
        {CATEGORIES.map((cat) => {
          const Icon = cat.icon;
          const count = summary ? summary[cat.key] : 0;
          const isSelected = selected === cat.key;
          return (
            <Grid key={cat.key} size={{ xs: 6, md: 3 }}>
              <Card sx={{ border: isSelected ? `2px solid ${cat.color}` : '2px solid transparent' }}>
                <CardActionArea onClick={() => handleCardClick(cat.key)}>
                  <CardContent sx={{ textAlign: 'center', py: 3 }}>
                    <Icon sx={{ fontSize: 40, color: cat.color, mb: 1 }} />
                    {loading ? (
                      <Skeleton width={40} height={36} sx={{ mx: 'auto' }} />
                    ) : (
                      <Typography variant="h4" sx={{ fontWeight: 700, color: count > 0 ? cat.color : 'rgba(255,255,255,0.5)' }}>
                        {count}
                      </Typography>
                    )}
                    <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.6)', fontSize: '0.7rem' }}>
                      {cat.label}
                    </Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          );
        })}
      </Grid>

      {renderDrillDown()}

      <Dialog open={mergeDialog.open} onClose={() => setMergeDialog({ open: false, group: null })} maxWidth="sm" fullWidth>
        <DialogTitle>Merge Duplicate Bands</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2, mt: 1 }}>
            Select the target band to keep. All albums from other bands in this group will be moved to the target.
          </Typography>
          {mergeDialog.group && (
            <FormControl fullWidth sx={{ mt: 1 }}>
              <InputLabel>Target Band</InputLabel>
              <Select value={mergeTarget} onChange={(e) => setMergeTarget(e.target.value)} label="Target Band">
                {mergeDialog.group.bands.map((band) => (
                  <MenuItem key={band.id} value={band.id}>
                    {band.name} ({band.albumCount} albums)
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMergeDialog({ open: false, group: null })}>Cancel</Button>
          <Button onClick={handleMerge} variant="contained" color="warning" disabled={actionLoading || !mergeTarget}>
            Merge
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
