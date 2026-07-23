import {
  useEffect,
  useState,
} from 'react';
import { useNavigate } from 'react-router-dom';
import {
  resumeAnalysisService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Button from '../../components/ui/Button';
import Badge from '../../components/ui/Badge';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';

function statusVariant(status) {
  if (status === 'Completed') {
    return 'success';
  }

  if (status === 'Failed') {
    return 'error';
  }

  if (status === 'Processing') {
    return 'info';
  }

  return 'warning';
}

export default function ResumeAnalysisPage() {
  const navigate = useNavigate();

  const [analysis, setAnalysis] =
    useState(null);
  const [loading, setLoading] =
    useState(true);
  const [analyzing, setAnalyzing] =
    useState(false);
  const [applying, setApplying] =
    useState(false);
  const [
    selectedSkills,
    setSelectedSkills,
  ] = useState([]);
  const [error, setError] =
    useState('');
  const [success, setSuccess] =
    useState('');

  const load = async () => {
    setLoading(true);
    setError('');

    try {
      const result =
        await resumeAnalysisService.get();

      setAnalysis(result);

      setSelectedSkills(
        result.extractedSkills || [],
      );
    } catch (loadError) {
      setAnalysis(null);
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const analyze = async () => {
    setAnalyzing(true);
    setError('');
    setSuccess('');

    try {
      const result =
        await resumeAnalysisService
          .analyze();

      setAnalysis(result);
      setSelectedSkills(
        result.extractedSkills || [],
      );

      setSuccess(
        'Resume analysis completed.',
      );
    } catch (analysisError) {
      setError(analysisError.message);
    } finally {
      setAnalyzing(false);
    }
  };

  const toggleSkill = (skill) => {
    setSelectedSkills(
      (current) =>
        current.includes(skill)
          ? current.filter(
              (item) => item !== skill,
            )
          : [...current, skill],
    );
  };

  const applySkills = async () => {
    if (
      selectedSkills.length === 0
    ) {
      setError(
        'Select at least one extracted skill.',
      );
      return;
    }

    setApplying(true);
    setError('');
    setSuccess('');

    try {
      const response =
        await resumeAnalysisService
          .applySkills(
            selectedSkills,
          );

      setSuccess(
        response.addedCount > 0
          ? `${response.addedCount} skills were added to your profile.`
          : 'All selected skills were already in your profile.',
      );
    } catch (applyError) {
      setError(applyError.message);
    } finally {
      setApplying(false);
    }
  };

  if (loading) {
    return (
      <Spinner label="Loading resume analysis..." />
    );
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 style={{ margin: 0 }}>
            AI Resume Analysis
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Extract skills and career signals
            from your PDF or DOCX resume
          </p>
        </div>

        <Button
          variant="secondary"
          onClick={() =>
            navigate('/candidate/profile')
          }
        >
          Manage Resume
        </Button>
      </div>

      {success && (
        <Alert
          variant="success"
          onClose={() =>
            setSuccess('')
          }
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

      {!analysis ? (
        <Card>
          <EmptyState
            title="Resume analysis is unavailable"
            message="Upload a PDF or DOCX resume from My Profile, then return here."
            action={
              <Button
                onClick={() =>
                  navigate(
                    '/candidate/profile',
                  )
                }
              >
                Open My Profile
              </Button>
            }
          />
        </Card>
      ) : (
        <>
          <Card className="mb-3">
            <div className="flex items-center justify-between gap-2 flex-wrap">
              <div>
                <h3 style={{ margin: 0 }}>
                  {analysis.fileName}
                </h3>

                <div className="text-sm text-muted">
                  Strategy:{' '}
                  {analysis.strategy ||
                    'Not analyzed yet'}
                </div>
              </div>

              <div className="flex items-center gap-1">
                <Badge
                  variant={statusVariant(
                    analysis.status,
                  )}
                >
                  {analysis.status}
                </Badge>

                <Button
                  loading={analyzing}
                  onClick={analyze}
                >
                  {analysis.status ===
                  'Completed'
                    ? 'Analyze Again'
                    : 'Analyze Resume'}
                </Button>
              </div>
            </div>
          </Card>

          {analysis.status ===
            'Completed' && (
            <>
              <div className="grid grid-4 mb-3">
                <Card>
                  <div className="stat-value">
                    {analysis.wordCount}
                  </div>
                  <div className="stat-label">
                    Extracted Words
                  </div>
                </Card>

                <Card>
                  <div className="stat-value">
                    {
                      analysis
                        .extractedSkills
                        .length
                    }
                  </div>
                  <div className="stat-label">
                    Skills Detected
                  </div>
                </Card>

                <Card>
                  <div className="stat-value">
                    {analysis
                      .suggestedYearsOfExperience ??
                      '—'}
                  </div>
                  <div className="stat-label">
                    Suggested Experience
                  </div>
                </Card>

                <Card>
                  <div className="stat-value">
                    {
                      analysis
                        .educationSignals
                        .length
                    }
                  </div>
                  <div className="stat-label">
                    Education Signals
                  </div>
                </Card>
              </div>

              <div className="grid grid-2 mb-3">
                <Card title="Detected Contact Information">
                  <div className="mb-2">
                    <div className="text-sm text-muted">
                      Email
                    </div>
                    <strong>
                      {analysis.extractedEmail ||
                        'Not detected'}
                    </strong>
                  </div>

                  <div>
                    <div className="text-sm text-muted">
                      Phone
                    </div>
                    <strong>
                      {analysis.extractedPhone ||
                        'Not detected'}
                    </strong>
                  </div>
                </Card>

                <Card
                  title="Detected Skills"
                  action={
                    analysis.extractedSkills
                      .length > 0 && (
                      <Button
                        size="sm"
                        loading={applying}
                        onClick={applySkills}
                      >
                        Apply Selected
                      </Button>
                    )
                  }
                >
                  {analysis.extractedSkills
                    .length === 0 ? (
                    <p className="text-muted">
                      No known skills were
                      detected.
                    </p>
                  ) : (
                    <div className="grid grid-2">
                      {analysis.extractedSkills.map(
                        (skill) => (
                          <label
                            key={skill}
                            className="flex items-center gap-1"
                            style={{
                              padding:
                                '0.55rem',
                              border:
                                '1px solid var(--color-border)',
                              borderRadius:
                                'var(--radius-md)',
                              cursor:
                                'pointer',
                            }}
                          >
                            <input
                              type="checkbox"
                              checked={selectedSkills.includes(
                                skill,
                              )}
                              onChange={() =>
                                toggleSkill(
                                  skill,
                                )
                              }
                            />

                            <span>
                              {skill}
                            </span>
                          </label>
                        ),
                      )}
                    </div>
                  )}
                </Card>
              </div>

              <div className="grid grid-2 mb-3">
                <Card title="Education Signals">
                  {analysis
                    .educationSignals
                    .length === 0 ? (
                    <p className="text-muted">
                      No clear education
                      signal was detected.
                    </p>
                  ) : (
                    <ul>
                      {analysis.educationSignals.map(
                        (signal) => (
                          <li key={signal}>
                            {signal}
                          </li>
                        ),
                      )}
                    </ul>
                  )}
                </Card>

                <Card title="Experience Signals">
                  {analysis
                    .experienceSignals
                    .length === 0 ? (
                    <p className="text-muted">
                      No clear experience
                      signal was detected.
                    </p>
                  ) : (
                    <ul>
                      {analysis.experienceSignals.map(
                        (signal) => (
                          <li key={signal}>
                            {signal}
                          </li>
                        ),
                      )}
                    </ul>
                  )}
                </Card>
              </div>

              {analysis.warnings.length >
                0 && (
                <Card
                  title="Analysis Warnings"
                  className="mb-3"
                >
                  {analysis.warnings.map(
                    (warning) => (
                      <Alert
                        key={warning}
                        variant="warning"
                      >
                        {warning}
                      </Alert>
                    ),
                  )}
                </Card>
              )}

              <Card title="Extracted Text Preview">
                <pre
                  style={{
                    whiteSpace:
                      'pre-wrap',
                    fontFamily:
                      'inherit',
                    fontSize:
                      '0.85rem',
                    maxHeight: 350,
                    overflowY: 'auto',
                  }}
                >
                  {analysis.textPreview ||
                    'No preview available.'}
                </pre>
              </Card>
            </>
          )}
        </>
      )}
    </div>
  );
}
