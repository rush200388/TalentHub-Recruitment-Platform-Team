import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, ROLES } from './context/AuthContext';
import { ProtectedRoute, PublicOnlyRoute, defaultPathFor } from './components/auth/ProtectedRoute';
import DashboardLayout from './components/layout/DashboardLayout';

import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import UnauthorizedPage from './pages/UnauthorizedPage';
import NotFoundPage from './pages/NotFoundPage';
import ProfilePage from './pages/ProfilePage';

import CandidateDashboard from './pages/candidate/CandidateDashboard';
import BrowseJobsPage from './pages/candidate/BrowseJobsPage';
import JobDetailsPage from './pages/candidate/JobDetailsPage';
import MyApplicationsPage from './pages/candidate/MyApplicationsPage';
import RecommendationsPage from './pages/candidate/RecommendationsPage';
import CandidateProfilePage from './pages/candidate/CandidateProfilePage';
import ResumeAnalysisPage from './pages/candidate/ResumeAnalysisPage';

import RecruiterDashboard from './pages/recruiter/RecruiterDashboard';
import ManageJobsPage from './pages/recruiter/ManageJobsPage';
import CandidatesPage from './pages/recruiter/CandidatesPage';
import InterviewSchedulingPage from './pages/recruiter/InterviewSchedulingPage';
import AiFeedbackPage from './pages/shared/AiFeedbackPage';

import ManagerDashboard from './pages/manager/ManagerDashboard';
import ShortlistPage from './pages/manager/ShortlistPage';
import ManagerInterviewsPage from './pages/manager/ManagerInterviewsPage';
import DecisionsPage from './pages/manager/DecisionsPage';

import AdminDashboard from './pages/admin/AdminDashboard';
import UsersPage from './pages/admin/UsersPage';
import DepartmentsPage from './pages/admin/DepartmentsPage';
import OrganizationsPage from './pages/admin/OrganizationsPage';
import AnalyticsPage from './pages/admin/AnalyticsPage';
import AuditLogsPage from './pages/admin/AuditLogsPage';
import MonitoringPage from './pages/admin/MonitoringPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/login" element={<PublicOnlyRoute><LoginPage /></PublicOnlyRoute>} />
          <Route path="/register" element={<PublicOnlyRoute><RegisterPage /></PublicOnlyRoute>} />
          <Route path="/unauthorized" element={<UnauthorizedPage />} />

          {/* Candidate */}
          <Route element={<ProtectedRoute allowedRoles={[ROLES.CANDIDATE]}><DashboardLayout /></ProtectedRoute>}>
            <Route path="/candidate" element={<CandidateDashboard />} />
            <Route path="/candidate/jobs" element={<BrowseJobsPage />} />
            <Route path="/candidate/jobs/:id" element={<JobDetailsPage />} />
            <Route path="/candidate/applications" element={<MyApplicationsPage />} />
            <Route path="/candidate/recommendations" element={<RecommendationsPage />} />
            <Route path="/candidate/profile" element={<CandidateProfilePage />} />
            <Route path="/candidate/resume-analysis" element={<ResumeAnalysisPage />} />
          </Route>

          {/* Recruiter */}
          <Route element={<ProtectedRoute allowedRoles={[ROLES.RECRUITER]}><DashboardLayout /></ProtectedRoute>}>
            <Route path="/recruiter" element={<RecruiterDashboard />} />
            <Route path="/recruiter/jobs" element={<ManageJobsPage />} />
            <Route path="/recruiter/candidates" element={<CandidatesPage />} />
            <Route path="/recruiter/interviews" element={<InterviewSchedulingPage />} />
            <Route path="/recruiter/ai-feedback" element={<AiFeedbackPage />} />
          </Route>

          {/* Hiring Manager */}
          <Route element={<ProtectedRoute allowedRoles={[ROLES.HIRING_MANAGER]}><DashboardLayout /></ProtectedRoute>}>
            <Route path="/manager" element={<ManagerDashboard />} />
            <Route path="/manager/shortlist" element={<ShortlistPage />} />
            <Route path="/manager/interviews" element={<ManagerInterviewsPage />} />
            <Route path="/manager/decisions" element={<DecisionsPage />} />
            <Route path="/manager/ai-feedback" element={<AiFeedbackPage />} />
          </Route>

          {/* Administrator */}
          <Route element={<ProtectedRoute allowedRoles={[ROLES.ADMIN]}><DashboardLayout /></ProtectedRoute>}>
            <Route path="/admin" element={<AdminDashboard />} />
            <Route path="/admin/users" element={<UsersPage />} />
            <Route path="/admin/departments" element={<DepartmentsPage />} />
            <Route path="/admin/organizations" element={<OrganizationsPage />} />
            <Route path="/admin/analytics" element={<AnalyticsPage />} />
            <Route path="/admin/audit" element={<AuditLogsPage />} />
            <Route path="/admin/monitoring" element={<MonitoringPage />} />
          </Route>

          {/* Shared profile */}
          <Route element={<ProtectedRoute><DashboardLayout /></ProtectedRoute>}>
            <Route path="/profile" element={<ProfilePage />} />
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
