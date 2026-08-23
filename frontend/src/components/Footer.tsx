import { RiInstagramLine, RiFacebookCircleLine, RiWhatsappLine } from 'react-icons/ri'
import { categories } from '../data/categories'

const institutionalLinks = ['Sobre nós', 'Contato', 'Trabalhe conosco', 'Lojas físicas']
const supportLinks = ['Central de ajuda', 'Trocas e devoluções', 'Perguntas frequentes']

function FooterColumn({ title, links }: { title: string; links: readonly string[] }) {
  return (
    <div>
      <h3 className="text-xs uppercase tracking-wider text-muted">{title}</h3>
      <ul className="mt-4 space-y-2">
        {links.map((link) => (
          <li key={link}>
            <a href="#" className="text-sm text-foreground transition-colors hover:text-muted">
              {link}
            </a>
          </li>
        ))}
      </ul>
    </div>
  )
}

export function Footer() {
  return (
    <footer className="border-t border-border bg-surface">
      <div className="mx-auto grid max-w-6xl grid-cols-2 gap-10 px-6 py-16 md:grid-cols-5">
        <div className="col-span-2">
          <p className="text-xl font-light tracking-[0.3em] text-foreground">MOON</p>
          <p className="mt-3 max-w-xs text-sm text-muted">
            Joias selecionadas com cuidado para alianças, anéis de noivado e presentes que ficam.
          </p>
          <div className="mt-6 flex items-center gap-4 text-foreground">
            <a href="#" aria-label="Instagram" className="transition-colors hover:text-muted">
              <RiInstagramLine className="size-5" />
            </a>
            <a href="#" aria-label="Facebook" className="transition-colors hover:text-muted">
              <RiFacebookCircleLine className="size-5" />
            </a>
            <a href="#" aria-label="WhatsApp" className="transition-colors hover:text-muted">
              <RiWhatsappLine className="size-5" />
            </a>
          </div>
        </div>

        <FooterColumn title="Institucional" links={institutionalLinks} />
        <FooterColumn title="Categorias" links={categories} />
        <FooterColumn title="Atendimento" links={supportLinks} />
      </div>

      <div className="border-t border-border px-6 py-6">
        <p className="mx-auto max-w-6xl text-xs text-muted">
          © {new Date().getFullYear()} Moon. Todos os direitos reservados.
        </p>
      </div>
    </footer>
  )
}
