import client from './client';

export const syncBandPhotos = () => client.post('/band-photos/sync');
