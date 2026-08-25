export function formatCardNumber(value: string) {
  return value
    .replace(/\D/g, '')
    .slice(0, 19)
    .replace(/(\d{4})(?=\d)/g, '$1 ')
}

// Algoritmo de Luhn — só valida o formato, não confirma se o cartão existe de verdade.
export function isValidCardNumber(digits: string) {
  if (!/^\d{13,19}$/.test(digits)) return false

  let sum = 0
  let shouldDouble = false

  for (let i = digits.length - 1; i >= 0; i--) {
    let digit = Number(digits[i])
    if (shouldDouble) {
      digit *= 2
      if (digit > 9) digit -= 9
    }
    sum += digit
    shouldDouble = !shouldDouble
  }

  return sum % 10 === 0
}

// Detecção por prefixo (BIN) — cobre as bandeiras mais comuns no Brasil, não é exaustiva.
export function detectCardBrand(digits: string): string {
  if (/^4/.test(digits)) return 'Visa'
  if (/^(5[1-5]|2(2[2-9]|[3-6]\d|7[01]|720))/.test(digits)) return 'Mastercard'
  if (/^3[47]/.test(digits)) return 'American Express'
  if (/^36/.test(digits)) return 'Diners Club'
  if (/^(4011|4312|4389|4514|4573|6277|6362|6363|650|6516|6550)/.test(digits)) return 'Elo'
  if (/^606282/.test(digits)) return 'Hipercard'
  return 'Cartão'
}
