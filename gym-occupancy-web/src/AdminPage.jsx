import { useState } from 'react'

function AdminPage() {
  const [clubs, setClubs] = useState([])
  const [message, setMessage] = useState('')
  const [analysis, setAnalysis] = useState(null)

  async function loadClubs() {
    setClubs([])
    setMessage('')
    setAnalysis(null)

    try {
      const response = await fetch('/api/admin/clubs')

      if (!response.ok) {
        setMessage('Nepavyko gauti klubų.')
        return
      }

      setClubs(await response.json())
    } 
    catch {
      setMessage('Nepavyko susisiekti su serveriu.')
    }
  }

  async function showAnalysis(event) {
    event.preventDefault()

        const form = event.currentTarget
        const formData = new FormData(form)

        const clubId = formData.get('club')
        const fromText = formData.get('from')
        const toText = formData.get('to')

        const from = new Date(fromText)
        const to = new Date(toText)

    setMessage('')
    setAnalysis(null)

    if (from >= to) {
      setMessage('Laikotarpio pradžia turi būti ankstesnė už pabaigą.')
      return
    }

    try {
      const parameters = new URLSearchParams({
        from: from.toISOString(),
        to: to.toISOString()
      })

      const response = await fetch(
        `/api/admin/clubs/${clubId}/analytics?${parameters}`
      )

      if (!response.ok) {
        setMessage('Nepavyko gauti klubo analizės.')
        return
      }

      const data = await response.json()

      if (data.average === null) {
        setMessage('Nepakanka duomenų užimtumo analizei.')
        return
      }

      setAnalysis(data)
    } 
    catch {
      setMessage('Nepavyko gauti analizės.')
    }
  }

  return (
    <main className="home">
      <h1>Įmonės klubų suvestinė</h1>

      <section className="occupancy-card">
        <div className="filters">
          <button type="button" onClick={loadClubs}>
            Rodyti mano klubus
          </button>
        </div>

        {clubs.length > 0 && (
          <form className="filters"
            onSubmit={showAnalysis}
            onChange={() => setAnalysis(null)}
          >
            <label> Klubas
              <select name="club">
                {clubs.map(club => (
                  <option key={club.clubId} value={club.clubId}>
                    {club.name}
                  </option>
                ))}
              </select>
            </label>

            <label> Nuo
              <input name="from" type="datetime-local" required />
            </label>

            <label> Iki
              <input name="to" type="datetime-local" required />
            </label>

            <button type="submit">Rodyti suvestinę</button>
          </form>
        )}

        {analysis !== null && (
          <p>
            Vidutinis žmonių skaičius pagal turimus laikotarpio duomenis:
            {' '}
            <strong>{analysis.average.toFixed(1)}</strong>
          </p>
        )}

        <p role="status">{message}</p>
      </section>
    </main>
  )
}

export default AdminPage