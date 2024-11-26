import { AlbumStatus, MediaType } from './enums';

export interface Album {
  id: string;
  distributorId: string;
  distributor: string;
  bandId: string;
  band: string;
  sku: string;
  name: string;
  releaseDate: string;
  genre?: string;
  price: number;
  purchaseUrl: string;
  photoUrl: string;
  media?: MediaType;
  label: string;
  press: string;
  description?: string;
  status?: AlbumStatus;
  modificationTime: string;
}