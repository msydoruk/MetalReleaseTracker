import React, { useState } from 'react';
import { IconButton, Menu, MenuItem, ListItemIcon, ListItemText } from '@mui/material';
import FavoriteIcon from '@mui/icons-material/Favorite';
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import BookmarkIcon from '@mui/icons-material/Bookmark';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import DeleteIcon from '@mui/icons-material/Delete';
import { useLanguage } from '../i18n/LanguageContext';

const COLLECTION_STATUS = {
  FAVORITE: 0,
  WANT: 1,
  OWNED: 2,
};

const CollectionStatusMenu = ({ currentStatus, onSelect, onRemove, size = 'small', sx = {} }) => {
  const { t } = useLanguage();
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);

  const handleClick = (event) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
  };

  const handleClose = (event) => {
    if (event) event.stopPropagation();
    setAnchorEl(null);
  };

  const handleSelect = (status) => (event) => {
    event.stopPropagation();
    setAnchorEl(null);
    if (onSelect) onSelect(status);
  };

  const handleRemove = (event) => {
    event.stopPropagation();
    setAnchorEl(null);
    if (onRemove) onRemove();
  };

  const getIcon = () => {
    if (currentStatus === COLLECTION_STATUS.FAVORITE) {
      return <FavoriteIcon sx={{ color: '#f44336', fontSize: 20 }} />;
    }
    if (currentStatus === COLLECTION_STATUS.WANT) {
      return <BookmarkIcon sx={{ color: '#ff9800', fontSize: 20 }} />;
    }
    if (currentStatus === COLLECTION_STATUS.OWNED) {
      return <CheckCircleIcon sx={{ color: '#4caf50', fontSize: 20 }} />;
    }
    return <FavoriteBorderIcon sx={{ color: 'white', fontSize: 20 }} />;
  };

  const isInCollection = currentStatus !== undefined && currentStatus !== null;

  return (
    <>
      <IconButton
        onClick={handleClick}
        size={size}
        sx={sx}
      >
        {getIcon()}
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        onClick={(event) => event.stopPropagation()}
        slotProps={{
          paper: {
            sx: {
              bgcolor: '#1e1e1e',
              border: '1px solid rgba(255,255,255,0.15)',
              minWidth: 180,
            }
          }
        }}
      >
        <MenuItem
          onClick={handleSelect(COLLECTION_STATUS.FAVORITE)}
          selected={currentStatus === COLLECTION_STATUS.FAVORITE}
        >
          <ListItemIcon>
            <FavoriteIcon sx={{ color: '#f44336' }} />
          </ListItemIcon>
          <ListItemText>{t('collection.favorite')}</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={handleSelect(COLLECTION_STATUS.WANT)}
          selected={currentStatus === COLLECTION_STATUS.WANT}
        >
          <ListItemIcon>
            <BookmarkIcon sx={{ color: '#ff9800' }} />
          </ListItemIcon>
          <ListItemText>{t('collection.want')}</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={handleSelect(COLLECTION_STATUS.OWNED)}
          selected={currentStatus === COLLECTION_STATUS.OWNED}
        >
          <ListItemIcon>
            <CheckCircleIcon sx={{ color: '#4caf50' }} />
          </ListItemIcon>
          <ListItemText>{t('collection.owned')}</ListItemText>
        </MenuItem>
        {isInCollection && (
          <MenuItem onClick={handleRemove}>
            <ListItemIcon>
              <DeleteIcon sx={{ color: 'text.secondary' }} />
            </ListItemIcon>
            <ListItemText>{t('collection.remove')}</ListItemText>
          </MenuItem>
        )}
      </Menu>
    </>
  );
};

export default CollectionStatusMenu;
