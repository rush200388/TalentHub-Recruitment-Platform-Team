import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  departmentService,
  jobService,
  organizationService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import Modal from '../../components/ui/Modal';
import { Alert, EmptyState } from '../../components/ui/Alert';
import {
  Input,
  Select,
  Textarea,
} from '../../components/ui/FormField';
import {
  limits,
  parseSkills,
  todayIso,
  validateIntegerRange,
  validateMoney,
  validateSkills,
} from '../../utils/validation';

const emptyJob = {
  organizationId: '',
  departmentId: '',
  title: '',
  location: '',
  employmentType: 'Full-time',
  workMode: 'On-site',
  minimumExperienceYears: 0,
  minimumSalary: '',
  maximumSalary: '',
  currency: 'LKR',
  description: '',
  responsibilities: '',
  requirements: '',
  skills: '',
  status: 'Open',
  closingDate: '',
};

function toForm(job) {
  return {
    organizationId: job.organizationId || '',
    departmentId: job.departmentId || '',
    title: job.title || '',
    location: job.location || '',
    employmentType: job.type || 'Full-time',
    workMode: job.remote || 'On-site',
    minimumExperienceYears: job.minimumExperienceYears ?? 0,
    minimumSalary: job.minimumSalary ?? '',
    maximumSalary: job.maximumSalary ?? '',
    currency: job.currency || 'LKR',
    description: job.description || '',
    responsibilities: job.responsibilities || '',
    requirements: job.requirements || '',
    skills: job.skills?.join(', ') || '',
    status: job.status || 'Open',
    closingDate: job.closingAtUtc
      ? job.closingAtUtc.slice(0, 10)
      : '',
  };
}

function toPayload(form) {
  return {
    organizationId: form.organizationId || null,
    departmentId: form.departmentId || null,
    title: form.title.trim(),
    location: form.location.trim(),
    employmentType: form.employmentType,
    workMode: form.workMode,
    minimumExperienceYears:
      Number(form.minimumExperienceYears) || 0,
    minimumSalary:
      form.minimumSalary === ''
        ? null
        : Number(form.minimumSalary),
    maximumSalary:
      form.maximumSalary === ''
        ? null
        : Number(form.maximumSalary),
    currency: form.currency.trim().toUpperCase(),
    description: form.description.trim(),
    responsibilities:
      form.responsibilities.trim() || null,
    requirements: form.requirements.trim() || null,
    skills: parseSkills(form.skills),
    status: form.status,
    closingAtUtc: form.closingDate
      ? new Date(`${form.closingDate}T23:59:59Z`).toISOString()
      : null,
  };
}

export default function ManageJobsPage() {
  const {
    data: jobs,
    loading,
    error,
    setData,
  } = useAsync(() => jobService.list({ mine: true }), []);

  const {
    data: organizations,
    loading: organizationsLoading,
  } = useAsync(() => organizationService.list(), []);

  const {
    data: departments,
    loading: departmentsLoading,
  } = useAsync(() => departmentService.list(), []);

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyJob);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(null);
  const [feedback, setFeedback] = useState('');
  const [actionError, setActionError] = useState('');

  const availableDepartments = useMemo(() => {
    if (!form.organizationId) return departments || [];

    return (departments || []).filter(
      (department) =>
        department.organizationId === form.organizationId,
    );
  }, [departments, form.organizationId]);

  const openCreate = () => {
    const firstOrganization = organizations?.[0];

    setEditing(null);
    setForm({
      ...emptyJob,
      organizationId: firstOrganization?.id || '',
      departmentId: '',
    });
    setErrors({});
    setActionError('');
    setModalOpen(true);
  };

  const openEdit = (job) => {
    setEditing(job);
    setForm(toForm(job));
    setErrors({});
    setActionError('');
    setModalOpen(true);
  };

  const validate = () => {
    const nextErrors = {};

    const title = form.title.trim();
    const location = form.location.trim();
    const description = form.description.trim();

    if (title.length < 3 || title.length > limits.title) {
      nextErrors.title =
        'Job title must contain 3–120 characters';
    }

    if (!form.organizationId) {
      nextErrors.organizationId =
        'Organization is required';
    }

    if (
      location.length < 2 ||
      location.length > limits.location
    ) {
      nextErrors.location =
        'Location must contain 2–120 characters';
    }

    if (
      description.length < 30 ||
      description.length > limits.description
    ) {
      nextErrors.description =
        'Description must contain 30–5000 characters';
    }

    if (
      form.responsibilities.trim() &&
      form.responsibilities.trim().length < 10
    ) {
      nextErrors.responsibilities =
        'Responsibilities must contain at least 10 characters';
    }

    if (
      form.requirements.trim() &&
      form.requirements.trim().length < 10
    ) {
      nextErrors.requirements =
        'Requirements must contain at least 10 characters';
    }

    const experienceError =
      validateIntegerRange(
        form.minimumExperienceYears,
        0,
        limits.maxExperience,
        'Minimum experience',
      );

    if (experienceError) {
      nextErrors.minimumExperienceYears =
        experienceError;
    }

    const minimumSalaryError =
      validateMoney(
        form.minimumSalary,
        'Minimum salary',
      );
    const maximumSalaryError =
      validateMoney(
        form.maximumSalary,
        'Maximum salary',
      );

    if (minimumSalaryError) {
      nextErrors.minimumSalary =
        minimumSalaryError;
    }

    if (maximumSalaryError) {
      nextErrors.maximumSalary =
        maximumSalaryError;
    }

    if (
      !minimumSalaryError &&
      !maximumSalaryError &&
      form.minimumSalary !== '' &&
      form.maximumSalary !== '' &&
      Number(form.minimumSalary) >
        Number(form.maximumSalary)
    ) {
      nextErrors.maximumSalary =
        'Maximum salary must be greater than or equal to minimum salary';
    }

    if (!/^[A-Za-z]{3}$/.test(form.currency.trim())) {
      nextErrors.currency =
        'Currency must contain exactly 3 letters, such as LKR';
    }

    if (
      form.closingDate &&
      form.closingDate <= todayIso()
    ) {
      nextErrors.closingDate =
        'Closing date must be in the future';
    }

    const skillsError =
      validateSkills(form.skills, {
        required: form.status === 'Open',
      });

    if (skillsError) {
      nextErrors.skills = skillsError;
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const onSave = async () => {
    setActionError('');

    if (!validate()) return;

    setSaving(true);

    try {
      const payload = toPayload(form);

      if (editing) {
        const updated = await jobService.update(
          editing.id,
          payload,
        );

        setData(
          (jobs || []).map((job) =>
            job.id === editing.id ? updated : job,
          ),
        );
      } else {
        const created = await jobService.create(payload);
        setData([created, ...(jobs || [])]);
      }

      setModalOpen(false);
      setFeedback(
        editing
          ? 'Job updated successfully.'
          : 'Job created successfully.',
      );
      setTimeout(() => setFeedback(''), 3000);
    } catch (saveError) {
      setActionError(saveError.message);
    } finally {
      setSaving(false);
    }
  };

  const onDelete = async () => {
    setActionError('');

    try {
      await jobService.remove(confirmDelete.id);
      setData(
        (jobs || []).filter(
          (job) => job.id !== confirmDelete.id,
        ),
      );
      setConfirmDelete(null);
      setFeedback('Job deleted.');
      setTimeout(() => setFeedback(''), 3000);
    } catch (deleteError) {
      setConfirmDelete(null);
      setActionError(deleteError.message);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>Manage Jobs</h1>
          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Create and manage database-backed job postings
          </p>
        </div>

        <Button
          onClick={openCreate}
          disabled={organizationsLoading}
        >
          + New Job
        </Button>
      </div>

      {feedback && (
        <Alert
          variant="success"
          onClose={() => setFeedback('')}
        >
          {feedback}
        </Alert>
      )}

      {actionError && (
        <Alert
          variant="error"
          onClose={() => setActionError('')}
        >
          {actionError}
        </Alert>
      )}

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">{error}</Alert>
      ) : null}

      {!loading && !error && (jobs || []).length === 0 && (
        <EmptyState
          title="No jobs yet"
          message="Create your first real job posting."
          action={
            <Button onClick={openCreate}>+ New Job</Button>
          }
        />
      )}

      {!loading && !error && (jobs || []).length > 0 && (
        <div className="grid grid-2">
          {(jobs || []).map((job) => (
            <Card key={job.id}>
              <div className="flex items-center justify-between gap-2">
                <h3 style={{ margin: 0 }}>{job.title}</h3>
                <Badge
                  variant={
                    job.status === 'Open'
                      ? 'success'
                      : job.status === 'Draft'
                        ? 'warning'
                        : 'neutral'
                  }
                >
                  {job.status}
                </Badge>
              </div>

              <div className="text-secondary text-sm">
                {job.organization} · {job.department} ·{' '}
                {job.location}
              </div>

              <div className="job-meta">
                <span>💼 {job.type}</span>
                <span>🏠 {job.remote}</span>
                <span>💰 {job.salary}</span>
              </div>

              <div className="job-tags mb-2">
                {job.skills?.map((skill) => (
                  <Badge key={skill} variant="primary">
                    {skill}
                  </Badge>
                ))}
              </div>

              <div className="actions">
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => openEdit(job)}
                >
                  Edit
                </Button>

                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => setConfirmDelete(job)}
                >
                  Delete
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Job' : 'New Job'}
        size="lg"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() => setModalOpen(false)}
            >
              Cancel
            </Button>
            <Button loading={saving} onClick={onSave}>
              {editing ? 'Save Changes' : 'Create Job'}
            </Button>
          </>
        }
      >
        {actionError && (
          <Alert variant="error">{actionError}</Alert>
        )}

        <div className="form-row">
          <Select
            label="Organization"
            required
            value={form.organizationId}
            onChange={(event) =>
              setForm({
                ...form,
                organizationId: event.target.value,
                departmentId: '',
              })
            }
            error={errors.organizationId}
          >
            <option value="">Select organization</option>
            {(organizations || []).map((organization) => (
              <option
                key={organization.id}
                value={organization.id}
              >
                {organization.name}
              </option>
            ))}
          </Select>

          <Select
            label="Department"
            value={form.departmentId}
            disabled={departmentsLoading}
            onChange={(event) =>
              setForm({
                ...form,
                departmentId: event.target.value,
              })
            }
          >
            <option value="">General</option>
            {availableDepartments.map((department) => (
              <option
                key={department.id}
                value={department.id}
              >
                {department.name}
              </option>
            ))}
          </Select>
        </div>

        <Input
          label="Job title"
          required
          maxLength={limits.title}
          value={form.title}
          onChange={(event) =>
            setForm({ ...form, title: event.target.value })
          }
          error={errors.title}
        />

        <div className="form-row">
          <Input
            label="Location"
            required
            maxLength={limits.location}
            value={form.location}
            onChange={(event) =>
              setForm({
                ...form,
                location: event.target.value,
              })
            }
            error={errors.location}
          />

          <Select
            label="Employment type"
            value={form.employmentType}
            onChange={(event) =>
              setForm({
                ...form,
                employmentType: event.target.value,
              })
            }
          >
            {[
              'Full-time',
              'Part-time',
              'Contract',
              'Internship',
              'Temporary',
            ].map((type) => (
              <option key={type}>{type}</option>
            ))}
          </Select>
        </div>

        <div className="form-row">
          <Select
            label="Work mode"
            value={form.workMode}
            onChange={(event) =>
              setForm({
                ...form,
                workMode: event.target.value,
              })
            }
          >
            {['On-site', 'Hybrid', 'Remote'].map((mode) => (
              <option key={mode}>{mode}</option>
            ))}
          </Select>

          <Input
            label="Minimum experience (years)"
            type="number"
            min="0"
            max={limits.maxExperience}
            step="1"
            value={form.minimumExperienceYears}
            onChange={(event) =>
              setForm({
                ...form,
                minimumExperienceYears: event.target.value,
              })
            }
            error={errors.minimumExperienceYears}
          />
        </div>

        <div className="form-row">
          <Input
            label="Minimum salary"
            type="number"
            min="0"
            max={limits.maxSalary}
            step="0.01"
            value={form.minimumSalary}
            onChange={(event) =>
              setForm({
                ...form,
                minimumSalary: event.target.value,
              })
            }
            error={errors.minimumSalary}
          />

          <Input
            label="Maximum salary"
            type="number"
            min="0"
            max={limits.maxSalary}
            step="0.01"
            value={form.maximumSalary}
            onChange={(event) =>
              setForm({
                ...form,
                maximumSalary: event.target.value,
              })
            }
            error={errors.maximumSalary}
          />
        </div>

        <div className="form-row">
          <Input
            label="Currency"
            value={form.currency}
            maxLength={3}
            onChange={(event) =>
              setForm({
                ...form,
                currency: event.target.value,
              })
            }
            error={errors.currency}
          />

          <Input
            label="Closing date"
            type="date"
            min={todayIso()}
            value={form.closingDate}
            onChange={(event) =>
              setForm({
                ...form,
                closingDate: event.target.value,
              })
            }
            error={errors.closingDate}
          />
        </div>

        <Textarea
          label="Description"
          required
          maxLength={limits.description}
          value={form.description}
          onChange={(event) =>
            setForm({
              ...form,
              description: event.target.value,
            })
          }
          error={errors.description}
        />

        <Textarea
          label="Responsibilities"
          maxLength={limits.description}
          value={form.responsibilities}
          onChange={(event) =>
            setForm({
              ...form,
              responsibilities: event.target.value,
            })
          }
          error={errors.responsibilities}
        />

        <Textarea
          label="Requirements"
          maxLength={limits.description}
          value={form.requirements}
          onChange={(event) =>
            setForm({
              ...form,
              requirements: event.target.value,
            })
          }
          error={errors.requirements}
        />

        <Input
          label="Required skills (comma separated)"
          maxLength={limits.skillsText}
          value={form.skills}
          placeholder="C#, React, PostgreSQL"
          onChange={(event) =>
            setForm({
              ...form,
              skills: event.target.value,
            })
          }
          error={errors.skills}
          hint="Maximum 30 skills"
        />

        <Select
          label="Status"
          value={form.status}
          onChange={(event) =>
            setForm({
              ...form,
              status: event.target.value,
            })
          }
        >
          <option value="Open">Open / Published</option>
          <option value="Draft">Draft</option>
          <option value="Closed">Closed</option>
          <option value="Archived">Archived</option>
        </Select>
      </Modal>

      <Modal
        open={Boolean(confirmDelete)}
        onClose={() => setConfirmDelete(null)}
        title="Delete Job?"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() => setConfirmDelete(null)}
            >
              Cancel
            </Button>
            <Button variant="danger" onClick={onDelete}>
              Delete
            </Button>
          </>
        }
      >
        <p>
          Delete <strong>{confirmDelete?.title}</strong>? Jobs
          with applications cannot be deleted.
        </p>
      </Modal>
    </div>
  );
}
