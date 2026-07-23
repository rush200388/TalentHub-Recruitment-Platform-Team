import api from './api';

export const authService = {
  register: async (data) => {
    const response = await api.post('/Auth/register', data);
    return response.data;
  },

  login: async (data) => {
    const response = await api.post('/Auth/login', data);
    return response.data;
  },

  me: async () => {
    const response = await api.get('/Auth/me');
    return response.data;
  },
};
