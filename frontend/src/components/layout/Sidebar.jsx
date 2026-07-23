import { NavLink } from 'react-router-dom';
import { ROLES } from '../../context/AuthContext';

const navByRole = {
  [ROLES.CANDIDATE]: [
    { section: 'Main', items: [
      { to: '/candidate', label: 'Dashboard', icon: '🏠', end: true },
      { to: '/candidate/jobs', label: 'Browse Jobs', icon: '🔍' },
      { to: '/candidate/recommendations', label: 'AI Recommendations', icon: '✨' },
      { to: '/candidate/resume-analysis', label: 'AI Resume Analysis', icon: '🤖' },
      { to: '/candidate/applications', label: 'My Applications', icon: '📋' },
      { to: '/candidate/profile', label: 'My Profile', icon: '👤' },
    ]},
  ],
  [ROLES.RECRUITER]: [
    { section: 'Main', items: [
      { to: '/recruiter', label: 'Dashboard', icon: '📊', end: true },
      { to: '/recruiter/jobs', label: 'Manage Jobs', icon: '💼' },
      { to: '/recruiter/candidates', label: 'Candidates', icon: '👥' },
      { to: '/recruiter/interviews', label: 'Interviews', icon: '📅' },
      { to: '/recruiter/ai-feedback', label: 'AI Feedback', icon: '🧠' },
    ]},
  ],
  [ROLES.HIRING_MANAGER]: [
    { section: 'Main', items: [
      { to: '/manager', label: 'Dashboard', icon: '📊', end: true },
      { to: '/manager/shortlist', label: 'Shortlisted', icon: '⭐' },
      { to: '/manager/interviews', label: 'Interviews', icon: '📅' },
      { to: '/manager/decisions', label: 'Hiring Decisions', icon: '✅' },
      { to: '/manager/ai-feedback', label: 'AI Feedback', icon: '🧠' },
    ]},
  ],
  [ROLES.ADMIN]: [
    { section: 'Main', items: [
      { to: '/admin', label: 'Dashboard', icon: '📊', end: true },
      { to: '/admin/users', label: 'Users & Roles', icon: '👥' },
      { to: '/admin/departments', label: 'Departments', icon: '🏢' },
      { to: '/admin/organizations', label: 'Organizations', icon: '🏛️' },
      { to: '/admin/analytics', label: 'Analytics', icon: '📈' },
      { to: '/admin/audit', label: 'Audit Logs', icon: '📜' },
      { to: '/admin/monitoring', label: 'System Monitoring', icon: '🖥️' },
    ]},
  ],
};

export default function Sidebar({ role, open, onClose }) {
  const groups = navByRole[role] || [];
  return (
    <>
      <aside className={`sidebar ${open ? 'open' : ''}`}>
        <div className="sidebar-brand">
          <span className="logo-dot">R</span>
          <span>TalentHub</span>
        </div>
        <nav className="sidebar-nav">
          {groups.map((group) => (
            <div key={group.section}>
              <div className="sidebar-section">{group.section}</div>
              {group.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
                  onClick={onClose}
                >
                  <span className="icon">{item.icon}</span>
                  <span>{item.label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="text-sm" style={{ color: 'rgba(255,255,255,0.6)' }}>v1.0 · PostgreSQL API</div>
        </div>
      </aside>
      {open && <div className="sidebar-overlay show" onClick={onClose} />}
    </>
  );
}
