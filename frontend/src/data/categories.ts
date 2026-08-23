export const categories = ['Alianças', 'Anéis', 'Solitários', 'Colares', 'Brincos', 'Pulseiras'] as const

export type Category = (typeof categories)[number]
