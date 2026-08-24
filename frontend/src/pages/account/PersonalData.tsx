import { useAuth } from '../../context/AuthContext'

export function PersonalData() {
  const { user } = useAuth()

  if (!user) return null

  return (
    <section>
      <h2 className="text-sm uppercase tracking-wider text-muted">Dados pessoais</h2>

      <dl className="mt-4 flex flex-col gap-3 text-sm">
        <div>
          <dt className="text-muted">Nome</dt>
          <dd className="mt-0.5 text-foreground">{user.name}</dd>
        </div>
        <div>
          <dt className="text-muted">E-mail</dt>
          <dd className="mt-0.5 text-foreground">{user.email}</dd>
        </div>
        {user.cpf && (
          <div>
            <dt className="text-muted">CPF</dt>
            <dd className="mt-0.5 text-foreground">{user.cpf}</dd>
          </div>
        )}
      </dl>
    </section>
  )
}
