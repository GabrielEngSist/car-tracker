import type { CSSProperties, ReactNode } from 'react'
import logoUrl from '../assets/cartracker-logo-transparent.png'

type PageTitleProps = {
  children: ReactNode
  style?: CSSProperties
  className?: string
}

/** Page heading (h1) with app logo — use for main titles across screens. */
export function PageTitle({ children, style, className }: PageTitleProps) {
  return (
    <h1 className={className ? `pageTitle ${className}` : 'pageTitle'} style={style}>
      <img src={logoUrl} alt="" className="pageTitleLogo" width={40} height={40} decoding="async" />
      <span className="pageTitleText">{children}</span>
    </h1>
  )
}
