import api, { getApiErrorMessage } from './api';

async function requestData(
  request,
  fallbackMessage,
) {
  try {
    const response = await request;
    return response.data;
  } catch (error) {
    throw new Error(
      getApiErrorMessage(
        error,
        fallbackMessage,
      ),
    );
  }
}

export const jobService = {
  list: (params = {}) =>
    requestData(
      api.get('/Jobs', { params }),
      'Unable to load jobs.',
    ),

  get: (id) =>
    requestData(
      api.get(`/Jobs/${id}`),
      'Unable to load this job.',
    ),

  create: (data) =>
    requestData(
      api.post('/Jobs', data),
      'Unable to create the job.',
    ),

  update: (id, data) =>
    requestData(
      api.put(`/Jobs/${id}`, data),
      'Unable to update the job.',
    ),

  remove: async (id) => {
    try {
      await api.delete(`/Jobs/${id}`);
      return true;
    } catch (error) {
      throw new Error(
        getApiErrorMessage(
          error,
          'Unable to delete the job.',
        ),
      );
    }
  },
};

export const organizationService = {
  list: () =>
    requestData(
      api.get('/Organizations'),
      'Unable to load organizations.',
    ),

  create: (data) =>
    requestData(
      api.post('/Organizations', data),
      'Unable to create the organization.',
    ),

  update: (id, data) =>
    requestData(
      api.put(`/Organizations/${id}`, data),
      'Unable to update the organization.',
    ),

  remove: async (id) => {
    try {
      await api.delete(
        `/Organizations/${id}`,
      );
      return true;
    } catch (error) {
      throw new Error(
        getApiErrorMessage(
          error,
          'Unable to delete the organization.',
        ),
      );
    }
  },
};

export const departmentService = {
  list: (params = {}) =>
    requestData(
      api.get('/Departments', { params }),
      'Unable to load departments.',
    ),

  create: (data) =>
    requestData(
      api.post('/Departments', data),
      'Unable to create the department.',
    ),

  update: (id, data) =>
    requestData(
      api.put(`/Departments/${id}`, data),
      'Unable to update the department.',
    ),

  remove: async (id) => {
    try {
      await api.delete(
        `/Departments/${id}`,
      );
      return true;
    } catch (error) {
      throw new Error(
        getApiErrorMessage(
          error,
          'Unable to delete the department.',
        ),
      );
    }
  },
};

export const skillService = {
  list: (params = {}) =>
    requestData(
      api.get('/Skills', { params }),
      'Unable to load skills.',
    ),
};

export const candidateService = {
  me: () =>
    requestData(
      api.get('/Candidates/me'),
      'Unable to load your candidate profile.',
    ),

  updateMe: (data) =>
    requestData(
      api.put('/Candidates/me', data),
      'Unable to save your candidate profile.',
    ),

  uploadResume: (file) => {
    const formData = new FormData();
    formData.append('file', file);

    return requestData(
      api.post(
        '/Candidates/me/resume',
        formData,
      ),
      'Unable to upload the resume.',
    );
  },

  recommendations: () =>
    requestData(
      api.get(
        '/Candidates/me/recommendations',
      ),
      'Unable to load job recommendations.',
    ),

  list: async (params = {}) => {
    const candidates =
      await requestData(
        api.get('/Candidates', {
          params,
        }),
        'Unable to load candidates.',
      );

    return candidates.map(
      (candidate) => ({
        ...candidate,
        title:
          candidate.currentJobTitle ||
          'Candidate',
        experience:
          candidate.yearsOfExperience ||
          0,
        score:
          candidate
            .completenessPercentage ||
          0,
      }),
    );
  },

  get: async (id) => {
    const candidate =
      await requestData(
        api.get(`/Candidates/${id}`),
        'Unable to load this candidate.',
      );

    return {
      ...candidate,
      title:
        candidate.currentJobTitle ||
        'Candidate',
      experience:
        candidate.yearsOfExperience ||
        0,
      score:
        candidate
          .completenessPercentage ||
        0,
    };
  },
};

export const resumeAnalysisService = {
  get: () =>
    requestData(
      api.get(
        '/Candidates/me/resume/analysis',
      ),
      'Unable to load resume analysis.',
    ),

  analyze: () =>
    requestData(
      api.post(
        '/Candidates/me/resume/analyze',
      ),
      'Unable to analyze the resume.',
    ),

  applySkills: (skills) =>
    requestData(
      api.post(
        '/Candidates/me/resume/apply-skills',
        { skills },
      ),
      'Unable to apply extracted skills.',
    ),
};

export const applicationService = {
  list: (params = {}) =>
    requestData(
      api.get('/Applications', {
        params,
      }),
      'Unable to load applications.',
    ),

  get: (id) =>
    requestData(
      api.get(`/Applications/${id}`),
      'Unable to load this application.',
    ),

  create: (data) =>
    requestData(
      api.post('/Applications', data),
      'Unable to submit the application.',
    ),

  updateStatus: (
    id,
    status,
    stage,
  ) =>
    requestData(
      api.patch(
        `/Applications/${id}/status`,
        {
          status,
          stage,
        },
      ),
      'Unable to update the application status.',
    ),
};

export const hiringManagerService = {
  list: () =>
    requestData(
      api.get('/HiringManagers'),
      'Unable to load hiring managers.',
    ),
};

export const interviewService = {
  list: (params = {}) =>
    requestData(
      api.get('/Interviews', {
        params,
      }),
      'Unable to load interviews.',
    ),

  get: (id) =>
    requestData(
      api.get(`/Interviews/${id}`),
      'Unable to load the interview.',
    ),

  create: (data) =>
    requestData(
      api.post('/Interviews', data),
      'Unable to schedule the interview.',
    ),

  update: (id, data) =>
    requestData(
      api.put(
        `/Interviews/${id}`,
        data,
      ),
      'Unable to update the interview.',
    ),

  submitFeedback: (id, data) =>
    requestData(
      api.post(
        `/Interviews/${id}/feedback`,
        data,
      ),
      'Unable to submit interview feedback.',
    ),
};

export const evaluationService = {
  list: (params = {}) =>
    requestData(
      api.get('/Evaluations', {
        params,
      }),
      'Unable to load evaluations.',
    ),

  save: (data) =>
    requestData(
      api.post('/Evaluations', data),
      'Unable to save the evaluation.',
    ),
};

export const decisionService = {
  list: () =>
    requestData(
      api.get('/HiringDecisions'),
      'Unable to load hiring decisions.',
    ),

  save: (data) =>
    requestData(
      api.post(
        '/HiringDecisions',
        data,
      ),
      'Unable to save the hiring decision.',
    ),
};

export const notificationService = {
  list: () =>
    requestData(
      api.get('/Notifications'),
      'Unable to load notifications.',
    ),

  unreadCount: () =>
    requestData(
      api.get(
        '/Notifications/unread-count',
      ),
      'Unable to load notification count.',
    ),

  markRead: async (id) => {
    try {
      await api.patch(
        `/Notifications/${id}/read`,
      );
      return true;
    } catch (error) {
      throw new Error(
        getApiErrorMessage(
          error,
          'Unable to mark the notification as read.',
        ),
      );
    }
  },

  markAllRead: async () => {
    try {
      await api.patch(
        '/Notifications/read-all',
      );
      return true;
    } catch (error) {
      throw new Error(
        getApiErrorMessage(
          error,
          'Unable to mark notifications as read.',
        ),
      );
    }
  },
};

export const analyticsService = {
  get: () =>
    requestData(
      api.get('/Analytics'),
      'Unable to load recruitment analytics.',
    ),
};

export const auditService = {
  list: (params = {}) =>
    requestData(
      api.get('/AuditLogs', {
        params,
      }),
      'Unable to load audit logs.',
    ),
};

export const userService = {
  list: () =>
    requestData(
      api.get('/Users'),
      'Unable to load users.',
    ),

  get: (id) =>
    requestData(
      api.get(`/Users/${id}`),
      'Unable to load this user.',
    ),

  create: (data) =>
    requestData(
      api.post('/Users', data),
      'Unable to create the user.',
    ),

  update: (id, data) =>
    requestData(
      api.put(`/Users/${id}`, data),
      'Unable to update the user.',
    ),

  updateStatus: (
    id,
    isActive,
  ) =>
    requestData(
      api.patch(
        `/Users/${id}/status`,
        { isActive },
      ),
      'Unable to update user status.',
    ),

  updateRoles: (id, data) =>
    requestData(
      api.patch(
        `/Users/${id}/roles`,
        data,
      ),
      'Unable to update user roles.',
    ),
};

export const monitoringService = {
  health: () =>
    requestData(
      api.get(
        '/SystemMonitoring/health',
      ),
      'Unable to load system health.',
    ),

  statistics: () =>
    requestData(
      api.get(
        '/SystemMonitoring/statistics',
      ),
      'Unable to load system statistics.',
    ),
};
export const aiFeedbackService = {
  generate: (applicationId) =>
    requestData(
      api.post(
        `/Applications/${applicationId}/ai-feedback`,
      ),
      'Unable to generate AI candidate feedback.',
    ),
};
