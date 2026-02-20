import React, { useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import { useLanguage } from '../i18n/LanguageContext';

const FeedbackPage = () => {
  const { t } = useLanguage();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [subject, setSubject] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();
    const body = `${t('feedback.nameLabel')}: ${name}\n${t('feedback.emailLabel')}: ${email}\n\n${message}`;
    const mailto = `mailto:sydoruk.m@gmail.com?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
    window.location.href = mailto;
  };

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 700, textAlign: 'center' }}>
        {t('feedback.title')}
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', mb: 4 }}>
        {t('feedback.subtitle')}
      </Typography>

      <Paper elevation={2} sx={{ p: 4, borderRadius: 2 }}>
        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
          <TextField
            label={t('feedback.nameLabel')}
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
            fullWidth
          />
          <TextField
            label={t('feedback.emailLabel')}
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            fullWidth
          />
          <TextField
            label={t('feedback.subjectLabel')}
            value={subject}
            onChange={(event) => setSubject(event.target.value)}
            required
            fullWidth
          />
          <TextField
            label={t('feedback.messageLabel')}
            value={message}
            onChange={(event) => setMessage(event.target.value)}
            required
            multiline
            rows={4}
            fullWidth
          />
          <Button
            type="submit"
            variant="contained"
            color="primary"
            size="large"
            endIcon={<SendIcon />}
            sx={{ mt: 1, fontWeight: 600 }}
          >
            {t('feedback.submit')}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default FeedbackPage;
