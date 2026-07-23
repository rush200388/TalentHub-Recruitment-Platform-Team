import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import { useAuth, ROLES } from '../../context/AuthContext';

const titles = {
  '/candidate': { title: 'Candidate Dashboard', subtitle: 'Track your applications and discover new opportunities' },
  '/candidate/jobs': { title: 'Browse Jobs', subtitle: 'Find your next role' },
  '/candidate/recommendations': { title: 'AI Recommendations', subtitle: 'Roles matched to your profile' },
  '/candidate/applications': { title: 'My Applications', subtitle: 'Track the status of your applications' },
  '/candidate/profile': { title: 'My Profile', subtitle: 'Manage your details and CV' },
  '/recruiter': { title: 'Recruiter Dashboard', subtitle: 'Manage your pipeline and candidates' },
  '/recruiter/jobs': { title: 'Manage Jobs', subtitle: 'Create and track job postings' },
  '/recruiter/candidates': { title: 'Candidates', subtitle: 'Search and evaluate talent' },
  '/recruiter/interviews': { title: 'Interviews', subtitle: 'Schedule and manage interviews' },
  '/manager': { title: 'Hiring Manager Dashboard', subtitle: 'Review shortlisted candidates' },
  '/manager/shortlist': { title: 'Shortlisted Candidates', subtitle: 'Evaluate and advance candidates' },
  '/manager/interviews': { title: 'Interviews', subtitle: 'Provide feedback on interviews' },
  '/manager/decisions': { title: 'Hiring Decisions', subtitle: 'Make final hiring decisions' },
  '/admin': { title: 'Administrator Dashboard', subtitle: 'Platform overview and controls' },
  '/admin/users': { title: 'Users & Roles', subtitle: 'Manage user accounts and permissions' },
  '/admin/departments': { title: 'Departments', subtitle: 'Organize teams and roles' },
  '/admin/organizations': { title: 'Organizations', subtitle: 'Manage tenant organizations' },
  '/admin/analytics': { title: 'Recruitment Analytics', subtitle: 'Insights across the platform' },
  '/admin/audit': { title: 'Audit Logs', subtitle: 'Track all administrative actions' },
  '/profile': { title: 'My Profile', subtitle: 'Manage your account' },
};

export default function DashboardLayout() {
  const { user } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();
  const meta = titles[location.pathname] || { title: 'Dashboard', subtitle: '' };

  return (
    <div className="app-layout">
      <Sidebar role={user?.role} open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="main-area">
        <Header
          title={meta.title}
          subtitle={meta.subtitle}
          onMenuToggle={() => setSidebarOpen((v) => !v)}
        />
        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
