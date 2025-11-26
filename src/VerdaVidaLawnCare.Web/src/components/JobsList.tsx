import { useState, useEffect } from 'react';
import { jobsApi } from '@/services/api';
import type { EstimateResponse } from '@/types/estimate';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Loader2, Search, CheckCircle } from 'lucide-react';
import { CompleteJobModal } from '@/components/CompleteJobModal';

export function JobsList() {
  const [jobs, setJobs] = useState<EstimateResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedJob, setSelectedJob] = useState<EstimateResponse | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const fetchJobs = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const fetchedJobs = await jobsApi.getAll({
        status: statusFilter,
        search: searchTerm || undefined,
      });
      setJobs(fetchedJobs);
    } catch (err) {
      console.error('Failed to fetch jobs:', err);
      setError('Failed to load jobs. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchJobs();
  }, [statusFilter, searchTerm]);

  const handleCompleteClick = (job: EstimateResponse) => {
    setSelectedJob(job);
    setIsModalOpen(true);
  };

  const handleJobCompleted = () => {
    setIsModalOpen(false);
    setSelectedJob(null);
    fetchJobs(); // Refresh the list
  };

  const getStatusBadge = (status: string) => {
    const statusLower = status.toLowerCase();
    
    if (statusLower === 'completed') {
      return <Badge className="bg-green-500">Completed</Badge>;
    } else if (statusLower === 'accepted') {
      return <Badge className="bg-blue-500">Open</Badge>;
    } else if (statusLower === 'draft') {
      return <Badge variant="secondary">Draft</Badge>;
    } else if (statusLower === 'sent') {
      return <Badge className="bg-yellow-500">Sent</Badge>;
    } else if (statusLower === 'viewed') {
      return <Badge className="bg-purple-500">Viewed</Badge>;
    } else if (statusLower === 'rejected') {
      return <Badge variant="destructive">Rejected</Badge>;
    } else if (statusLower === 'expired') {
      return <Badge variant="outline">Expired</Badge>;
    }
    
    return <Badge variant="outline">{status}</Badge>;
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }).format(date);
  };

  const isJobCompletable = (job: EstimateResponse) => {
    const status = job.status.toLowerCase();
    // Can complete jobs that are Accepted, Sent, or Viewed
    // Cannot complete Draft, Rejected, Expired, Cancelled, or already Completed jobs
    return status === 'accepted' || status === 'sent' || status === 'viewed';
  };

  return (
    <div className="container mx-auto py-8">
      <Card>
        <CardHeader>
          <CardTitle>Jobs</CardTitle>
          <CardDescription>
            View and manage all jobs. Any estimate that is not cancelled is considered a job.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col md:flex-row gap-4 mb-6">
            <div className="flex-1">
              <Label htmlFor="search">Search Customer</Label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  id="search"
                  type="text"
                  placeholder="Search by customer name, email, or job number..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="w-full md:w-48">
              <Label htmlFor="status">Status</Label>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger id="status">
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Jobs</SelectItem>
                  <SelectItem value="open">Open</SelectItem>
                  <SelectItem value="completed">Completed</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Loading State */}
          {isLoading && (
            <div className="flex justify-center items-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-gray-400" />
            </div>
          )}

          {/* Error State */}
          {error && !isLoading && (
            <div className="text-center py-12">
              <p className="text-red-500 mb-4">{error}</p>
              <Button onClick={fetchJobs}>Try Again</Button>
            </div>
          )}

          {/* Jobs Table */}
          {!isLoading && !error && (
            <>
              {jobs.length === 0 ? (
                <div className="text-center py-12">
                  <p className="text-gray-500">No jobs found.</p>
                </div>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Job #</TableHead>
                        <TableHead>Customer</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Scheduled Date</TableHead>
                        <TableHead>Completed Date</TableHead>
                        <TableHead className="text-right">Amount</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {jobs.map((job) => (
                        <TableRow key={job.id}>
                          <TableCell className="font-medium">
                            {job.estimateNumber}
                          </TableCell>
                          <TableCell>
                            <div>
                              <div className="font-medium">
                                {job.customer.firstName} {job.customer.lastName}
                              </div>
                              <div className="text-sm text-gray-500">
                                {job.customer.email}
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>{getStatusBadge(job.status)}</TableCell>
                          <TableCell>{formatDate(job.scheduledDate)}</TableCell>
                          <TableCell>{formatDate(job.completedDate)}</TableCell>
                          <TableCell className="text-right font-medium">
                            {formatCurrency(job.totalAmount)}
                          </TableCell>
                          <TableCell className="text-right">
                            {isJobCompletable(job) && (
                              <Button
                                size="sm"
                                onClick={() => handleCompleteClick(job)}
                              >
                                <CheckCircle className="h-4 w-4 mr-1" />
                                Mark Complete
                              </Button>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Complete Job Modal */}
      {selectedJob && (
        <CompleteJobModal
          job={selectedJob}
          isOpen={isModalOpen}
          onClose={() => {
            setIsModalOpen(false);
            setSelectedJob(null);
          }}
          onSuccess={handleJobCompleted}
        />
      )}
    </div>
  );
}

