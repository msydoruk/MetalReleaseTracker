import { useState, useEffect } from 'react';
import { Snackbar, Button, IconButton } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import GetAppIcon from '@mui/icons-material/GetApp';
import { useLanguage } from '../i18n/LanguageContext';

const InstallPrompt = () => {
  const { t } = useLanguage();
  const [deferredPrompt, setDeferredPrompt] = useState(null);
  const [showPrompt, setShowPrompt] = useState(false);

  useEffect(() => {
    const dismissed = sessionStorage.getItem('installPromptDismissed');
    if (dismissed) return;

    const handler = (event) => {
      event.preventDefault();
      setDeferredPrompt(event);
      setShowPrompt(true);
    };

    window.addEventListener('beforeinstallprompt', handler);
    return () => window.removeEventListener('beforeinstallprompt', handler);
  }, []);

  const handleInstall = async () => {
    if (!deferredPrompt) return;
    deferredPrompt.prompt();
    await deferredPrompt.userChoice;
    setDeferredPrompt(null);
    setShowPrompt(false);
  };

  const handleDismiss = () => {
    setShowPrompt(false);
    sessionStorage.setItem('installPromptDismissed', 'true');
  };

  return (
    <Snackbar
      open={showPrompt}
      message={t('pwa.installMessage') || 'Install Metal Release Tracker for quick access'}
      action={
        <>
          <Button
            color="primary"
            size="small"
            startIcon={<GetAppIcon />}
            onClick={handleInstall}
            sx={{ fontWeight: 600 }}
          >
            {t('pwa.install') || 'Install'}
          </Button>
          <IconButton size="small" color="inherit" onClick={handleDismiss}>
            <CloseIcon fontSize="small" />
          </IconButton>
        </>
      }
      sx={{
        '& .MuiSnackbarContent-root': {
          backgroundColor: 'background.paper',
          color: 'text.primary',
        },
      }}
    />
  );
};

export default InstallPrompt;
