export function ScoreBar({ score }) {
  const variant = score >= 80 ? 'high' : score >= 60 ? 'mid' : 'low';
  return (
    <div className="flex items-center gap-2">
      <div className="score-bar flex-1">
        <div className={`score-bar-fill ${variant}`} style={{ width: `${score}%` }} />
      </div>
      <span className="text-sm" style={{ fontWeight: 700, minWidth: 32 }}>{score}</span>
    </div>
  );
}
