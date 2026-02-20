import React, { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Alert
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import BugReportIcon from '@mui/icons-material/BugReport';
import LinkOffIcon from '@mui/icons-material/LinkOff';
import LibraryMusicIcon from '@mui/icons-material/LibraryMusic';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import { useLanguage } from '../i18n/LanguageContext';

const FeedbackPage = () => {
  const { t } = useLanguage();
  const [message, setMessage] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();
    const mailto = `mailto:sydoruk.m@gmail.com?subject=${encodeURIComponent(t('feedback.emailSubject'))}&body=${encodeURIComponent(message)}`;
    window.location.href = mailto;
  };

  const issues = [
    { icon: <LibraryMusicIcon sx={{ color: 'warning.main' }} />, text: t('feedback.issueMissing') },
    { icon: <BugReportIcon sx={{ color: 'error.main' }} />, text: t('feedback.issueIncorrect') },
    { icon: <LinkOffIcon sx={{ color: 'info.main' }} />, text: t('feedback.issueBrokenLinks') },
  ];

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
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
          />
          <Button
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            endIcon={<SendIcon />}
            sx={{ fontWeight: 600 }}
          >
            {t('feedback.submit')}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default FeedbackPage;
