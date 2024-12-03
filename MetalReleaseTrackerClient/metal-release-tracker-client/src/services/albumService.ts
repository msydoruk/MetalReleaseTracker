import axios from "axios";

const API_BASE_URL = "https://localhost:44354/api";

export const fetchFilteredAlbums = async (filters: {
  DistributorId?: string;
  BandId?: string;
  AlbumName?: string;
  Media?: number;
  Status?: number;
  Page?: number;
  PageSize?: number;
}) => {
  const response = await axios.get(`${API_BASE_URL}/albums/filter`, { params: filters });
  return response.data;
};

export const fetchAlbumById = async (id: string) => {
    const response = await axios.get(`${API_BASE_URL}/albums/${id}`);
    return response.data;
};

export const fetchAvailableBands = async () => {
  const response = await axios.get(`${API_BASE_URL}/bands`);
  return response.data;
};