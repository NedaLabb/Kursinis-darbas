import { useState } from 'react'
import './App.css'
import VisitorPage from './VisitorPage'
import AdminPage from './AdminPage'

function App() {
  const [user, setUser] = useState(null)
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  async function login(event) {
    event.preventDefault()

    const form = new FormData(event.currentTarget)

    setMessage('')
    setLoading(true)

    try {
      const response = await fetch('/api/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          username: form.get('username'),
          password: form.get('password')
        })
      })

      if (response.status === 401) {
        setMessage('Neteisingas naudotojo vardas arba slaptažodis.')
        return
      }

      if (!response.ok) {
        throw new Error('Prisijungimas nepavyko.')
      }

      const account = await response.json()
      setUser(account)
    } 
      catch {
      setMessage('Nepavyko prisijungti.')
    } 
    finally {
      setLoading(false)
    }
  }

  if (user?.role === 'Admin') {
    return <AdminPage />
  }

  if (user?.role === 'Visitor') {
    return <VisitorPage />
  }

  return (
    <main className="home">
      <h1>Sporto klubų užimtumas</h1>

      <section className="login-card">
        <h2>Prisijungimas</h2>

        <form className="login-form" onSubmit={login}>
          <label> Naudotojo vardas
            <input
              name="username"
              autoComplete="username"
              required
            />
          </label>

          <label> Slaptažodis
            <input
              name="password"
              type="password"
              autoComplete="current-password"
              required
            />
          </label>

          <button type="submit" disabled={loading}>
            Prisijungti
          </button>

          <p role="status">{message}</p>
        </form>
      </section>
    </main>
  )
}

export default App