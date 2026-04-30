export function MaterialIcon({
  name,
  size = 22,
  className,
  title,
}: {
  name: string
  size?: number
  className?: string
  title?: string
}) {
  return (
    <span
      className={`material-symbols-rounded ${className ?? ''}`.trim()}
      aria-hidden={title ? undefined : true}
      title={title}
      style={{ fontSize: size, lineHeight: 1 }}
    >
      {name}
    </span>
  )
}

