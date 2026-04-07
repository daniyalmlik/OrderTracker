import { api } from '../api/axios'
import type {
  CreateOrderRequest,
  OrderDto,
  OrderEventDto,
  UpdateOrderStatusRequest,
} from '../api/types'

export async function list(): Promise<OrderDto[]> {
  const { data } = await api.get<OrderDto[]>('/orders')
  return data
}

export async function create(
  itemName: string,
  quantity: number
): Promise<OrderDto> {
  const body: CreateOrderRequest = { itemName, quantity }
  const { data } = await api.post<OrderDto>('/orders', body)
  return data
}

export async function updateStatus(
  orderId: number,
  newStatus: UpdateOrderStatusRequest['newStatus']
): Promise<OrderDto> {
  const body: UpdateOrderStatusRequest = { newStatus }
  const { data } = await api.patch<OrderDto>(`/orders/${orderId}/status`, body)
  return data
}

export async function cancel(orderId: number): Promise<void> {
  await api.delete(`/orders/${orderId}`)
}

export async function getEvents(orderId: number): Promise<OrderEventDto[]> {
  const { data } = await api.get<OrderEventDto[]>(`/orders/${orderId}/events`)
  return data
}
