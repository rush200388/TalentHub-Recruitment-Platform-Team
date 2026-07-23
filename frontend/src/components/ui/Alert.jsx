export function Alert({ variant = 'info', children, onClose }) {
  return (
    <div className={`alert alert-${variant}`}>
      <span style={{ flex: 1 }}>{children}</span>
      {onClose && (
        <button onClick={onClose} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.1rem', color: 'inherit' }}>&times;</button>
      )}
    </div>
  );
}

export function EmptyState({ title = 'Nothing here yet', message, action }) {
  return (
    <div className="empty-state">
      <h3>{title}</h3>
      {message && <p>{message}</p>}
      {action && <div className="mt-2">{action}</div>}
    </div>
  );
}
