import { Formik, Form, Field, FieldArray, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { estimateApi, servicesApi } from '@/services/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Plus, Trash2, Loader2 } from 'lucide-react';
import { useState, useEffect } from 'react';
import type { ServiceDto } from '@/types/service';

const validationSchema = Yup.object({
  customer: Yup.object({
    firstName: Yup.string().required('First name is required'),
    lastName: Yup.string().required('Last name is required'),
    email: Yup.string().email('Invalid email').required('Email is required'),
    phone: Yup.string()
      .required('Phone is required')
      .matches(
        /^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4}$/,
        'Please enter a valid phone number (e.g., (123) 456-7890, 123-456-7890, or 1234567890)'
      ),
    address: Yup.string().required('Address is required'),
    city: Yup.string().required('City is required'),
    state: Yup.string().required('State is required'),
    postalCode: Yup.string().required('Postal code is required'),
  }),
  lineItems: Yup.array()
    .of(
      Yup.object({
        description: Yup.string().required('Description is required'),
        quantity: Yup.number().integer('Must be a whole number').positive('Must be positive').required('Required'),
        unitPrice: Yup.number().positive('Must be positive').required('Required'),
      })
    )
    .min(1, 'At least one line item is required'),
  notes: Yup.string(),
  terms: Yup.string(),
  expirationDate: Yup.date(),
});

interface LineItem {
  serviceId?: number;
  description: string;
  quantity: number;
  unitPrice: number;
}

export function EstimateForm() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [isLoadingServices, setIsLoadingServices] = useState(false);

  useEffect(() => {
    const fetchServices = async () => {
      setIsLoadingServices(true);
      try {
        const fetchedServices = await servicesApi.getAll();
        setServices(fetchedServices);
      } catch (error) {
        console.error('Failed to fetch services:', error);
        // Continue without services - user can still enter manually
      } finally {
        setIsLoadingServices(false);
      }
    };

    fetchServices();
  }, []);

  const initialValues = {
    customer: {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      address: '',
      city: '',
      state: '',
      postalCode: '',
    },
    lineItems: [
      { serviceId: undefined, description: '', quantity: 0, unitPrice: 0, lineTotal: 0 }
    ] as (LineItem & { lineTotal: number })[],
    notes: '',
    terms: '',
    expirationDate: '',
  };

  const calculateLineTotal = (quantity: number, unitPrice: number): number => {
    return quantity * unitPrice;
  };

  const handleSubmit = async (values: typeof initialValues) => {
    setIsSubmitting(true);
    setSubmitError(null);
    setSubmitSuccess(null);

    try {
      // Convert to API format
      const payload = {
        customer: values.customer,
        lineItems: values.lineItems.map(item => ({
          serviceId: item.serviceId || undefined,
          description: item.description,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          lineTotal: calculateLineTotal(item.quantity, item.unitPrice),
        })),
        notes: values.notes,
        terms: values.terms,
        expirationDate: values.expirationDate || undefined,
      };

      const response = await estimateApi.create(payload);
      setSubmitSuccess(`Estimate created successfully! Estimate Number: ${response.estimateNumber}`);
      
      // Reset form after successful submission
      setTimeout(() => {
        window.location.reload();
      }, 2000);
    } catch (error: any) {
      const errorMessage = error.response?.data?.detail || error.message || 'An error occurred';
      setSubmitError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto py-8 px-4 max-w-4xl">
      <Card>
        <CardHeader>
          <CardTitle>Create Estimate</CardTitle>
          <CardDescription>
            Enter customer information and estimate details
          </CardDescription>
        </CardHeader>
        <CardContent>
          {submitError && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-md text-red-700">
              {submitError}
            </div>
          )}
          {submitSuccess && (
            <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-md text-green-700">
              {submitSuccess}
            </div>
          )}
          
          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
          >
            {({ values, errors, touched, setFieldValue }) => {
              const calculateSubtotal = () => {
                return values.lineItems.reduce(
                  (sum, item) => sum + calculateLineTotal(item.quantity, item.unitPrice),
                  0
                );
              };

              const handleServiceChange = async (index: number, serviceId: string) => {
                const selectedServiceId = serviceId ? parseInt(serviceId, 10) : undefined;
                await setFieldValue(`lineItems.${index}.serviceId`, selectedServiceId);
                
                if (selectedServiceId) {
                  const selectedService = services.find(s => s.id === selectedServiceId);
                  if (selectedService) {
                    await setFieldValue(`lineItems.${index}.description`, selectedService.description);
                  }
                }
              };

              return (
                <Form className="space-y-6">
                  {/* Customer Information */}
                  <div>
                    <h2 className="text-xl font-semibold mb-4">Customer Information</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <Label htmlFor="customer.firstName">First Name *</Label>
                        <Field name="customer.firstName" as={Input} />
                        <ErrorMessage name="customer.firstName" component="div" className="text-red-500 text-sm mt-1" />
                      </div>
                      
                      <div>
                        <Label htmlFor="customer.lastName">Last Name *</Label>
                        <Field name="customer.lastName" as={Input} />
                        <ErrorMessage name="customer.lastName" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="customer.email">Email *</Label>
                        <Field name="customer.email" type="email" as={Input} />
                        <ErrorMessage name="customer.email" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="customer.phone">Phone *</Label>
                        <Field name="customer.phone" as={Input} />
                        <ErrorMessage name="customer.phone" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div className="md:col-span-2">
                        <Label htmlFor="customer.address">Address *</Label>
                        <Field name="customer.address" as={Input} />
                        <ErrorMessage name="customer.address" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="customer.city">City *</Label>
                        <Field name="customer.city" as={Input} />
                        <ErrorMessage name="customer.city" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="customer.state">State *</Label>
                        <Field name="customer.state" as={Input} />
                        <ErrorMessage name="customer.state" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="customer.postalCode">Postal Code *</Label>
                        <Field name="customer.postalCode" as={Input} />
                        <ErrorMessage name="customer.postalCode" component="div" className="text-red-500 text-sm mt-1" />
                      </div>
                    </div>
                  </div>

                  {/* Line Items */}
                  <div>
                    <h2 className="text-xl font-semibold mb-4">Line Items</h2>
                    <FieldArray name="lineItems">
                      {({ push, remove }) => (
                        <div className="space-y-4">
                          {values.lineItems.map((item, index) => (
                            <div key={index} className="border rounded-lg p-4 space-y-4">
                              <div className="space-y-4">
                                <div className="grid grid-cols-12 gap-4">
                                  <div className="col-span-12 md:col-span-6">
                                    <Label>Service (Optional)</Label>
                                    <select
                                      className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white placeholder:text-slate-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                      value={item.serviceId || ''}
                                      onChange={(e) => handleServiceChange(index, e.target.value)}
                                      disabled={isLoadingServices}
                                    >
                                      <option value="">Select a service (optional)</option>
                                      {services.map((service) => (
                                        <option key={service.id} value={service.id}>
                                          {service.name}
                                        </option>
                                      ))}
                                    </select>
                                  </div>
                                  
                                  <div className="col-span-12 md:col-span-6">
                                    <Label>Description *</Label>
                                    <Field name={`lineItems.${index}.description`} as={Input} />
                                    <ErrorMessage 
                                      name={`lineItems.${index}.description`} 
                                      component="div" 
                                      className="text-red-500 text-sm mt-1" 
                                    />
                                  </div>
                                </div>
                                
                                <div className="grid grid-cols-12 gap-4">
                                  <div className="col-span-4 md:col-span-3">
                                    <Label>Quantity *</Label>
                                    <Field 
                                      name={`lineItems.${index}.quantity`} 
                                      type="number" 
                                      min="0" 
                                      step="1"
                                      as={Input}
                                    />
                                    <ErrorMessage 
                                      name={`lineItems.${index}.quantity`} 
                                      component="div" 
                                      className="text-red-500 text-sm mt-1" 
                                    />
                                  </div>
                                  
                                  <div className="col-span-4 md:col-span-3">
                                    <Label>Unit Price *</Label>
                                    <Field 
                                      name={`lineItems.${index}.unitPrice`} 
                                      type="number" 
                                      min="0" 
                                      step="0.01"
                                      as={Input}
                                    />
                                    <ErrorMessage 
                                      name={`lineItems.${index}.unitPrice`} 
                                      component="div" 
                                      className="text-red-500 text-sm mt-1" 
                                    />
                                  </div>
                                  
                                  <div className="col-span-4 md:col-span-3">
                                    <Label>Line Total</Label>
                                    <div className="flex h-10 w-full rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm items-center">
                                      ${calculateLineTotal(item.quantity, item.unitPrice).toFixed(2)}
                                    </div>
                                  </div>

                                  <div className="col-span-12 md:col-span-3 flex items-end justify-end md:justify-start">
                                    {values.lineItems.length > 1 && (
                                      <Button
                                        type="button"
                                        variant="destructive"
                                        size="icon"
                                        onClick={() => remove(index)}
                                      >
                                        <Trash2 className="h-4 w-4" />
                                      </Button>
                                    )}
                                  </div>
                                </div>
                              </div>
                            </div>
                          ))}
                          
                          <Button
                            type="button"
                            variant="outline"
                            onClick={() => push({ serviceId: undefined, description: '', quantity: 0, unitPrice: 0, lineTotal: 0 })}
                            className="w-full"
                          >
                            <Plus className="h-4 w-4 mr-2" />
                            Add Line Item
                          </Button>

                          {touched.lineItems && errors.lineItems && (
                            <div className="text-red-500 text-sm">
                              {typeof errors.lineItems === 'string' 
                                ? errors.lineItems 
                                : Array.isArray(errors.lineItems)
                                  ? errors.lineItems.filter(err => err).join(', ')
                                  : 'Please correct the line items above'}
                            </div>
                          )}
                        </div>
                      )}
                    </FieldArray>
                  </div>

                  {/* Totals */}
                  <div className="border rounded-lg p-4 bg-slate-50">
                    <div className="flex justify-between text-lg font-semibold">
                      <span>Subtotal:</span>
                      <span>${calculateSubtotal().toFixed(2)}</span>
                    </div>
                  </div>

                  {/* Additional Fields */}
                  <div>
                    <h2 className="text-xl font-semibold mb-4">Additional Information</h2>
                    <div className="space-y-4">
                      <div>
                        <Label htmlFor="expirationDate">Expiration Date</Label>
                        <Field name="expirationDate" type="date" as={Input} />
                        <ErrorMessage name="expirationDate" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="notes">Notes</Label>
                        <Field name="notes" as={Textarea} rows={4} />
                        <ErrorMessage name="notes" component="div" className="text-red-500 text-sm mt-1" />
                      </div>

                      <div>
                        <Label htmlFor="terms">Terms & Conditions</Label>
                        <Field name="terms" as={Textarea} rows={4} />
                        <ErrorMessage name="terms" component="div" className="text-red-500 text-sm mt-1" />
                      </div>
                    </div>
                  </div>

                  {/* Submit */}
                  <div className="flex justify-end space-x-4">
                    <Button
                      type="submit"
                      disabled={isSubmitting}
                      className="min-w-[120px]"
                    >
                      {isSubmitting ? (
                        <>
                          <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                          Submitting...
                        </>
                      ) : (
                        'Create Estimate'
                      )}
                    </Button>
                  </div>
                </Form>
              );
            }}
          </Formik>
        </CardContent>
      </Card>
    </div>
  );
}
