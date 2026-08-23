import { RiArrowLeftSLine, RiArrowRightSLine } from 'react-icons/ri'

interface PaginationProps {
  page: number
  totalPages: number
  onChange: (page: number) => void
}

export function Pagination({ page, totalPages, onChange }: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <nav className="mt-12 flex items-center justify-center gap-2">
      <button
        type="button"
        onClick={() => onChange(page - 1)}
        disabled={page === 1}
        aria-label="Página anterior"
        className="flex size-9 cursor-pointer items-center justify-center border border-border text-foreground transition-colors hover:border-foreground disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:border-border"
      >
        <RiArrowLeftSLine className="size-4" />
      </button>

      {Array.from({ length: totalPages }, (_, index) => index + 1).map((pageNumber) => (
        <button
          key={pageNumber}
          type="button"
          onClick={() => onChange(pageNumber)}
          aria-current={pageNumber === page ? 'page' : undefined}
          className={`flex size-9 cursor-pointer items-center justify-center border text-sm transition-colors ${
            pageNumber === page
              ? 'border-foreground text-foreground'
              : 'border-border text-muted hover:border-foreground hover:text-foreground'
          }`}
        >
          {pageNumber}
        </button>
      ))}

      <button
        type="button"
        onClick={() => onChange(page + 1)}
        disabled={page === totalPages}
        aria-label="Próxima página"
        className="flex size-9 cursor-pointer items-center justify-center border border-border text-foreground transition-colors hover:border-foreground disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:border-border"
      >
        <RiArrowRightSLine className="size-4" />
      </button>
    </nav>
  )
}
