import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  decisionService,
  interviewService,
  jobService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import {
  ScoreBar,
} from '../../components/ui/ScoreBar';

function localDateTime(value) {
  return new Intl.DateTimeFormat(
    undefined,
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  ).format(new Date(value));
}

export default function ManagerDashboard() {
  const {
    data: applications,
    loading,
    error,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const {
    data: interviews,
  } = useAsync(
    () => interviewService.list(),
    [],
  );

  const {
    data: decisions,
  } = useAsync(
    () => decisionService.list(),
    [],
  );

  const {
    data: jobs,
  } = useAsync(
    () => jobService.list(),
    [],
  );

  const shortlisted =
    (applications || []).filter(
      (application) =>
        application.status ===
          'Shortlisted' ||
        application.status ===
          'Interview Scheduled',
    );

  const upcoming =
    (interviews || []).filter(
      (interview) =>
        interview.status ===
          'Scheduled' &&
        new Date(
          interview.startTimeUtc,
        ) >= new Date(),
    );

  const completed =
    (interviews || []).filter(
      (interview) =>
        interview.status ===
        'Completed',
    );

  const hired =
    (decisions || []).filter(
      (decision) =>
        decision.decision === 'Hired',
    );

  const stats = [
    {
      label: 'Shortlisted',
      value: shortlisted.length,
      icon: '⭐',
      bg: '#fef3c7',
      color: '#d97706',
    },
    {
      label: 'Upcoming Interviews',
      value: upcoming.length,
      icon: '📅',
      bg: '#e0f2fe',
      color: '#0284c7',
    },
    {
      label: 'Completed Interviews',
      value: completed.length,
      icon: '✅',
      bg: '#dcfce7',
      color: '#16a34a',
    },
    {
      label: 'Hired',
      value: hired.length,
      icon: '🎉',
      bg: '#dbeafe',
      color: '#1d4ed8',
    },
  ];

  return (
    <div>
      <div className="grid grid-4 mb-3">
        {stats.map((stat) => (
          <Card key={stat.label}>
            <div className="stat-card">
              <div
                className="stat-icon"
                style={{
                  background:
                    stat.bg,
                  color:
                    stat.color,
                }}
              >
                {stat.icon}
              </div>

              <div>
                <div className="stat-value">
                  {stat.value}
                </div>

                <div className="stat-label">
                  {stat.label}
                </div>
              </div>
            </div>
          </Card>
        ))}
      </div>

      <Card
        title="Candidates Requiring Review"
        className="mb-3"
      >
        {loading ? (
          <Spinner />
        ) : error ? (
          <Alert variant="error">
            {error}
          </Alert>
        ) : null}

        {!loading &&
          !error &&
          shortlisted.length === 0 && (
            <EmptyState
              title="No candidates waiting"
              message="Shortlisted candidates will appear here."
            />
          )}

        {!loading &&
          !error &&
          shortlisted.length > 0 && (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Candidate</th>
                    <th>Job</th>
                    <th>AI Match</th>
                    <th>Stage</th>
                    <th>Status</th>
                  </tr>
                </thead>

                <tbody>
                  {shortlisted.map(
                    (application) => (
                      <tr
                        key={
                          application.id
                        }
                      >
                        <td
                          style={{
                            fontWeight: 600,
                          }}
                        >
                          {
                            application.candidateName
                          }
                        </td>

                        <td>
                          {
                            application.jobTitle
                          }
                        </td>

                        <td
                          style={{
                            minWidth: 140,
                          }}
                        >
                          <ScoreBar
                            score={Math.round(
                              application.matchScore,
                            )}
                          />
                        </td>

                        <td>
                          {
                            application.stage
                          }
                        </td>

                        <td>
                          <Badge
                            variant="success"
                          >
                            {
                              application.status
                            }
                          </Badge>
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          )}
      </Card>

      <Card title="Upcoming Interviews">
        {upcoming.length === 0 ? (
          <p className="text-muted">
            No upcoming interviews.
          </p>
        ) : (
          <div className="grid grid-2">
            {upcoming.map(
              (interview) => (
                <Card
                  key={interview.id}
                  style={{
                    boxShadow: 'none',
                  }}
                >
                  <div className="flex items-center justify-between">
                    <h4
                      style={{
                        margin: 0,
                      }}
                    >
                      {
                        interview.candidateName
                      }
                    </h4>

                    <Badge variant="info">
                      {interview.type}
                    </Badge>
                  </div>

                  <div className="text-sm text-muted">
                    📅{' '}
                    {localDateTime(
                      interview.startTimeUtc,
                    )}
                  </div>

                  <div className="text-sm text-secondary">
                    {interview.jobTitle}
                  </div>
                </Card>
              ),
            )}
          </div>
        )}
      </Card>

      <div className="mt-3">
        <Card title="Open Jobs">
          <strong>
            {
              (jobs || []).filter(
                (job) =>
                  job.status ===
                  'Open',
              ).length
            }
          </strong>{' '}
          <span className="text-muted">
            published positions
          </span>
        </Card>
      </div>
    </div>
  );
}
