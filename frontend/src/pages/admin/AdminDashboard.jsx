import { useAsync } from '../../hooks/useAsync';
import { analyticsService, userService, jobService } from '../../services/services';
import Card from '../../components/ui/Card';
import Spinner from '../../components/ui/Spinner';
import { Alert } from '../../components/ui/Alert';

export default function AdminDashboard() {
  const { data: analytics, loading: al, error: ae } = useAsync(() => analyticsService.get(), []);
  const { data: users } = useAsync(() => userService.list(), []);
  const { data: jobs } = useAsync(() => jobService.list(), []);

  const a = analytics || {};
  const stats = [
    { label: 'Total Users', value: (users || []).length, icon: '👥', bg: '#dbeafe', color: '#1d4ed8' },
    { label: 'Active Jobs', value: a.activeJobs || (jobs || []).filter((j) => j.status === 'Open').length, icon: '💼', bg: '#e0f2fe', color: '#0284c7' },
    { label: 'Applications', value: a.totalApplications || 0, icon: '📋', bg: '#fef3c7', color: '#d97706' },
    { label: 'Hired', value: a.hired || 0, icon: '✅', bg: '#dcfce7', color: '#16a34a' },
  ];

  const maxApps = Math.max(...(a.byDepartment || []).map((d) => d.applications), 1);

  return (
    <div>
      <div className="grid grid-4 mb-3">
        {stats.map((s) => (
          <Card key={s.label}>
            <div className="stat-card">
              <div className="stat-icon" style={{ background: s.bg, color: s.color }}>{s.icon}</div>
              <div><div className="stat-value">{s.value}</div><div className="stat-label">{s.label}</div></div>
            </div>
          </Card>
        ))}
      </div>

      {al ? <Spinner /> : ae ? <Alert variant="error">{ae}</Alert> : null}
      {!al && !ae && (
        <div className="grid grid-2">
          <Card title="Applications by Department">
            {(a.byDepartment || []).map((d) => (
              <div key={d.department} className="mb-2">
                <div className="flex justify-between text-sm mb-1">
                  <span>{d.department}</span><span className="text-muted">{d.applications} apps · {d.hires} hires</span>
                </div>
                <div className="score-bar"><div className="score-bar-fill high" style={{ width: `${(d.applications / maxApps) * 100}%` }} /></div>
              </div>
            ))}
          </Card>
          <Card title="Monthly Trend">
            <div className="table-wrap"><table className="table">
              <thead><tr><th>Month</th><th>Applications</th><th>Hires</th></tr></thead>
              <tbody>
                {(a.monthlyTrend || []).map((m) => (
                  <tr key={m.month}><td style={{ fontWeight: 600 }}>{m.month}</td><td>{m.applications}</td><td>{m.hires}</td></tr>
                ))}
              </tbody>
            </table></div>
          </Card>
        </div>
      )}
    </div>
  );
}
