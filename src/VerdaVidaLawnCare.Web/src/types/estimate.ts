export interface CustomerInfoDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  postalCode: string;
}

export interface EstimateLineItemDto {
  serviceId?: number;
  equipmentId?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  serviceName?: string;
  equipmentName?: string;
}

export interface CreateEstimateRequest {
  customer: CustomerInfoDto;
  lineItems: EstimateLineItemDto[];
  notes: string;
  terms: string;
  expirationDate?: string; // ISO date string
}

export interface CustomerDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  fullAddress: string;
  city: string;
  state: string;
  postalCode: string;
}

export interface EstimateResponse {
  id: number;
  estimateNumber: string;
  customer: CustomerDto;
  estimateDate: string;
  expirationDate: string;
  status: string;
  notes?: string;
  terms?: string;
  lineItems: EstimateLineItemDto[];
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  daysUntilExpiration: number;
  isExpired: boolean;
  createdAt: string;
  updatedAt: string;
}
