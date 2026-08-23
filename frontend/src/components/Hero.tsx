export function Hero() {
  return (
    <section className="bg-moon-100">
      <div className="mx-auto flex max-w-6xl flex-col items-center px-6 py-24 text-center">
        <p className="text-xs uppercase tracking-[0.3em] text-muted">Coleção atual</p>
        <h1 className="mt-4 max-w-2xl text-4xl font-light leading-tight text-foreground md:text-5xl">
          Joias para os momentos que não se repetem
        </h1>
        <p className="mt-4 max-w-md text-sm text-muted">
          Peças selecionadas com cuidado para alianças, anéis de noivado e presentes que ficam.
        </p>
        <a
          href="#"
          className="mt-8 border border-foreground px-8 py-3 text-sm tracking-wide text-foreground transition-colors hover:border-accent hover:text-accent"
        >
          Ver coleção
        </a>
      </div>
    </section>
  )
}
