import axios from 'axios';
import type { CreateEstimateRequest, EstimateResponse, CompleteJobRequest } from '@/types/estimate';
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

export const jobsApi = {
  getAll: async (filters?: { 
    status?: string; 
    search?: string; 
    customerId?: number 
  }): Promise<EstimateResponse[]> => {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.search) params.append('search', filters.search);
    if (filters?.customerId) params.append('customerId', filters.customerId.toString());
    
    const response = await api.get<EstimateResponse[]>(
      `/api/jobs${params.toString() ? `?${params.toString()}` : ''}`
    );
    return response.data;
  },
  
  complete: async (id: number, request: CompleteJobRequest): Promise<EstimateResponse> => {
    const response = await api.post<EstimateResponse>(
      `/api/jobs/${id}/complete`, 
      request
    );
    return response.data;
  },
};

export default api;
