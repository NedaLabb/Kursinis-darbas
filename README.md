# Kursinis-darbas
Sporto klubo užimtumo stebėjimo sistema

## Reikalingi įrankiai

- Docker Desktop su Linux konteineriais.
- Docker Engine - 29.9.3 versija.
- Docker Compose - 5.7.0 versija.
- .NET 8 SDK - testams paleisti kompiuteryje.

Docker konteineriuose naudojama .NET 8, Node.js 22, SQL Servr 2022 ir RabbitMQ 3.

## Pradinis paruošimas ir konfigūracija

- Atsiųsti projektą ir išarchyvuoti.
- Paleisti Docker Desktop.
- Atidaryti PowerShell projekto aplanke, kuriame yra `compose.yaml`.
- Nukopijuoti .env.example failą tame pačiame aplanke ir kopiją pervadinti į .env. Jame nustatyti SQL Server ir RabbitMQ slaptažodžius.

Po paruošimo visa sistema paleidžiama viena komanda:

```bash
docker compose up --build -d
```

Duomenų bazės lentelės, paskyros ir klubai sukuriami automatiškai.

Svetainė: http://localhost:5173

## Testai

Turint .NET 8 SDK, projekto aplanke vykdyti:

```bash
dotnet test ./GymOccupancy.Tests/GymOccupancy.Tests.csproj
```

## Pavyzdiniai duomenys

Visų demonstracinių paskyrų slaptažodis: `testas123`.

| Naudotojas | Prieiga |
| --- | --- |
| lankytojas | Klubų užimtumas ir lankomumo istorija |
| admin1 | Centro ir Parko klubų suvestinės |
| admin2 | Šiaurės klubo suvestinė |

## Demonstravimas

Atidaryti įvykių generatorių:

```bash
docker compose attach scanner
```

Jei meniu nematomas paspausti Enter. Jei generatorius sustabdytas prieš prisijungiant vykdyti `docker compose start scanner`.

1. Generatoriuje pasirinkti `1`. Parko klubui išsiunčiama 30 įėjimų ir 5 išėjimai.
2. Palaukti kol įvykiai bus apdoroti. Svetainėje prisijunkti, kaip `lankytojas` pasirinkti Parko klubą ir spausti „Rodyti užimtumą“. Jei klubas prieš scenarijų buvo tuščias rezultatas bus 25.
3. Generatoriuje pasirinkti `4`. Pakartotinai siunčiamas tas pats įvykis, todėl užimtumas neturi pasikeisti.
4. Pasirinkus `3` generuojama Centro klubo savaitės istorija. Jai apdorojus svetainėje pasirinkti Centro klubą laikotarpį nuo 2026-11-09 00:00 iki 2026-11-17 00:00 ir spausti "Analizuoti".
5. Prisijunkti kaip `admin1` ir `admin2`. Spausti „Rodyti mano klubus“ ir patikrinti, kad kiekvienas mato tik savo įmonės klubus. Pasirinkti klubą, laikotarpį ir spausti „Rodyti suvestinę“.

## Sustabdymas

```bash
docker compose down
```
