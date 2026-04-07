import type { OrderStatus } from '../api/types'

export const STATUS_LABELS: Record<OrderStatus, string> = {
  Placed: 'Placed',
  Confirmed: 'Confirmed',
  Preparing: 'Preparing',
  OutForDelivery: 'Out for Delivery',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
}

export const STATUS_COLORS: Record<OrderStatus, string> = {
  Placed: 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-200',
  Confirmed: 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-200',
  Preparing:
    'bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-200',
  OutForDelivery:
    'bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-200',
  Delivered:
    'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-200',
  Cancelled: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-200',
}

export const TERMINAL_STATUSES: OrderStatus[] = ['Delivered', 'Cancelled']

export const NEXT_STATUSES: Record<OrderStatus, OrderStatus[]> = {
  Placed: ['Confirmed', 'Cancelled'],
  Confirmed: ['Preparing', 'Cancelled'],
  Preparing: ['OutForDelivery', 'Cancelled'],
  OutForDelivery: ['Delivered', 'Cancelled'],
  Delivered: [],
  Cancelled: [],
}
