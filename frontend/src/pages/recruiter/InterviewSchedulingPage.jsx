import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  hiringManagerService,
  interviewService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import Modal from '../../components/ui/Modal';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import {
  Input,
  Select,
  Textarea,
} from '../../components/ui/FormField';

const initialForm = {
  applicationId: '',
  interviewerUserId: '',
  date: '',
  time: '10:00',
  durationMinutes: 60,
  type: 'Online',
  meetingLink: '',
  location: '',
  notes: '',
};

function localDateTime(value) {
  if (!value) return '';

  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  ).format(new Date(value));
}

function toUtcRange(form) {
  const start = new Date(
    `${form.date}T${form.time}:00`,
  );

  const end = new Date(
    start.getTime() +
      Number(form.durationMinutes) *
        60 *
        1000,
  );

  return {
    startTimeUtc:
      start.toISOString(),
    endTimeUtc:
      end.toISOString(),
  };
}

export default function InterviewSchedulingPage() {
  const {
    data: interviews,
    loading,
    error,
    reload: reloadInterviews,
  } = useAsync(
    () => interviewService.list(),
    [],
  );

  const {
    data: applications,
    loading: applicationsLoading,
    reload: reloadApplications,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const {
    data: managers,
    loading: managersLoading,
  } = useAsync(
    () => hiringManagerService.list(),
    [],
  );

  const [modalOpen, setModalOpen] =
    useState(false);
  const [form, setForm] =
    useState(initialForm);
  const [errors, setErrors] =
    useState({});
  const [saving, setSaving] =
    useState(false);
  const [feedback, setFeedback] =
    useState('');
  const [actionError, setActionError] =
    useState('');
  const [cancelTarget, setCancelTarget] =
    useState(null);

  const shortlisted = useMemo(
    () =>
      (applications || []).filter(
        (application) =>
          application.status ===
          'Shortlisted',
      ),
    [applications],
  );

  const openSchedule = () => {
    setForm({
      ...initialForm,
      interviewerUserId:
        managers?.[0]?.userId || '',
    });
    setErrors({});
    setActionError('');
    setModalOpen(true);
  };

  const validate = () => {
    const nextErrors = {};

    if (!form.applicationId) {
      nextErrors.applicationId =
        'Select a shortlisted candidate';
    }

    if (!form.interviewerUserId) {
      nextErrors.interviewerUserId =
        'Select a hiring manager';
    }

    if (!form.date) {
      nextErrors.date =
        'Interview date is required';
    }

    if (!form.time) {
      nextErrors.time =
        'Start time is required';
    }

    const duration =
      Number(form.durationMinutes);

    if (
      !Number.isInteger(duration) ||
      duration < 15 ||
      duration > 480
    ) {
      nextErrors.durationMinutes =
        'Duration must be 15–480 minutes';
    }

    if (
      form.date &&
      form.time &&
      new Date(
        `${form.date}T${form.time}:00`,
      ) <= new Date()
    ) {
      nextErrors.time =
        'Interview time must be in the future';
    }

    if (form.type === 'Online') {
      try {
        const link = new URL(
          form.meetingLink,
        );

        if (
          !['http:', 'https:'].includes(
            link.protocol,
          )
        ) {
          nextErrors.meetingLink =
            'Use a valid http:// or https:// meeting link';
        }
      } catch {
        nextErrors.meetingLink =
          'Online interviews require a valid meeting link';
      }
    }

    if (
      form.type === 'Onsite' &&
      !form.location.trim()
    ) {
      nextErrors.location =
        'Onsite interviews require a location';
    }

    if (form.notes.length > 3000) {
      nextErrors.notes =
        'Notes cannot exceed 3000 characters';
    }

    setErrors(nextErrors);

    return (
      Object.keys(nextErrors).length ===
      0
    );
  };

  const onSchedule = async () => {
    setActionError('');

    if (!validate()) return;

    setSaving(true);

    try {
      const range = toUtcRange(form);

      await interviewService.create({
        jobApplicationId:
          form.applicationId,
        interviewerUserId:
          form.interviewerUserId,
        ...range,
        type: form.type,
        meetingLink:
          form.type === 'Online'
            ? form.meetingLink.trim()
            : null,
        location:
          form.type === 'Onsite'
            ? form.location.trim()
            : null,
        notes:
          form.notes.trim() || null,
      });

      setModalOpen(false);

      await Promise.all([
        reloadInterviews(),
        reloadApplications(),
      ]);

      setFeedback(
        'Interview scheduled successfully.',
      );

      setTimeout(
        () => setFeedback(''),
        3000,
      );
    } catch (scheduleError) {
      setActionError(
        scheduleError.message,
      );
    } finally {
      setSaving(false);
    }
  };

  const onCancel = async () => {
    if (!cancelTarget) return;

    setSaving(true);
    setActionError('');

    try {
      await interviewService.update(
        cancelTarget.id,
        {
          interviewerUserId:
            cancelTarget
              .interviewerUserId,
          startTimeUtc:
            cancelTarget.startTimeUtc,
          endTimeUtc:
            cancelTarget.endTimeUtc,
          type: cancelTarget.type,
          status: 'Cancelled',
          meetingLink:
            cancelTarget.meetingLink,
          location:
            cancelTarget.location,
          notes:
            cancelTarget.notes,
        },
      );

      setCancelTarget(null);

      await Promise.all([
        reloadInterviews(),
        reloadApplications(),
      ]);

      setFeedback(
        'Interview cancelled.',
      );

      setTimeout(
        () => setFeedback(''),
        3000,
      );
    } catch (cancelError) {
      setCancelTarget(null);
      setActionError(
        cancelError.message,
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            Interviews
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Schedule interviews and assign
            hiring managers
          </p>
        </div>

        <Button
          onClick={openSchedule}
          disabled={
            shortlisted.length === 0 ||
            managersLoading
          }
        >
          + Schedule Interview
        </Button>
      </div>

      {feedback && (
        <Alert
          variant="success"
          onClose={() =>
            setFeedback('')
          }
        >
          {feedback}
        </Alert>
      )}

      {actionError && (
        <Alert
          variant="error"
          onClose={() =>
            setActionError('')
          }
        >
          {actionError}
        </Alert>
      )}

      {shortlisted.length === 0 &&
        !applicationsLoading && (
          <Alert variant="info">
            Shortlist a candidate before
            scheduling an interview.
          </Alert>
        )}

      <Card>
        {loading ? (
          <Spinner />
        ) : error ? (
          <Alert variant="error">
            {error}
          </Alert>
        ) : null}

        {!loading &&
          !error &&
          (interviews || []).length ===
            0 && (
            <EmptyState
              title="No interviews scheduled"
              message="Scheduled interviews will appear here."
            />
          )}

        {!loading &&
          !error &&
          (interviews || []).length >
            0 && (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Candidate</th>
                    <th>Job</th>
                    <th>Date &amp; time</th>
                    <th>Type</th>
                    <th>Interviewer</th>
                    <th>Status</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {(interviews || []).map(
                    (interview) => (
                      <tr key={interview.id}>
                        <td
                          style={{
                            fontWeight: 600,
                          }}
                        >
                          {
                            interview.candidateName
                          }
                        </td>

                        <td>
                          {interview.jobTitle}
                        </td>

                        <td>
                          {localDateTime(
                            interview.startTimeUtc,
                          )}
                        </td>

                        <td>
                          {interview.type}
                        </td>

                        <td>
                          {
                            interview.interviewerName
                          }
                        </td>

                        <td>
                          <Badge
                            variant={
                              interview.status ===
                              'Completed'
                                ? 'success'
                                : interview.status ===
                                    'Cancelled'
                                  ? 'error'
                                  : 'info'
                            }
                          >
                            {
                              interview.status
                            }
                          </Badge>
                        </td>

                        <td>
                          {interview.status ===
                            'Scheduled' && (
                            <Button
                              size="sm"
                              variant="danger"
                              onClick={() =>
                                setCancelTarget(
                                  interview,
                                )
                              }
                            >
                              Cancel
                            </Button>
                          )}
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          )}
      </Card>

      <Modal
        open={modalOpen}
        onClose={() =>
          setModalOpen(false)
        }
        title="Schedule Interview"
        size="lg"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setModalOpen(false)
              }
            >
              Cancel
            </Button>

            <Button
              loading={saving}
              onClick={onSchedule}
            >
              Schedule
            </Button>
          </>
        }
      >
        {actionError && (
          <Alert variant="error">
            {actionError}
          </Alert>
        )}

        <Select
          label="Shortlisted candidate"
          required
          value={form.applicationId}
          onChange={(event) =>
            setForm({
              ...form,
              applicationId:
                event.target.value,
            })
          }
          error={errors.applicationId}
        >
          <option value="">
            Select candidate...
          </option>

          {shortlisted.map(
            (application) => (
              <option
                key={application.id}
                value={application.id}
              >
                {application.candidateName} —{' '}
                {application.jobTitle}
              </option>
            ),
          )}
        </Select>

        <Select
          label="Hiring manager"
          required
          value={form.interviewerUserId}
          onChange={(event) =>
            setForm({
              ...form,
              interviewerUserId:
                event.target.value,
            })
          }
          error={
            errors.interviewerUserId
          }
        >
          <option value="">
            Select manager...
          </option>

          {(managers || []).map(
            (manager) => (
              <option
                key={manager.userId}
                value={manager.userId}
              >
                {manager.name} —{' '}
                {manager.department}
              </option>
            ),
          )}
        </Select>

        <div className="form-row">
          <Input
            label="Date"
            type="date"
            required
            min={
              new Date()
                .toISOString()
                .slice(0, 10)
            }
            value={form.date}
            onChange={(event) =>
              setForm({
                ...form,
                date: event.target.value,
              })
            }
            error={errors.date}
          />

          <Input
            label="Start time"
            type="time"
            required
            value={form.time}
            onChange={(event) =>
              setForm({
                ...form,
                time: event.target.value,
              })
            }
            error={errors.time}
          />
        </div>

        <div className="form-row">
          <Input
            label="Duration (minutes)"
            type="number"
            min="15"
            max="480"
            step="15"
            value={
              form.durationMinutes
            }
            onChange={(event) =>
              setForm({
                ...form,
                durationMinutes:
                  event.target.value,
              })
            }
            error={
              errors.durationMinutes
            }
          />

          <Select
            label="Interview type"
            value={form.type}
            onChange={(event) =>
              setForm({
                ...form,
                type: event.target.value,
              })
            }
          >
            <option value="Online">
              Online
            </option>
            <option value="Onsite">
              Onsite
            </option>
            <option value="Phone">
              Phone
            </option>
          </Select>
        </div>

        {form.type === 'Online' && (
          <Input
            label="Meeting link"
            type="url"
            required
            maxLength={500}
            placeholder="https://meet.example.com/..."
            value={form.meetingLink}
            onChange={(event) =>
              setForm({
                ...form,
                meetingLink:
                  event.target.value,
              })
            }
            error={errors.meetingLink}
          />
        )}

        {form.type === 'Onsite' && (
          <Input
            label="Location"
            required
            maxLength={200}
            value={form.location}
            onChange={(event) =>
              setForm({
                ...form,
                location:
                  event.target.value,
              })
            }
            error={errors.location}
          />
        )}

        <Textarea
          label="Notes"
          maxLength={3000}
          value={form.notes}
          onChange={(event) =>
            setForm({
              ...form,
              notes: event.target.value,
            })
          }
          error={errors.notes}
          hint={`${form.notes.length}/3000 characters`}
        />
      </Modal>

      <Modal
        open={Boolean(cancelTarget)}
        onClose={() =>
          setCancelTarget(null)
        }
        title="Cancel Interview?"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setCancelTarget(null)
              }
            >
              Keep Interview
            </Button>

            <Button
              variant="danger"
              loading={saving}
              onClick={onCancel}
            >
              Cancel Interview
            </Button>
          </>
        }
      >
        <p>
          Cancel the interview for{' '}
          <strong>
            {cancelTarget?.candidateName}
          </strong>
          ?
        </p>
      </Modal>
    </div>
  );
}
