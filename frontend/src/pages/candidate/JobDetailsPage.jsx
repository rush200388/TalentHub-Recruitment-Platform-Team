import { useState } from 'react';
import {
  useNavigate,
  useParams,
} from 'react-router-dom';
import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  jobService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Spinner from '../../components/ui/Spinner';
import Modal from '../../components/ui/Modal';
import { Alert } from '../../components/ui/Alert';
import { Textarea } from '../../components/ui/FormField';
import { limits } from '../../utils/validation';

export default function JobDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const {
    data: job,
    loading,
    error,
  } = useAsync(
    () => jobService.get(id),
    [id],
  );

  const [applyOpen, setApplyOpen] =
    useState(false);
  const [coverLetter, setCoverLetter] =
    useState('');
  const [submitting, setSubmitting] =
    useState(false);
  const [success, setSuccess] =
    useState('');
  const [submitError, setSubmitError] =
    useState('');

  const onApply = async () => {
    setSubmitError('');

    if (
      coverLetter.length >
      limits.coverLetter
    ) {
      setSubmitError(
        'Cover letter cannot exceed 3000 characters.',
      );
      return;
    }

    setSubmitting(true);

    try {
      await applicationService.create({
        jobId: id,
        coverLetter:
          coverLetter.trim() || null,
      });

      setApplyOpen(false);
      setSuccess(
        'Application submitted successfully.',
      );
    } catch (applicationError) {
      setSubmitError(
        applicationError.message,
      );
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <Spinner />;

  if (error) {
    return (
      <Alert variant="error">
        {error}
      </Alert>
    );
  }

  if (!job) {
    return (
      <Alert variant="error">
        Job not found
      </Alert>
    );
  }

  return (
    <div>
      {success && (
        <Alert
          variant="success"
          onClose={() => setSuccess('')}
        >
          {success}
        </Alert>
      )}

      <Button
        variant="secondary"
        className="mb-2"
        onClick={() => navigate(-1)}
      >
        ← Back
      </Button>

      <Card>
        <div className="flex items-center justify-between gap-2 flex-wrap">
          <div>
            <h1>{job.title}</h1>
            <div className="text-secondary">
              {job.organization} ·{' '}
              {job.department} ·{' '}
              {job.recruiter}
            </div>
          </div>

          <Badge
            variant={
              job.status === 'Open'
                ? 'success'
                : 'neutral'
            }
          >
            {job.status}
          </Badge>
        </div>

        <div className="job-meta">
          <span>📍 {job.location}</span>
          <span>💼 {job.type}</span>
          <span>🏠 {job.remote}</span>
          <span>💰 {job.salary}</span>
          <span>⏱ {job.experience}</span>
          <span>
            📅 Posted {job.posted}
          </span>
        </div>

        <div className="job-tags mb-3">
          {job.skills?.map((skill) => (
            <Badge
              key={skill}
              variant="primary"
            >
              {skill}
            </Badge>
          ))}
        </div>

        <h3>Job Description</h3>
        <p>{job.description}</p>

        {job.responsibilities && (
          <>
            <h3>Responsibilities</h3>
            <p>{job.responsibilities}</p>
          </>
        )}

        {job.requirements && (
          <>
            <h3>Requirements</h3>
            <p>{job.requirements}</p>
          </>
        )}

        <div className="mt-3">
          {job.status === 'Open' ? (
            <Button
              onClick={() =>
                setApplyOpen(true)
              }
            >
              Apply Now
            </Button>
          ) : (
            <Badge variant="neutral">
              This position is closed
            </Badge>
          )}
        </div>
      </Card>

      <Modal
        open={applyOpen}
        onClose={() =>
          setApplyOpen(false)
        }
        title={`Apply for ${job.title}`}
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setApplyOpen(false)
              }
            >
              Cancel
            </Button>

            <Button
              loading={submitting}
              onClick={onApply}
            >
              Submit Application
            </Button>
          </>
        }
      >
        {submitError && (
          <Alert variant="error">
            {submitError}
          </Alert>
        )}

        <p className="text-secondary">
          Your saved profile and skills will
          be used to calculate the match score.
        </p>

        <Textarea
          label="Cover letter (optional)"
          maxLength={limits.coverLetter}
          value={coverLetter}
          onChange={(event) =>
            setCoverLetter(
              event.target.value,
            )
          }
          placeholder="Tell the employer why you are a strong fit..."
          hint={`${coverLetter.length}/${limits.coverLetter} characters`}
        />
      </Modal>
    </div>
  );
}
