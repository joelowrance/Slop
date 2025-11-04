export interface CustomerSearchResult {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
  fullAddress: string;
}

export interface CustomerSearchResponse {
  customers: CustomerSearchResult[];
  totalCount: number;
}

export interface CustomerSearchRequest {
  query: string;
  maxResults?: number;
}

