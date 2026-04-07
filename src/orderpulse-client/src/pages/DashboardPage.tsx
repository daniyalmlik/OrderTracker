import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import type { OrderDto } from '../api/types'
import * as ordersService from '../services/ordersService'
import { useSignalR } from '../hooks/useSignalR'
import { OrderCard } from '../components/OrderCard'
import { OrderEventModal } from '../components/OrderEventModal'
import { OrderForm } from '../components/OrderForm'

export function DashboardPage() {
  const [orders, setOrders] = useState<OrderDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedOrder, setSelectedOrder] = useState<OrderDto | null>(null)

  useEffect(() => {
    ordersService
      .list()
      .then(setOrders)
      .catch(() => setError('Failed to load orders'))
      .finally(() => setIsLoading(false))
  }, [])

  useSignalR({
    onNewOrder: (payload) => {
      ordersService
        .list()
        .then((all) => {
          const incoming = all.find((o) => o.id === payload.orderId)
          if (incoming) {
            setOrders((prev) => {
              const exists = prev.some((o) => o.id === incoming.id)
              return exists ? prev : [incoming, ...prev]
            })
            toast(`New order #${payload.orderId} placed`)
          }
        })
        .catch(() => {})
    },
    onOrderUpdated: (payload) => {
      const newStatus = payload.data?.newStatus as OrderDto['status'] | undefined
      setOrders((prev) =>
        prev.map((o) =>
          o.id === payload.orderId
            ? { ...o, status: newStatus ?? o.status }
            : o
        )
      )
    },
  })

  function handleCreated(order: OrderDto) {
    setOrders((prev) => [order, ...prev])
  }

  function handleUpdated(updated: OrderDto) {
    setOrders((prev) => prev.map((o) => (o.id === updated.id ? updated : o)))
  }

  function handleCancelled(orderId: number) {
    setOrders((prev) => prev.filter((o) => o.id !== orderId))
  }

  return (
    <div className="space-y-6">
      <OrderForm onCreated={handleCreated} />

      <section>
        <h2 className="mb-4 text-base font-semibold" style={{ color: 'var(--color-text)' }}>
          Your Orders
        </h2>

        {isLoading && (
          <div className="flex justify-center py-12">
            <div className="h-7 w-7 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
          </div>
        )}

        {error && (
          <p className="py-8 text-center text-sm text-red-500">{error}</p>
        )}

        {!isLoading && !error && orders.length === 0 && (
          <p className="py-8 text-center text-sm" style={{ color: 'var(--color-text-muted)' }}>
            No orders yet. Place your first order above!
          </p>
        )}

        {!isLoading && orders.length > 0 && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {orders.map((order) => (
              <OrderCard
                key={order.id}
                order={order}
                onUpdated={handleUpdated}
                onCancelled={handleCancelled}
                onViewEvents={setSelectedOrder}
              />
            ))}
          </div>
        )}
      </section>

      <OrderEventModal
        order={selectedOrder}
        onClose={() => setSelectedOrder(null)}
      />
    </div>
  )
}
