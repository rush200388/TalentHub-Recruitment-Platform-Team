import {
  useEffect,
  useRef,
  useState,
} from 'react';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import {
  Input,
  Textarea,
} from '../../components/ui/FormField';
import { Alert } from '../../components/ui/Alert';
import Spinner from '../../components/ui/Spinner';
import { candidateService } from '../../services/services';
import {
  limits,
  parseSkills,
  validateIntegerRange,
  validateName,
  validateOptionalUrl,
  validatePhone,
  validateSkills,
} from '../../utils/validation';

const emptyForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  location: '',
  currentJobTitle: '',
  professionalSummary: '',
  yearsOfExperience: 0,
  linkedInUrl: '',
  portfolioUrl: '',
  skills: '',
};

export default function CandidateProfilePage() {
  const fileRef = useRef(null);

  const [form, setForm] =
    useState(emptyForm);
  const [resume, setResume] =
    useState(null);
  const [completeness, setCompleteness] =
    useState(0);
  const [loading, setLoading] =
    useState(true);
  const [saving, setSaving] =
    useState(false);
  const [uploading, setUploading] =
    useState(false);
  const [success, setSuccess] =
    useState('');
  const [error, setError] =
    useState('');
  const [errors, setErrors] =
    useState({});

  useEffect(() => {
    let active = true;

    async function loadProfile() {
      try {
        const profile =
          await candidateService.me();

        if (!active) return;

        setForm({
          firstName: profile.firstName || '',
          lastName: profile.lastName || '',
          email: profile.email || '',
          phone: profile.phone || '',
          location: profile.location || '',
          currentJobTitle:
            profile.currentJobTitle || '',
          professionalSummary:
            profile.professionalSummary || '',
          yearsOfExperience:
            profile.yearsOfExperience || 0,
          linkedInUrl:
            profile.linkedInUrl || '',
          portfolioUrl:
            profile.portfolioUrl || '',
          skills:
            profile.skills?.join(', ') || '',
        });

        setResume(profile.resume || null);
        setCompleteness(
          profile.completenessPercentage || 0,
        );
      } catch (loadError) {
        if (active) {
          setError(loadError.message);
        }
      } finally {
        if (active) setLoading(false);
      }
    }

    loadProfile();

    return () => {
      active = false;
    };
  }, []);

  const validate = () => {
    const nextErrors = {};

    const firstNameError =
      validateName(form.firstName, 'First name');
    const lastNameError =
      validateName(form.lastName, 'Last name');
    const phoneError =
      validatePhone(form.phone);
    const experienceError =
      validateIntegerRange(
        form.yearsOfExperience,
        0,
        limits.maxExperience,
        'Years of experience',
      );
    const linkedInError =
      validateOptionalUrl(
        form.linkedInUrl,
        'LinkedIn URL',
      );
    const portfolioError =
      validateOptionalUrl(
        form.portfolioUrl,
        'Portfolio URL',
      );
    const skillsError =
      validateSkills(form.skills);

    if (firstNameError) {
      nextErrors.firstName = firstNameError;
    }

    if (lastNameError) {
      nextErrors.lastName = lastNameError;
    }

    if (phoneError) {
      nextErrors.phone = phoneError;
    }

    if (
      form.location.trim() &&
      form.location.trim().length < 2
    ) {
      nextErrors.location =
        'Location must contain at least 2 characters';
    }

    if (
      form.currentJobTitle.trim() &&
      form.currentJobTitle.trim().length < 2
    ) {
      nextErrors.currentJobTitle =
        'Professional title must contain at least 2 characters';
    }

    if (
      form.professionalSummary.trim() &&
      form.professionalSummary.trim().length < 20
    ) {
      nextErrors.professionalSummary =
        'Professional summary must contain at least 20 characters';
    }

    if (experienceError) {
      nextErrors.yearsOfExperience =
        experienceError;
    }

    if (linkedInError) {
      nextErrors.linkedInUrl =
        linkedInError;
    }

    if (portfolioError) {
      nextErrors.portfolioUrl =
        portfolioError;
    }

    if (skillsError) {
      nextErrors.skills = skillsError;
    }

    setErrors(nextErrors);

    return Object.keys(nextErrors).length === 0;
  };

  const onSave = async (event) => {
    event.preventDefault();
    setError('');
    setSuccess('');

    if (!validate()) return;

    setSaving(true);

    try {
      const profile =
        await candidateService.updateMe({
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          phone: form.phone.trim() || null,
          location:
            form.location.trim() || null,
          currentJobTitle:
            form.currentJobTitle.trim() || null,
          professionalSummary:
            form.professionalSummary.trim() ||
            null,
          yearsOfExperience:
            Number(form.yearsOfExperience) || 0,
          linkedInUrl:
            form.linkedInUrl.trim() || null,
          portfolioUrl:
            form.portfolioUrl.trim() || null,
          skills: parseSkills(form.skills),
        });

      setCompleteness(
        profile.completenessPercentage,
      );
      setResume(profile.resume || resume);
      setSuccess('Profile saved successfully.');
    } catch (saveError) {
      setError(saveError.message);
    } finally {
      setSaving(false);
    }
  };

  const onFile = async (event) => {
    const file =
      event.target.files?.[0];

    event.target.value = '';

    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      setError(
        'Resume files must be 5 MB or smaller.',
      );
      return;
    }

    const extension =
      file.name.split('.').pop()?.toLowerCase();

    if (!['pdf', 'docx'].includes(extension)) {
      setError(
        'Only PDF and DOCX files are allowed.',
      );
      return;
    }

    setUploading(true);
    setError('');
    setSuccess('');

    try {
      const uploaded =
        await candidateService.uploadResume(file);

      setResume(uploaded);
      setSuccess('Resume uploaded successfully.');

      const refreshed =
        await candidateService.me();

      setCompleteness(
        refreshed.completenessPercentage,
      );
    } catch (uploadError) {
      setError(uploadError.message);
    } finally {
      setUploading(false);
    }
  };

  if (loading) return <Spinner />;

  return (
    <div className="grid grid-2">
      <Card title="Profile Information">
        {success && (
          <Alert
            variant="success"
            onClose={() => setSuccess('')}
          >
            {success}
          </Alert>
        )}

        {error && (
          <Alert
            variant="error"
            onClose={() => setError('')}
          >
            {error}
          </Alert>
        )}

        <form onSubmit={onSave} noValidate>
          <div className="form-row">
            <Input
              label="First name"
              required
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
            value={form.email}
            disabled
            hint="The login email cannot be changed here"
          />

          <div className="form-row">
            <Input
              label="Phone"
              inputMode="numeric"
              pattern="[0-9]{10}"
              maxLength={limits.phone}
              value={form.phone}
              onChange={(event) =>
                setForm({
                  ...form,
                  phone: event.target.value
                    .replace(/\D/g, '')
                    .slice(0, limits.phone),
                })
              }
              error={errors.phone}
            />

            <Input
              label="Location"
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
          </div>

          <Input
            label="Professional title"
            maxLength={limits.title}
            value={form.currentJobTitle}
            onChange={(event) =>
              setForm({
                ...form,
                currentJobTitle:
                  event.target.value,
              })
            }
            error={errors.currentJobTitle}
          />

          <Input
            label="Years of experience"
            type="number"
            min="0"
            max={limits.maxExperience}
            value={form.yearsOfExperience}
            onChange={(event) =>
              setForm({
                ...form,
                yearsOfExperience:
                  event.target.value,
              })
            }
            error={errors.yearsOfExperience}
          />

          <Textarea
            label="Professional summary"
            maxLength={limits.summary}
            value={form.professionalSummary}
            onChange={(event) =>
              setForm({
                ...form,
                professionalSummary:
                  event.target.value,
              })
            }
            error={errors.professionalSummary}
          />

          <Input
            label="Skills (comma separated)"
            maxLength={limits.skillsText}
            value={form.skills}
            onChange={(event) =>
              setForm({
                ...form,
                skills: event.target.value,
              })
            }
            error={errors.skills}
            hint="Maximum 30 skills; separate them with commas"
          />

          <div className="form-row">
            <Input
              label="LinkedIn URL"
              type="url"
              maxLength={500}
              value={form.linkedInUrl}
              onChange={(event) =>
                setForm({
                  ...form,
                  linkedInUrl:
                    event.target.value,
                })
              }
              error={errors.linkedInUrl}
            />

            <Input
              label="Portfolio URL"
              type="url"
              maxLength={500}
              value={form.portfolioUrl}
              onChange={(event) =>
                setForm({
                  ...form,
                  portfolioUrl:
                    event.target.value,
                })
              }
              error={errors.portfolioUrl}
            />
          </div>

          <Button
            type="submit"
            loading={saving}
          >
            Save Profile
          </Button>
        </form>
      </Card>

      <div>
        <Card
          title="CV / Resume"
          className="mb-3"
        >
          <div
            className="flex items-center gap-2 mb-2"
            style={{
              padding: '1rem',
              border:
                '1px dashed var(--color-border-dark)',
              borderRadius:
                'var(--radius-md)',
            }}
          >
            <span style={{ fontSize: '2rem' }}>
              📄
            </span>

            <div className="flex-1">
              <div style={{ fontWeight: 600 }}>
                {resume?.originalFileName ||
                  'No resume uploaded'}
              </div>

              {resume && (
                <div className="text-sm text-muted">
                  {Math.ceil(
                    resume.fileSizeBytes / 1024,
                  )}{' '}
                  KB
                </div>
              )}
            </div>

            {resume && (
              <Badge variant="success">
                Uploaded
              </Badge>
            )}
          </div>

          <input
            ref={fileRef}
            type="file"
            accept=".pdf,.docx"
            onChange={onFile}
            style={{ display: 'none' }}
          />

          <Button
            variant="secondary"
            loading={uploading}
            onClick={() =>
              fileRef.current?.click()
            }
          >
            {resume
              ? 'Replace Resume'
              : 'Upload Resume'}
          </Button>

          <div className="form-hint">
            PDF or DOCX, maximum 5 MB
          </div>
        </Card>

        <Card title="Profile Completeness">
          <div className="flex items-center justify-between mb-1">
            <strong>{completeness}%</strong>
            <Badge
              variant={
                completeness >= 80
                  ? 'success'
                  : 'warning'
              }
            >
              {completeness >= 80
                ? 'Ready'
                : 'Incomplete'}
            </Badge>
          </div>

          <div className="score-bar mb-2">
            <div
              className={`score-bar-fill ${
                completeness >= 80
                  ? 'high'
                  : completeness >= 50
                    ? 'mid'
                    : 'low'
              }`}
              style={{
                width: `${completeness}%`,
              }}
            />
          </div>

          <p className="text-sm text-secondary">
            Complete your skills, experience,
            summary, and resume to improve
            recommendation quality.
          </p>
        </Card>
      </div>
    </div>
  );
}
