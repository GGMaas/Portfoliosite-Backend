# Portfolio Backend API - WPFW Opdracht 3 

Dit repository bevat de backend API voor de persoonlijke portfolio-website, ontwikkeld in het kader van de cursus Web Programming Frameworks (WPFW). 

typ in terminal "dotnet run" om lokaal app te runnen

## 📌 Algemene Gegevens
* **Student:** GGMaas
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
3. **Datalaag (`Data/` & `Models/`):** Beheert de entiteiten (`Project`, `Blogpost`, `ApplicationUser`) en communiceert met SQL Server via `AppDbContext` (afgeleid van `IdentityDbContext`).

### Gekozen API-stijl
Er is gekozen voor **Controller-based Web API's** (`[ApiController]`). Dit biedt een duidelijke scheiding per entiteit, maakt gebruik van ASP.NET Core Dependency Injection en is eenvoudig uit te breiden en te onderhouden.

---

## 🗄️ SQL Server Configuratie & Database Aanmaken

### 1. Connection String instellen
De connection string is geconfigureerd in `appsettings.json`. Pas indien nodig de servernaam aan naar jouw lokale SQL Server instantie:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PortfolioDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
