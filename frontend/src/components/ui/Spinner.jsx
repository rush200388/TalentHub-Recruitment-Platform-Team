export default function Spinner({ label, large }) {
  return (
    <div className="loading-overlay">
      <span className={`spinner ${large ? 'spinner-lg' : ''}`} />
      {label && <span>{label}</span>}
    </div>
  );
}
