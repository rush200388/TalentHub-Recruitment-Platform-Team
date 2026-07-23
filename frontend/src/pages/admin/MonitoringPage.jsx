import { useAsync } from '../../hooks/useAsync';
import {
  monitoringService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';

function formatBytes(bytes) {
  if (!bytes) return '0 B';

  const units = [
    'B',
    'KB',
    'MB',
    'GB',
  ];

  const index = Math.min(
    Math.floor(
      Math.log(bytes) /
        Math.log(1024),
    ),
    units.length - 1,
  );

  return `${(
    bytes /
    1024 ** index
  ).toFixed(index === 0 ? 0 : 1)} ${
    units[index]
  }`;
}

function formatUptime(uptime) {
  if (!uptime) return 'Unknown';

  if (typeof uptime === 'string') {
    return uptime;
  }

  const days =
    Number(uptime.days) || 0;
  const hours =
    Number(uptime.hours) || 0;
  const minutes =
    Number(uptime.minutes) || 0;

  return `${days}d ${hours}h ${minutes}m`;
}

export default function MonitoringPage() {
  const {
    data: health,
    loading: healthLoading,
    error: healthError,
    reload: reloadHealth,
  } = useAsync(
    () => monitoringService.health(),
    [],
  );

  const {
    data: statistics,
    loading: statisticsLoading,
    error: statisticsError,
    reload: reloadStatistics,
  } = useAsync(
    () =>
      monitoringService.statistics(),
    [],
  );

  const refresh = async () => {
    await Promise.all([
      reloadHealth(),
      reloadStatistics(),
    ]);
  };

  const stats = [
    {
      label: 'Active Users',
      value:
        statistics?.activeUsers ||
        0,
      icon: '👥',
    },
    {
      label: 'Locked Users',
      value:
        statistics?.lockedUsers ||
        0,
      icon: '🔒',
    },
    {
      label: 'Failed Logins',
      value:
        statistics
          ?.failedLoginAttempts ||
        0,
      icon: '⚠️',
    },
    {
      label: 'Stored Resumes',
      value:
        statistics?.storedResumes ||
        0,
      icon: '📄',
    },
    {
      label: 'Analyzed Resumes',
      value:
        statistics
          ?.analyzedResumes || 0,
      icon: '🤖',
    },
    {
      label: 'Applications',
      value:
        statistics?.applications ||
        0,
      icon: '📋',
    },
    {
      label: 'Interviews',
      value:
        statistics?.interviews ||
        0,
      icon: '📅',
    },
    {
      label: 'Audit Records',
      value:
        statistics?.auditLogCount ||
        0,
      icon: '📜',
    },
  ];

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            System Monitoring
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            API, database, security, and
            storage status
          </p>
        </div>

        <button
          className="btn btn-secondary"
          onClick={refresh}
        >
          Refresh
        </button>
      </div>

      {healthError && (
        <Alert variant="error">
          {healthError}
        </Alert>
      )}

      {statisticsError && (
        <Alert variant="error">
          {statisticsError}
        </Alert>
      )}

      {healthLoading ||
      statisticsLoading ? (
        <Spinner />
      ) : (
        <>
          <div className="grid grid-3 mb-3">
            <Card>
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm text-muted">
                    API Health
                  </div>

                  <div
                    style={{
                      fontSize: '1.5rem',
                      fontWeight: 800,
                    }}
                  >
                    {health?.status ||
                      'Unknown'}
                  </div>
                </div>

                <Badge
                  variant={
                    health?.status ===
                    'Healthy'
                      ? 'success'
                      : 'error'
                  }
                >
                  {health?.database ||
                    'Unknown'}
                </Badge>
              </div>
            </Card>

            <Card>
              <div className="text-sm text-muted">
                Environment
              </div>

              <div
                style={{
                  fontSize: '1.25rem',
                  fontWeight: 700,
                }}
              >
                {health?.environment ||
                  'Unknown'}
              </div>

              <div className="text-sm text-secondary">
                .NET{' '}
                {health?.framework ||
                  'Unknown'}
              </div>
            </Card>

            <Card>
              <div className="text-sm text-muted">
                Uptime
              </div>

              <div
                style={{
                  fontSize: '1.25rem',
                  fontWeight: 700,
                }}
              >
                {formatUptime(
                  health?.uptime,
                )}
              </div>

              <div className="text-sm text-secondary">
                Version{' '}
                {health?.applicationVersion ||
                  '1.0.0'}
              </div>
            </Card>
          </div>

          <div className="grid grid-4 mb-3">
            {stats.map((stat) => (
              <Card key={stat.label}>
                <div className="stat-card">
                  <div className="stat-icon">
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

          <div className="grid grid-2 mb-3">
            <Card title="Database Records">
              <div className="table-wrap">
                <table className="table">
                  <tbody>
                    <tr>
                      <td>Users</td>
                      <td>
                        <strong>
                          {
                            statistics
                              ?.totalUsers
                          }
                        </strong>
                      </td>
                    </tr>

                    <tr>
                      <td>Organizations</td>
                      <td>
                        <strong>
                          {
                            statistics
                              ?.organizations
                          }
                        </strong>
                      </td>
                    </tr>

                    <tr>
                      <td>Departments</td>
                      <td>
                        <strong>
                          {
                            statistics
                              ?.departments
                          }
                        </strong>
                      </td>
                    </tr>

                    <tr>
                      <td>Jobs</td>
                      <td>
                        <strong>
                          {
                            statistics
                              ?.jobs
                          }
                        </strong>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </Card>

            <Card title="Resume Storage">
              <div className="mb-2">
                <div className="text-sm text-muted">
                  Storage Provider
                </div>

                <strong>
                  {statistics
                    ?.resumeStorageProvider ||
                    'Unknown'}
                </strong>
              </div>

              <div className="mb-2">
                <div className="text-sm text-muted">
                  Total Size
                </div>

                <strong>
                  {formatBytes(
                    statistics
                      ?.storedResumeBytes,
                  )}
                </strong>
              </div>

              <div>
                <div className="text-sm text-muted">
                  Analysis Coverage
                </div>

                <strong>
                  {statistics?.storedResumes
                    ? Math.round(
                        (statistics.analyzedResumes /
                          statistics.storedResumes) *
                          100,
                      )
                    : 0}
                  %
                </strong>
              </div>
            </Card>
          </div>

          <Card title="Recent System Activity">
            {(statistics
              ?.recentActivity || [])
              .length === 0 ? (
              <EmptyState title="No activity recorded" />
            ) : (
              <div className="table-wrap">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Time</th>
                      <th>User</th>
                      <th>Action</th>
                      <th>Entity</th>
                      <th>Details</th>
                    </tr>
                  </thead>

                  <tbody>
                    {statistics.recentActivity.map(
                      (activity) => (
                        <tr
                          key={activity.id}
                        >
                          <td className="text-sm text-muted">
                            {new Date(
                              activity.timestampUtc,
                            ).toLocaleString()}
                          </td>

                          <td>
                            {activity.user}
                          </td>

                          <td>
                            <Badge variant="info">
                              {
                                activity.action
                              }
                            </Badge>
                          </td>

                          <td>
                            {
                              activity.entity
                            }
                          </td>

                          <td className="text-sm text-secondary">
                            {
                              activity.details
                            }
                          </td>
                        </tr>
                      ),
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </>
      )}
    </div>
  );
}
