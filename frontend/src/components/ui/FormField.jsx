export function Input({ label, error, required, hint, className = '', ...props }) {
  return (
    <div className="form-group">
      {label && (
        <label className="form-label">
          {label}{required && <span className="req"> *</span>}
        </label>
      )}
      <input className={`form-input ${error ? 'error' : ''} ${className}`} {...props} />
      {error && <div className="form-error">{error}</div>}
      {hint && !error && <div className="form-hint">{hint}</div>}
    </div>
  );
}

export function Select({ label, error, required, children, className = '', ...props }) {
  return (
    <div className="form-group">
      {label && (
        <label className="form-label">
          {label}{required && <span className="req"> *</span>}
        </label>
      )}
      <select className={`form-select ${error ? 'error' : ''} ${className}`} {...props}>
        {children}
      </select>
      {error && <div className="form-error">{error}</div>}
    </div>
  );
}

export function Textarea({ label, error, required, hint, className = '', ...props }) {
  return (
    <div className="form-group">
      {label && (
        <label className="form-label">
          {label}{required && <span className="req"> *</span>}
        </label>
      )}
      <textarea className={`form-textarea ${error ? 'error' : ''} ${className}`} {...props} />
      {error && <div className="form-error">{error}</div>}
      {hint && !error && <div className="form-hint">{hint}</div>}
    </div>
  );
}
