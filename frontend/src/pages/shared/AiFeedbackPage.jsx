import {
  useEffect,
  useMemo,
  useState,
} from 'react';
import {
  aiFeedbackService,
  applicationService,
} from '../../services/services';
import { useAsync } from '../../hooks/useAsync';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import {
  Select,
} from '../../components/ui/FormField';
import {
  ScoreBar,
} from '../../components/ui/ScoreBar';

function recommendationVariant(
  recommendation,
) {
  if (
    [
      'Strong Hire',
      'Hire',
    ].includes(recommendation)
  ) {
    return 'success';
  }

  if (
    [
      'Reject',
      'Strong Reject',
    ].includes(recommendation)
  ) {
    return 'error';
  }

  return 'warning';
}

export default function AiFeedbackPage() {
  const {
    data: applications,
    loading,
    error,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const [
    applicationId,
    setApplicationId,
  ] = useState('');

  const [result, setResult] =
    useState(null);
  const [generating, setGenerating] =
    useState(false);
  const [
    generationError,
    setGenerationError,
  ] = useState('');

  useEffect(() => {
    if (
      !applicationId &&
      applications?.length
    ) {
      setApplicationId(
        applications[0].id,
      );
    }
  }, [
    applications,
    applicationId,
  ]);

  const selected =
    useMemo(
      () =>
        (applications || []).find(
          (application) =>
            application.id ===
            applicationId,
        ),
      [
        applications,
        applicationId,
      ],
    );

  const generate = async () => {
    if (!applicationId) {
      setGenerationError(
        'Select an application.',
      );
      return;
    }

    setGenerating(true);
    setGenerationError('');
    setResult(null);

    try {
      const response =
        await aiFeedbackService
          .generate(applicationId);

      setResult(response);
    } catch (generateError) {
      setGenerationError(
        generateError.message,
      );
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            AI Candidate Feedback
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Generate explainable,
            job-related feedback with
            external-AI and rule-based
            fallback support
          </p>
        </div>
      </div>

      {error && (
        <Alert variant="error">
          {error}
        </Alert>
      )}

      {generationError && (
        <Alert
          variant="error"
          onClose={() =>
            setGenerationError('')
          }
        >
          {generationError}
        </Alert>
      )}

      {loading ? (
        <Spinner />
      ) : (applications || [])
          .length === 0 ? (
        <Card>
          <EmptyState
            title="No applications available"
            message="Applications that your account is permitted to review will appear here."
          />
        </Card>
      ) : (
        <>
          <Card className="mb-3">
            <div className="form-row">
              <Select
                label="Application"
                value={applicationId}
                onChange={(event) => {
                  setApplicationId(
                    event.target.value,
                  );
                  setResult(null);
                }}
              >
                {(applications || []).map(
                  (application) => (
                    <option
                      key={
                        application.id
                      }
                      value={
                        application.id
                      }
                    >
                      {
                        application.candidateName
                      }{' '}
                      —{' '}
                      {
                        application.jobTitle
                      }
                    </option>
                  ),
                )}
              </Select>

              <div
                style={{
                  display: 'flex',
                  alignItems:
                    'flex-end',
                }}
              >
                <Button
                  loading={generating}
                  onClick={generate}
                >
                  Generate AI Feedback
                </Button>
              </div>
            </div>

            {selected && (
              <div className="grid grid-3 mt-2">
                <div>
                  <div className="text-sm text-muted">
                    Candidate
                  </div>

                  <strong>
                    {selected.candidateName}
                  </strong>
                </div>

                <div>
                  <div className="text-sm text-muted">
                    Position
                  </div>

                  <strong>
                    {selected.jobTitle}
                  </strong>
                </div>

                <div>
                  <div className="text-sm text-muted">
                    Current Status
                  </div>

                  <Badge variant="info">
                    {selected.status}
                  </Badge>
                </div>
              </div>
            )}
          </Card>

          {selected && (
            <div className="grid grid-2 mb-3">
              <Card title="Current Match Evidence">
                <ScoreBar
                  score={Math.round(
                    selected.matchScore,
                  )}
                />

                <p className="text-sm text-secondary">
                  {selected.matchReason}
                </p>
              </Card>

              <Card title="Skills Evidence">
                <div className="mb-2">
                  <div className="text-sm text-muted">
                    Matched
                  </div>

                  <div className="job-tags">
                    {selected
                      .matchedSkills
                      ?.length ? (
                      selected.matchedSkills.map(
                        (skill) => (
                          <Badge
                            key={skill}
                            variant="success"
                          >
                            {skill}
                          </Badge>
                        ),
                      )
                    ) : (
                      <span className="text-sm text-muted">
                        None
                      </span>
                    )}
                  </div>
                </div>

                <div>
                  <div className="text-sm text-muted">
                    Missing
                  </div>

                  <div className="job-tags">
                    {selected
                      .missingSkills
                      ?.length ? (
                      selected.missingSkills.map(
                        (skill) => (
                          <Badge
                            key={skill}
                            variant="neutral"
                          >
                            {skill}
                          </Badge>
                        ),
                      )
                    ) : (
                      <span className="text-sm text-muted">
                        None
                      </span>
                    )}
                  </div>
                </div>
              </Card>
            </div>
          )}

          {result && (
            <>
              <Card className="mb-3">
                <div className="flex items-center justify-between gap-2 flex-wrap">
                  <div>
                    <div className="text-sm text-muted">
                      Provider
                    </div>

                    <strong>
                      {result.provider}
                    </strong>
                  </div>

                  <div className="flex items-center gap-1">
                    <Badge
                      variant={
                        result.usedExternalAi
                          ? 'success'
                          : 'warning'
                      }
                    >
                      {result.usedExternalAi
                        ? 'External AI'
                        : 'Rule-Based Fallback'}
                    </Badge>

                    <Badge
                      variant={recommendationVariant(
                        result.recommendation,
                      )}
                    >
                      {result.recommendation}
                    </Badge>
                  </div>
                </div>

                {result.fallbackReason && (
                  <Alert
                    variant="warning"
                    style={{
                      marginTop: '1rem',
                    }}
                  >
                    {result.fallbackReason}
                  </Alert>
                )}
              </Card>

              <div className="grid grid-2 mb-3">
                <Card title="Summary">
                  <p>{result.summary}</p>
                </Card>

                <Card title="Suggested Candidate Feedback">
                  <p>
                    {
                      result.suggestedFeedback
                    }
                  </p>
                </Card>
              </div>

              <div className="grid grid-2">
                <Card title="Strengths">
                  {result.strengths
                    ?.length ? (
                    <ul>
                      {result.strengths.map(
                        (strength) => (
                          <li
                            key={
                              strength
                            }
                          >
                            {strength}
                          </li>
                        ),
                      )}
                    </ul>
                  ) : (
                    <p className="text-muted">
                      No strengths were
                      generated.
                    </p>
                  )}
                </Card>

                <Card title="Risks and Review Points">
                  {result.risks
                    ?.length ? (
                    <ul>
                      {result.risks.map(
                        (risk) => (
                          <li key={risk}>
                            {risk}
                          </li>
                        ),
                      )}
                    </ul>
                  ) : (
                    <p className="text-muted">
                      No material risks were
                      generated.
                    </p>
                  )}
                </Card>
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
}
