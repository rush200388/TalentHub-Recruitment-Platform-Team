import {
  useMemo,
  useState,
} from 'react';
import { useAsync } from '../../hooks/useAsync';
import {
  departmentService,
  organizationService,
  userService,
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
import {
  Input,
  Select,
} from '../../components/ui/FormField';
import Avatar from '../../components/ui/Avatar';
import {
  ROLES,
  ROLE_OPTIONS,
} from '../../context/AuthContext';
import {
  limits,
  validateName,
  validatePhone,
} from '../../utils/validation';

const emptyUser = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  role: ROLES.CANDIDATE,
  isActive: true,
  organizationId: '',
  departmentId: '',
  jobTitle: '',
  phone: '',
};

function requiresOrganization(role) {
  return [
    ROLES.RECRUITER,
    ROLES.HIRING_MANAGER,
  ].includes(role);
}

export default function UsersPage() {
  const {
    data: users,
    loading,
    error,
    reload,
  } = useAsync(
    () => userService.list(),
    [],
  );

  const {
    data: organizations,
  } = useAsync(
    () => organizationService.list(),
    [],
  );

  const {
    data: departments,
  } = useAsync(
    () => departmentService.list(),
    [],
  );

  const [modalOpen, setModalOpen] =
    useState(false);
  const [editing, setEditing] =
    useState(null);
  const [form, setForm] =
    useState(emptyUser);
  const [errors, setErrors] =
    useState({});
  const [saving, setSaving] =
    useState(false);
  const [statusTarget, setStatusTarget] =
    useState(null);
  const [search, setSearch] =
    useState('');
  const [toast, setToast] =
    useState('');
  const [actionError, setActionError] =
    useState('');

  const filtered = (users || []).filter(
    (user) =>
      !search ||
      user.name
        .toLowerCase()
        .includes(
          search.toLowerCase(),
        ) ||
      user.email
        .toLowerCase()
        .includes(
          search.toLowerCase(),
        ),
  );

  const availableDepartments =
    useMemo(() => {
      if (!form.organizationId) {
        return [];
      }

      return (departments || []).filter(
        (department) =>
          department.organizationId ===
          form.organizationId,
      );
    }, [
      departments,
      form.organizationId,
    ]);

  const openCreate = () => {
    setEditing(null);
    setForm(emptyUser);
    setErrors({});
    setActionError('');
    setModalOpen(true);
  };

  const openEdit = (user) => {
    setEditing(user);

    setForm({
      firstName:
        user.firstName || '',
      lastName:
        user.lastName || '',
      email: user.email || '',
      password: '',
      role:
        user.primaryRole ||
        ROLES.CANDIDATE,
      isActive: user.isActive,
      organizationId:
        user.organizationId || '',
      departmentId:
        user.departmentId || '',
      jobTitle:
        user.jobTitle || '',
      phone: user.phone || '',
    });

    setErrors({});
    setActionError('');
    setModalOpen(true);
  };

  const validate = () => {
    const nextErrors = {};

    const firstNameError =
      validateName(
        form.firstName,
        'First name',
      );

    const lastNameError =
      validateName(
        form.lastName,
        'Last name',
      );

    if (firstNameError) {
      nextErrors.firstName =
        firstNameError;
    }

    if (lastNameError) {
      nextErrors.lastName =
        lastNameError;
    }

    if (
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(
        form.email,
      )
    ) {
      nextErrors.email =
        'Enter a valid email';
    }

    if (
      !editing &&
      form.password.length < 8
    ) {
      nextErrors.password =
        'Password must contain at least 8 characters';
    }

    const phoneError =
      validatePhone(form.phone);

    if (phoneError) {
      nextErrors.phone = phoneError;
    }

    if (
      requiresOrganization(
        form.role,
      ) &&
      !form.organizationId
    ) {
      nextErrors.organizationId =
        'Organization is required for this role';
    }

    if (
      form.jobTitle.length >
      limits.title
    ) {
      nextErrors.jobTitle =
        'Job title cannot exceed 120 characters';
    }

    setErrors(nextErrors);

    return (
      Object.keys(nextErrors).length ===
      0
    );
  };

  const onSave = async () => {
    if (!validate()) return;

    setSaving(true);
    setActionError('');

    try {
      const common = {
        firstName:
          form.firstName.trim(),
        lastName:
          form.lastName.trim(),
        email:
          form.email
            .trim()
            .toLowerCase(),
        organizationId:
          requiresOrganization(
            form.role,
          )
            ? form.organizationId ||
              null
            : null,
        departmentId:
          requiresOrganization(
            form.role,
          )
            ? form.departmentId ||
              null
            : null,
        jobTitle:
          form.jobTitle.trim() ||
          null,
        phone:
          form.phone.trim() || null,
      };

      if (editing) {
        await userService.update(
          editing.id,
          common,
        );

        await userService.updateRoles(
          editing.id,
          {
            roles: [form.role],
            organizationId:
              common.organizationId,
            departmentId:
              common.departmentId,
            jobTitle:
              common.jobTitle,
          },
        );

        if (
          editing.isActive !==
          form.isActive
        ) {
          await userService
            .updateStatus(
              editing.id,
              form.isActive,
            );
        }
      } else {
        await userService.create({
          ...common,
          password:
            form.password,
          roles: [form.role],
        });
      }

      setModalOpen(false);
      await reload();

      setToast(
        editing
          ? 'User updated successfully.'
          : 'User created successfully.',
      );

      setTimeout(
        () => setToast(''),
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

  const changeStatus = async () => {
    if (!statusTarget) return;

    setSaving(true);
    setActionError('');

    try {
      await userService.updateStatus(
        statusTarget.id,
        !statusTarget.isActive,
      );

      setStatusTarget(null);
      await reload();

      setToast(
        statusTarget.isActive
          ? 'User suspended.'
          : 'User activated.',
      );

      setTimeout(
        () => setToast(''),
        3000,
      );
    } catch (statusError) {
      setStatusTarget(null);
      setActionError(
        statusError.message,
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
            Users &amp; Roles
          </h1>

          <p
            className="text-sm text-muted"
            style={{ margin: 0 }}
          >
            Manage real Identity accounts,
            roles, and assignments
          </p>
        </div>

        <Button onClick={openCreate}>
          + Add User
        </Button>
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

      <Card className="mb-3">
        <Input
          placeholder="Search by name or email..."
          value={search}
          onChange={(event) =>
            setSearch(
              event.target.value,
            )
          }
        />
      </Card>

      <Card>
        {loading ? (
          <Spinner />
        ) : error ? (
          <Alert variant="error">
            {error}
          </Alert>
        ) : null}

        {!loading &&
          !error &&
          filtered.length === 0 && (
          <EmptyState title="No users found" />
        )}

        {!loading &&
          !error &&
          filtered.length > 0 && (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Role</th>
                  <th>Assignment</th>
                  <th>Last Login</th>
                  <th>Security</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {filtered.map(
                  (user) => (
                    <tr key={user.id}>
                      <td>
                        <div className="flex items-center gap-1">
                          <Avatar
                            name={user.name}
                            size="sm"
                          />

                          <div>
                            <div
                              style={{
                                fontWeight:
                                  600,
                              }}
                            >
                              {user.name}
                            </div>

                            <div className="text-sm text-muted">
                              {user.email}
                            </div>
                          </div>
                        </div>
                      </td>

                      <td>
                        <Badge variant="info">
                          {ROLE_OPTIONS.find(
                            (role) =>
                              role.value ===
                              user.primaryRole,
                          )?.label ||
                            user.primaryRole}
                        </Badge>
                      </td>

                      <td>
                        <div>
                          {user.organization}
                        </div>

                        <div className="text-sm text-muted">
                          {user.department}
                        </div>
                      </td>

                      <td className="text-sm">
                        {user.lastLoginAtUtc
                          ? new Date(
                              user.lastLoginAtUtc,
                            ).toLocaleString()
                          : 'Never'}
                      </td>

                      <td>
                        {user.isLockedOut ? (
                          <Badge variant="error">
                            Locked
                          </Badge>
                        ) : user.failedLoginAttempts >
                          0 ? (
                          <Badge variant="warning">
                            {
                              user.failedLoginAttempts
                            }{' '}
                            failed
                          </Badge>
                        ) : (
                          <Badge variant="success">
                            Normal
                          </Badge>
                        )}
                      </td>

                      <td>
                        <Badge
                          variant={
                            user.isActive
                              ? 'success'
                              : 'error'
                          }
                        >
                          {user.status}
                        </Badge>
                      </td>

                      <td>
                        <div className="actions">
                          <Button
                            size="sm"
                            variant="secondary"
                            onClick={() =>
                              openEdit(user)
                            }
                          >
                            Edit
                          </Button>

                          <Button
                            size="sm"
                            variant={
                              user.isActive
                                ? 'danger'
                                : 'primary'
                            }
                            onClick={() =>
                              setStatusTarget(
                                user,
                              )
                            }
                          >
                            {user.isActive
                              ? 'Suspend'
                              : 'Activate'}
                          </Button>
                        </div>
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
        open={modalOpen}
        onClose={() =>
          setModalOpen(false)
        }
        title={
          editing
            ? 'Edit User'
            : 'Add User'
        }
        size="lg"
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setModalOpen(false)
              }
            >
              Cancel
            </Button>

            <Button
              loading={saving}
              onClick={onSave}
            >
              {editing
                ? 'Save Changes'
                : 'Create User'}
            </Button>
          </>
        }
      >
        {actionError && (
          <Alert variant="error">
            {actionError}
          </Alert>
        )}

        <div className="form-row">
          <Input
            label="First name"
            required
            maxLength={limits.name}
            value={form.firstName}
            onChange={(event) =>
              setForm({
                ...form,
                firstName:
                  event.target.value,
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
                lastName:
                  event.target.value,
              })
            }
            error={errors.lastName}
          />
        </div>

        <div className="form-row">
          <Input
            label="Email"
            type="email"
            required
            maxLength={256}
            value={form.email}
            onChange={(event) =>
              setForm({
                ...form,
                email:
                  event.target.value,
              })
            }
            error={errors.email}
          />

          {!editing && (
            <Input
              label="Temporary password"
              type="password"
              required
              maxLength={100}
              value={form.password}
              onChange={(event) =>
                setForm({
                  ...form,
                  password:
                    event.target.value,
                })
              }
              error={errors.password}
              hint="At least 8 characters with uppercase, lowercase, and a number"
            />
          )}
        </div>

        <div className="form-row">
          <Select
            label="Role"
            required
            value={form.role}
            onChange={(event) =>
              setForm({
                ...form,
                role:
                  event.target.value,
                organizationId: '',
                departmentId: '',
              })
            }
          >
            {ROLE_OPTIONS.map(
              (role) => (
                <option
                  key={role.value}
                  value={role.value}
                >
                  {role.label}
                </option>
              ),
            )}
          </Select>

          <Select
            label="Status"
            value={
              form.isActive
                ? 'Active'
                : 'Suspended'
            }
            onChange={(event) =>
              setForm({
                ...form,
                isActive:
                  event.target.value ===
                  'Active',
              })
            }
          >
            <option value="Active">
              Active
            </option>
            <option value="Suspended">
              Suspended
            </option>
          </Select>
        </div>

        {requiresOrganization(
          form.role,
        ) && (
          <div className="form-row">
            <Select
              label="Organization"
              required
              value={
                form.organizationId
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  organizationId:
                    event.target.value,
                  departmentId: '',
                })
              }
              error={
                errors.organizationId
              }
            >
              <option value="">
                Select organization...
              </option>

              {(organizations || []).map(
                (organization) => (
                  <option
                    key={
                      organization.id
                    }
                    value={
                      organization.id
                    }
                  >
                    {
                      organization.name
                    }
                  </option>
                ),
              )}
            </Select>

            <Select
              label="Department"
              value={
                form.departmentId
              }
              onChange={(event) =>
                setForm({
                  ...form,
                  departmentId:
                    event.target.value,
                })
              }
            >
              <option value="">
                Not assigned
              </option>

              {availableDepartments.map(
                (department) => (
                  <option
                    key={
                      department.id
                    }
                    value={
                      department.id
                    }
                  >
                    {
                      department.name
                    }
                  </option>
                ),
              )}
            </Select>
          </div>
        )}

        <div className="form-row">
          <Input
            label="Job title"
            maxLength={limits.title}
            value={form.jobTitle}
            onChange={(event) =>
              setForm({
                ...form,
                jobTitle:
                  event.target.value,
              })
            }
            error={errors.jobTitle}
          />

          <Input
            label="Phone"
            inputMode="numeric"
            pattern="[0-9]{10}"
            maxLength={10}
            value={form.phone}
            onChange={(event) =>
              setForm({
                ...form,
                phone:
                  event.target.value
                    .replace(/\D/g, '')
                    .slice(0, 10),
              })
            }
            error={errors.phone}
          />
        </div>
      </Modal>

      <Modal
        open={Boolean(statusTarget)}
        onClose={() =>
          setStatusTarget(null)
        }
        title={
          statusTarget?.isActive
            ? 'Suspend User?'
            : 'Activate User?'
        }
        footer={
          <>
            <Button
              variant="secondary"
              onClick={() =>
                setStatusTarget(null)
              }
            >
              Cancel
            </Button>

            <Button
              variant={
                statusTarget?.isActive
                  ? 'danger'
                  : 'primary'
              }
              loading={saving}
              onClick={changeStatus}
            >
              {statusTarget?.isActive
                ? 'Suspend'
                : 'Activate'}
            </Button>
          </>
        }
      >
        <p>
          {statusTarget?.isActive
            ? 'Suspend'
            : 'Activate'}{' '}
          <strong>
            {statusTarget?.name}
          </strong>
          ?
        </p>
      </Modal>
    </div>
  );
}
