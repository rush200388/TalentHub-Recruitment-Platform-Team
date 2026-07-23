import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import { jobService } from '../../services/services';
import JobCard from '../../components/jobs/JobCard';
import Card from '../../components/ui/Card';
import Spinner from '../../components/ui/Spinner';
import { Alert, EmptyState } from '../../components/ui/Alert';
import { Input, Select } from '../../components/ui/FormField';

export default function BrowseJobsPage() {
  const {
    data: jobs,
    loading,
    error,
  } = useAsync(() => jobService.list(), []);

  const [search, setSearch] = useState('');
  const [department, setDepartment] = useState('');
  const [type, setType] = useState('');

  const departments = useMemo(
    () => [
      ...new Set(
        (jobs || [])
          .map((job) => job.department)
          .filter(Boolean),
      ),
    ],
    [jobs],
  );

  const types = useMemo(
    () => [
      ...new Set(
        (jobs || []).map((job) => job.type).filter(Boolean),
      ),
    ],
    [jobs],
  );

  const filtered = (jobs || []).filter((job) => {
    const term = search.toLowerCase();

    const matchesSearch =
      !search ||
      job.title.toLowerCase().includes(term) ||
      job.organization.toLowerCase().includes(term) ||
      job.skills?.some((skill) =>
        skill.toLowerCase().includes(term),
      );

    const matchesDepartment =
      !department || job.department === department;

    const matchesType = !type || job.type === type;

    return (
      matchesSearch &&
      matchesDepartment &&
      matchesType &&
      job.status === 'Open'
    );
  });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>Browse Jobs</h1>
          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Published vacancies loaded from PostgreSQL
          </p>
        </div>
      </div>

      <Card className="mb-3">
        <div className="search-bar">
          <Input
            placeholder="Search title, company, or skill..."
            value={search}
            onChange={(event) =>
              setSearch(event.target.value)
            }
            className="flex-1"
          />

          <Select
            value={department}
            onChange={(event) =>
              setDepartment(event.target.value)
            }
            style={{ minWidth: 180 }}
          >
            <option value="">All departments</option>
            {departments.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </Select>

          <Select
            value={type}
            onChange={(event) =>
              setType(event.target.value)
            }
            style={{ minWidth: 140 }}
          >
            <option value="">All types</option>
            {types.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </Select>
        </div>
      </Card>

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">{error}</Alert>
      ) : null}

      {!loading && !error && filtered.length === 0 && (
        <EmptyState
          title="No published jobs found"
          message="A recruiter can publish a job from the recruiter dashboard."
        />
      )}

      {!loading && !error && filtered.length > 0 && (
        <div className="grid grid-3">
          {filtered.map((job) => (
            <JobCard key={job.id} job={job} />
          ))}
        </div>
      )}
    </div>
  );
}
