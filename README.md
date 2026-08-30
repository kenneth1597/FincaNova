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
- [x] **Hito 2 — Lotes y micro lotes + períodos productivos** *(commit `Hito 2`)*
  - Listado con panel por estado, búsqueda y filtros; alta de lotes y micro lotes
    con código único; edición de ficha técnica (código no editable).
  - Cambio de estado con responsable, inactivación (borrado lógico que conserva el
    historial) y eliminación solo sin registros asociados.
  - Detalle del lote con bitácora cronológica (labores, enfermedades, recolecciones,
    producción y gastos) filtrable por fechas.
  - Períodos productivos: uno activo a la vez, sin fechas solapadas.
  - Identidad visual cafetalera: logotipo de grano de café, favicon, ilustración de
    rama de café en el acceso, íconos y línea de tiempo temáticos.
- [ ] **Hito 3** — Registro de labores + colaboradores + planilla
- [x] **Hito 3 — Registro de labores, colaboradores y planilla** *(commit `Hito 3`)*
  - Colaboradores: CRUD con identificación única, tarifas por jornada/hora/cajuela,
    activar/inactivar, búsqueda y detalle con historial de participación.
  - Registro de labores agrícolas con lote, período, modalidad (jornada u hora),
    cantidad y varios colaboradores; validaciones de campos, duración > 0 y al menos
    un colaborador. El Trabajador también puede registrar.
  - Cálculo automático del costo por colaborador (tarifa propia o tarifa única), con
    vista previa en vivo y desglose guardado por participante.
  - Planilla por colaborador y período: total a pagar con su desglose (la parte de
    recolección se activa en el Hito 5).
  - Las labores aparecen en la bitácora del lote.
- [ ] **Hito 4** — Control de enfermedades + tratamientos + alertas
- [x] **Hito 4 — Control de enfermedades** *(commit `Hito 4`)*
  - Registro de detecciones de enfermedad/plaga por lote (lote válido y activo,
    validación de datos) y edición con estado de seguimiento.
  - Tratamientos aplicados (acción, producto, dosis, fecha, resultado); al aplicar
    el primero, el seguimiento pasa a «En tratamiento».
  - Historial con filtros avanzados simultáneos: lote, tipo, estado, rango de fechas
    y texto del tratamiento.
  - Alerta automática de enfermedad recurrente cuando se supera el umbral configurado
    dentro de la ventana de días, con dedupe y panel de alertas con «marcar atendida».
  - Las detecciones aparecen en la bitácora del lote.
- [ ] **Hito 5** — Recolección y producción + merma
- [ ] **Hito 6** — Ingresos, gastos, inventario, reportes PDF/Excel
- [ ] **Hito 7** — Dashboard de indicadores

La documentación funcional del proyecto está en `OneDrive_1_29-8-2026/`.
