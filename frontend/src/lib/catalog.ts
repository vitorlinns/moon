import { apiFetch } from './api'

export interface Category {
  id: string
  name: string
  slug: string
}

export interface Product {
  id: string
  name: string
  slug: string
  categoryId: string
  categoryName: string
  categorySlug: string
  price: number
  /** ISO date (yyyy-MM-dd), usada na ordenação por "lançamentos" */
  launchedAt: string
  salesCount: number
  featured: boolean
  imageUrl: string | null
}

export interface ProductListResponse {
  items: Product[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface FetchProductsParams {
  category?: string
  sort?: string
  featured?: boolean
  page?: number
  pageSize?: number
}

export function fetchCategories(): Promise<Category[]> {
  return apiFetch<Category[]>('/categories')
}

export function fetchProducts(params: FetchProductsParams = {}): Promise<ProductListResponse> {
  const query = new URLSearchParams()

  if (params.category) query.set('category', params.category)
  if (params.sort) query.set('sort', params.sort)
  if (params.featured !== undefined) query.set('featured', String(params.featured))
  if (params.page !== undefined) query.set('page', String(params.page))
  if (params.pageSize !== undefined) query.set('pageSize', String(params.pageSize))

  const queryString = query.toString()
  return apiFetch<ProductListResponse>(`/products${queryString ? `?${queryString}` : ''}`)
}
