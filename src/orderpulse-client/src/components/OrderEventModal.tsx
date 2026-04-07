import { useEffect, useRef, useState } from 'react'
import type { OrderDto, OrderEventDto } from '../api/types'
import * as ordersService from '../services/ordersService'
import { formatDate, formatEventType } from '../utils/formatters'

interface OrderEventModalProps {
  order: OrderDto | null
  onClose: () => void
}

function parseEventData(raw: string): Record<string, unknown> {
  try {
    return JSON.parse(raw) as Record<string, unknown>
  } catch {
    return {}
  }
}

export function OrderEventModal({ order, onClose }: OrderEventModalProps) {
  const [events, setEvents] = useState<OrderEventDto[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const backdropRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!order) return
    setEvents([])
    setError(null)
    setIsLoading(true)
    ordersService
      .getEvents(order.id)
      .then(setEvents)
      .catch(() => setError('Failed to load events'))
      .finally(() => setIsLoading(false))
  }, [order])

  useEffect(() => {
    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleKey)
    return () => document.removeEventListener('keydown', handleKey)
  }, [onClose])

  if (!order) return null

  return (
    <div
      ref={backdropRef}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
      onClick={(e) => {
        if (e.target === backdropRef.current) onClose()
      }}
    >
      <div
        className="w-full max-w-lg rounded-xl shadow-xl"
        style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-text)' }}
      >
        <div
          className="flex items-center justify-between border-b px-5 py-4"
          style={{ borderColor: 'var(--color-border)' }}
        >
          <div>
            <h2 className="font-semibold">Event History</h2>
            <p className="text-xs" style={{ color: 'var(--color-text-muted)' }}>
              Order #{order.id} &middot; {order.itemName}
            </p>
          </div>
          <button
            onClick={onClose}
            className="rounded-md p-1 text-xl leading-none transition-colors hover:bg-gray-100 dark:hover:bg-gray-700"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="max-h-96 overflow-y-auto px-5 py-4">
          {isLoading && (
            <div className="flex justify-center py-8">
              <div className="h-6 w-6 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
            </div>
          )}

          {error && (
            <p className="py-4 text-center text-sm text-red-500">{error}</p>
          )}

          {!isLoading && !error && events.length === 0 && (
            <p className="py-4 text-center text-sm" style={{ color: 'var(--color-text-muted)' }}>
              No events yet
            </p>
          )}

          {!isLoading && events.length > 0 && (
            <ol className="relative border-l-2" style={{ borderColor: 'var(--color-border)' }}>
              {events.map((event) => {
                const data = parseEventData(event.eventData)
                return (
                  <li key={event.id} className="mb-5 ml-4">
                    <div
                      className="absolute -left-1.5 mt-1 h-3 w-3 rounded-full border-2 border-white bg-blue-500"
                      style={{ borderColor: 'var(--color-surface)' }}
                    />
                    <p className="text-sm font-medium" style={{ color: 'var(--color-text)' }}>
                      {formatEventType(event.eventType)}
                    </p>
                    <p className="text-xs" style={{ color: 'var(--color-text-muted)' }}>
                      {formatDate(event.createdAt)}
                    </p>
                    {!!data.newStatus && (
                      <p className="mt-1 text-xs" style={{ color: 'var(--color-text-muted)' }}>
                        {data.oldStatus ? `${String(data.oldStatus)} → ` : ''}
                        {String(data.newStatus)}
                      </p>
                    )}
                    {!!data.itemName && !data.newStatus && (
                      <p className="mt-1 text-xs" style={{ color: 'var(--color-text-muted)' }}>
                        {String(data.itemName)} × {String(data.quantity ?? 1)}
                      </p>
                    )}
                  </li>
                )
              })}
            </ol>
          )}
        </div>
      </div>
    </div>
  )
}
