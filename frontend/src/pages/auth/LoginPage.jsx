import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { defaultPathFor } from '../../components/auth/ProtectedRoute';
import { Input } from '../../components/ui/FormField';
import Button from '../../components/ui/Button';
import { Alert } from '../../components/ui/Alert';
import { getApiErrorMessage } from '../../services/api';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [form, setForm] = useState({
    email: '',
    password: '',
  });

  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [loading, setLoading] = useState(false);

  const validate = () => {
    const validationErrors = {};

    if (!form.email) {
      validationErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      validationErrors.email = 'Enter a valid email';
    }

    if (!form.password) {
      validationErrors.password = 'Password is required';
    }

    setErrors(validationErrors);
    return Object.keys(validationErrors).length === 0;
  };

  const onSubmit = async (event) => {
    event.preventDefault();
    setServerError('');

    if (!validate()) return;

    setLoading(true);

    try {
      const authenticatedUser = await login({
        email: form.email.trim(),
        password: form.password,
      });

      const requestedPath = location.state?.from?.pathname;
      const destination =
        requestedPath || defaultPathFor(authenticatedUser.role);

      navigate(destination, { replace: true });
    } catch (error) {
      setServerError(
        getApiErrorMessage(
          error,
          'Unable to sign in. Check your email and password.',
        ),
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-wrapper">
      <aside className="auth-aside">
        <div
          className="sidebar-brand"
          style={{
            border: 'none',
            padding: '0',
            marginBottom: '2rem',
          }}
        >
          <span className="logo-dot">R</span>
          <span>TalentHub</span>
        </div>

        <h2>AI-Powered Recruitment &amp; Talent Management</h2>

        <p>
          Connect candidates, recruiters, and hiring managers with
          intelligent matching and a seamless hiring workflow.
        </p>

        <ul className="feature-list">
          <li>
            <span className="check">✓</span>
            AI-powered job recommendations
          </li>
          <li>
            <span className="check">✓</span>
            Candidate ranking &amp; scoring
          </li>
          <li>
            <span className="check">✓</span>
            Interview scheduling and feedback
          </li>
          <li>
            <span className="check">✓</span>
            Secure role-based dashboards
          </li>
        </ul>
      </aside>

      <main className="auth-main">
        <div className="auth-card card">
          <h1>Welcome back</h1>
          <p className="subtitle">Sign in to your TalentHub account</p>

          {serverError && (
            <Alert variant="error">{serverError}</Alert>
          )}

          <form onSubmit={onSubmit} noValidate>
            <Input
              label="Email"
              type="email"
              required
              autoComplete="email"
              placeholder="you@example.com"
              value={form.email}
              onChange={(event) =>
                setForm({ ...form, email: event.target.value })
              }
              error={errors.email}
            />

            <Input
              label="Password"
              type="password"
              required
              autoComplete="current-password"
              placeholder="••••••••"
              value={form.password}
              onChange={(event) =>
                setForm({ ...form, password: event.target.value })
              }
              error={errors.password}
            />

            <Button
              type="submit"
              loading={loading}
              className="btn-block"
            >
              Sign In
            </Button>
          </form>

          <div className="auth-footer">
            Don&apos;t have an account?{' '}
            <Link to="/register">Create one</Link>
          </div>
        </div>
      </main>
    </div>
  );
}
