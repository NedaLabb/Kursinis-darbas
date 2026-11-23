import { useEffect, useState } from 'react'

const days = {
  1: 'Pirmadienis',
  2: 'Antradienis',
  3: 'Trečiadienis',
  4: 'Ketvirtadienis',
  5: 'Penktadienis',
  6: 'Šeštadienis',
  0: 'Sekmadienis'
}

function VisitorPage() {
  const [clubs, setClubs] = useState([])
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const [count, setCount] = useState(null)
  const [analysis, setAnalysis] = useState(null)

  useEffect(() => {loadClubs()}, [])

  async function loadClubs() {
    setLoading(true)
    setMessage('')
    setClubs([])
    setCount(null)
    setAnalysis(null)

    try {
      const response = await fetch('/api/visitor/clubs')

      if (response.status === 401) {
        setMessage('Reikia prisijungti.')
        return
      }

      if (response.status === 403) {
        setMessage('Šis puslapis skirtas lankytojui.')
        return
      }

      if (!response.ok) {
        setMessage('Nepavyko gauti klubų.')
        return
      }

      const data = await response.json()
      setClubs(data)

      if (data.length === 0) {
        setMessage('Klubų nėra.')
      }
    } 
    catch {
      setMessage('Nepavyko susisiekti su serveriu.')
    } 
    finally {
      setLoading(false)
    }
  }

  async function showOccupancy(event) {
    event.preventDefault()

    const form = event.currentTarget
    const data = new FormData(form)
    const clubId = data.get('club')

    setCount(null)
    setMessage('')
    setLoading(true)

    try {
      const response = await fetch(
        `/api/visitor/clubs/${clubId}/occupancy`
      )

      if (!response.ok) {
        setMessage('Nepavyko gauti klubo užimtumo.')
        return
      }

      const data = await response.json()
      setCount(data.count)
    } 
    catch {
      setMessage('Nepavyko susisiekti su serveriu.')
    } 
    finally {
      setLoading(false)
    }
  }

  async function showAnalysis(event) {
    event.preventDefault()

    const form = event.currentTarget
    const data = new FormData(form)

    const clubId = data.get('club')
    const fromText = data.get('from')
    const toText = data.get('to')

    const from = new Date(fromText)
    const to = new Date(toText)

    setMessage('')
    setAnalysis(null)

    if (from >= to) {
      setMessage('Laikotarpio pradžia turi būti ankstesnė už pabaigą.')
      return
    }

    setLoading(true)

    try {
      const parameters = new URLSearchParams({
        from: from.toISOString(),
        to: to.toISOString()
      })

      const response = await fetch(
        `/api/visitor/clubs/${clubId}/analytics?${parameters}`
      )

      if (!response.ok) {
        setMessage('Nepavyko gauti analizės. Patikrinkite prisijungimą ir laikotarpį.')
        return
      }

      const data = await response.json()

      if (data.hours.length === 0) {
        setMessage('Nepakanka duomenų gauti užimtumo analizei.')
        return
      }

      setAnalysis(data)
    } 
    catch {
      setMessage('Nepavyko gauti analizės. Patikrinkite, ar API veikia.')
    } 
    finally {
      setLoading(false)
    }
  }

  return (
    <main className="home">
      <h1>Klubų užimtumas</h1>

      <section className="occupancy-card">

        {clubs.length > 0 && (
          <form className="filters" onSubmit={showOccupancy}>
            <label> Klubas
              <select
                name="club"
                disabled={loading}
                onChange={() => setCount(null)}
              >
                {clubs.map(club => (
                  <option key={club.clubId} value={club.clubId}>
                    {club.name}
                  </option>
                ))}
              </select>
            </label>

            <button type="submit" disabled={loading}>
              Rodyti užimtumą
            </button>
          </form>
        )}

        {count !== null && (
          <p>
            Lankytojų skaičius dabar: <strong>{count}</strong>
          </p>
        )}

        {clubs.length > 0 && (
          <section>
            <h2>Lankomumo istorija</h2>

            <form
              className="filters"
              onSubmit={showAnalysis}
              onChange={() => setAnalysis(null)}
            >
              <label>
                Klubas
                <select name="club" disabled={loading}>
                  {clubs.map(club => (
                    <option key={club.clubId} value={club.clubId}>
                      {club.name}
                    </option>
                  ))}
                </select>
              </label>

              <label> Nuo
                <input name="from" type="datetime-local" required disabled={loading} />
              </label>

              <label> Iki
                <input name="to" type="datetime-local" required disabled={loading} />
              </label>

              <button type="submit" disabled={loading}>
                Analizuoti
              </button>
            </form>

              {analysis !== null && analysis.quietestHour && (
              <p>
                Pagal pasirinkto laikotarpio istoriją mažiausiai žmonių buvo:
                {' '}
                <strong>
                  {days[analysis.quietestHour.day]},
                  {' '}
                  {analysis.quietestHour.hour}-{analysis.quietestHour.hour + 1} val.
                </strong>
                {' '}
                Vidutiniškai apie {Math.round(analysis.quietestHour.averagePeople)} žmonių.
              </p>
            )}
          </section>
        )}

        <p role="status">{message}</p>
      </section>
    </main>
  )
}

export default VisitorPage