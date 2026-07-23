import { Link } from 'react-router-dom';
import Button from '../components/ui/Button';
import { useAuth } from '../context/AuthContext';
import { defaultPathFor } from '../components/auth/ProtectedRoute';

export default function NotFoundPage() {
  const { user } = useAuth();
  return (
    <div className="loading-overlay" style={{ minHeight: '70vh' }}>
      <div style={{ fontSize: '3rem' }}>🧭</div>
      <h2>Page Not Found</h2>
      <p>The page you&apos;re looking for doesn&apos;t exist.</p>
      <Link to={user ? defaultPathFor(user.role) : '/login'}>
        <Button>Go Home</Button>
      </Link>
    </div>
  );
}
