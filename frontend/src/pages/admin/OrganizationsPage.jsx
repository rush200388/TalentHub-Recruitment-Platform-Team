import { useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
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
  Textarea,
} from '../../components/ui/FormField';
import {
  limits,
  validateOptionalUrl,
} from '../../utils/validation';

const emptyOrganization = {
  name: '',
  description: '',
  website: '',
  isActive: true,
};

export default function OrganizationsPage() {
  const {
    data: organizations,
    loading,
    error,
    setData,
  } = useAsync(() => organizationService.list(), []);

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyOrganization);
  const [saving, setSaving] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(null);
  const [feedback, setFeedback] = useState('');
  const [actionError, setActionError] = useState('');

  const openCreate = () => {
    setEditing(null);
    setForm(emptyOrganization);
    setActionError('');
    setModalOpen(true);
  };

  const openEdit = (organization) => {
    setEditing(organization);
    setForm({
      name: organization.name,
      description: organization.description || '',
      website: organization.website || '',
      isActive: organization.isActive,
    });
    setActionError('');
    setModalOpen(true);
  };

  const onSave = async () => {
    const name = form.name.trim();
    const websiteError =
      validateOptionalUrl(
        form.website,
        'Website',
      );

    if (
      name.length < 2 ||
      name.length > limits.organizationName
    ) {
      setActionError(
        'Organization name must contain 2–150 characters.',
      );
      return;
    }

    if (websiteError) {
      setActionError(websiteError);
      return;
    }

    if (
      form.description.length > 1000
    ) {
      setActionError(
        'Description cannot exceed 1000 characters.',
      );
      return;
    }

    setSaving(true);
    setActionError('');

    try {
      const payload = {
        name: form.name.trim(),
        description: form.description.trim() || null,
        website: form.website.trim() || null,
        isActive: form.isActive,
      };

      if (editing) {
        const updated = await organizationService.update(
          editing.id,
          payload,
        );

        setData(
          (organizations || []).map((organization) =>
            organization.id === editing.id
              ? updated
              : organization,
          ),
        );
      } else {
        const created =
          await organizationService.create(payload);

        setData([created, ...(organizations || [])]);
      }

      setModalOpen(false);
      setFeedback(
        editing
          ? 'Organization updated.'
          : 'Organization created.',
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
      await organizationService.remove(confirmDelete.id);

      setData(
        (organizations || []).filter(
          (organization) =>
            organization.id !== confirmDelete.id,
        ),
      );

      setConfirmDelete(null);
      setFeedback('Organization deleted.');
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
          <h1 style={{ margin: 0 }}>Organizations</h1>
          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Manage organizations stored in PostgreSQL
          </p>
        </div>

        <Button onClick={openCreate}>
          + Add Organization
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

      {!loading &&
        !error &&
        (organizations || []).length === 0 && (
          <EmptyState title="No organizations" />
        )}

      {!loading &&
        !error &&
        (organizations || []).length > 0 && (
          <Card>
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Organization</th>
                    <th>Departments</th>
                    <th>Jobs</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>

                <tbody>
                  {(organizations || []).map(
                    (organization) => (
                      <tr key={organization.id}>
                        <td>
                          <strong>{organization.name}</strong>
                          <div className="text-sm text-muted">
                            {organization.website || 'No website'}
                          </div>
                        </td>
                        <td>{organization.departmentCount}</td>
                        <td>{organization.jobCount}</td>
                        <td>
                          <Badge
                            variant={
                              organization.isActive
                                ? 'success'
                                : 'error'
                            }
                          >
                            {organization.isActive
                              ? 'Active'
                              : 'Inactive'}
                          </Badge>
                        </td>
                        <td>
                          <div className="actions">
                            <Button
                              size="sm"
                              variant="secondary"
                              onClick={() =>
                                openEdit(organization)
                              }
                            >
                              Edit
                            </Button>
                            <Button
                              size="sm"
                              variant="danger"
                              onClick={() =>
                                setConfirmDelete(
                                  organization,
                                )
                              }
                            >
                              Delete
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ),
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        )}

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={
          editing
            ? 'Edit Organization'
            : 'Add Organization'
        }
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() => setModalOpen(false)}
            >
              Cancel
            </Button>
            <Button loading={saving} onClick={onSave}>
              {editing ? 'Save' : 'Create'}
            </Button>
          </>
        }
      >
        {actionError && (
          <Alert variant="error">{actionError}</Alert>
        )}

        <Input
          label="Organization name"
          required
          maxLength={limits.organizationName}
          value={form.name}
          onChange={(event) =>
            setForm({
              ...form,
              name: event.target.value,
            })
          }
        />

        <Input
          label="Website"
          type="url"
          maxLength={limits.website}
          value={form.website}
          onChange={(event) =>
            setForm({
              ...form,
              website: event.target.value,
            })
          }
        />

        <Textarea
          label="Description"
          maxLength={1000}
          value={form.description}
          onChange={(event) =>
            setForm({
              ...form,
              description: event.target.value,
            })
          }
        />

        <label className="form-label">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={(event) =>
              setForm({
                ...form,
                isActive: event.target.checked,
              })
            }
          />{' '}
          Active organization
        </label>
      </Modal>

      <Modal
        open={Boolean(confirmDelete)}
        onClose={() => setConfirmDelete(null)}
        title="Delete Organization?"
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
          Delete <strong>{confirmDelete?.name}</strong>?
          Organizations with departments or jobs cannot be deleted.
        </p>
      </Modal>
    </div>
  );
}
