import { type SelectHTMLAttributes, forwardRef } from 'react'

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  error?: string
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, className = '', id, children, ...props },
  ref
) {
  const selectId = id ?? props.name

  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label htmlFor={selectId} className="text-sm font-medium text-slate-700">
          {label}
        </label>
      )}
      <select
        ref={ref}
        id={selectId}
        className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-900 shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500/40 ${
          error ? 'border-red-400 focus:border-red-500' : 'border-slate-300 focus:border-indigo-500'
        } ${className}`}
        {...props}
      >
        {children}
      </select>
      {error && <span className="text-xs font-medium text-red-600">{error}</span>}
    </div>
  )
})
