import { createTheme, alpha } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#b71c1c',
      light: '#e53935',
      dark: '#7f0000',
    },
    secondary: {
      main: '#9e9e9e',
    },
    background: {
      default: '#0e0e0e',
      paper: '#1a1a1a',
    },
    success: {
      main: '#4caf50',
    },
    warning: {
      main: '#ff9800',
    },
    error: {
      main: '#f44336',
    },
    divider: 'rgba(255, 255, 255, 0.08)',
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h4: { fontWeight: 700, letterSpacing: '-0.02em' },
    h5: { fontWeight: 600, letterSpacing: '-0.01em' },
    h6: { fontWeight: 600 },
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        '*': {
          scrollbarWidth: 'thin',
          scrollbarColor: 'rgba(183, 28, 28, 0.4) rgba(255, 255, 255, 0.05)',
        },
        '*::-webkit-scrollbar': {
          width: 8,
          height: 8,
        },
        '*::-webkit-scrollbar-track': {
          background: 'rgba(255, 255, 255, 0.05)',
          borderRadius: 4,
        },
        '*::-webkit-scrollbar-thumb': {
          background: 'rgba(183, 28, 28, 0.4)',
          borderRadius: 4,
          '&:hover': {
            background: 'rgba(183, 28, 28, 0.6)',
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#0a0a0a',
          borderBottom: '1px solid rgba(183, 28, 28, 0.3)',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', borderRadius: 6, fontWeight: 500 },
        contained: {
          boxShadow: 'none',
          '&:hover': { boxShadow: '0px 4px 12px rgba(183, 28, 28, 0.4)' },
        },
        outlined: {
          borderWidth: '1.5px',
          '&:hover': { borderWidth: '1.5px' },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundColor: '#1a1a1a',
          borderRadius: 12,
          border: '1px solid rgba(255, 255, 255, 0.06)',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          backgroundColor: '#111111',
          borderRight: '1px solid rgba(255, 255, 255, 0.06)',
        },
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          margin: '2px 8px',
          '&.Mui-selected': {
            backgroundColor: 'rgba(183, 28, 28, 0.15)',
            borderLeft: '3px solid #b71c1c',
            '&:hover': {
              backgroundColor: 'rgba(183, 28, 28, 0.25)',
            },
          },
          '&:hover': {
            backgroundColor: 'rgba(255, 255, 255, 0.04)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 500,
        },
        sizeSmall: {
          height: 24,
          fontSize: '0.75rem',
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-notchedOutline': {
            borderColor: 'rgba(255, 255, 255, 0.12)',
          },
          '&:hover .MuiOutlinedInput-notchedOutline': {
            borderColor: 'rgba(255, 255, 255, 0.2)',
          },
        },
      },
    },
    MuiDataGrid: {
      styleOverrides: {
        root: {
          border: '1px solid rgba(255, 255, 255, 0.06)',
          borderRadius: 12,
          backgroundColor: '#1a1a1a',
          '& .MuiDataGrid-withBorderColor': {
            borderColor: 'rgba(255, 255, 255, 0.06)',
          },
        },
        columnHeaders: {
          backgroundColor: '#141414',
          borderBottom: '2px solid rgba(183, 28, 28, 0.3)',
          '& .MuiDataGrid-columnHeaderTitle': {
            fontWeight: 600,
            fontSize: '0.8rem',
            textTransform: 'uppercase',
            letterSpacing: '0.05em',
            color: 'rgba(255, 255, 255, 0.7)',
          },
        },
        row: {
          '&:nth-of-type(even)': {
            backgroundColor: 'rgba(255, 255, 255, 0.015)',
          },
          '&:hover': {
            backgroundColor: 'rgba(183, 28, 28, 0.08) !important',
          },
          '&.Mui-selected': {
            backgroundColor: 'rgba(183, 28, 28, 0.15) !important',
            '&:hover': {
              backgroundColor: 'rgba(183, 28, 28, 0.2) !important',
            },
          },
        },
        cell: {
          borderBottom: '1px solid rgba(255, 255, 255, 0.04)',
          display: 'flex',
          alignItems: 'center',
        },
        footerContainer: {
          borderTop: '2px solid rgba(183, 28, 28, 0.3)',
          backgroundColor: '#141414',
        },
        toolbarContainer: {
          padding: '8px 16px',
        },
        overlay: {
          backgroundColor: 'rgba(0, 0, 0, 0.6)',
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          backgroundColor: '#1a1a1a',
          border: '1px solid rgba(255, 255, 255, 0.1)',
          borderRadius: 12,
        },
      },
    },
    MuiAlert: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        },
      },
    },
    MuiTooltip: {
      styleOverrides: {
        tooltip: {
          backgroundColor: '#2a2a2a',
          border: '1px solid rgba(255, 255, 255, 0.1)',
          fontSize: '0.75rem',
        },
      },
    },
    MuiLinearProgress: {
      styleOverrides: {
        root: {
          borderRadius: 4,
          backgroundColor: 'rgba(255, 255, 255, 0.08)',
        },
      },
    },
  },
});

export default theme;
