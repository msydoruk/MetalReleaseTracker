import { AlbumStatus } from "../types/enums";

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
