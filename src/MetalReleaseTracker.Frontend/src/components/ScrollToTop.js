import React, { useState, useEffect } from 'react';
import { Fab, Zoom, useTheme } from '@mui/material';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';

const SCROLL_THRESHOLD = 300;

const ScrollToTop = () => {
  const theme = useTheme();
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      setVisible(window.scrollY > SCROLL_THRESHOLD);
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleClick = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <Zoom in={visible}>
      <Fab
        size="medium"
        color="primary"
        aria-label="scroll to top"
        onClick={handleClick}
        sx={{
          position: 'fixed',
          bottom: { xs: 24, sm: 32 },
          right: { xs: 16, sm: 32 },
          zIndex: theme.zIndex.fab,
          opacity: 0.85,
          '&:hover': { opacity: 1 },
        }}
      >
        <KeyboardArrowUpIcon />
      </Fab>
    </Zoom>
  );
};

export default ScrollToTop;
