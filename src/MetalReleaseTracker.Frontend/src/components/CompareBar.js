import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Avatar from '@mui/material/Avatar';
import Slide from '@mui/material/Slide';
import CloseIcon from '@mui/icons-material/Close';
import CompareArrowsIcon from '@mui/icons-material/CompareArrows';
import { useNavigate } from 'react-router-dom';
import { useCompare } from '../contexts/CompareContext';
import { useLanguage } from '../i18n/LanguageContext';

const CompareBar = () => {
  const { compareItems, removeFromCompare, clearCompare, count } = useCompare();
  const navigate = useNavigate();
  const { t } = useLanguage();

  return (
    <Slide direction="up" in={count > 0} mountOnEnter unmountOnExit>
      <Box sx={{
        position: 'fixed', bottom: 0, left: 0, right: 0, zIndex: 1300,
        bgcolor: 'background.paper', borderTop: '2px solid', borderColor: 'primary.main',
        px: 2, py: 1.5, display: 'flex', alignItems: 'center', gap: 1.5,
        boxShadow: '0 -4px 20px rgba(0,0,0,0.3)',
      }}>
        <CompareArrowsIcon color="primary" />
        <Typography variant="body2" sx={{ fontWeight: 600, mr: 1 }}>
          {count}/5
        </Typography>

        <Box sx={{ display: 'flex', gap: 1, flexGrow: 1, overflow: 'auto' }}>
          {compareItems.map((item) => (
            <Box key={item.id} sx={{ position: 'relative', flexShrink: 0 }}>
              <Avatar
                src={item.photoUrl}
                variant="rounded"
                sx={{ width: 40, height: 40 }}
              />
              <IconButton
                size="small"
                onClick={() => removeFromCompare(item.id)}
                sx={{
                  position: 'absolute', top: -8, right: -8,
                  bgcolor: 'error.main', color: 'white',
                  width: 18, height: 18, '&:hover': { bgcolor: 'error.dark' },
                }}
              >
                <CloseIcon sx={{ fontSize: 12 }} />
              </IconButton>
            </Box>
          ))}
        </Box>

        <Button size="small" variant="outlined" onClick={clearCompare} sx={{ textTransform: 'none' }}>
          {t('compare.clear') || 'Clear'}
        </Button>
        <Button
          size="small"
          variant="contained"
          onClick={() => navigate('/compare')}
          sx={{ textTransform: 'none' }}
        >
          {t('compare.compare') || 'Compare'}
        </Button>
      </Box>
    </Slide>
  );
};

export default CompareBar;
