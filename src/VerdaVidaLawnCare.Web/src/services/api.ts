import axios from 'axios';
import type { CreateEstimateRequest, EstimateResponse } from '@/types/estimate';
import type { ServiceDto } from '@/types/service';
import type { CustomerSearchResponse, CustomerSearchRequest } from '@/types/customer';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

export const estimateApi = {
  create: async (data: CreateEstimateRequest): Promise<EstimateResponse> => {
    const response = await api.post<EstimateResponse>('/api/estimates', data);
    return response.data;
  },
};

export const servicesApi = {
  getAll: async (): Promise<ServiceDto[]> => {
    const response = await api.get<ServiceDto[]>('/api/services');
    return response.data;
  },
};

export const customerApi = {
  search: async (request: CustomerSearchRequest): Promise<CustomerSearchResponse> => {
    const response = await api.get<CustomerSearchResponse>('/api/customers/search', {
      params: {
        query: request.query,
        maxResults: request.maxResults,
      },
    });
    return response.data;
  },
};

export default api;
