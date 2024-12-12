import axios from "axios";

const API_BASE_URL = "https://localhost:44354/api";

export const fetchFilteredAlbums = async (filters: {
  DistributorId?: string;
  BandId?: string;
  AlbumName?: string;
  Media?: number;
  ReleaseDateStart?: string;
  ReleaseDateEnd?: string; 
  Status?: number;
  Take?: number; 
  Skip?: number; 
  OrderBy?: string;
  Descending?: boolean;
}) => {
  const response = await axios.get(`${API_BASE_URL}/albums/filter`, { params: filters });
  return response.data;
};

export const fetchAlbumById = async (id: string) => {
    const response = await axios.get(`${API_BASE_URL}/albums/${id}`);
    return response.data;
};

export const fetchAvailableBands = async (skip: number, take: number) => {
  try {
    const response = await axios.get(`${API_BASE_URL}/bands`, {
      params: {
        skip: skip,
        take: take,
      },
    });
    return response.data;
  } catch (error) {
    throw new Error("Error loading bands");
  }
};

export const fetchAvailableDistributors = async () => {
  const response = await axios.get(`${API_BASE_URL}/distributors`);
  return response.data;
};