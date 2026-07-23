import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { defaultPathFor } from '../components/auth/ProtectedRoute';
import Button from '../components/ui/Button';

export default function UnauthorizedPage() {
  const { user } = useAuth();
  return (
    <div className="loading-overlay" style={{ minHeight: '70vh' }}>
      <div style={{ fontSize: '3rem' }}>🔒</div>
      <h2>Access Denied</h2>
      <p>You don&apos;t have permission to view this page.</p>
      <Link to={user ? defaultPathFor(user.role) : '/login'}>
        <Button>Back to Dashboard</Button>
      </Link>
    </div>
  );
}
