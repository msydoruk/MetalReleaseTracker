import React, { useState, useRef, useEffect, useCallback } from 'react';
import {
  Box,
  IconButton,
  TextField,
  InputAdornment,
  Paper,
  Typography,
  Tooltip,
  ClickAwayListener,
  Collapse,
  useMediaQuery,
  useTheme
} from '@mui/material';
import {
  Search as SearchIcon,
  Close as CloseIcon,
  MusicNote as MusicNoteIcon,
  Album as AlbumIcon
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { fetchSuggestions } from '../services/api';
import { useLanguage } from '../i18n/LanguageContext';

const HeaderSearch = () => {
  const { t } = useLanguage();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const location = useLocation();

  const [isExpanded, setIsExpanded] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [showSuggestions, setShowSuggestions] = useState(false);

  const inputRef = useRef(null);
  const suggestTimerRef = useRef(null);

  useEffect(() => {
    setIsExpanded(false);
    setSearchQuery('');
    setSuggestions([]);
    setShowSuggestions(false);
  }, [location.pathname, location.search]);

  useEffect(() => {
    return () => {
      if (suggestTimerRef.current) clearTimeout(suggestTimerRef.current);
    };
  }, []);

  const handleExpand = useCallback(() => {
    setIsExpanded(true);
    setTimeout(() => inputRef.current?.focus(), 100);
  }, []);

  const handleCollapse = useCallback(() => {
    setIsExpanded(false);
    setSearchQuery('');
    setSuggestions([]);
    setShowSuggestions(false);
  }, []);

  const handleSearchChange = useCallback((event) => {
    const value = event.target.value;
    setSearchQuery(value);

    if (suggestTimerRef.current) clearTimeout(suggestTimerRef.current);
    if (value.length >= 2) {
      suggestTimerRef.current = setTimeout(async () => {
        try {
          const response = await fetchSuggestions(value);
          setSuggestions(response.data || []);
          setShowSuggestions(true);
        } catch (error) {
          console.error('Error fetching suggestions:', error);
        }
      }, 300);
    } else {
      setSuggestions([]);
      setShowSuggestions(false);
    }
  }, []);

  const handleSuggestionClick = useCallback((suggestion) => {
    setShowSuggestions(false);
    setSuggestions([]);
    if (suggestion.type === 'band') {
      navigate(`/bands/${suggestion.id}`);
    } else {
      navigate(`/albums/${suggestion.id}`);
    }
  }, [navigate]);

  const handleKeyDown = useCallback((event) => {
    if (event.key === 'Enter' && searchQuery.trim()) {
      setShowSuggestions(false);
      navigate(`/albums?name=${encodeURIComponent(searchQuery.trim())}`);
    }
    if (event.key === 'Escape') {
      handleCollapse();
    }
  }, [searchQuery, navigate, handleCollapse]);

  const suggestionsList = showSuggestions && suggestions.length > 0 && (
    <Paper
      sx={{
        position: 'absolute',
        top: '100%',
        left: 0,
        right: 0,
        zIndex: 1300,
        mt: 0.5,
        maxHeight: 300,
        overflow: 'auto',
        backgroundColor: '#1e1e1e',
        border: '1px solid rgba(255, 255, 255, 0.15)',
      }}
    >
      {suggestions.map((suggestion, index) => (
        <Box
          key={`${suggestion.type}-${suggestion.id}-${index}`}
          onMouseDown={(event) => {
            event.preventDefault();
            handleSuggestionClick(suggestion);
          }}
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            px: 2,
            py: 1,
            cursor: 'pointer',
            minHeight: 44,
            '&:hover': {
              backgroundColor: 'rgba(255, 255, 255, 0.08)',
            },
            borderBottom: index < suggestions.length - 1 ? '1px solid rgba(255, 255, 255, 0.06)' : 'none',
          }}
        >
          {suggestion.type === 'band' ? (
            <MusicNoteIcon sx={{ color: 'primary.main', fontSize: 20 }} />
          ) : (
            <AlbumIcon sx={{ color: 'text.secondary', fontSize: 20 }} />
          )}
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography
              variant="body2"
              sx={{
                color: 'white',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
              }}
            >
              {suggestion.text}
            </Typography>
          </Box>
          <Typography
            variant="caption"
            sx={{ color: 'text.secondary', flexShrink: 0 }}
          >
            {suggestion.type === 'band' ? t('albums.suggestionBand') : t('albums.suggestionAlbum')}
          </Typography>
        </Box>
      ))}
    </Paper>
  );

  if (isMobile) {
    return (
      <>
        <Tooltip title={t('header.searchTooltip')}>
          <IconButton
            color="inherit"
            onClick={isExpanded ? handleCollapse : handleExpand}
            sx={{ minWidth: 44, minHeight: 44 }}
            aria-label={isExpanded ? t('header.closeSearch') : t('header.searchTooltip')}
          >
            {isExpanded ? <CloseIcon /> : <SearchIcon />}
          </IconButton>
        </Tooltip>
        <Collapse in={isExpanded} sx={{
          position: 'absolute',
          top: '100%',
          left: 0,
          right: 0,
          zIndex: 1200,
          backgroundColor: 'primary.main',
        }}>
          <ClickAwayListener onClickAway={isExpanded ? handleCollapse : () => {}}>
            <Box sx={{ position: 'relative', px: 2, py: 1.5 }}>
              <TextField
                fullWidth
                size="small"
                placeholder={t('header.searchPlaceholder')}
                value={searchQuery}
                onChange={handleSearchChange}
                onKeyDown={handleKeyDown}
                inputRef={inputRef}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon sx={{ color: 'rgba(255, 255, 255, 0.5)' }} />
                    </InputAdornment>
                  ),
                }}
                sx={{
                  backgroundColor: 'rgba(255, 255, 255, 0.1)',
                  borderRadius: 1,
                  '& .MuiOutlinedInput-root': {
                    '& fieldset': { borderColor: 'rgba(255, 255, 255, 0.3)' },
                    '&:hover fieldset': { borderColor: 'rgba(255, 255, 255, 0.5)' },
                    '&.Mui-focused fieldset': { borderColor: 'rgba(255, 255, 255, 0.7)' },
                  },
                  '& .MuiInputBase-input': { color: 'white' },
                }}
              />
              {suggestionsList}
            </Box>
          </ClickAwayListener>
        </Collapse>
      </>
    );
  }

  return (
    <ClickAwayListener onClickAway={isExpanded ? handleCollapse : () => {}}>
      <Box sx={{ position: 'relative', display: 'flex', alignItems: 'center', mr: 1 }}>
        {isExpanded ? (
          <TextField
            size="small"
            placeholder={t('header.searchPlaceholder')}
            value={searchQuery}
            onChange={handleSearchChange}
            onKeyDown={handleKeyDown}
            inputRef={inputRef}
            autoFocus
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon sx={{ color: 'rgba(255, 255, 255, 0.5)' }} />
                </InputAdornment>
              ),
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton size="small" onClick={handleCollapse} sx={{ color: 'rgba(255, 255, 255, 0.5)' }}>
                    <CloseIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ),
            }}
            sx={{
              width: 280,
              backgroundColor: 'rgba(255, 255, 255, 0.1)',
              borderRadius: 1,
              '& .MuiOutlinedInput-root': {
                '& fieldset': { borderColor: 'rgba(255, 255, 255, 0.3)' },
                '&:hover fieldset': { borderColor: 'rgba(255, 255, 255, 0.5)' },
                '&.Mui-focused fieldset': { borderColor: 'rgba(255, 255, 255, 0.7)' },
              },
              '& .MuiInputBase-input': { color: 'white' },
            }}
          />
        ) : (
          <Tooltip title={t('header.searchTooltip')}>
            <IconButton color="inherit" onClick={handleExpand} aria-label={t('header.searchTooltip')}>
              <SearchIcon />
            </IconButton>
          </Tooltip>
        )}
        {suggestionsList}
      </Box>
    </ClickAwayListener>
  );
};

export default HeaderSearch;
