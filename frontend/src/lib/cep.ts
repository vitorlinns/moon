export function formatCep(value: string) {
  return value
    .replace(/\D/g, '')
    .slice(0, 8)
    .replace(/(\d{5})(\d)/, '$1-$2')
}

export interface CepLookupResult {
  street: string
  neighborhood: string
  city: string
  state: string
}

// ViaCEP é um serviço público read-only, sem necessidade de chave — a chamada
// sai direto do navegador, sem passar pelo nosso backend.
export async function lookupCep(cep: string): Promise<CepLookupResult | null> {
  const digits = cep.replace(/\D/g, '')
  if (digits.length !== 8) return null

  try {
    const response = await fetch(`https://viacep.com.br/ws/${digits}/json/`)
    if (!response.ok) return null

    const data = await response.json()
    if (data.erro) return null

    return {
      street: data.logradouro ?? '',
      neighborhood: data.bairro ?? '',
      city: data.localidade ?? '',
      state: data.uf ?? '',
    }
  } catch {
    return null
  }
}
