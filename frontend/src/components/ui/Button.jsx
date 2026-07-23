export default function Button({ children, variant = 'primary', size, loading, disabled, className = '', ...props }) {
  const classes = ['btn', `btn-${variant}`, size === 'sm' ? 'btn-sm' : '', className].filter(Boolean).join(' ');
  return (
    <button className={classes} disabled={disabled || loading} {...props}>
      {loading && <span className="spinner" />}
      {children}
    </button>
  );
}
