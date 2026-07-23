import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { defaultPathFor } from '../../components/auth/ProtectedRoute';
import { Input } from '../../components/ui/FormField';
import Button from '../../components/ui/Button';
import { Alert } from '../../components/ui/Alert';
import { getApiErrorMessage } from '../../services/api';
import {
  limits,
  validateName,
} from '../../utils/validation';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirm: '',
  });

  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [loading, setLoading] = useState(false);

  const validate = () => {
    const validationErrors = {};

    const firstNameError =
      validateName(form.firstName, 'First name');
    const lastNameError =
      validateName(form.lastName, 'Last name');

    if (firstNameError) {
      validationErrors.firstName = firstNameError;
    }

    if (lastNameError) {
      validationErrors.lastName = lastNameError;
    }

    if (!form.email) {
      validationErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      validationErrors.email = 'Enter a valid email';
    }

    if (!form.password) {
      validationErrors.password = 'Password is required';
    } else if (form.password.length < 8) {
      validationErrors.password = 'Use at least 8 characters';
    } else if (
      !/[A-Z]/.test(form.password) ||
      !/[a-z]/.test(form.password) ||
      !/[0-9]/.test(form.password)
    ) {
      validationErrors.password =
        'Include uppercase, lowercase, and a number';
    }

    if (form.confirm !== form.password) {
      validationErrors.confirm = 'Passwords do not match';
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
      const authenticatedUser = await register({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password,
      });

      navigate(defaultPathFor(authenticatedUser.role), {
        replace: true,
      });
    } catch (error) {
      setServerError(
        getApiErrorMessage(
          error,
          'Unable to create your account. Please try again.',
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

        <h2>Join TalentHub today</h2>

        <p>
          Create a candidate account and begin your journey with our
          AI-powered recruitment platform.
        </p>

        <ul className="feature-list">
          <li>
            <span className="check">✓</span>
            Free candidate registration
          </li>
          <li>
            <span className="check">✓</span>
            Smart profile matching
          </li>
          <li>
            <span className="check">✓</span>
            Application tracking
          </li>
          <li>
            <span className="check">✓</span>
            Career insights
          </li>
        </ul>
      </aside>

      <main className="auth-main">
        <div className="auth-card card">
          <h1>Create candidate account</h1>
          <p className="subtitle">
            Recruiter and manager accounts are created by an administrator
          </p>

          {serverError && (
            <Alert variant="error">{serverError}</Alert>
          )}

          <form onSubmit={onSubmit} noValidate>
            <div className="form-row">
              <Input
                label="First name"
                required
                autoComplete="given-name"
                placeholder="Jane"
                maxLength={limits.name}
                value={form.firstName}
                onChange={(event) =>
                  setForm({
                    ...form,
                    firstName: event.target.value,
                  })
                }
                error={errors.firstName}
              />

              <Input
                label="Last name"
                required
                autoComplete="family-name"
                placeholder="Doe"
                maxLength={limits.name}
                value={form.lastName}
                onChange={(event) =>
                  setForm({
                    ...form,
                    lastName: event.target.value,
                  })
                }
                error={errors.lastName}
              />
            </div>

            <Input
              label="Email"
              type="email"
              required
              autoComplete="email"
              placeholder="you@example.com"
              maxLength={256}
              value={form.email}
              onChange={(event) =>
                setForm({ ...form, email: event.target.value })
              }
              error={errors.email}
            />

            <div className="form-row">
              <Input
                label="Password"
                type="password"
                required
                autoComplete="new-password"
                placeholder="••••••••"
                maxLength={100}
                value={form.password}
                onChange={(event) =>
                  setForm({
                    ...form,
                    password: event.target.value,
                  })
                }
                error={errors.password}
              />

              <Input
                label="Confirm"
                type="password"
                required
                autoComplete="new-password"
                placeholder="••••••••"
                maxLength={100}
                value={form.confirm}
                onChange={(event) =>
                  setForm({
                    ...form,
                    confirm: event.target.value,
                  })
                }
                error={errors.confirm}
              />
            </div>

            <Button
              type="submit"
              loading={loading}
              className="btn-block"
            >
              Create Account
            </Button>
          </form>

          <div className="auth-footer">
            Already have an account?{' '}
            <Link to="/login">Sign in</Link>
          </div>
        </div>
      </main>
    </div>
  );
}
