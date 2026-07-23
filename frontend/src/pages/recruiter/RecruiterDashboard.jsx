import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  jobService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Spinner from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';
import Badge from '../../components/ui/Badge';
import { ScoreBar } from '../../components/ui/ScoreBar';

function statusVariant(status) {
  if (status === 'Shortlisted') {
    return 'success';
  }

  if (status === 'Rejected') {
    return 'error';
  }

  return 'warning';
}

export default function RecruiterDashboard() {
  const {
    data: jobs,
    loading: jobsLoading,
    error: jobsError,
  } = useAsync(
    () => jobService.list({ mine: true }),
    [],
  );

  const {
    data: applications,
    loading: applicationsLoading,
    error: applicationsError,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const openJobs = (jobs || []).filter(
    (job) => job.status === 'Open',
  );

  const shortlisted = (
    applications || []
  ).filter(
    (application) =>
      application.status === 'Shortlisted',
  );

  const uniqueCandidates = new Set(
    (applications || []).map(
      (application) =>
        application.candidateId,
    ),
  );

  const stats = [
    {
      label: 'Active Jobs',
      value: openJobs.length,
      icon: '💼',
      bg: '#dbeafe',
      color: '#1d4ed8',
    },
    {
      label: 'Applications',
      value: (applications || []).length,
      icon: '📋',
      bg: '#e0f2fe',
      color: '#0284c7',
    },
    {
      label: 'Shortlisted',
      value: shortlisted.length,
      icon: '⭐',
      bg: '#fef3c7',
      color: '#d97706',
    },
    {
      label: 'Candidates',
      value: uniqueCandidates.size,
      icon: '👥',
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

      <Card title="Your Job Postings">
        {jobsLoading ? (
          <Spinner />
        ) : jobsError ? (
          <Alert variant="error">
            {jobsError}
          </Alert>
        ) : null}

        {!jobsLoading &&
          !jobsError &&
          (jobs || []).length === 0 && (
            <p className="text-muted">
              You have not posted any jobs yet.
            </p>
          )}

        {!jobsLoading &&
          !jobsError &&
          (jobs || []).length > 0 && (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Department</th>
                    <th>Applicants</th>
                    <th>Status</th>
                    <th>Posted</th>
                  </tr>
                </thead>

                <tbody>
                  {(jobs || []).map((job) => {
                    const count = (
                      applications || []
                    ).filter(
                      (application) =>
                        application.jobId ===
                        job.id,
                    ).length;

                    return (
                      <tr key={job.id}>
                        <td>
                          <strong>
                            {job.title}
                          </strong>
                        </td>
                        <td>
                          {job.department}
                        </td>
                        <td>{count}</td>
                        <td>
                          <Badge
                            variant={
                              job.status ===
                              'Open'
                                ? 'success'
                                : 'neutral'
                            }
                          >
                            {job.status}
                          </Badge>
                        </td>
                        <td>{job.posted}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
      </Card>

      <div className="mt-3">
        <Card title="Recent Applications">
          {applicationsLoading ? (
            <Spinner />
          ) : applicationsError ? (
            <Alert variant="error">
              {applicationsError}
            </Alert>
          ) : null}

          {!applicationsLoading &&
            !applicationsError &&
            (applications || []).length ===
              0 && (
              <p className="text-muted">
                No applications yet.
              </p>
            )}

          {!applicationsLoading &&
            !applicationsError &&
            (applications || []).length >
              0 && (
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
                    {(applications || [])
                      .slice(0, 6)
                      .map((application) => (
                        <tr
                          key={
                            application.id
                          }
                        >
                          <td>
                            <strong>
                              {
                                application.candidateName
                              }
                            </strong>
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
      </div>
    </div>
  );
}
