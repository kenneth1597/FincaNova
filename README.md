# FincaNova

Sistema web para la gestión integral de una finca cafetalera (cliente: **Café Chaperno**,
Poás, Alajuela). Proyecto académico — Universidad Fidélitas, cursos SC-702 / SC-803.

## Tecnología

| Capa | Herramienta |
|------|-------------|
| Framework | ASP.NET Core **MVC** (.NET 8) |
| Lenguaje | C# |
| Datos | Entity Framework Core 8 + **SQL Server** |
| Seguridad | ASP.NET Core Identity (roles: Administrador, Caficultor, Trabajador) |
| UI | Razor + Bootstrap 5 + Bootstrap Icons |

## Requisitos

- .NET SDK 8 o superior
- SQL Server (probado con la instancia local `.\SQL2025`)

## Puesta en marcha

```bash
cd src/FincaNova.Web
dotnet ef database update      # crea la BD FincaNovaDb y aplica migraciones
dotnet run
```

Al primer arranque se crean automáticamente los roles, la finca de Café Chaperno,
el catálogo de enfermedades y el usuario administrador:

- **Usuario:** `admin@fincanova.local`
- **Contraseña:** `FincaNova2026$`

La cadena de conexión está en `src/FincaNova.Web/appsettings.json` (`ConnectionStrings:DefaultConnection`).

## Estructura

```
src/FincaNova.Web/
├─ Domain/            Entidades y enums del negocio
├─ Data/              AppDbContext, migraciones, DbSeeder
├─ Security/          Constantes de roles
├─ Services/          Servicios transversales (auditoría, …)
├─ Areas/<Módulo>/    Controllers + Views + ViewModels por módulo
└─ Views/Shared/      Layout y sistema de diseño
```

## Estado del desarrollo (por hitos)

- [x] **Hito 0** — Solución, EF Core, Identity, layout, datos iniciales
- [ ] **Hito 1** — Seguridad: login ✔, gestión de usuarios, recuperación de contraseña, bitácora
- [ ] **Hito 2** — Lotes y micro lotes + períodos productivos
- [ ] **Hito 3** — Registro de labores + colaboradores + planilla
- [ ] **Hito 4** — Control de enfermedades + tratamientos + alertas
- [ ] **Hito 5** — Recolección y producción + merma
- [ ] **Hito 6** — Ingresos, gastos, inventario, reportes PDF/Excel
- [ ] **Hito 7** — Dashboard de indicadores

La documentación funcional del proyecto está en `OneDrive_1_29-8-2026/`.
