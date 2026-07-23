import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { authService } from '../services/authService';
import {
  TOKEN_STORAGE_KEY,
  USER_STORAGE_KEY,
} from '../services/api';

const AuthContext = createContext(null);

export const ROLES = {
  CANDIDATE: 'Candidate',
  RECRUITER: 'Recruiter',
  HIRING_MANAGER: 'HiringManager',
  ADMIN: 'Administrator',
};

export const ROLE_OPTIONS = [
  { value: ROLES.CANDIDATE, label: 'Candidate', icon: '👤' },
  { value: ROLES.RECRUITER, label: 'Recruiter', icon: '🎯' },
  { value: ROLES.HIRING_MANAGER, label: 'Hiring Manager', icon: '📊' },
  { value: ROLES.ADMIN, label: 'Administrator', icon: '⚙️' },
];

function normalizeUser(apiUser) {
  if (!apiUser) return null;

  const roles = Array.isArray(apiUser.roles)
    ? apiUser.roles
    : apiUser.role
      ? [apiUser.role]
      : [];

  return {
    id: apiUser.id,
    firstName: apiUser.firstName || '',
    lastName: apiUser.lastName || '',
    name:
      `${apiUser.firstName || ''} ${apiUser.lastName || ''}`.trim() ||
      apiUser.name ||
      apiUser.email,
    email: apiUser.email,
    roles,
    role: roles[0] || apiUser.role || null,
    status: 'Active',
  };
}

function readStoredUser() {
  try {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(readStoredUser);
  const [loading, setLoading] = useState(true);

  const clearSession = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
  }, []);

  const saveSession = useCallback((authResponse) => {
    const normalizedUser = normalizeUser(authResponse.user);

    localStorage.setItem(TOKEN_STORAGE_KEY, authResponse.token);
    localStorage.setItem(
      USER_STORAGE_KEY,
      JSON.stringify(normalizedUser),
    );

    setUser(normalizedUser);
    return normalizedUser;
  }, []);

  useEffect(() => {
    let active = true;

    async function restoreSession() {
      const token = localStorage.getItem(TOKEN_STORAGE_KEY);

      if (!token) {
        if (active) {
          setUser(null);
          setLoading(false);
        }
        return;
      }

      try {
        const currentUser = await authService.me();
        const normalizedUser = normalizeUser(currentUser);

        if (active) {
          localStorage.setItem(
            USER_STORAGE_KEY,
            JSON.stringify(normalizedUser),
          );
          setUser(normalizedUser);
        }
      } catch {
        if (active) clearSession();
      } finally {
        if (active) setLoading(false);
      }
    }

    restoreSession();

    return () => {
      active = false;
    };
  }, [clearSession]);

  const login = useCallback(
    async ({ email, password }) => {
      const response = await authService.login({ email, password });
      return saveSession(response);
    },
    [saveSession],
  );

  const register = useCallback(
    async ({ firstName, lastName, email, password }) => {
      const response = await authService.register({
        firstName,
        lastName,
        email,
        password,
      });

      return saveSession(response);
    },
    [saveSession],
  );

  const logout = useCallback(() => {
    clearSession();
  }, [clearSession]);

  const value = useMemo(
    () => ({
      user,
      loading,
      login,
      register,
      logout,
    }),
    [user, loading, login, register, logout],
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }

  return context;
}
