// Centralized mock data used until the C# ASP.NET Web API is connected.

export const mockUsers = [
  { id: 'u1', name: 'Alex Morgan', email: 'alex@example.com', role: 'Candidate', status: 'Active', department: '-', phone: '+1 555-0101', location: 'Boston, MA', joined: '2024-02-12' },
  { id: 'u2', name: 'Priya Sharma', email: 'priya@example.com', role: 'Candidate', status: 'Active', department: '-', phone: '+1 555-0102', location: 'Austin, TX', joined: '2024-03-04' },
  { id: 'u3', name: 'Diego Ramos', email: 'diego@example.com', role: 'Candidate', status: 'Active', department: '-', phone: '+1 555-0103', location: 'Remote', joined: '2024-04-21' },
  { id: 'u4', name: 'Sara Lee', email: 'sara@example.com', role: 'Recruiter', status: 'Active', department: 'Talent Acquisition', phone: '+1 555-0201', location: 'HQ', joined: '2023-11-10' },
  { id: 'u5', name: 'John Carter', email: 'john@example.com', role: 'Recruiter', status: 'Active', department: 'Engineering', phone: '+1 555-0202', location: 'HQ', joined: '2023-09-01' },
  { id: 'u6', name: 'Emily Chen', email: 'emily@example.com', role: 'HiringManager', status: 'Active', department: 'Engineering', phone: '+1 555-0301', location: 'HQ', joined: '2022-06-15' },
  { id: 'u7', name: 'Marcus Webb', email: 'marcus@example.com', role: 'HiringManager', status: 'Active', department: 'Product', phone: '+1 555-0302', location: 'HQ', joined: '2022-01-20' },
  { id: 'u8', name: 'Admin Root', email: 'admin@example.com', role: 'Administrator', status: 'Active', department: 'IT', phone: '+1 555-0900', location: 'HQ', joined: '2021-01-01' },
  { id: 'u9', name: 'Nina Patel', email: 'nina@example.com', role: 'Candidate', status: 'Suspended', department: '-', phone: '+1 555-0104', location: 'Chicago, IL', joined: '2024-05-18' },
];

export const mockDepartments = [
  { id: 'd1', name: 'Engineering', head: 'Emily Chen', headcount: 42, openRoles: 3 },
  { id: 'd2', name: 'Product', head: 'Marcus Webb', headcount: 18, openRoles: 1 },
  { id: 'd3', name: 'Talent Acquisition', head: 'Sara Lee', headcount: 8, openRoles: 0 },
  { id: 'd4', name: 'Marketing', head: '—', headcount: 12, openRoles: 2 },
  { id: 'd5', name: 'Finance', head: '—', headcount: 7, openRoles: 0 },
];

export const mockOrganizations = [
  { id: 'o1', name: 'Acme Corp', plan: 'Enterprise', seats: 250, status: 'Active' },
  { id: 'o2', name: 'Globex LLC', plan: 'Pro', seats: 80, status: 'Active' },
  { id: 'o3', name: 'Initech', plan: 'Starter', seats: 15, status: 'Suspended' },
];

export const mockJobs = [
  { id: 'j1', title: 'Senior Frontend Engineer', department: 'Engineering', location: 'Boston, MA', type: 'Full-time', remote: 'Hybrid', salary: '$130k - $160k', experience: '5+ years', posted: '2024-05-01', status: 'Open', description: 'Build delightful web experiences with React and TypeScript. You will own features end-to-end and collaborate with design and backend teams.', skills: ['React', 'TypeScript', 'CSS', 'REST APIs'], recruiter: 'Sara Lee' },
  { id: 'j2', title: 'Backend Engineer (.NET)', department: 'Engineering', location: 'Remote', type: 'Full-time', remote: 'Remote', salary: '$120k - $150k', experience: '4+ years', posted: '2024-05-03', status: 'Open', description: 'Design and build scalable C# ASP.NET Core APIs and microservices powering our talent platform.', skills: ['C#', 'ASP.NET Core', 'SQL Server', 'Azure'], recruiter: 'John Carter' },
  { id: 'j3', title: 'Product Manager', department: 'Product', location: 'Austin, TX', type: 'Full-time', remote: 'On-site', salary: '$140k - $175k', experience: '6+ years', posted: '2024-04-28', status: 'Open', description: 'Drive product strategy and execution for our recruitment suite. Own roadmaps and partner with engineering.', skills: ['Roadmapping', 'Analytics', 'User Research'], recruiter: 'Sara Lee' },
  { id: 'j4', title: 'UX Designer', department: 'Product', location: 'Remote', type: 'Contract', remote: 'Remote', salary: '$90k - $110k', experience: '3+ years', posted: '2024-05-10', status: 'Open', description: 'Create intuitive, accessible interfaces for candidates and recruiters. Conduct research and prototype rapidly.', skills: ['Figma', 'Prototyping', 'Accessibility'], recruiter: 'John Carter' },
  { id: 'j5', title: 'Data Analyst', department: 'Engineering', location: 'Boston, MA', type: 'Full-time', remote: 'Hybrid', salary: '$95k - $120k', experience: '2+ years', posted: '2024-05-12', status: 'Open', description: 'Turn recruitment data into actionable insights. Build dashboards and reports for hiring teams.', skills: ['SQL', 'Power BI', 'Python'], recruiter: 'Sara Lee' },
  { id: 'j6', title: 'DevOps Engineer', department: 'Engineering', location: 'Remote', type: 'Full-time', remote: 'Remote', salary: '$125k - $155k', experience: '4+ years', posted: '2024-04-20', status: 'Closed', description: 'Automate deployments, manage cloud infrastructure, and improve reliability across our platform.', skills: ['Kubernetes', 'Terraform', 'AWS'], recruiter: 'John Carter' },
  { id: 'j7', title: 'Marketing Specialist', department: 'Marketing', location: 'Austin, TX', type: 'Full-time', remote: 'On-site', salary: '$70k - $90k', experience: '2+ years', posted: '2024-05-08', status: 'Open', description: 'Plan and execute campaigns to attract top talent and promote our employer brand.', skills: ['SEO', 'Content', 'Campaigns'], recruiter: 'Sara Lee' },
];

export const mockApplications = [
  { id: 'a1', jobId: 'j1', candidateId: 'u1', candidateName: 'Alex Morgan', appliedOn: '2024-05-05', status: 'Shortlisted', score: 88, stage: 'Technical Interview' },
  { id: 'a2', jobId: 'j1', candidateId: 'u2', candidateName: 'Priya Sharma', appliedOn: '2024-05-06', status: 'Pending', score: 76, stage: 'Screening' },
  { id: 'a3', jobId: 'j1', candidateId: 'u3', candidateName: 'Diego Ramos', appliedOn: '2024-05-07', status: 'Rejected', score: 54, stage: 'Screening' },
  { id: 'a4', jobId: 'j2', candidateId: 'u1', candidateName: 'Alex Morgan', appliedOn: '2024-05-09', status: 'Pending', score: 71, stage: 'Screening' },
  { id: 'a5', jobId: 'j2', candidateId: 'u2', candidateName: 'Priya Sharma', appliedOn: '2024-05-10', status: 'Shortlisted', score: 92, stage: 'Final Interview' },
  { id: 'a6', jobId: 'j3', candidateId: 'u3', candidateName: 'Diego Ramos', appliedOn: '2024-05-11', status: 'Shortlisted', score: 81, stage: 'Manager Review' },
];

export const mockCandidates = [
  { id: 'u1', name: 'Alex Morgan', email: 'alex@example.com', role: 'Candidate', title: 'Senior Frontend Engineer', skills: ['React', 'TypeScript', 'Node.js'], experience: 6, location: 'Boston, MA', score: 88, status: 'Available', phone: '+1 555-0101' },
  { id: 'u2', name: 'Priya Sharma', email: 'priya@example.com', role: 'Candidate', title: 'Backend Engineer', skills: ['C#', 'ASP.NET', 'SQL Server'], experience: 5, location: 'Austin, TX', score: 92, status: 'Available', phone: '+1 555-0102' },
  { id: 'u3', name: 'Diego Ramos', email: 'diego@example.com', role: 'Candidate', title: 'Product Manager', skills: ['Roadmapping', 'Analytics'], experience: 7, location: 'Remote', score: 81, status: 'Available', phone: '+1 555-0103' },
  { id: 'u9', name: 'Nina Patel', email: 'nina@example.com', role: 'Candidate', title: 'UX Designer', skills: ['Figma', 'Prototyping'], experience: 3, location: 'Chicago, IL', score: 74, status: 'Available', phone: '+1 555-0104' },
];

export const mockInterviews = [
  { id: 'i1', applicationId: 'a1', candidateName: 'Alex Morgan', jobTitle: 'Senior Frontend Engineer', date: '2024-05-20', time: '10:00', type: 'Technical Interview', interviewer: 'Emily Chen', status: 'Scheduled', feedback: '' },
  { id: 'i2', applicationId: 'a5', candidateName: 'Priya Sharma', jobTitle: 'Backend Engineer (.NET)', date: '2024-05-22', time: '14:00', type: 'Final Interview', interviewer: 'Marcus Webb', status: 'Scheduled', feedback: '' },
  { id: 'i3', applicationId: 'a6', candidateName: 'Diego Ramos', jobTitle: 'Product Manager', date: '2024-05-19', time: '11:30', type: 'Manager Review', interviewer: 'Marcus Webb', status: 'Completed', feedback: 'Strong product sense, great communication. Recommended for offer.' },
];

export const mockRecommendations = [
  { jobId: 'j1', matchScore: 92, reason: 'Strong React + TypeScript experience matches 8/8 required skills.' },
  { jobId: 'j5', matchScore: 78, reason: 'SQL and analytics background aligns with data role requirements.' },
  { jobId: 'j4', matchScore: 65, reason: 'Adjacent design experience; may need ramp-up on accessibility.' },
];

export const mockAuditLogs = [
  { id: 'l1', user: 'Admin Root', action: 'UPDATE_ROLE', target: 'Nina Patel', detail: 'Role changed Candidate -> Suspended', timestamp: '2024-05-12 09:14' },
  { id: 'l2', user: 'Sara Lee', action: 'CREATE_JOB', target: 'Senior Frontend Engineer', detail: 'New job posted', timestamp: '2024-05-01 08:02' },
  { id: 'l3', user: 'Emily Chen', action: 'SHORTLIST', target: 'Alex Morgan', detail: 'Shortlisted for Senior Frontend Engineer', timestamp: '2024-05-06 15:40' },
  { id: 'l4', user: 'John Carter', action: 'REJECT', target: 'Diego Ramos', detail: 'Rejected at Screening', timestamp: '2024-05-07 11:22' },
  { id: 'l5', user: 'Admin Root', action: 'CREATE_DEPARTMENT', target: 'Marketing', detail: 'Department created', timestamp: '2024-04-25 10:00' },
];

export const mockAnalytics = {
  totalApplications: 128,
  activeJobs: 7,
  shortlisted: 34,
  hired: 12,
  monthlyTrend: [
    { month: 'Jan', applications: 18, hires: 2 },
    { month: 'Feb', applications: 22, hires: 3 },
    { month: 'Mar', applications: 28, hires: 4 },
    { month: 'Apr', applications: 31, hires: 5 },
    { month: 'May', applications: 29, hires: 4 },
  ],
  byDepartment: [
    { department: 'Engineering', applications: 64, hires: 7 },
    { department: 'Product', applications: 32, hires: 3 },
    { department: 'Marketing', applications: 18, hires: 1 },
    { department: 'Finance', applications: 14, hires: 1 },
  ],
};

// Simulated async latency so loading states are visible.
export function delay(ms = 400) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
