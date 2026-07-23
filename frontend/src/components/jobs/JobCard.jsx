import { useNavigate } from 'react-router-dom';
import Badge from '../ui/Badge';

export default function JobCard({ job }) {
  const navigate = useNavigate();

  return (
    <div
      className="card job-card"
      onClick={() =>
        navigate(`/candidate/jobs/${job.id}`)
      }
    >
      <div className="flex items-center justify-between gap-2">
        <h3>{job.title}</h3>
        <Badge
          variant={
            job.status === 'Open' ? 'success' : 'neutral'
          }
        >
          {job.status}
        </Badge>
      </div>

      <div className="text-secondary text-sm">
        {job.organization} · {job.department}
      </div>

      <div className="job-meta">
        <span>📍 {job.location}</span>
        <span>💼 {job.type}</span>
        <span>🏠 {job.remote}</span>
        <span>💰 {job.salary}</span>
      </div>

      <div className="job-tags">
        {job.skills?.slice(0, 4).map((skill) => (
          <Badge key={skill} variant="primary">
            {skill}
          </Badge>
        ))}
      </div>
    </div>
  );
}
