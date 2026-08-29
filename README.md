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

- [x] **Hito 0 — Andamiaje** *(commit `Hito 0`)*
  - Solución .NET 8 MVC organizada por Áreas (un área por módulo).
  - Modelo de dominio completo: lotes/micro lotes, períodos productivos, colaboradores,
    labores, enfermedades, recolección/producción, finanzas, inventario, alertas y auditoría.
  - `AppDbContext` con EF Core + Identity (tablas en español), índices únicos y sello
    automático de trazabilidad. Migración `InitialCreate` aplicada a SQL Server.
  - `DbSeeder` idempotente: 3 roles, finca Café Chaperno + configuración, usuario
    administrador y catálogo base de enfermedades del café.
  - Sistema de diseño (paleta verde, Segoe UI, Bootstrap 5 + Icons), layout de
    aplicación con menú lateral responsivo (colapsa en móvil) y layout de autenticación.
- [x] **Hito 1 — Módulo de Seguridad** *(commit `Hito 1`)*
  - Inicio y cierre de sesión con registro en bitácora; bloqueo tras 5 intentos
    fallidos (15 min) y expiración de sesión por inactividad (20 min).
  - Gestión de usuarios (solo Administrador): listado con búsqueda y filtros, alta con
    asignación de rol y correo único, edición de datos y rol, activar/inactivar
    conservando el historial, y desbloqueo manual.
  - Reglas de negocio: escalar a Administrador exige la contraseña del administrador
    actual; no se puede degradar/inactivar al último Administrador ni la propia cuenta.
  - Recuperación de contraseña: solicitud con respuesta genérica, enlace con token que
    vence a los 30 minutos y formulario de nueva contraseña. En desarrollo el correo se
    guarda como archivo (`App_Data/correos/`); en producción se usa un `IEmailSender` SMTP.
  - Visor de la bitácora de auditoría de **solo lectura** con filtros y paginación.
- [ ] **Hito 2** — Lotes y micro lotes + períodos productivos
- [ ] **Hito 3** — Registro de labores + colaboradores + planilla
- [ ] **Hito 4** — Control de enfermedades + tratamientos + alertas
- [ ] **Hito 5** — Recolección y producción + merma
- [ ] **Hito 6** — Ingresos, gastos, inventario, reportes PDF/Excel
- [ ] **Hito 7** — Dashboard de indicadores

La documentación funcional del proyecto está en `OneDrive_1_29-8-2026/`.
