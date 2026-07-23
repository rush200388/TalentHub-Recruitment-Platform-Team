import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  candidateService,
  jobService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import Badge from '../../components/ui/Badge';
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

export default function CandidateDashboard() {
  const {
    data: applications,
    loading: applicationsLoading,
    error: applicationsError,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const {
    data: recommendations,
    loading: recommendationsLoading,
  } = useAsync(
    () => candidateService.recommendations(),
    [],
  );

  const {
    data: jobs,
  } = useAsync(
    () => jobService.list(),
    [],
  );

  const stats = [
    {
      label: 'Applications',
      value: (applications || []).length,
      icon: '📋',
      bg: '#dbeafe',
      color: '#1d4ed8',
    },
    {
      label: 'Shortlisted',
      value: (applications || []).filter(
        (application) =>
          application.status ===
          'Shortlisted',
      ).length,
      icon: '⭐',
      bg: '#fef3c7',
      color: '#d97706',
    },
    {
      label: 'In Progress',
      value: (applications || []).filter(
        (application) =>
          ![
            'Rejected',
            'Hired',
            'Withdrawn',
          ].includes(application.status),
      ).length,
      icon: '⏳',
      bg: '#e0f2fe',
      color: '#0284c7',
    },
    {
      label: 'Open Jobs',
      value: (jobs || []).filter(
        (job) => job.status === 'Open',
      ).length,
      icon: '💼',
      bg: '#dcfce7',
      color: '#16a34a',
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
                  background: stat.bg,
                  color: stat.color,
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
        title="Recent Applications"
        action={
          <Badge variant="neutral">
            {(applications || []).length}{' '}
            total
          </Badge>
        }
      >
        {applicationsLoading ? (
          <Spinner />
        ) : applicationsError ? (
          <Alert variant="error">
            {applicationsError}
          </Alert>
        ) : null}

        {!applicationsLoading &&
          !applicationsError &&
          (applications || []).length === 0 && (
            <EmptyState
              title="No applications yet"
              message="Browse jobs and apply to get started."
            />
          )}

        {!applicationsLoading &&
          !applicationsError &&
          (applications || []).length > 0 && (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Job</th>
                    <th>Applied</th>
                    <th>AI Match</th>
                    <th>Stage</th>
                    <th>Status</th>
                  </tr>
                </thead>

                <tbody>
                  {(applications || [])
                    .slice(0, 5)
                    .map((application) => (
                      <tr
                        key={application.id}
                      >
                        <td>
                          <strong>
                            {
                              application.jobTitle
                            }
                          </strong>
                        </td>

                        <td>
                          {
                            application.appliedOn
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
                          {application.stage}
                        </td>

                        <td>
                          <Badge
                            variant={statusVariant(
                              application.status,
                            )}
                          >
                            {
                              application.status
                            }
                          </Badge>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>
            </div>
          )}
      </Card>

      <div className="mt-3">
        <Card title="Top Recommendations">
          {recommendationsLoading ? (
            <Spinner />
          ) : (
            <div className="grid grid-3">
              {(recommendations || [])
                .slice(0, 3)
                .map((recommendation) => (
                  <div
                    key={
                      recommendation.jobId
                    }
                    className="card"
                    style={{
                      padding: '1rem',
                      boxShadow: 'none',
                    }}
                  >
                    <h4
                      style={{
                        marginBottom:
                          '0.3rem',
                      }}
                    >
                      {recommendation.title}
                    </h4>

                    <div className="text-sm text-muted mb-1">
                      {
                        recommendation.organization
                      }{' '}
                      ·{' '}
                      {
                        recommendation.location
                      }
                    </div>

                    <Badge
                      variant={
                        recommendation.matchScore >=
                        80
                          ? 'success'
                          : 'warning'
                      }
                    >
                      {Math.round(
                        recommendation.matchScore,
                      )}
                      % match
                    </Badge>
                  </div>
                ))}
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}
