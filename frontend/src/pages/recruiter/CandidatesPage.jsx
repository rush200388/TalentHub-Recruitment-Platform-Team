import {
  useMemo,
  useState,
} from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Spinner from '../../components/ui/Spinner';
import Modal from '../../components/ui/Modal';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import {
  Input,
  Select,
} from '../../components/ui/FormField';
import Avatar from '../../components/ui/Avatar';
import { ScoreBar } from '../../components/ui/ScoreBar';

function statusVariant(status) {
  if (
    [
      'Shortlisted',
      'Interview Scheduled',
      'Offered',
      'Hired',
    ].includes(status)
  ) {
    return 'success';
  }

  if (status === 'Rejected') {
    return 'error';
  }

  return 'warning';
}

export default function CandidatesPage() {
  const {
    data: applications,
    loading,
    error,
    reload,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const [search, setSearch] =
    useState('');
  const [statusFilter, setStatusFilter] =
    useState('');
  const [selected, setSelected] =
    useState(null);
  const [action, setAction] =
    useState(null);
  const [feedback, setFeedback] =
    useState('');
  const [actionError, setActionError] =
    useState('');
  const [updating, setUpdating] =
    useState(false);

  const filtered = useMemo(() => {
    const term =
      search.trim().toLowerCase();

    return (applications || []).filter(
      (application) => {
        const matchesSearch =
          !term ||
          application.candidateName
            .toLowerCase()
            .includes(term) ||
          application.jobTitle
            .toLowerCase()
            .includes(term) ||
          application.candidateSkills?.some(
            (skill) =>
              skill
                .toLowerCase()
                .includes(term),
          );

        const matchesStatus =
          !statusFilter ||
          application.status ===
            statusFilter;

        return (
          matchesSearch &&
          matchesStatus
        );
      },
    );
  }, [
    applications,
    search,
    statusFilter,
  ]);

  const doAction = async () => {
    if (!action) return;

    setUpdating(true);
    setActionError('');

    try {
      await applicationService.updateStatus(
        action.application.id,
        action.status,
        action.status === 'Shortlisted'
          ? 'Shortlist'
          : 'Closed',
      );

      setAction(null);
      setSelected(null);
      await reload();

      setFeedback(
        action.status === 'Shortlisted'
          ? 'Candidate shortlisted successfully.'
          : 'Candidate rejected.',
      );

      setTimeout(
        () => setFeedback(''),
        3000,
      );
    } catch (updateError) {
      setActionError(
        updateError.message,
      );
    } finally {
      setUpdating(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            Applicant Review
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Review applications ranked by
            AI match score
          </p>
        </div>
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

      <Card className="mb-3">
        <div className="search-bar">
          <Input
            placeholder="Search candidate, job, or skill..."
            value={search}
            onChange={(event) =>
              setSearch(event.target.value)
            }
            className="flex-1"
          />

          <Select
            value={statusFilter}
            onChange={(event) =>
              setStatusFilter(
                event.target.value,
              )
            }
            style={{ minWidth: 180 }}
          >
            <option value="">
              All statuses
            </option>
            <option value="Pending">
              Pending
            </option>
            <option value="Shortlisted">
              Shortlisted
            </option>
            <option value="Rejected">
              Rejected
            </option>
          </Select>
        </div>
      </Card>

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">
          {error}
        </Alert>
      ) : null}

      {!loading &&
        !error &&
        filtered.length === 0 && (
          <EmptyState
            title="No applications found"
            message="Applications to your jobs will appear here."
          />
        )}

      {!loading &&
        !error &&
        filtered.length > 0 && (
          <div className="grid grid-3">
            {filtered.map(
              (application) => (
                <Card key={application.id}>
                  <div className="flex items-center gap-2 mb-1">
                    <Avatar
                      name={
                        application.candidateName
                      }
                    />

                    <div className="flex-1">
                      <h3 style={{ margin: 0 }}>
                        {
                          application.candidateName
                        }
                      </h3>

                      <div className="text-sm text-muted">
                        {application
                          .candidateTitle ||
                          'Candidate'}
                      </div>
                    </div>

                    <Badge
                      variant={statusVariant(
                        application.status,
                      )}
                    >
                      {application.status}
                    </Badge>
                  </div>

                  <div className="text-sm text-secondary mb-1">
                    Applied for{' '}
                    <strong>
                      {application.jobTitle}
                    </strong>
                  </div>

                  <div className="text-sm text-secondary mb-2">
                    📍{' '}
                    {application.candidateLocation ||
                      'Not provided'}{' '}
                    ·{' '}
                    {
                      application.candidateExperience
                    }{' '}
                    yrs
                  </div>

                  <div className="job-tags mb-2">
                    {application.candidateSkills
                      ?.slice(0, 6)
                      .map((skill) => (
                        <Badge
                          key={skill}
                          variant="primary"
                        >
                          {skill}
                        </Badge>
                      ))}
                  </div>

                  <div className="mb-2">
                    <div className="text-sm text-muted mb-1">
                      AI Match Score
                    </div>

                    <ScoreBar
                      score={Math.round(
                        application.matchScore,
                      )}
                    />
                  </div>

                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() =>
                      setSelected(application)
                    }
                  >
                    Review Application
                  </Button>
                </Card>
              ),
            )}
          </div>
        )}

      <Modal
        open={Boolean(selected)}
        onClose={() =>
          setSelected(null)
        }
        title="Application Review"
        size="lg"
      >
        {selected && (
          <div>
            <div className="flex items-center gap-2 mb-3">
              <Avatar
                name={selected.candidateName}
                size="lg"
              />

              <div>
                <h2 style={{ margin: 0 }}>
                  {selected.candidateName}
                </h2>

                <div className="text-secondary">
                  {selected.candidateTitle ||
                    'Candidate'}
                </div>

                <div className="text-sm text-muted">
                  {selected.candidateEmail}
                  {selected.candidatePhone
                    ? ` · ${selected.candidatePhone}`
                    : ''}
                </div>
              </div>
            </div>

            <div className="grid grid-2 mb-2">
              <div>
                <strong>Job:</strong>{' '}
                {selected.jobTitle}
              </div>

              <div>
                <strong>Department:</strong>{' '}
                {selected.department}
              </div>

              <div>
                <strong>Location:</strong>{' '}
                {selected.candidateLocation ||
                  'Not provided'}
              </div>

              <div>
                <strong>Experience:</strong>{' '}
                {
                  selected.candidateExperience
                }{' '}
                years
              </div>
            </div>

            <h4>AI Match</h4>

            <ScoreBar
              score={Math.round(
                selected.matchScore,
              )}
            />

            <p className="text-sm text-secondary">
              {selected.matchReason}
            </p>

            <h4>Matched Skills</h4>
            <div className="job-tags mb-2">
              {selected.matchedSkills?.length ? (
                selected.matchedSkills.map(
                  (skill) => (
                    <Badge
                      key={skill}
                      variant="success"
                    >
                      {skill}
                    </Badge>
                  ),
                )
              ) : (
                <span className="text-sm text-muted">
                  No required skills matched
                </span>
              )}
            </div>

            <h4>Missing Skills</h4>
            <div className="job-tags mb-2">
              {selected.missingSkills?.length ? (
                selected.missingSkills.map(
                  (skill) => (
                    <Badge
                      key={skill}
                      variant="neutral"
                    >
                      {skill}
                    </Badge>
                  ),
                )
              ) : (
                <span className="text-sm text-muted">
                  No missing required skills
                </span>
              )}
            </div>

            {selected.coverLetter && (
              <>
                <h4>Cover Letter</h4>
                <p>{selected.coverLetter}</p>
              </>
            )}

            {selected.status ===
              'Pending' && (
              <div className="actions mt-3">
                <Button
                  onClick={() =>
                    setAction({
                      application:
                        selected,
                      status:
                        'Shortlisted',
                    })
                  }
                >
                  Shortlist
                </Button>

                <Button
                  variant="danger"
                  onClick={() =>
                    setAction({
                      application:
                        selected,
                      status: 'Rejected',
                    })
                  }
                >
                  Reject
                </Button>
              </div>
            )}
          </div>
        )}
      </Modal>

      <Modal
        open={Boolean(action)}
        onClose={() =>
          setAction(null)
        }
        title="Confirm Decision"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setAction(null)
              }
            >
              Cancel
            </Button>

            <Button
              variant={
                action?.status ===
                'Rejected'
                  ? 'danger'
                  : 'primary'
              }
              loading={updating}
              onClick={doAction}
            >
              {action?.status ===
              'Shortlisted'
                ? 'Shortlist Candidate'
                : 'Reject Candidate'}
            </Button>
          </>
        }
      >
        <p>
          Confirm the decision for{' '}
          <strong>
            {
              action?.application
                ?.candidateName
            }
          </strong>
          ?
        </p>
      </Modal>
    </div>
  );
}
