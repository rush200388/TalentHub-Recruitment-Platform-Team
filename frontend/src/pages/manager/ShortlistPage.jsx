import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  evaluationService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import Modal from '../../components/ui/Modal';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import Avatar from '../../components/ui/Avatar';
import {
  ScoreBar,
} from '../../components/ui/ScoreBar';
import {
  Input,
  Textarea,
} from '../../components/ui/FormField';

const emptyEvaluation = {
  skillsScore: 70,
  experienceScore: 70,
  interviewScore: 70,
  comments: '',
};

function calculateOverall(form) {
  const skills =
    Number(form.skillsScore) || 0;
  const experience =
    Number(form.experienceScore) ||
    0;
  const interview =
    Number(form.interviewScore) ||
    0;

  return Math.round(
    (skills * 0.3 +
      experience * 0.2 +
      interview * 0.5) *
      100,
  ) / 100;
}

function scoreValidation(
  value,
  label,
) {
  const number = Number(value);

  if (
    !Number.isFinite(number) ||
    number < 0 ||
    number > 100
  ) {
    return `${label} must be between 0 and 100`;
  }

  return '';
}

export default function ShortlistPage() {
  const {
    data: applications,
    loading,
    error,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const {
    data: evaluations,
    reload: reloadEvaluations,
  } = useAsync(
    () => evaluationService.list(),
    [],
  );

  const [selected, setSelected] =
    useState(null);
  const [form, setForm] =
    useState(emptyEvaluation);
  const [errors, setErrors] =
    useState({});
  const [saving, setSaving] =
    useState(false);
  const [feedback, setFeedback] =
    useState('');
  const [actionError, setActionError] =
    useState('');

  const eligible = useMemo(
    () =>
      (applications || []).filter(
        (application) =>
          [
            'Shortlisted',
            'Interview Scheduled',
          ].includes(
            application.status,
          ),
      ),
    [applications],
  );

  const evaluationMap = useMemo(
    () =>
      new Map(
        (evaluations || []).map(
          (evaluation) => [
            evaluation.applicationId,
            evaluation,
          ],
        ),
      ),
    [evaluations],
  );

  const openEvaluation = (
    application,
  ) => {
    const existing =
      evaluationMap.get(
        application.id,
      );

    setSelected(application);

    setForm(
      existing
        ? {
            skillsScore:
              existing.skillsScore,
            experienceScore:
              existing.experienceScore,
            interviewScore:
              existing.interviewScore,
            comments:
              existing.comments || '',
          }
        : {
            ...emptyEvaluation,
            skillsScore:
              Math.round(
                application.matchScore,
              ),
          },
    );

    setErrors({});
    setActionError('');
  };

  const validate = () => {
    const nextErrors = {};

    [
      ['skillsScore', 'Skills score'],
      [
        'experienceScore',
        'Experience score',
      ],
      [
        'interviewScore',
        'Interview score',
      ],
    ].forEach(([key, label]) => {
      const message =
        scoreValidation(
          form[key],
          label,
        );

      if (message) {
        nextErrors[key] = message;
      }
    });

    if (form.comments.length > 3000) {
      nextErrors.comments =
        'Comments cannot exceed 3000 characters';
    }

    setErrors(nextErrors);

    return (
      Object.keys(nextErrors).length ===
      0
    );
  };

  const onEvaluate = async () => {
    if (!selected || !validate()) {
      return;
    }

    setSaving(true);
    setActionError('');

    try {
      await evaluationService.save({
        jobApplicationId:
          selected.id,
        skillsScore:
          Number(
            form.skillsScore,
          ),
        experienceScore:
          Number(
            form.experienceScore,
          ),
        interviewScore:
          Number(
            form.interviewScore,
          ),
        comments:
          form.comments.trim() ||
          null,
      });

      setSelected(null);
      await reloadEvaluations();

      setFeedback(
        'Evaluation saved successfully.',
      );

      setTimeout(
        () => setFeedback(''),
        3000,
      );
    } catch (saveError) {
      setActionError(
        saveError.message,
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            Shortlisted Candidates
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Create formal candidate
            evaluations
          </p>
        </div>
      </div>

      {feedback && (
        <Alert
          variant="success"
          onClose={() =>
            setFeedback('')
          }
        >
          {feedback}
        </Alert>
      )}

      {actionError && (
        <Alert
          variant="error"
          onClose={() =>
            setActionError('')
          }
        >
          {actionError}
        </Alert>
      )}

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">
          {error}
        </Alert>
      ) : null}

      {!loading &&
        !error &&
        eligible.length === 0 && (
          <EmptyState
            title="No shortlisted candidates"
            message="Recruiters will shortlist candidates for your review."
          />
        )}

      {!loading &&
        !error &&
        eligible.length > 0 && (
          <div className="grid grid-2">
            {eligible.map(
              (application) => {
                const evaluation =
                  evaluationMap.get(
                    application.id,
                  );

                return (
                  <Card
                    key={application.id}
                  >
                    <div className="flex items-center gap-2 mb-1">
                      <Avatar
                        name={
                          application.candidateName
                        }
                      />

                      <div className="flex-1">
                        <h3
                          style={{
                            margin: 0,
                          }}
                        >
                          {
                            application.candidateName
                          }
                        </h3>

                        <div className="text-sm text-muted">
                          {
                            application.candidateTitle
                          }
                        </div>
                      </div>

                      <Badge
                        variant="success"
                      >
                        {
                          application.status
                        }
                      </Badge>
                    </div>

                    <div className="text-sm text-secondary mb-1">
                      Applied for:{' '}
                      <strong>
                        {
                          application.jobTitle
                        }
                      </strong>
                    </div>

                    <div className="mb-2">
                      <div className="text-sm text-muted mb-1">
                        AI Match Score
                      </div>

                      <ScoreBar
                        score={Math.round(
                          application.matchScore,
                        )}
                      />
                    </div>

                    {evaluation && (
                      <div className="mb-2">
                        <div className="text-sm text-muted mb-1">
                          Formal Evaluation
                        </div>

                        <ScoreBar
                          score={Math.round(
                            evaluation.overallScore,
                          )}
                        />
                      </div>
                    )}

                    <Button
                      variant="secondary"
                      size="sm"
                      onClick={() =>
                        openEvaluation(
                          application,
                        )
                      }
                    >
                      {evaluation
                        ? 'Edit Evaluation'
                        : 'Evaluate'}
                    </Button>
                  </Card>
                );
              },
            )}
          </div>
        )}

      <Modal
        open={Boolean(selected)}
        onClose={() =>
          setSelected(null)
        }
        title="Evaluate Candidate"
        size="lg"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setSelected(null)
              }
            >
              Cancel
            </Button>

            <Button
              loading={saving}
              onClick={onEvaluate}
            >
              Save Evaluation
            </Button>
          </>
        }
      >
        {selected && (
          <div>
            {actionError && (
              <Alert variant="error">
                {actionError}
              </Alert>
            )}

            <div className="flex items-center gap-2 mb-3">
              <Avatar
                name={
                  selected.candidateName
                }
                size="lg"
              />

              <div>
                <h3 style={{ margin: 0 }}>
                  {
                    selected.candidateName
                  }
                </h3>

                <div className="text-sm text-muted">
                  {selected.jobTitle}
                </div>
              </div>
            </div>

            <div className="form-row">
              <Input
                label="Skills score"
                type="number"
                min="0"
                max="100"
                step="0.01"
                value={form.skillsScore}
                onChange={(event) =>
                  setForm({
                    ...form,
                    skillsScore:
                      event.target.value,
                  })
                }
                error={
                  errors.skillsScore
                }
              />

              <Input
                label="Experience score"
                type="number"
                min="0"
                max="100"
                step="0.01"
                value={
                  form.experienceScore
                }
                onChange={(event) =>
                  setForm({
                    ...form,
                    experienceScore:
                      event.target.value,
                  })
                }
                error={
                  errors.experienceScore
                }
              />
            </div>

            <Input
              label="Interview score"
              type="number"
              min="0"
              max="100"
              step="0.01"
              value={form.interviewScore}
              onChange={(event) =>
                setForm({
                  ...form,
                  interviewScore:
                    event.target.value,
                })
              }
              error={
                errors.interviewScore
              }
            />

            <Card className="mb-2">
              <div className="text-sm text-muted">
                Calculated Overall Score
              </div>

              <div
                style={{
                  fontSize: '2rem',
                  fontWeight: 800,
                }}
              >
                {calculateOverall(form)}%
              </div>

              <div className="text-sm text-secondary">
                Skills 30% + Experience
                20% + Interview 50%
              </div>
            </Card>

            <Textarea
              label="Comments"
              maxLength={3000}
              value={form.comments}
              onChange={(event) =>
                setForm({
                  ...form,
                  comments:
                    event.target.value,
                })
              }
              error={errors.comments}
              hint={`${form.comments.length}/3000 characters`}
            />
          </div>
        )}
      </Modal>
    </div>
  );
}
