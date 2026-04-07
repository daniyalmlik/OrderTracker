import { useState } from 'react'
import toast from 'react-hot-toast'
import type { OrderDto } from '../api/types'
import * as ordersService from '../services/ordersService'
import { getApiErrorMessage } from '../utils/formatters'

interface OrderFormProps {
  onCreated: (order: OrderDto) => void
}

export function OrderForm({ onCreated }: OrderFormProps) {
  const [itemName, setItemName] = useState('')
  const [quantity, setQuantity] = useState(1)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!itemName.trim()) return

    setIsSubmitting(true)
    try {
      const order = await ordersService.create(itemName.trim(), quantity)
      onCreated(order)
      setItemName('')
      setQuantity(1)
      toast.success(`Order #${order.id} created!`)
    } catch (err) {
      toast.error(getApiErrorMessage(err))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-xl border p-5 shadow-sm"
      style={{
        backgroundColor: 'var(--color-surface)',
        borderColor: 'var(--color-border)',
      }}
    >
      <h2 className="mb-4 text-base font-semibold" style={{ color: 'var(--color-text)' }}>
        New Order
      </h2>
      <div className="space-y-3">
        <div>
          <label
            htmlFor="itemName"
            className="mb-1 block text-sm font-medium"
            style={{ color: 'var(--color-text-muted)' }}
          >
            Item Name
          </label>
          <input
            id="itemName"
            type="text"
            value={itemName}
            onChange={(e) => setItemName(e.target.value)}
            maxLength={200}
            required
            placeholder="e.g. Margherita Pizza"
            className="w-full rounded-md border px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-blue-500"
            style={{
              backgroundColor: 'var(--color-bg)',
              borderColor: 'var(--color-border)',
              color: 'var(--color-text)',
            }}
          />
        </div>
        <div>
          <label
            htmlFor="quantity"
            className="mb-1 block text-sm font-medium"
            style={{ color: 'var(--color-text-muted)' }}
          >
            Quantity
          </label>
          <input
            id="quantity"
            type="number"
            value={quantity}
            onChange={(e) => setQuantity(Math.max(1, Number(e.target.value)))}
            min={1}
            max={1000}
            required
            className="w-full rounded-md border px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-blue-500"
            style={{
              backgroundColor: 'var(--color-bg)',
              borderColor: 'var(--color-border)',
              color: 'var(--color-text)',
            }}
          />
        </div>
        <button
          type="submit"
          disabled={isSubmitting || !itemName.trim()}
          className="w-full rounded-md bg-blue-600 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isSubmitting ? 'Placing…' : 'Place Order'}
        </button>
      </div>
    </form>
  )
}
