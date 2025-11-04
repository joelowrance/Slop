import { useState, useEffect, useRef } from 'react';
import { customerApi } from '@/services/api';
import type { CustomerSearchResult } from '@/types/customer';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Loader2, Search, X } from 'lucide-react';

interface CustomerSearchProps {
  onSelectCustomer: (customer: CustomerSearchResult) => void;
}

export function CustomerSearch({ onSelectCustomer }: CustomerSearchProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [results, setResults] = useState<CustomerSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isOpen, setIsOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerSearchResult | null>(null);
  const searchTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  // Debounced search
  useEffect(() => {
    // Clear previous timeout
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    // If search query is empty, clear results
    if (!searchQuery.trim()) {
      setResults([]);
      setIsOpen(false);
      return;
    }

    // If customer is already selected, don't search
    if (selectedCustomer) {
      return;
    }

    // Set up debounced search (300ms delay)
    setIsLoading(true);
    setError(null);

    searchTimeoutRef.current = setTimeout(async () => {
      try {
        const response = await customerApi.search({
          query: searchQuery.trim(),
          maxResults: 20,
        });
        setResults(response.customers);
        setIsOpen(response.customers.length > 0);
      } catch (err: any) {
        setError(err.response?.data?.detail || err.message || 'Failed to search customers');
        setResults([]);
        setIsOpen(false);
      } finally {
        setIsLoading(false);
      }
    }, 300);

    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, [searchQuery, selectedCustomer]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleSelectCustomer = (customer: CustomerSearchResult) => {
    setSelectedCustomer(customer);
    setSearchQuery('');
    setResults([]);
    setIsOpen(false);
    setError(null);
    onSelectCustomer(customer);
  };

  const handleClearSelection = () => {
    setSelectedCustomer(null);
    setSearchQuery('');
    setResults([]);
    setIsOpen(false);
    setError(null);
  };

  return (
    <div ref={containerRef} className="relative">
      <div className="space-y-2">
        <Label htmlFor="customer-search">
          Search Existing Customer <span className="text-slate-500 font-normal">(Optional)</span>
        </Label>
        <div className="relative">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-slate-400" />
            <Input
              id="customer-search"
              type="text"
              placeholder="Search by phone, email, or street address..."
              value={searchQuery}
              onChange={(e) => {
                setSearchQuery(e.target.value);
                setSelectedCustomer(null);
              }}
              disabled={!!selectedCustomer}
              className="pl-10 pr-10"
            />
            {isLoading && (
              <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                <Loader2 className="h-4 w-4 animate-spin text-slate-400" />
              </div>
            )}
            {selectedCustomer && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={handleClearSelection}
                className="absolute right-1 top-1/2 transform -translate-y-1/2 h-8 w-8 p-0"
              >
                <X className="h-4 w-4" />
              </Button>
            )}
          </div>

          {/* Search Results Dropdown */}
          {isOpen && results.length > 0 && (
            <div className="absolute z-50 w-full mt-1 bg-white border border-slate-200 rounded-md shadow-lg max-h-96 overflow-y-auto">
              {results.map((customer) => (
                <button
                  key={customer.id}
                  type="button"
                  onClick={() => handleSelectCustomer(customer)}
                  className="w-full text-left px-4 py-3 hover:bg-slate-50 border-b border-slate-100 last:border-b-0 transition-colors"
                >
                  <div className="font-semibold text-slate-900">
                    {customer.firstName} {customer.lastName}
                  </div>
                  <div className="text-sm text-slate-600 mt-1">{customer.email}</div>
                  <div className="text-sm text-slate-600">{customer.phone}</div>
                  <div className="text-sm text-slate-500 mt-1">{customer.fullAddress}</div>
                </button>
              ))}
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="mt-2 text-sm text-red-600">{error}</div>
          )}

          {/* No Results Message */}
          {!isLoading && searchQuery.trim() && results.length === 0 && !error && !selectedCustomer && (
            <div className="mt-2 text-sm text-slate-500">
              No customers found matching &quot;{searchQuery}&quot;
            </div>
          )}

          {/* Selected Customer Indicator */}
          {selectedCustomer && (
            <div className="mt-2 p-3 bg-green-50 border border-green-200 rounded-md">
              <div className="flex items-center justify-between">
                <div>
                  <div className="font-semibold text-green-900">
                    Selected: {selectedCustomer.firstName} {selectedCustomer.lastName}
                  </div>
                  <div className="text-sm text-green-700 mt-1">{selectedCustomer.fullAddress}</div>
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={handleClearSelection}
                  className="text-green-700 hover:text-green-900"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

