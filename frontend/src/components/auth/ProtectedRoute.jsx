import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export function ProtectedRoute({ allowedRoles, children }) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <div className="loading-overlay"><span className="spinner spinner-lg" /></div>;
  }
  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/unauthorized" replace />;
  }
  return children;
}

export function PublicOnlyRoute({ children }) {
  const { user, loading } = useAuth();
  if (loading) return null;
  if (user) return <Navigate to={defaultPathFor(user.role)} replace />;
  return children;
}

export function defaultPathFor(role) {
  switch (role) {
    case 'Candidate': return '/candidate';
    case 'Recruiter': return '/recruiter';
    case 'HiringManager': return '/manager';
    case 'Administrator': return '/admin';
    default: return '/login';
  }
}
