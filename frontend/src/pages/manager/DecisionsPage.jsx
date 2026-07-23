import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  applicationService,
  decisionService,
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
  Textarea,
} from '../../components/ui/FormField';

function decisionVariant(decision) {
  if (decision === 'Hired') {
    return 'success';
  }

  if (decision === 'Rejected') {
    return 'error';
  }

  return 'warning';
}

export default function DecisionsPage() {
  const {
    data: applications,
    loading,
    error,
    reload: reloadApplications,
  } = useAsync(
    () => applicationService.list(),
    [],
  );

  const {
    data: decisions,
    reload: reloadDecisions,
  } = useAsync(
    () => decisionService.list(),
    [],
  );

  const [selected, setSelected] =
    useState(null);
  const [notes, setNotes] =
    useState('');
  const [saving, setSaving] =
    useState(false);
  const [toast, setToast] =
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
          ) ||
          application.stage ===
            'On Hold',
      ),
    [applications],
  );

  const decisionMap = useMemo(
    () =>
      new Map(
        (decisions || []).map(
          (decision) => [
            decision.applicationId,
            decision,
          ],
        ),
      ),
    [decisions],
  );

  const openDecision = (
    application,
  ) => {
    const existing =
      decisionMap.get(
        application.id,
      );

    setSelected(application);
    setNotes(
      existing?.notes || '',
    );
    setActionError('');
  };

  const onDecide = async (
    decision,
  ) => {
    if (!selected) return;

    if (notes.length > 3000) {
      setActionError(
        'Decision notes cannot exceed 3000 characters.',
      );
      return;
    }

    setSaving(true);
    setActionError('');

    try {
      const saved =
        await decisionService.save({
          jobApplicationId:
            selected.id,
          decision,
          notes:
            notes.trim() || null,
        });

      setSelected(null);

      await Promise.all([
        reloadApplications(),
        reloadDecisions(),
      ]);

      setToast(
        `Decision saved: ${saved.decision}.`,
      );

      setTimeout(
        () => setToast(''),
        3000,
      );
    } catch (decisionError) {
      setActionError(
        decisionError.message,
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
            Hiring Decisions
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Record final decisions and
            notify candidates
          </p>
        </div>
      </div>

      {toast && (
        <Alert
          variant="success"
          onClose={() => setToast('')}
        >
          {toast}
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
            title="No candidates awaiting a decision"
            message="Shortlisted or interviewed candidates will appear here."
          />
        )}

      {!loading &&
        !error &&
        eligible.length > 0 && (
          <div className="grid grid-2 mb-3">
            {eligible.map(
              (application) => {
                const existing =
                  decisionMap.get(
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

                      {existing && (
                        <Badge
                          variant={decisionVariant(
                            existing.decision,
                          )}
                        >
                          {
                            existing.decision
                          }
                        </Badge>
                      )}
                    </div>

                    <div className="text-sm text-secondary mb-1">
                      Role:{' '}
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

                    {existing
                      ?.evaluationScore !=
                      null && (
                      <div className="mb-2">
                        <div className="text-sm text-muted mb-1">
                          Evaluation Score
                        </div>

                        <ScoreBar
                          score={Math.round(
                            existing.evaluationScore,
                          )}
                        />
                      </div>
                    )}

                    <Button
                      onClick={() =>
                        openDecision(
                          application,
                        )
                      }
                    >
                      {existing
                        ? 'Update Decision'
                        : 'Make Decision'}
                    </Button>
                  </Card>
                );
              },
            )}
          </div>
        )}

      <Card title="Decision History">
        {(decisions || []).length ===
        0 ? (
          <p className="text-muted">
            No hiring decisions recorded.
          </p>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Candidate</th>
                  <th>Job</th>
                  <th>Decision</th>
                  <th>Manager</th>
                  <th>Date</th>
                </tr>
              </thead>

              <tbody>
                {(decisions || []).map(
                  (decision) => (
                    <tr key={decision.id}>
                      <td>
                        <strong>
                          {
                            decision.candidateName
                          }
                        </strong>
                      </td>

                      <td>
                        {decision.jobTitle}
                      </td>

                      <td>
                        <Badge
                          variant={decisionVariant(
                            decision.decision,
                          )}
                        >
                          {
                            decision.decision
                          }
                        </Badge>
                      </td>

                      <td>
                        {
                          decision.decidedByName
                        }
                      </td>

                      <td>
                        {decision.decidedAtUtc
                          ? new Date(
                              decision.decidedAtUtc,
                            ).toLocaleDateString()
                          : 'Pending'}
                      </td>
                    </tr>
                  ),
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <Modal
        open={Boolean(selected)}
        onClose={() =>
          setSelected(null)
        }
        title="Hiring Decision"
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
              variant="secondary"
              loading={saving}
              onClick={() =>
                onDecide('On Hold')
              }
            >
              On Hold
            </Button>

            <Button
              variant="danger"
              loading={saving}
              onClick={() =>
                onDecide('Reject')
              }
            >
              Reject
            </Button>

            <Button
              loading={saving}
              onClick={() =>
                onDecide('Hire')
              }
            >
              Hire
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

            <div className="flex items-center gap-2 mb-2">
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

            <Textarea
              label="Decision notes"
              maxLength={3000}
              value={notes}
              onChange={(event) =>
                setNotes(
                  event.target.value,
                )
              }
              hint={`${notes.length}/3000 characters`}
              placeholder="Explain the reason for the decision..."
            />
          </div>
        )}
      </Modal>
    </div>
  );
}
