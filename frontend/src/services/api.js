import axios from 'axios';

export const TOKEN_STORAGE_KEY = 'rtm_auth_token';
export const USER_STORAGE_KEY = 'rtm_auth_user';

const api = axios.create({
  baseURL:
    import.meta.env.VITE_API_URL ||
    'http://localhost:5117/api',
  timeout: 20000,
});

api.interceptors.request.use((config) => {
  const token =
    localStorage.getItem(TOKEN_STORAGE_KEY);

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export function getApiErrorMessage(
  error,
  fallbackMessage,
) {
  const responseData = error?.response?.data;

  if (
    typeof responseData === 'string' &&
    responseData.trim()
  ) {
    return responseData;
  }

  if (responseData?.message) {
    return responseData.message;
  }

  if (Array.isArray(responseData?.errors)) {
    return responseData.errors.join(' ');
  }

  if (
    responseData?.errors &&
    typeof responseData.errors === 'object'
  ) {
    const validationMessages = Object.values(
      responseData.errors,
    )
      .flat()
      .filter(Boolean);

    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }
  }

  if (error?.code === 'ERR_NETWORK') {
    return 'Cannot connect to the backend API. Make sure the ASP.NET API is running.';
  }

  return fallbackMessage;
}

export default api;
