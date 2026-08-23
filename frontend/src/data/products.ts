import type { Category } from './categories'

export interface Product {
  name: string
  category: Category
  price: number
  /** ISO date, usada na ordenação por "lançamentos" */
  launchedAt: string
  /** valor mockado, usado na ordenação por "mais vendidos" */
  sales: number
  featured?: boolean
}

export const products: Product[] = [
  { name: 'Aliança Clássica Ouro 18k', category: 'Alianças', price: 1890, launchedAt: '2026-01-10', sales: 340, featured: true },
  { name: 'Aliança Trabalhada Ouro Rosé', category: 'Alianças', price: 2150, launchedAt: '2026-05-02', sales: 120 },
  { name: 'Aliança Anatômica Prata', category: 'Alianças', price: 690, launchedAt: '2025-11-20', sales: 210 },
  { name: 'Aliança Diamantada Ouro Branco', category: 'Alianças', price: 2450, launchedAt: '2026-03-12', sales: 88 },
  { name: 'Aliança Lisa Abaulada Prata', category: 'Alianças', price: 520, launchedAt: '2025-07-22', sales: 265 },
  { name: 'Aliança Compromisso Ouro 18k', category: 'Alianças', price: 2890, launchedAt: '2026-07-10', sales: 42 },
  { name: 'Aliança Textura Fosca Ouro Rosé', category: 'Alianças', price: 2340, launchedAt: '2026-02-01', sales: 130 },

  { name: 'Anel Solitário Diamante', category: 'Anéis', price: 4250, launchedAt: '2026-02-14', sales: 95, featured: true },
  { name: 'Anel Meia Aliança Zircônias', category: 'Anéis', price: 990, launchedAt: '2026-06-01', sales: 150 },
  { name: 'Anel Vintage Ouro Branco', category: 'Anéis', price: 1680, launchedAt: '2025-09-15', sales: 80 },
  { name: 'Anel Cravejado Zircônias', category: 'Anéis', price: 850, launchedAt: '2026-04-05', sales: 175 },
  { name: 'Anel Empilhável Ouro', category: 'Anéis', price: 690, launchedAt: '2026-06-20', sales: 210 },
  { name: 'Anel Signet Ouro Amarelo', category: 'Anéis', price: 1290, launchedAt: '2025-10-30', sales: 60 },
  { name: 'Anel Torcido Prata', category: 'Anéis', price: 480, launchedAt: '2026-01-15', sales: 190 },

  { name: 'Solitário Diamante 30pts', category: 'Solitários', price: 6800, launchedAt: '2026-03-05', sales: 40 },
  { name: 'Solitário Esmeralda', category: 'Solitários', price: 5200, launchedAt: '2025-12-01', sales: 25 },
  { name: 'Solitário Safira Azul', category: 'Solitários', price: 5900, launchedAt: '2026-05-08', sales: 30 },
  { name: 'Solitário Rubi', category: 'Solitários', price: 6100, launchedAt: '2025-09-25', sales: 22 },
  { name: 'Solitário Diamante 50pts', category: 'Solitários', price: 8900, launchedAt: '2026-06-30', sales: 15 },
  { name: 'Solitário Ouro Rosé Diamante', category: 'Solitários', price: 4700, launchedAt: '2026-03-20', sales: 48 },

  { name: 'Colar Gota Prata', category: 'Colares', price: 590, launchedAt: '2026-04-18', sales: 300, featured: true },
  { name: 'Colar Ponto de Luz Ouro', category: 'Colares', price: 1290, launchedAt: '2026-07-01', sales: 60 },
  { name: 'Colar Choker Prata', category: 'Colares', price: 450, launchedAt: '2025-10-10', sales: 175 },
  { name: 'Colar Corrente Veneziana Ouro', category: 'Colares', price: 1650, launchedAt: '2026-02-28', sales: 95 },
  { name: 'Colar Coração Zircônia', category: 'Colares', price: 720, launchedAt: '2026-05-15', sales: 220 },
  { name: 'Colar Corrente Cartier Prata', category: 'Colares', price: 890, launchedAt: '2025-12-18', sales: 140 },
  { name: 'Colar Camafeu Vintage', category: 'Colares', price: 980, launchedAt: '2025-08-05', sales: 55 },

  { name: 'Brinco Argola Ouro', category: 'Brincos', price: 780, launchedAt: '2026-01-25', sales: 260, featured: true },
  { name: 'Brinco Ponto de Luz Diamante', category: 'Brincos', price: 2100, launchedAt: '2026-05-20', sales: 70 },
  { name: 'Brinco Pérola Clássico', category: 'Brincos', price: 620, launchedAt: '2025-08-30', sales: 190 },
  { name: 'Brinco Cristal Gota', category: 'Brincos', price: 560, launchedAt: '2026-04-22', sales: 165 },
  { name: 'Brinco Ear Cuff Prata', category: 'Brincos', price: 390, launchedAt: '2026-06-05', sales: 245 },
  { name: 'Brinco Botão Ouro', category: 'Brincos', price: 470, launchedAt: '2025-11-12', sales: 200 },
  { name: 'Brinco Comprido Cascata', category: 'Brincos', price: 1120, launchedAt: '2026-03-30', sales: 68 },

  { name: 'Pulseira Riviera Zircônias', category: 'Pulseiras', price: 1450, launchedAt: '2026-06-15', sales: 55 },
  { name: 'Pulseira Berloques Prata', category: 'Pulseiras', price: 540, launchedAt: '2025-11-05', sales: 230 },
  { name: 'Pulseira Elos Ouro', category: 'Pulseiras', price: 1580, launchedAt: '2026-01-05', sales: 78 },
  { name: 'Pulseira Tênis Zircônias', category: 'Pulseiras', price: 2200, launchedAt: '2026-05-25', sales: 50 },
  { name: 'Pulseira Corrente Cadeado Prata', category: 'Pulseiras', price: 610, launchedAt: '2025-09-08', sales: 185 },
  { name: 'Pulseira Charm Coração', category: 'Pulseiras', price: 490, launchedAt: '2026-07-05', sales: 160 },
]
