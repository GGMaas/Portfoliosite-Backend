# Portfolio Backend API - WPFW Opdracht 3

Dit repository bevat de backend API voor de persoonlijke portfolio-website, ontwikkeld in het kader van de cursus Web Programming Frameworks (WPFW).

typ in terminal "dotnet run" om lokaal app te runnen

## 📌 Algemene Gegevens

- **Student:** Guido van der Maas
- **Repository:** [Portfoliosite-Backend](https://github.com/GGMaas/Portfoliosite-Backend)
- **Kader:** WPFW Opdracht 4 - De Haagse Hogeschool

---

## 🛠️ Technologieën & Versies

- **.NET SDK:** .NET 10 (`net10.0`)
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core 10
- **Database:** Microsoft SQL Server (LocalDB / Express)
- **Authenticatie:** ASP.NET Core Identity Framework

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

## Tests

`PortfolioSite.Tests` bevat HTTP-integratietests voor de authenticatie- en CRUD-routes. De tests gebruiken de echte controllers en services, maar een aparte in-memory database; de lokale SQL Server-database wordt niet aangepast. De CRUD-tests gebruiken testauthenticatie, terwijl registratie- en logintests de echte Identity-cookieauthenticatie gebruiken. Elke test maakt Arrange, Act en Assert herkenbaar.

De authenticatietests controleren registratie van een account en de standaardrol `Gast`, dat registratie niet automatisch inlogt, ongeldige e-mailadressen en wachtwoorden, dubbele registratie, succesvol inloggen met een cookie, en afwijzing van onjuiste of onbekende inloggegevens. Een volgend beschermd verzoek controleert dat de login-cookie daadwerkelijk wordt herkend.

De tests controleren voor beide resources:

- Publiek ophalen van alle items en een bestaand item, plus `404 Not Found` voor een ontbrekend item.
- Aanmaken als Editor en Admin, inclusief `201 Created` en de teruggegeven en opgeslagen gegevens.
- Wijzigen en verwijderen van bestaande items (`204 No Content`), plus `404 Not Found` wanneer het item ontbreekt.
- Alle schrijfmethoden geven `401 Unauthorized` zonder authenticatie en `403 Forbidden` voor de rol `Gast`.

Na uitvoering rapporteert Coverlet 100% line- en branch-coverage voor de registratie- en loginacties en de CRUD-controllers en CRUD-services. De registratie-service zelf is getest; een zeldzaam foutpad bij het toekennen van de standaardrol blijft ongedekt. De logout-actie en opstartcode vallen ook buiten deze tests. De totale backendcoverage is 25,6% line coverage. Coverage is gemeten met Coverlet; dit percentage betekent niet dat elke mogelijke invoer of productieomgeving is getest.

Voer de tests uit vanuit de backendmap:

```powershell
dotnet test .\PortfolioSite.Tests\PortfolioSite.Tests.csproj --configuration Release --collect:"XPlat Code Coverage"
```

Het Cobertura-rapport wordt onder `PortfolioSite.Tests\TestResults` opgeslagen.
