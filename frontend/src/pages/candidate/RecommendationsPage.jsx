import { useAsync } from '../../hooks/useAsync';
import {
  candidateService,
} from '../../services/services';
import Card from '../../components/ui/Card';
import Badge from '../../components/ui/Badge';
import Button from '../../components/ui/Button';
import Spinner from '../../components/ui/Spinner';
import {
  Alert,
  EmptyState,
} from '../../components/ui/Alert';
import { useNavigate } from 'react-router-dom';
import { ScoreBar } from '../../components/ui/ScoreBar';

export default function RecommendationsPage() {
  const {
    data: recommendations,
    loading,
    error,
  } = useAsync(
    () => candidateService.recommendations(),
    [],
  );

  const navigate = useNavigate();

  return (
    <div>
      <Card
        className="mb-3"
        bodyClassName=""
      >
        <div className="flex items-center gap-2">
          <span
            style={{ fontSize: '1.5rem' }}
          >
            ✨
          </span>

          <div>
            <h3 style={{ margin: 0 }}>
              AI-Powered Recommendations
            </h3>

            <p
              className="text-sm text-muted"
              style={{ margin: 0 }}
            >
              Explainable matching based on
              skills and experience
            </p>
          </div>
        </div>
      </Card>

      {loading ? (
        <Spinner label="Finding best matches..." />
      ) : error ? (
        <Alert variant="error">
          {error}
        </Alert>
      ) : null}

      {!loading &&
        !error &&
        (recommendations || []).length === 0 && (
          <EmptyState
            title="No recommendations yet"
            message="Complete your profile and add skills."
          />
        )}

      {!loading &&
        !error &&
        (recommendations || []).length > 0 && (
          <div className="grid grid-2">
            {(recommendations || []).map(
              (recommendation) => (
                <Card
                  key={recommendation.jobId}
                >
                  <div className="flex items-center justify-between gap-2">
                    <h3 style={{ margin: 0 }}>
                      {recommendation.title}
                    </h3>

                    <Badge
                      variant={
                        recommendation.matchScore >=
                        80
                          ? 'success'
                          : recommendation.matchScore >=
                              60
                            ? 'warning'
                            : 'neutral'
                      }
                    >
                      {Math.round(
                        recommendation.matchScore,
                      )}
                      % match
                    </Badge>
                  </div>

                  <div className="text-secondary text-sm">
                    {recommendation.organization} ·{' '}
                    {recommendation.department} ·{' '}
                    {recommendation.location}
                  </div>

                  <div className="job-meta">
                    <span>
                      💼 {recommendation.type}
                    </span>
                    <span>
                      🏠 {recommendation.workMode}
                    </span>
                    <span>
                      💰 {recommendation.salary}
                    </span>
                  </div>

                  <div className="mb-2">
                    <ScoreBar
                      score={Math.round(
                        recommendation.matchScore,
                      )}
                    />
                  </div>

                  <p className="text-sm">
                    {recommendation.reason}
                  </p>

                  {recommendation
                    .matchedSkills?.length > 0 && (
                    <>
                      <div className="text-sm text-muted mb-1">
                        Matched skills
                      </div>

                      <div className="job-tags mb-2">
                        {recommendation.matchedSkills.map(
                          (skill) => (
                            <Badge
                              key={skill}
                              variant="success"
                            >
                              {skill}
                            </Badge>
                          ),
                        )}
                      </div>
                    </>
                  )}

                  {recommendation
                    .missingSkills?.length > 0 && (
                    <>
                      <div className="text-sm text-muted mb-1">
                        Skills to improve
                      </div>

                      <div className="job-tags mb-2">
                        {recommendation.missingSkills.map(
                          (skill) => (
                            <Badge
                              key={skill}
                              variant="neutral"
                            >
                              {skill}
                            </Badge>
                          ),
                        )}
                      </div>
                    </>
                  )}

                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() =>
                      navigate(
                        `/candidate/jobs/${recommendation.jobId}`,
                      )
                    }
                  >
                    View Job
                  </Button>
                </Card>
              ),
            )}
          </div>
        )}
    </div>
  );
}
