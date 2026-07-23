import { useAsync } from '../../hooks/useAsync';
import {
  analyticsService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';

export default function AnalyticsPage() {
  const {
    data,
    loading,
    error,
  } = useAsync(
    () => analyticsService.get(),
    [],
  );

  const analytics = data || {};

  const maxDepartment = Math.max(
    ...(analytics.byDepartment || []).map(
      (department) =>
        department.applications,
    ),
    1,
  );

  const maxTrend = Math.max(
    ...(analytics.monthlyTrend || []).map(
      (month) =>
        month.applications,
    ),
    1,
  );

  const maxFunnel = Math.max(
    ...(analytics.funnel || []).map(
      (stage) => stage.count,
    ),
    1,
  );

  const summary = [
    {
      label: 'Applications',
      value:
        analytics.totalApplications ||
        0,
      icon: '📋',
    },
    {
      label: 'Active Jobs',
      value:
        analytics.activeJobs || 0,
      icon: '💼',
    },
    {
      label: 'Shortlisted',
      value:
        analytics.shortlisted || 0,
      icon: '⭐',
    },
    {
      label: 'Scheduled Interviews',
      value:
        analytics
          .scheduledInterviews || 0,
      icon: '📅',
    },
    {
      label: 'Completed Interviews',
      value:
        analytics
          .completedInterviews || 0,
      icon: '✅',
    },
    {
      label: 'Hired',
      value:
        analytics.hired || 0,
      icon: '🎉',
    },
    {
      label: 'Rejected',
      value:
        analytics.rejected || 0,
      icon: '❌',
    },
    {
      label: 'Avg. Time to Hire',
      value: `${
        analytics
          .averageTimeToHireDays || 0
      } days`,
      icon: '⏱',
    },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            Recruitment Analytics
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Live recruitment insights from
            PostgreSQL
          </p>
        </div>
      </div>

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">
          {error}
        </Alert>
      ) : null}

      {!loading && !error && (
        <>
          <div className="grid grid-4 mb-3">
            {summary.map((stat) => (
              <Card key={stat.label}>
                <div className="stat-card">
                  <div
                    className="stat-icon"
                    style={{
                      background:
                        '#dbeafe',
                      color:
                        '#1d4ed8',
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

          <div className="grid grid-3 mb-3">
            <Card>
              <div className="text-sm text-muted">
                Average AI Match Score
              </div>

              <div
                style={{
                  fontSize: '2rem',
                  fontWeight: 800,
                }}
              >
                {analytics
                  .averageMatchScore ||
                  0}
                %
              </div>
            </Card>

            <Card>
              <div className="text-sm text-muted">
                Average Evaluation Score
              </div>

              <div
                style={{
                  fontSize: '2rem',
                  fontWeight: 800,
                }}
              >
                {analytics
                  .averageEvaluationScore ||
                  0}
                %
              </div>
            </Card>

            <Card>
              <div className="text-sm text-muted">
                Hiring Conversion
              </div>

              <div
                style={{
                  fontSize: '2rem',
                  fontWeight: 800,
                }}
              >
                {analytics.totalApplications
                  ? Math.round(
                      (analytics.hired /
                        analytics.totalApplications) *
                        100,
                    )
                  : 0}
                %
              </div>
            </Card>
          </div>

          <div className="grid grid-2 mb-3">
            <Card title="Recruitment Funnel">
              {(analytics.funnel || [])
                .length === 0 ? (
                <EmptyState title="No funnel data" />
              ) : (
                (analytics.funnel || []).map(
                  (stage) => (
                    <div
                      key={stage.stage}
                      className="mb-2"
                    >
                      <div className="flex justify-between text-sm mb-1">
                        <span>
                          {stage.stage}
                        </span>

                        <strong>
                          {stage.count}
                        </strong>
                      </div>

                      <div className="score-bar">
                        <div
                          className="score-bar-fill high"
                          style={{
                            width: `${
                              (stage.count /
                                maxFunnel) *
                              100
                            }%`,
                          }}
                        />
                      </div>
                    </div>
                  ),
                )
              )}
            </Card>

            <Card title="Applications by Department">
              {(analytics.byDepartment ||
                []).length === 0 ? (
                <EmptyState title="No department data" />
              ) : (
                (
                  analytics.byDepartment ||
                  []
                ).map((department) => (
                  <div
                    key={
                      department.department
                    }
                    className="mb-2"
                  >
                    <div className="flex justify-between text-sm mb-1">
                      <span>
                        {
                          department.department
                        }
                      </span>

                      <span className="text-muted">
                        {
                          department.applications
                        }{' '}
                        apps ·{' '}
                        {department.hires}{' '}
                        hires
                      </span>
                    </div>

                    <div className="score-bar">
                      <div
                        className="score-bar-fill high"
                        style={{
                          width: `${
                            (department.applications /
                              maxDepartment) *
                            100
                          }%`,
                        }}
                      />
                    </div>
                  </div>
                ))
              )}
            </Card>
          </div>

          <Card title="Monthly Applications Trend">
            <div
              className="flex items-end gap-1"
              style={{
                height: 220,
              }}
            >
              {(analytics.monthlyTrend ||
                []).map((month) => (
                <div
                  key={month.month}
                  className="flex-1 flex flex-col items-center gap-1"
                >
                  <div className="text-sm">
                    {month.applications}
                  </div>

                  <div
                    style={{
                      width: '100%',
                      background:
                        'var(--color-primary)',
                      borderRadius:
                        '6px 6px 0 0',
                      height: `${
                        (month.applications /
                          maxTrend) *
                        160
                      }px`,
                      minHeight: 4,
                    }}
                    title={`${month.applications} applications`}
                  />

                  <div className="text-sm text-muted">
                    {month.month}
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </>
      )}
    </div>
  );
}
