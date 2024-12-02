import axios from "axios";

const API_BASE_URL = "https://localhost:44354/api";

export const fetchAllAlbums = async (distributorId: string) => {
  const response = await axios.get(`${API_BASE_URL}/albums`, {
    params: { distributorId },
  });
  return response.data;
};

export const fetchFilteredAlbums = async (filters: {
  BandName?: string;
  Media?: number;
  Status?: number;
  Page?: number;
  PageSize?: number;
}) => {
  const response = await axios.get(`${API_BASE_URL}/albums/filter`, { params: filters });
  return response.data;
};

export const fetchAlbumById = async (id: string) => {
    const response = await axios.get(`${API_BASE_URL}/albums/album?id=${id}`);
    return response.data;
};

export const fetchAvailableBands = async () => {
  const response = await axios.get(`${API_BASE_URL}/bands`);
  return response.data;
};