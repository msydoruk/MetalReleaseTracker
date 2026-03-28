import { useState } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import DownloadIcon from '@mui/icons-material/Download';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import PageHeader from '../components/PageHeader';
import { exportAlbums, exportBands, importAlbums } from '../api/bulkData';

const downloadBlob = (blob, filename) => {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
};

export default function BulkDataPage() {
  const [exportFormat, setExportFormat] = useState('csv');
  const [exporting, setExporting] = useState(null);
  const [importFile, setImportFile] = useState(null);
  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] = useState(null);
  const [error, setError] = useState(null);

  const handleExport = async (type) => {
    setExporting(type);
    setError(null);
    try {
      const response = type === 'albums'
        ? await exportAlbums(exportFormat)
        : await exportBands(exportFormat);
      downloadBlob(response.data, `${type}-export.${exportFormat}`);
    } catch {
      setError(`Failed to export ${type}`);
    } finally {
      setExporting(null);
    }
  };

  const handleImportPreview = async () => {
    if (!importFile) return;
    setImporting(true);
    setError(null);
    setImportResult(null);
    try {
      const { data } = await importAlbums(importFile, false);
      setImportResult({ ...data, confirmed: false });
    } catch {
      setError('Failed to parse import file');
    } finally {
      setImporting(false);
    }
  };

  const handleImportConfirm = async () => {
    if (!importFile) return;
    setImporting(true);
    setError(null);
    try {
      const { data } = await importAlbums(importFile, true);
      setImportResult({ ...data, confirmed: true });
      setImportFile(null);
    } catch {
      setError('Failed to import albums');
    } finally {
      setImporting(false);
    }
  };

  return (
    <Box>
      <PageHeader title="Bulk Data" subtitle="Import and export data" />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <DownloadIcon /> Export
              </Typography>
              <TextField
                select
                label="Format"
                value={exportFormat}
                onChange={(e) => setExportFormat(e.target.value)}
                size="small"
                sx={{ mb: 2, minWidth: 120 }}
              >
                <MenuItem value="csv">CSV</MenuItem>
                <MenuItem value="json">JSON</MenuItem>
              </TextField>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="contained" onClick={() => handleExport('albums')} disabled={!!exporting} startIcon={<DownloadIcon />}>
                  {exporting === 'albums' ? 'Exporting...' : 'Export Albums'}
                </Button>
                <Button variant="outlined" onClick={() => handleExport('bands')} disabled={!!exporting} startIcon={<DownloadIcon />}>
                  {exporting === 'bands' ? 'Exporting...' : 'Export Bands'}
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <UploadFileIcon /> Import Albums
              </Typography>
              <Button variant="outlined" component="label" sx={{ mb: 2 }}>
                {importFile ? importFile.name : 'Choose File (CSV/JSON)'}
                <input type="file" hidden accept=".csv,.json" onChange={(e) => { setImportFile(e.target.files[0]); setImportResult(null); }} />
              </Button>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button variant="contained" color="info" onClick={handleImportPreview} disabled={!importFile || importing}>
                  {importing ? 'Processing...' : 'Preview'}
                </Button>
                {importResult && !importResult.confirmed && (
                  <Button variant="contained" color="warning" onClick={handleImportConfirm} disabled={importing}>
                    Confirm Import
                  </Button>
                )}
              </Box>
              {importResult && (
                <Alert severity={importResult.confirmed ? 'success' : 'info'} sx={{ mt: 2 }}>
                  {importResult.confirmed ? 'Import completed! ' : 'Preview: '}
                  Processed: {importResult.processed}, Created: {importResult.created}, Skipped: {importResult.skipped}
                  {importResult.errors?.length > 0 && (
                    <Box sx={{ mt: 1 }}>
                      {importResult.errors.map((err, i) => <Typography key={i} variant="caption" display="block" color="error">{err}</Typography>)}
                    </Box>
                  )}
                </Alert>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
