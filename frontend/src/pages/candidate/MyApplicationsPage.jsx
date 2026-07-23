import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
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

  if (
    ['Rejected', 'Withdrawn'].includes(status)
  ) {
    return 'error';
  }

  return 'warning';
}

export default function MyApplicationsPage() {
  const {
    data: applications,
    loading,
    error,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  return (
    <Card title="My Applications">
      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">
          {error}
        </Alert>
      ) : null}

      {!loading &&
        !error &&
        (applications || []).length === 0 && (
          <EmptyState
            title="No applications yet"
            message="Apply to a published job to see it here."
          />
        )}

      {!loading &&
        !error &&
        (applications || []).length > 0 && (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Job</th>
                  <th>Organization</th>
                  <th>Applied</th>
                  <th>AI Match</th>
                  <th>Stage</th>
                  <th>Status</th>
                </tr>
              </thead>

              <tbody>
                {(applications || []).map(
                  (application) => (
                    <tr key={application.id}>
                      <td>
                        <strong>
                          {application.jobTitle}
                        </strong>
                        <div className="text-sm text-muted">
                          {application.department}
                        </div>
                      </td>

                      <td>
                        {application.organization}
                      </td>

                      <td>
                        {application.appliedOn}
                      </td>

                      <td style={{ minWidth: 150 }}>
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
                          {application.status}
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
  );
}
