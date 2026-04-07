import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { useEffect, useRef } from 'react'
import type { SignalROrderPayload } from '../api/types'
import { useAuth } from './useAuth'

interface UseSignalROptions {
  onNewOrder: (payload: SignalROrderPayload) => void
  onOrderUpdated: (payload: SignalROrderPayload) => void
}

export function useSignalR({ onNewOrder, onOrderUpdated }: UseSignalROptions) {
  const { accessToken } = useAuth()
  const callbacksRef = useRef({ onNewOrder, onOrderUpdated })
  callbacksRef.current = { onNewOrder, onOrderUpdated }

  useEffect(() => {
    if (!accessToken) return

    const connection = new HubConnectionBuilder()
      .withUrl(`http://localhost:5098/hubs/orders?access_token=${accessToken}`)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('NewOrderPlaced', (payload: SignalROrderPayload) => {
      callbacksRef.current.onNewOrder(payload)
    })

    connection.on('OrderStatusUpdated', (payload: SignalROrderPayload) => {
      callbacksRef.current.onOrderUpdated(payload)
    })

    connection.start().catch((err: unknown) => {
      if (connection.state !== HubConnectionState.Disconnected) {
        console.warn('SignalR connection failed:', err)
      }
    })

    return () => {
      connection.stop()
    }
  }, [accessToken])
}
