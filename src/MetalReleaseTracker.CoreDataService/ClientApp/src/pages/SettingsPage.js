import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import Snackbar from '@mui/material/Snackbar';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import PageHeader from '../components/PageHeader';
import { fetchCategorySettings, updateCategorySettings } from '../api/settings';

function TabPanel({ children, value, index }) {
  return value === index ? <Box sx={{ pt: 2 }}>{children}</Box> : null;
}

function CategorySettingsTab({ category, fields }) {
  const [settings, setSettings] = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' });

  const loadSettings = useCallback(async () => {
    try {
      const { data } = await fetchCategorySettings(category);
      setSettings(data.settings || {});
    } catch {
      setSnackbar({ open: true, message: 'Failed to load settings', severity: 'error' });
    } finally {
      setLoading(false);
    }
  }, [category]);

  useEffect(() => { loadSettings(); }, [loadSettings]);

  const handleChange = (key, value) => {
    setSettings((prev) => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await updateCategorySettings(category, settings);
      setSnackbar({ open: true, message: 'Settings saved', severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: 'Failed to save settings', severity: 'error' });
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>;

  return (
    <Card sx={{ backgroundColor: 'rgba(255, 255, 255, 0.03)', border: '1px solid rgba(255, 255, 255, 0.08)' }}>
      <CardContent sx={{ p: 3 }}>
        {fields.map((field) => (
          <Box key={field.key} sx={{ mb: 2 }}>
            {field.type === 'boolean' ? (
              <FormControlLabel
                control={
                  <Switch
                    checked={settings[field.key] === 'true'}
                    onChange={(e) => handleChange(field.key, e.target.checked ? 'true' : 'false')}
                  />
                }
                label={field.label}
              />
            ) : (
              <TextField
                fullWidth
                label={field.label}
                type={field.type === 'number' ? 'number' : 'text'}
                value={settings[field.key] || ''}
                onChange={(e) => handleChange(field.key, e.target.value)}
                size="small"
                helperText={field.description}
              />
            )}
          </Box>
        ))}
        <Button variant="contained" onClick={handleSave} disabled={saving} sx={{ mt: 1 }}>
          {saving ? 'Saving...' : 'Save Settings'}
        </Button>
      </CardContent>
      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity}>{snackbar.message}</Alert>
      </Snackbar>
    </Card>
  );
}

const TABS = [
  {
    label: 'Authentication',
    category: 'Authentication',
    fields: [
      { key: 'JwtExpiresMinutes', label: 'JWT Expiry (minutes)', type: 'number', description: 'Access token expiration time' },
      { key: 'RefreshTokenExpirationDays', label: 'Refresh Token Expiry (days)', type: 'number', description: 'Refresh token expiration time' },
      { key: 'PasswordMinLength', label: 'Password Min Length', type: 'number', description: 'Minimum password length' },
      { key: 'PasswordRequireDigit', label: 'Require Digit', type: 'boolean' },
      { key: 'PasswordRequireUppercase', label: 'Require Uppercase', type: 'boolean' },
      { key: 'PasswordRequireLowercase', label: 'Require Lowercase', type: 'boolean' },
      { key: 'PasswordRequireNonAlphanumeric', label: 'Require Special Character', type: 'boolean' },
      { key: 'LockoutTimeSpanMinutes', label: 'Lockout Duration (minutes)', type: 'number' },
      { key: 'MaxFailedAccessAttempts', label: 'Max Failed Attempts', type: 'number' },
    ],
  },
  {
    label: 'Pagination',
    category: 'Pagination',
    fields: [
      { key: 'DefaultPageSize', label: 'Default Page Size', type: 'number' },
      { key: 'MaxPageSize', label: 'Max Page Size', type: 'number' },
    ],
  },
  {
    label: 'Storage',
    category: 'Storage',
    fields: [
      { key: 'PresignedUrlExpiryDays', label: 'Presigned URL Expiry (days)', type: 'number', description: 'MinIO presigned URL expiration' },
    ],
  },
  {
    label: 'Features',
    category: 'FeatureToggles',
    fields: [
      { key: 'TelegramBotEnabled', label: 'Telegram Bot', type: 'boolean' },
      { key: 'NotificationsEnabled', label: 'Notifications', type: 'boolean' },
      { key: 'ReviewsEnabled', label: 'Reviews', type: 'boolean' },
      { key: 'RegistrationEnabled', label: 'Registration', type: 'boolean' },
      { key: 'GoogleAuthEnabled', label: 'Google OAuth', type: 'boolean' },
    ],
  },
  {
    label: 'Telegram',
    category: 'Telegram',
    fields: [
      { key: 'LinkTokenExpiryMinutes', label: 'Link Token Expiry (minutes)', type: 'number' },
    ],
  },
  {
    label: 'Notifications',
    category: 'Notifications',
    fields: [
      { key: 'PriceDropEnabled', label: 'Price Drop Notifications', type: 'boolean' },
      { key: 'BackInStockEnabled', label: 'Back In Stock Notifications', type: 'boolean' },
      { key: 'RestockEnabled', label: 'Restock Notifications', type: 'boolean' },
      { key: 'NewVariantEnabled', label: 'New Variant Notifications', type: 'boolean' },
    ],
  },
  {
    label: 'SEO',
    category: 'Seo',
    fields: [
      { key: 'SiteName', label: 'Site Name', type: 'text', description: 'Displayed in page titles, header, footer' },
      { key: 'SiteUrl', label: 'Site URL', type: 'text', description: 'Base URL for canonical links, sitemap, JSON-LD' },
      { key: 'DefaultMetaDescription', label: 'Default Meta Description', type: 'text', description: 'Fallback description for pages without custom meta' },
      { key: 'DefaultOgImage', label: 'Default OG Image URL', type: 'text', description: 'Default social sharing image' },
      { key: 'ContactEmail', label: 'Contact Email', type: 'text', description: 'Displayed in footer for suggestions' },
      { key: 'OrganizationName', label: 'Organization Name', type: 'text', description: 'Used in JSON-LD Organization schema' },
      { key: 'OrganizationLogoUrl', label: 'Organization Logo URL', type: 'text', description: 'Used in JSON-LD Organization schema' },
      { key: 'RobotsTxt', label: 'Robots.txt Content', type: 'text', description: 'Content for /robots.txt (Sitemap line added automatically)' },
    ],
  },
  {
    label: 'AI SEO',
    category: 'AiSeo',
    fields: [
      { key: 'IsEnabled', label: 'AI SEO Enabled', type: 'boolean', description: 'Enable AI-powered SEO generation' },
      { key: 'ApiKey', label: 'Anthropic API Key', type: 'text', description: 'Claude API key for SEO generation' },
      { key: 'Model', label: 'Model', type: 'text', description: 'Claude model (e.g., claude-sonnet-4-20250514)' },
      { key: 'MaxTokens', label: 'Max Tokens', type: 'number', description: 'Maximum tokens per response' },
      { key: 'BandPrompt', label: 'Band SEO Prompt', type: 'text', description: 'Prompt template for band SEO. Variables: {{bandName}}, {{genre}}, {{formationYear}}, {{description}}, {{albumCount}}' },
      { key: 'AlbumPrompt', label: 'Album SEO Prompt', type: 'text', description: 'Prompt template for album SEO. Variables: {{bandName}}, {{albumTitle}}, {{genre}}, {{year}}, {{media}}, {{price}}, {{label}}, {{description}}' },
    ],
  },
];

export default function SettingsPage() {
  const [tab, setTab] = useState(0);

  return (
    <Box>
      <PageHeader title="Settings" subtitle="Manage system configuration" />
      <Tabs
        value={tab}
        onChange={(e, newValue) => setTab(newValue)}
        variant="scrollable"
        scrollButtons="auto"
        sx={{ borderBottom: 1, borderColor: 'divider' }}
      >
        {TABS.map((t) => <Tab key={t.category} label={t.label} />)}
      </Tabs>
      {TABS.map((t, index) => (
        <TabPanel key={t.category} value={tab} index={index}>
          <CategorySettingsTab category={t.category} fields={t.fields} />
        </TabPanel>
      ))}
    </Box>
  );
}
