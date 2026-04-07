export interface AuthResponse {
  accessToken: string
  expiresAt: string
}

export type OrderStatus =
  | 'Placed'
  | 'Confirmed'
  | 'Preparing'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled'

export interface OrderDto {
  id: number
  userId: number
  itemName: string
  quantity: number
  status: OrderStatus
  createdAt: string
  updatedAt: string
}

export interface OrderEventDto {
  id: number
  orderId: number
  eventType: string
  eventData: string
  idempotencyKey: string
  createdAt: string
}

export interface CreateOrderRequest {
  itemName: string
  quantity: number
}

export interface UpdateOrderStatusRequest {
  newStatus: OrderStatus
}

export interface SignalROrderPayload {
  orderId: number
  eventType: string
  data: Record<string, unknown>
  timestamp: string
}
