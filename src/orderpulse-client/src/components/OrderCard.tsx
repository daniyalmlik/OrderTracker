import { useState } from 'react'
import toast from 'react-hot-toast'
import type { OrderDto, OrderStatus } from '../api/types'
import * as ordersService from '../services/ordersService'
import { NEXT_STATUSES, STATUS_LABELS, TERMINAL_STATUSES } from '../utils/constants'
import { formatDate, getApiErrorMessage } from '../utils/formatters'
import { StatusBadge } from './StatusBadge'

interface OrderCardProps {
  order: OrderDto
  onUpdated: (order: OrderDto) => void
  onCancelled: (orderId: number) => void
  onViewEvents: (order: OrderDto) => void
}

export function OrderCard({
  order,
  onUpdated,
  onCancelled,
  onViewEvents,
}: OrderCardProps) {
  const [isUpdating, setIsUpdating] = useState(false)
  const [isCancelling, setIsCancelling] = useState(false)
  const [selectedStatus, setSelectedStatus] = useState<OrderStatus | ''>('')

  const isTerminal = TERMINAL_STATUSES.includes(order.status)
  const nextStatuses = NEXT_STATUSES[order.status]

  async function handleUpdateStatus() {
    if (!selectedStatus) return
    setIsUpdating(true)
    try {
      const updated = await ordersService.updateStatus(order.id, selectedStatus)
      onUpdated(updated)
      setSelectedStatus('')
      toast.success(`Order #${order.id} → ${STATUS_LABELS[updated.status]}`)
    } catch (err) {
      toast.error(getApiErrorMessage(err))
    } finally {
      setIsUpdating(false)
    }
  }

  async function handleCancel() {
    if (!confirm(`Cancel order #${order.id}?`)) return
    setIsCancelling(true)
    try {
      await ordersService.cancel(order.id)
      onCancelled(order.id)
      toast.success(`Order #${order.id} cancelled`)
    } catch (err) {
      toast.error(getApiErrorMessage(err))
    } finally {
      setIsCancelling(false)
    }
  }

  return (
    <div
      className="rounded-xl border p-4 shadow-sm transition-shadow hover:shadow-md"
      style={{
        backgroundColor: 'var(--color-surface)',
        borderColor: 'var(--color-border)',
      }}
    >
      <div className="mb-3 flex items-start justify-between gap-2">
        <div>
          <p className="font-medium" style={{ color: 'var(--color-text)' }}>
            {order.itemName}
          </p>
          <p className="text-xs" style={{ color: 'var(--color-text-muted)' }}>
            Order #{order.id} &middot; Qty {order.quantity}
          </p>
        </div>
        <StatusBadge status={order.status} />
      </div>

      <p className="mb-3 text-xs" style={{ color: 'var(--color-text-muted)' }}>
        {formatDate(order.createdAt)}
      </p>

      {!isTerminal && nextStatuses.length > 0 && (
        <div className="mb-2 flex gap-2">
          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value as OrderStatus)}
            className="flex-1 rounded-md border px-2 py-1.5 text-sm outline-none focus:ring-2 focus:ring-blue-500"
            style={{
              backgroundColor: 'var(--color-bg)',
              borderColor: 'var(--color-border)',
              color: 'var(--color-text)',
            }}
          >
            <option value="">Update status…</option>
            {nextStatuses.map((s) => (
              <option key={s} value={s}>
                {STATUS_LABELS[s]}
              </option>
            ))}
          </select>
          <button
            onClick={handleUpdateStatus}
            disabled={!selectedStatus || isUpdating}
            className="rounded-md bg-blue-600 px-3 py-1.5 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:opacity-50"
          >
            {isUpdating ? '…' : 'Set'}
          </button>
        </div>
      )}

      <div className="flex gap-2">
        <button
          onClick={() => onViewEvents(order)}
          className="flex-1 rounded-md border px-3 py-1.5 text-sm transition-colors hover:bg-gray-50 dark:hover:bg-gray-700"
          style={{ borderColor: 'var(--color-border)', color: 'var(--color-text-muted)' }}
        >
          Events
        </button>
        {!isTerminal && (
          <button
            onClick={handleCancel}
            disabled={isCancelling}
            className="flex-1 rounded-md border border-red-200 px-3 py-1.5 text-sm text-red-600 transition-colors hover:bg-red-50 dark:border-red-800 dark:text-red-400 dark:hover:bg-red-900/20 disabled:opacity-50"
          >
            {isCancelling ? '…' : 'Cancel'}
          </button>
        )}
      </div>
    </div>
  )
}
