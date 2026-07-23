import { useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
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

const blankFeedback = {
  overallRating: 3,
  technicalScore: 3,
  communicationScore: 3,
  cultureFitScore: 3,
  recommendation: 'Consider',
  comments: '',
};

function localDateTime(value) {
  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  ).format(new Date(value));
}

function scoreError(value, label) {
  const number = Number(value);

  if (
    !Number.isInteger(number) ||
    number < 1 ||
    number > 5
  ) {
    return `${label} must be 1–5`;
  }

  return '';
}

export default function ManagerInterviewsPage() {
  const {
    data: interviews,
    loading,
    error,
    reload,
  } = useAsync(
    () => interviewService.list(),
    [],
  );

  const [selected, setSelected] =
    useState(null);
  const [form, setForm] =
    useState(blankFeedback);
  const [errors, setErrors] =
    useState({});
  const [saving, setSaving] =
    useState(false);
  const [toast, setToast] =
    useState('');
  const [actionError, setActionError] =
    useState('');

  const openInterview = (
    interview,
  ) => {
    setSelected(interview);

    setForm(
      interview.feedback
        ? {
            overallRating:
              interview.feedback
                .overallRating,
            technicalScore:
              interview.feedback
                .technicalScore,
            communicationScore:
              interview.feedback
                .communicationScore,
            cultureFitScore:
              interview.feedback
                .cultureFitScore,
            recommendation:
              interview.feedback
                .recommendation,
            comments:
              interview.feedback
                .comments || '',
          }
        : blankFeedback,
    );

    setErrors({});
    setActionError('');
  };

  const validate = () => {
    const nextErrors = {};

    [
      [
        'overallRating',
        'Overall rating',
      ],
      [
        'technicalScore',
        'Technical score',
      ],
      [
        'communicationScore',
        'Communication score',
      ],
      [
        'cultureFitScore',
        'Culture-fit score',
      ],
    ].forEach(([key, label]) => {
      const message = scoreError(
        form[key],
        label,
      );

      if (message) {
        nextErrors[key] = message;
      }
    });

    if (!form.recommendation) {
      nextErrors.recommendation =
        'Select a recommendation';
    }

    if (form.comments.length > 3000) {
      nextErrors.comments =
        'Comments cannot exceed 3000 characters';
    }

    setErrors(nextErrors);

    return (
      Object.keys(nextErrors).length ===
      0
    );
  };

  const submit = async () => {
    if (!selected || !validate()) {
      return;
    }

    setSaving(true);
    setActionError('');

    try {
      await interviewService
        .submitFeedback(
          selected.id,
          {
            overallRating:
              Number(
                form.overallRating,
              ),
            technicalScore:
              Number(
                form.technicalScore,
              ),
            communicationScore:
              Number(
                form.communicationScore,
              ),
            cultureFitScore:
              Number(
                form.cultureFitScore,
              ),
            recommendation:
              form.recommendation,
            comments:
              form.comments.trim() ||
              null,
          },
        );

      setSelected(null);
      await reload();

      setToast(
        'Interview feedback submitted.',
      );

      setTimeout(
        () => setToast(''),
        3000,
      );
    } catch (submitError) {
      setActionError(
        submitError.message,
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
            Review assigned interviews and
            submit structured feedback
          </p>
        </div>
      </div>

      {toast && (
        <Alert
          variant="success"
          onClose={() => setToast('')}
        >
          {toast}
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
              title="No assigned interviews"
              message="Interviews assigned by recruiters will appear here."
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
                          {interview.status !==
                            'Cancelled' && (
                            <Button
                              size="sm"
                              variant={
                                interview.status ===
                                'Completed'
                                  ? 'ghost'
                                  : 'secondary'
                              }
                              onClick={() =>
                                openInterview(
                                  interview,
                                )
                              }
                            >
                              {interview.status ===
                              'Completed'
                                ? 'View'
                                : 'Add Feedback'}
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
        open={Boolean(selected)}
        onClose={() =>
          setSelected(null)
        }
        title="Interview Feedback"
        size="lg"
        footer={
          selected?.status ===
          'Completed' ? (
            <Button
              variant="secondary"
              onClick={() =>
                setSelected(null)
              }
            >
              Close
            </Button>
          ) : (
            <>
              <Button
                variant="secondary"
                onClick={() =>
                  setSelected(null)
                }
              >
                Cancel
              </Button>

              <Button
                loading={saving}
                onClick={submit}
              >
                Submit Feedback
              </Button>
            </>
          )
        }
      >
        {selected && (
          <div>
            {actionError && (
              <Alert variant="error">
                {actionError}
              </Alert>
            )}

            <div className="mb-2">
              <strong>
                {selected.candidateName}
              </strong>{' '}
              — {selected.jobTitle}
            </div>

            <div className="text-sm text-muted mb-2">
              {selected.type} ·{' '}
              {localDateTime(
                selected.startTimeUtc,
              )}
            </div>

            <div className="form-row">
              <Input
                label="Overall rating"
                type="number"
                min="1"
                max="5"
                step="1"
                value={form.overallRating}
                disabled={
                  selected.status ===
                  'Completed'
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    overallRating:
                      event.target.value,
                  })
                }
                error={
                  errors.overallRating
                }
              />

              <Input
                label="Technical score"
                type="number"
                min="1"
                max="5"
                step="1"
                value={form.technicalScore}
                disabled={
                  selected.status ===
                  'Completed'
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    technicalScore:
                      event.target.value,
                  })
                }
                error={
                  errors.technicalScore
                }
              />
            </div>

            <div className="form-row">
              <Input
                label="Communication score"
                type="number"
                min="1"
                max="5"
                step="1"
                value={
                  form.communicationScore
                }
                disabled={
                  selected.status ===
                  'Completed'
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    communicationScore:
                      event.target.value,
                  })
                }
                error={
                  errors.communicationScore
                }
              />

              <Input
                label="Culture-fit score"
                type="number"
                min="1"
                max="5"
                step="1"
                value={
                  form.cultureFitScore
                }
                disabled={
                  selected.status ===
                  'Completed'
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    cultureFitScore:
                      event.target.value,
                  })
                }
                error={
                  errors.cultureFitScore
                }
              />
            </div>

            <Select
              label="Recommendation"
              required
              value={form.recommendation}
              disabled={
                selected.status ===
                'Completed'
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  recommendation:
                    event.target.value,
                })
              }
              error={
                errors.recommendation
              }
            >
              {[
                'Strong Hire',
                'Hire',
                'Consider',
                'Reject',
                'Strong Reject',
              ].map(
                (recommendation) => (
                  <option
                    key={recommendation}
                    value={recommendation}
                  >
                    {recommendation}
                  </option>
                ),
              )}
            </Select>

            <Textarea
              label="Comments"
              maxLength={3000}
              value={form.comments}
              disabled={
                selected.status ===
                'Completed'
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  comments:
                    event.target.value,
                })
              }
              error={errors.comments}
              hint={`${form.comments.length}/3000 characters`}
            />
          </div>
        )}
      </Modal>
    </div>
  );
}
