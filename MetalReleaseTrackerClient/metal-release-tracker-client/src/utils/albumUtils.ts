import { AlbumStatus, MediaType } from "../types/enums";

export const getAlbumStatus = (status: number): string => {
  switch (status) {
    case 0:
      return AlbumStatus.New;
    case 1:
      return AlbumStatus.Restock;
    case 2:
      return AlbumStatus.Preorder;
    case 3:
      return AlbumStatus.Unavailable;
    default:
      return "";
  }
};

export const getAlbumMediaType = (media: number): string => {
  switch (media) {
    case 0:
      return MediaType.CD;
    case 1:
      return MediaType.LP;
    case 2:
      return MediaType.Tape;
    default:
      return "";
  }
};