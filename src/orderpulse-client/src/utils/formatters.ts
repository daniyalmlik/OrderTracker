export function formatDate(iso: string): string {
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(iso))
}

export function formatEventType(eventType: string): string {
  switch (eventType) {
    case 'OrderPlaced':
      return 'Order Placed'
    case 'StatusChanged':
      return 'Status Changed'
    case 'OrderCancelled':
      return 'Order Cancelled'
    default:
      return eventType
  }
}

export function getApiErrorMessage(error: unknown): string {
  if (
    error &&
    typeof error === 'object' &&
    'response' in error &&
    error.response &&
    typeof error.response === 'object' &&
    'data' in error.response
  ) {
    const data = error.response.data
    if (typeof data === 'string') return data
    if (
      data &&
      typeof data === 'object' &&
      'title' in data &&
      typeof data.title === 'string'
    )
      return data.title
  }
  if (error instanceof Error) return error.message
  return 'An unexpected error occurred'
}
