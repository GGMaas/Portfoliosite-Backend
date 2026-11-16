# Portfolio Backend API - WPFW Opdracht 3 

Dit repository bevat de backend API voor de persoonlijke portfolio-website, ontwikkeld in het kader van de cursus Web Programming Frameworks (WPFW). 

typ in terminal "dotnet run" om lokaal app te runnen

## 📌 Algemene Gegevens
* **Student:** Guido van der Maas
* **Repository:** [Portfoliosite-Backend](https://github.com/GGMaas/Portfoliosite-Backend)
* **Kader:** WPFW Opdracht 3 - De Haagse Hogeschool

---

## 🛠️ Technologieën & Versies
* **.NET SDK:** .NET 10 (`net10.0`)
* **Framework:** ASP.NET Core Web API
* **ORM:** Entity Framework Core 10
* **Database:** Microsoft SQL Server (LocalDB / Express)
* **Authenticatie:** ASP.NET Core Identity Framework

---

## 🏛️ Architectuur & Codestructuur

De applicatie is gebouwd volgens een **gelaagde architectuur** (Layered / N-Tier Architecture) waarbij verantwoordelijkheden strikt gescheiden zijn:

1. **HTTP-laag (`Controllers/`):** Ontvangt HTTP-verzoeken, valideert invoer via DTO's en stuurt gepaste HTTP-statuscodes terug (`200 OK`, `201 Created`, `404 Not Found`).
2. **Applicatielaag (`Services/`):** Handelt de business logic af en verzorgt de mapping tussen entiteiten en DTO's via een generieke `ICrudService`.
3. **Datalaag (`Data/` & `Entities/`):** Beheert de entiteiten (`Project`, `Blogpost`, `User`) en communiceert met SQL Server via `AppDbContext` (afgeleid van `IdentityDbContext`).

### Gekozen API-stijl
Er is gekozen voor **Controller-based Web API's** (`[ApiController]`). Dit biedt een duidelijke scheiding per entiteit, maakt gebruik van ASP.NET Core Dependency Injection en is eenvoudig uit te breiden en te onderhouden.

---

## 🗄️ SQL Server Configuratie & Database Aanmaken

### 1. Connection String instellen
De connection string is geconfigureerd in `appsettings.json`. Pas indien nodig de servernaam aan naar jouw lokale SQL Server instantie:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortfolioDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 2. Database migreren
Voer de bestaande EF Core-migraties uit voordat je de API start:

```powershell
dotnet ef database update
```

Je kunt dit ook uitvoeren met `Update-Database` in de Visual Studio Package Manager Console.

## Authenticatie en autorisatie

ASP.NET Core Identity slaat gebruikers en rollen op in SQL Server. Bij het starten maakt de API de rollen `Gast`, `Editor` en `Admin` aan als ze nog niet bestaan.

- `POST /api/Auth/register` maakt een account aan en kent automatisch de rol `Gast` toe.
- `POST /api/Auth/login` controleert e-mailadres en wachtwoord. Na een geslaagde login stuurt de API een Identity-authenticatiecookie terug.
- `POST /api/Auth/logout` meldt de gebruiker af en verwijdert de cookie.
- Projecten en blogposts kunnen zonder inloggen worden opgevraagd.
- Alleen `Editor` en `Admin` mogen projecten en blogposts aanmaken, wijzigen of verwijderen. De policy `CanEditContent` noemt beide rollen expliciet.
- Een schrijfverzoek zonder geldige cookie krijgt `401 Unauthorized`. Een ingelogde gebruiker zonder `Editor` of `Admin` krijgt `403 Forbidden`.

### Een lokale Editor- of Admin-rol toekennen

1. Start de API en registreer eerst een gebruiker. De gebruiker krijgt standaard `Gast`.
2. Open `Scripts/AssignLocalRole.sql` in SQL Server Management Studio en selecteer de lokale portfoliodatabase.
3. Vervang `REPLACE_WITH_REGISTERED_EMAIL` door het geregistreerde e-mailadres. Kies als `@RoleName` `Editor` of `Admin`.
4. Voer het script uit. Het voegt de rol toe en wijzigt de security stamp, zodat bestaande cookies niet geldig blijven.
5. Log opnieuw in via `POST /api/Auth/login`. De browser ontvangt een cookie met de nieuwe rol.

Het script kent alleen `Editor` of `Admin` toe; het maakt geen account aan en bevat geen wachtwoorden of sleutels. Voer het opnieuw uit voor een andere gebruiker of rol. Na wijzigingen aan autorisatiebeleid moet de API opnieuw worden gestart en moet de gebruiker opnieuw inloggen.

Gebruik voor lokale wachtwoorden alleen lokale testaccounts. Commit geen wachtwoorden, connection-stringwachtwoorden of sleutels naar de repository.

De voorbeelden in `C++.http` lezen het testwachtwoord uit een lokale `.env`-file. Maak die file zelf aan met `PORTFOLIO_TEST_PASSWORD=...`; `.env` wordt door Git genegeerd. Je kunt de registratie- en loginverzoeken ook rechtstreeks in Swagger invullen.

Als een aparte frontend de cookie moet gebruiken, stel de exacte origin in (vervang het voorbeeld door de origin van jouw frontend):

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173"]
}
```

De frontend moet daarnaast cookies meesturen, bijvoorbeeld met `credentials: "include"`. Gebruik voor cookie-authenticatie geen wildcard-origin.
