import { useMemo, useState } from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  departmentService,
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
import { limits } from '../../utils/validation';

const emptyDepartment = {
  organizationId: '',
  name: '',
  description: '',
  isActive: true,
};

export default function DepartmentsPage() {
  const {
    data: departments,
    loading,
    error,
    setData,
  } = useAsync(() => departmentService.list(), []);

  const {
    data: organizations,
    loading: organizationsLoading,
  } = useAsync(() => organizationService.list(), []);

  const [filterOrganization, setFilterOrganization] =
    useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyDepartment);
  const [saving, setSaving] = useState(false);
  const [confirmDelete, setConfirmDelete] = useState(null);
  const [feedback, setFeedback] = useState('');
  const [actionError, setActionError] = useState('');

  const visibleDepartments = useMemo(() => {
    if (!filterOrganization) return departments || [];

    return (departments || []).filter(
      (department) =>
        department.organizationId === filterOrganization,
    );
  }, [departments, filterOrganization]);

  const openCreate = () => {
    setEditing(null);
    setForm({
      ...emptyDepartment,
      organizationId: organizations?.[0]?.id || '',
    });
    setActionError('');
    setModalOpen(true);
  };

  const openEdit = (department) => {
    setEditing(department);
    setForm({
      organizationId: department.organizationId,
      name: department.name,
      description: department.description || '',
      isActive: department.isActive,
    });
    setActionError('');
    setModalOpen(true);
  };

  const onSave = async () => {
    const name = form.name.trim();

    if (!form.organizationId) {
      setActionError(
        'Organization is required.',
      );
      return;
    }

    if (
      name.length < 2 ||
      name.length > limits.departmentName
    ) {
      setActionError(
        'Department name must contain 2–100 characters.',
      );
      return;
    }

    if (form.description.length > 1000) {
      setActionError(
        'Description cannot exceed 1000 characters.',
      );
      return;
    }

    setSaving(true);
    setActionError('');

    try {
      const payload = {
        organizationId: form.organizationId,
        name: form.name.trim(),
        description: form.description.trim() || null,
        isActive: form.isActive,
      };

      if (editing) {
        const updated = await departmentService.update(
          editing.id,
          payload,
        );

        setData(
          (departments || []).map((department) =>
            department.id === editing.id
              ? updated
              : department,
          ),
        );
      } else {
        const created =
          await departmentService.create(payload);

        setData([created, ...(departments || [])]);
      }

      setModalOpen(false);
      setFeedback(
        editing
          ? 'Department updated.'
          : 'Department created.',
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
      await departmentService.remove(confirmDelete.id);

      setData(
        (departments || []).filter(
          (department) =>
            department.id !== confirmDelete.id,
        ),
      );

      setConfirmDelete(null);
      setFeedback('Department deleted.');
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
          <h1 style={{ margin: 0 }}>Departments</h1>
          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Manage organization departments and open roles
          </p>
        </div>

        <Button
          onClick={openCreate}
          disabled={organizationsLoading}
        >
          + Add Department
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

      <Card className="mb-3">
        <Select
          label="Filter by organization"
          value={filterOrganization}
          onChange={(event) =>
            setFilterOrganization(event.target.value)
          }
        >
          <option value="">All organizations</option>
          {(organizations || []).map((organization) => (
            <option
              key={organization.id}
              value={organization.id}
            >
              {organization.name}
            </option>
          ))}
        </Select>
      </Card>

      {loading ? (
        <Spinner />
      ) : error ? (
        <Alert variant="error">{error}</Alert>
      ) : null}

      {!loading &&
        !error &&
        visibleDepartments.length === 0 && (
          <EmptyState title="No departments" />
        )}

      {!loading &&
        !error &&
        visibleDepartments.length > 0 && (
          <div className="grid grid-3">
            {visibleDepartments.map((department) => (
              <Card key={department.id}>
                <div className="flex items-center justify-between mb-1">
                  <h3 style={{ margin: 0 }}>
                    {department.name}
                  </h3>

                  <Badge
                    variant={
                      department.isActive
                        ? 'success'
                        : 'neutral'
                    }
                  >
                    {department.isActive
                      ? 'Active'
                      : 'Inactive'}
                  </Badge>
                </div>

                <div className="text-sm text-muted mb-2">
                  {department.organizationName}
                </div>

                <p className="text-sm">
                  {department.description ||
                    'No description provided.'}
                </p>

                <div className="mb-2">
                  <strong>{department.openRoles}</strong>{' '}
                  <span className="text-sm text-muted">
                    open roles
                  </span>
                </div>

                <div className="actions">
                  <Button
                    size="sm"
                    variant="secondary"
                    onClick={() => openEdit(department)}
                  >
                    Edit
                  </Button>

                  <Button
                    size="sm"
                    variant="danger"
                    onClick={() =>
                      setConfirmDelete(department)
                    }
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
        title={
          editing
            ? 'Edit Department'
            : 'Add Department'
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

        <Select
          label="Organization"
          required
          value={form.organizationId}
          onChange={(event) =>
            setForm({
              ...form,
              organizationId: event.target.value,
            })
          }
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

        <Input
          label="Department name"
          required
          maxLength={limits.departmentName}
          value={form.name}
          onChange={(event) =>
            setForm({
              ...form,
              name: event.target.value,
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
          Active department
        </label>
      </Modal>

      <Modal
        open={Boolean(confirmDelete)}
        onClose={() => setConfirmDelete(null)}
        title="Delete Department?"
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
          Departments with jobs cannot be deleted.
        </p>
      </Modal>
    </div>
  );
}
