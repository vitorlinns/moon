import { ApiError } from './api'

export function getAuthErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    switch (err.status) {
      case 401:
        return 'E-mail ou senha inválidos.'
      case 409:
        return 'Já existe uma conta com esse CPF ou e-mail.'
      case 400:
      case 422:
        return err.message || 'Verifique os dados informados.'
      default:
        return 'Não foi possível completar a solicitação. Tente novamente.'
    }
  }

  return 'Não foi possível conectar ao servidor. Tente novamente.'
}
