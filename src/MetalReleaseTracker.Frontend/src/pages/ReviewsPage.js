import React, { useState, useEffect, useCallback } from 'react';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Alert,
  CircularProgress,
  Avatar
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import RateReviewIcon from '@mui/icons-material/RateReview';
import { useLanguage } from '../i18n/LanguageContext';
import { fetchReviews, submitReview } from '../services/api';
import authService from '../services/auth';
import usePageMeta from '../hooks/usePageMeta';

const ReviewsPage = () => {
  const { t } = useLanguage();
  usePageMeta(t('pageMeta.reviewsTitle'), t('pageMeta.reviewsDescription'));
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [reviewsLoading, setReviewsLoading] = useState(true);
  const [user, setUser] = useState(null);

  const loadReviews = useCallback(async () => {
    try {
      const response = await fetchReviews();
      setReviews(response.data);
    } catch (err) {
      console.error('Failed to load reviews:', err);
    } finally {
      setReviewsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadReviews();
    const checkUser = async () => {
      const currentUser = await authService.getUser();
      setUser(currentUser);
    };
    checkUser();
  }, [loadReviews]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setSuccess(false);
    setError(null);

    try {
      await submitReview({ message });
      setSuccess(true);
      setMessage('');
      await loadReviews();
    } catch (err) {
      setError(t('reviews.error'));
    } finally {
      setLoading(false);
    }
  };

  const getInitials = (name) => {
    if (!name) return '?';
    return name.split(' ').map(part => part[0]).join('').toUpperCase().slice(0, 2);
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  return (
    <Container maxWidth="lg" sx={{ py: 6 }}>
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 700, textAlign: 'center' }}>
        {t('reviews.title')}
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 4, textAlign: 'center' }}>
        {t('reviews.subtitle')}
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2, borderRadius: 2 }}>
          {t('reviews.success')}
        </Alert>
      )}

      {error && (
        <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
          {error}
        </Alert>
      )}

      {user ? (
        <Paper elevation={2} sx={{ p: 3, borderRadius: 2, mb: 4 }}>
          <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              label={t('reviews.messageLabel')}
              value={message}
              onChange={(event) => setMessage(event.target.value)}
              required
              multiline
              rows={3}
              fullWidth
              placeholder={t('reviews.messagePlaceholder')}
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
              {loading ? t('reviews.sending') : t('reviews.submit')}
            </Button>
          </Box>
        </Paper>
      ) : (
        <Alert severity="info" sx={{ mb: 4, borderRadius: 2 }}>
          {t('reviews.loginRequired')}
        </Alert>
      )}

      <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
        {t('reviews.listTitle')}
      </Typography>

      {reviewsLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : reviews.length === 0 ? (
        <Paper elevation={1} sx={{ p: 4, borderRadius: 2, textAlign: 'center' }}>
          <RateReviewIcon sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
          <Typography color="text.secondary">
            {t('reviews.empty')}
          </Typography>
        </Paper>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {reviews.map((review, index) => (
            <Paper key={review.id} elevation={1} sx={{ p: 2.5, borderRadius: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1.5 }}>
                <Avatar sx={{ width: 36, height: 36, bgcolor: 'primary.main', fontSize: '0.875rem' }}>
                  {getInitials(review.userName)}
                </Avatar>
                <Box>
                  <Typography variant="subtitle2" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                    {review.userName}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {formatDate(review.createdDate)}
                  </Typography>
                </Box>
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: 'pre-line' }}>
                {review.message}
              </Typography>
            </Paper>
          ))}
        </Box>
      )}
    </Container>
  );
};

export default ReviewsPage;
