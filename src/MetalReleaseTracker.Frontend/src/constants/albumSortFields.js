/**
 * Album sort field mapping between UI and API
 * These values match the AlbumSortField enum on the backend
 */
export const ALBUM_SORT_FIELDS = {
  NAME: 0,        // AlbumSortField.Name 
  PRICE: 1,       // AlbumSortField.Price
  RELEASE_DATE: 2, // AlbumSortField.ReleaseDate
  BAND: 3,        // AlbumSortField.Band
  DISTRIBUTOR: 4, // AlbumSortField.Distributor
  MEDIA: 5,       // AlbumSortField.Media
  STATUS: 6,      // AlbumSortField.Status
  ORIGINAL_YEAR: 7 // AlbumSortField.OriginalYear
};

// UI-friendly names for the sort fields
export const SORT_FIELD_NAMES = {
  [ALBUM_SORT_FIELDS.NAME]: 'Name',
  [ALBUM_SORT_FIELDS.PRICE]: 'Price',
  [ALBUM_SORT_FIELDS.RELEASE_DATE]: 'Release Date',
  [ALBUM_SORT_FIELDS.BAND]: 'Band',
  [ALBUM_SORT_FIELDS.DISTRIBUTOR]: 'Distributor',
  [ALBUM_SORT_FIELDS.MEDIA]: 'Media Type',
  [ALBUM_SORT_FIELDS.STATUS]: 'Status',
  [ALBUM_SORT_FIELDS.ORIGINAL_YEAR]: 'Year'
};

export default ALBUM_SORT_FIELDS; 