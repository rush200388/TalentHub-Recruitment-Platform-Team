import { useState } from 'react';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Badge from '../components/ui/Badge';
import { Input } from '../components/ui/FormField';
import { Alert } from '../components/ui/Alert';
import Avatar from '../components/ui/Avatar';
import { useAuth, ROLE_OPTIONS } from '../context/AuthContext';

export default function ProfilePage() {
  const { user } = useAuth();
  const [form, setForm] = useState({ name: user?.name || '', email: user?.email || '', phone: user?.phone || '', location: user?.location || '' });
  const [saved, setSaved] = useState(false);

  const onSave = (e) => { e.preventDefault(); setSaved(true); setTimeout(() => setSaved(false), 3000); };
  const roleLabel = ROLE_OPTIONS.find((r) => r.value === user?.role)?.label || user?.role;

  return (
    <div className="grid grid-2">
      <Card title="Account">
        <div className="flex items-center gap-2 mb-3">
          <Avatar name={user?.name} size="lg" />
          <div>
            <h2 style={{ margin: 0 }}>{user?.name}</h2>
            <div className="text-sm text-muted">{user?.email}</div>
            <div className="mt-1"><Badge variant="info">{roleLabel}</Badge></div>
          </div>
        </div>
        {saved && <Alert variant="success">Profile updated!</Alert>}
        <form onSubmit={onSave}>
          <Input label="Full name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          <div className="form-row">
            <Input label="Phone" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
            <Input label="Location" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
          </div>
          <Button type="submit">Save Changes</Button>
        </form>
      </Card>

      <Card title="Security">
        <h4>Change Password</h4>
        <p className="text-sm text-muted">Password changes will be handled by the ASP.NET API once connected.</p>
        <Button variant="secondary" disabled>Change Password</Button>
        <div className="mt-3">
          <h4>Session</h4>
          <p className="text-sm text-muted">You are signed in as <strong>{roleLabel}</strong>.</p>
        </div>
      </Card>
    </div>
  );
}
