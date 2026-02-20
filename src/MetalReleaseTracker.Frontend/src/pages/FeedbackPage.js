import React, { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Alert,
  CircularProgress
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import BugReportIcon from '@mui/icons-material/BugReport';
import LinkOffIcon from '@mui/icons-material/LinkOff';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import { useLanguage } from '../i18n/LanguageContext';
import { submitFeedback } from '../services/api';

const FeedbackPage = () => {
  const { t } = useLanguage();
  const [message, setMessage] = useState('');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setSuccess(false);
    setError(null);

    try {
      await submitFeedback({ message, email: email || undefined });
      setSuccess(true);
      setMessage('');
      setEmail('');
    } catch (err) {
      setError(t('feedback.error'));
    } finally {
      setLoading(false);
    }
  };

  const issues = [
    { icon: <LibraryMusicIcon sx={{ color: 'warning.main' }} />, text: t('feedback.issueMissing') },
    { icon: <BugReportIcon sx={{ color: 'error.main' }} />, text: t('feedback.issueIncorrect') },
    { icon: <LinkOffIcon sx={{ color: 'info.main' }} />, text: t('feedback.issueBrokenLinks') },
  ];

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 700, textAlign: 'center' }}>
        {t('feedback.title')}
      </Typography>

      <Alert icon={<InfoOutlinedIcon />} severity="info" sx={{ mb: 3, borderRadius: 2 }}>
        {t('feedback.nonCommercial')}
      </Alert>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 2, lineHeight: 1.8 }}>
        {t('feedback.description')}
      </Typography>

      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>
        {t('feedback.issuesTitle')}
      </Typography>

      <Box sx={{ mb: 3, display: 'flex', flexDirection: 'column', gap: 1 }}>
        {issues.map((issue, index) => (
          <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            {issue.icon}
            <Typography variant="body2" color="text.secondary">
              {issue.text}
            </Typography>
          </Box>
        ))}
      </Box>

      {success && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }}>
          {t('feedback.success')}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
          {error}
        </Alert>
      )}

      <Paper elevation={2} sx={{ p: 4, borderRadius: 2 }}>
        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
          <TextField
            label={t('feedback.messageLabel')}
            value={message}
            onChange={(event) => setMessage(event.target.value)}
            required
            multiline
            rows={5}
            fullWidth
            placeholder={t('feedback.messagePlaceholder')}
            disabled={loading}
          />
          <TextField
            label={t('feedback.emailLabel')}
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            type="email"
            fullWidth
            placeholder={t('feedback.emailPlaceholder')}
            disabled={loading}
          />
          <Button
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            disabled={loading}
            endIcon={loading ? <CircularProgress size={20} color="inherit" /> : <SendIcon />}
            sx={{ fontWeight: 600 }}
          >
            {loading ? t('feedback.sending') : t('feedback.submit')}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default FeedbackPage;
