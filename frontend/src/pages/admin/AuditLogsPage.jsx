import { useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  auditService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import {
  Input,
  Select,
} from '../../components/ui/FormField';

const actionVariant = (action) => {
  const value =
    action.toUpperCase();

  if (
    value.includes('CREATE') ||
    value.includes('SCHEDULE') ||
    value.includes('HIRE')
  ) {
    return 'success';
  }

  if (
    value.includes('DELETE') ||
    value.includes('REJECT') ||
    value.includes('CANCEL')
  ) {
    return 'error';
  }

  if (
    value.includes('UPDATE') ||
    value.includes('SHORTLIST') ||
    value.includes('EVALUATE')
  ) {
    return 'warning';
  }

  return 'info';
};

export default function AuditLogsPage() {
  const {
    data: logs,
    loading,
    error,
  } = useAsync(
    () => auditService.list(),
    [],
  );

  const [search, setSearch] =
    useState('');
  const [
    actionFilter,
    setActionFilter,
  ] = useState('');

  const actions = [
    ...new Set(
      (logs || []).map(
        (log) => log.action,
      ),
    ),
  ];

  const filtered =
    (logs || []).filter((log) => {
      const term =
        search.toLowerCase();

      const matchesSearch =
        !search ||
        (log.user || '')
          .toLowerCase()
          .includes(term) ||
        (log.target || '')
          .toLowerCase()
          .includes(term) ||
        (log.detail || '')
          .toLowerCase()
          .includes(term);

      const matchesAction =
        !actionFilter ||
        log.action === actionFilter;

      return (
        matchesSearch &&
        matchesAction
      );
    });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            Audit Logs
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Real security and workflow
            activity records
          </p>
        </div>
      </div>

      <Card className="mb-3">
        <div className="search-bar">
          <Input
            placeholder="Search user, target, or detail..."
            value={search}
            onChange={(event) =>
              setSearch(
                event.target.value,
              )
            }
            className="flex-1"
          />

          <Select
            value={actionFilter}
            onChange={(event) =>
              setActionFilter(
                event.target.value,
              )
            }
            style={{
              minWidth: 180,
            }}
          >
            <option value="">
              All actions
            </option>

            {actions.map((action) => (
              <option
                key={action}
                value={action}
              >
                {action}
              </option>
            ))}
          </Select>
        </div>
      </Card>

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
          filtered.length === 0 && (
            <EmptyState title="No logs found" />
          )}

        {!loading &&
          !error &&
          filtered.length > 0 && (
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Timestamp</th>
                    <th>User</th>
                    <th>Action</th>
                    <th>Target</th>
                    <th>Detail</th>
                  </tr>
                </thead>

                <tbody>
                  {filtered.map(
                    (log) => (
                      <tr key={log.id}>
                        <td
                          className="text-sm text-muted"
                          style={{
                            whiteSpace:
                              'nowrap',
                          }}
                        >
                          {log.timestamp}
                        </td>

                        <td
                          style={{
                            fontWeight: 600,
                          }}
                        >
                          {log.user}
                        </td>

                        <td>
                          <Badge
                            variant={actionVariant(
                              log.action,
                            )}
                          >
                            {log.action}
                          </Badge>
                        </td>

                        <td>
                          {log.target}
                        </td>

                        <td className="text-sm text-secondary">
                          {log.detail}
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          )}
      </Card>
    </div>
  );
}
