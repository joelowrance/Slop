import { useState } from 'react';
import { jobsApi } from '@/services/api';
import type { EstimateResponse } from '@/types/estimate';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Loader2, CheckCircle, AlertCircle } from 'lucide-react';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface CompleteJobModalProps {
  job: EstimateResponse;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function CompleteJobModal({ job, isOpen, onClose, onSuccess }: CompleteJobModalProps) {
  const [completionNotes, setCompletionNotes] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await jobsApi.complete(job.id, { completionNotes });
      onSuccess();
    } catch (err: any) {
      console.error('Failed to complete job:', err);
      const errorMessage = err.response?.data?.detail || 'Failed to complete job. Please try again.';
      setError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    if (!isSubmitting) {
      setCompletionNotes('');
      setError(null);
      onClose();
    }
  };

  const today = new Date().toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-500" />
              Complete Job
            </DialogTitle>
            <DialogDescription>
              Mark this job as completed and add any final notes.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            {/* Job Details */}
            <div className="rounded-lg bg-gray-50 p-4 space-y-2 border border-gray-200">
              <div className="flex justify-between">
                <span className="text-sm font-medium text-gray-600">Job Number:</span>
                <span className="text-sm font-semibold text-gray-900">{job.estimateNumber}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm font-medium text-gray-600">Customer:</span>
                <span className="text-sm font-semibold text-gray-900">
                  {job.customer.firstName} {job.customer.lastName}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm font-medium text-gray-600">Completion Date:</span>
                <span className="text-sm font-semibold text-gray-900">{today}</span>
              </div>
            </div>

            {/* Completion Notes */}
            <div className="space-y-2">
              <Label htmlFor="completionNotes" className="text-gray-700">
                Completion Notes <span className="text-gray-400">(Optional)</span>
              </Label>
              <Textarea
                id="completionNotes"
                placeholder="Enter any notes about the completed job (e.g., services performed, issues encountered, customer feedback)..."
                value={completionNotes}
                onChange={(e) => setCompletionNotes(e.target.value)}
                rows={5}
                disabled={isSubmitting}
                className="resize-none bg-white border-gray-300 text-gray-900"
              />
              <p className="text-xs text-gray-600">
                These notes will be saved with the job record for future reference.
              </p>
            </div>

            {/* Error Alert */}
            {error && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Completing...
                </>
              ) : (
                <>
                  <CheckCircle className="mr-2 h-4 w-4" />
                  Complete Job
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

