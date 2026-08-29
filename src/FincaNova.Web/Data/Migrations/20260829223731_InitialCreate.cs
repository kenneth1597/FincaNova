using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FincaNova.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ReferenciaId = table.Column<int>(type: "int", nullable: true),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Atendida = table.Column<bool>(type: "bit", nullable: false),
                    FechaAtencion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtendidaPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Accion = table.Column<int>(type: "int", nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ValorAnterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colaboradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TarifaJornada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TarifaHora = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TarifaCajuela = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboradores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fincas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Propietario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fincas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEnfermedad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEnfermedad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesFinca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FincaId = table.Column<int>(type: "int", nullable: false),
                    KilogramosPorCajuela = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    UmbralRecurrenciaEnfermedad = table.Column<int>(type: "int", nullable: false),
                    DiasVentanaRecurrencia = table.Column<int>(type: "int", nullable: false),
                    TamanoMaximoAdjuntoMb = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesFinca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesFinca_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FincaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    LotePadreId = table.Column<int>(type: "int", nullable: true),
                    AreaHectareas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VariedadCafe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AnioSiembra = table.Column<int>(type: "int", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaUltimoCambioEstado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lotes_Lotes_LotePadreId",
                        column: x => x.LotePadreId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosProductivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FincaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosProductivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosProductivos_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FincaId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsumoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Costo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: true),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: true),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ComprobanteArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gastos_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Labores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: true),
                    TipoLabor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modalidad = table.Column<int>(type: "int", nullable: false),
                    DuracionHoras = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Jornadas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoCalculado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labores_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Labores_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recolecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: false),
                    ColaboradorId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cajuelas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PesoEstimadoKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recolecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recolecciones_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recolecciones_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recolecciones_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosEnfermedad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    TipoEnfermedadId = table.Column<int>(type: "int", nullable: false),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: true),
                    FechaDeteccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sintomas = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosEnfermedad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosEnfermedad_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosEnfermedad_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosEnfermedad_TiposEnfermedad_TipoEnfermedadId",
                        column: x => x.TipoEnfermedadId,
                        principalTable: "TiposEnfermedad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosProduccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: false),
                    Etapa = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosProduccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosProduccion_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosProduccion_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasCafe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: true),
                    PeriodoProductivoId = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comprador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasCafe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentasCafe_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasCafe_PeriodosProductivos_PeriodoProductivoId",
                        column: x => x.PeriodoProductivoId,
                        principalTable: "PeriodosProductivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioClaims_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UsuarioLogins_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UsuarioTokens_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaborColaboradores",
                columns: table => new
                {
                    LaborId = table.Column<int>(type: "int", nullable: false),
                    ColaboradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborColaboradores", x => new { x.LaborId, x.ColaboradorId });
                    table.ForeignKey(
                        name: "FK_LaborColaboradores_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LaborColaboradores_Labores_LaborId",
                        column: x => x.LaborId,
                        principalTable: "Labores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistroEnfermedadId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    InsumoId = table.Column<int>(type: "int", nullable: true),
                    Dosis = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CantidadInsumoUsada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaAplicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultadoObservado = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tratamientos_RegistrosEnfermedad_RegistroEnfermedadId",
                        column: x => x.RegistroEnfermedadId,
                        principalTable: "RegistrosEnfermedad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entidad_EntidadId",
                table: "AuditLogs",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FechaHora",
                table: "AuditLogs",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_Identificacion",
                table: "Colaboradores",
                column: "Identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesFinca_FincaId",
                table: "ConfiguracionesFinca",
                column: "FincaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_LoteId",
                table: "Gastos",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_PeriodoProductivoId",
                table: "Gastos",
                column: "PeriodoProductivoId");

            migrationBuilder.CreateIndex(
                name: "IX_LaborColaboradores_ColaboradorId",
                table: "LaborColaboradores",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Labores_LoteId",
                table: "Labores",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Labores_PeriodoProductivoId",
                table: "Labores",
                column: "PeriodoProductivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_FincaId_Codigo",
                table: "Lotes",
                columns: new[] { "FincaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_LotePadreId",
                table: "Lotes",
                column: "LotePadreId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_InsumoId",
                table: "MovimientosInventario",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosProductivos_FincaId",
                table: "PeriodosProductivos",
                column: "FincaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_ColaboradorId",
                table: "Recolecciones",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_LoteId",
                table: "Recolecciones",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_PeriodoProductivoId",
                table: "Recolecciones",
                column: "PeriodoProductivoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEnfermedad_LoteId",
                table: "RegistrosEnfermedad",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEnfermedad_PeriodoProductivoId",
                table: "RegistrosEnfermedad",
                column: "PeriodoProductivoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEnfermedad_TipoEnfermedadId",
                table: "RegistrosEnfermedad",
                column: "TipoEnfermedadId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosProduccion_LoteId",
                table: "RegistrosProduccion",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosProduccion_PeriodoProductivoId",
                table: "RegistrosProduccion",
                column: "PeriodoProductivoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolClaims_RoleId",
                table: "RolClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_InsumoId",
                table: "Tratamientos",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_RegistroEnfermedadId",
                table: "Tratamientos",
                column: "RegistroEnfermedadId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioClaims_UserId",
                table: "UsuarioClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioLogins_UserId",
                table: "UsuarioLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RoleId",
                table: "UsuarioRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_FincaId",
                table: "Usuarios",
                column: "FincaId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuarios",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VentasCafe_LoteId",
                table: "VentasCafe",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentasCafe_PeriodoProductivoId",
                table: "VentasCafe",
                column: "PeriodoProductivoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ConfiguracionesFinca");

            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "LaborColaboradores");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Recolecciones");

            migrationBuilder.DropTable(
                name: "RegistrosProduccion");

            migrationBuilder.DropTable(
                name: "RolClaims");

            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropTable(
                name: "UsuarioClaims");

            migrationBuilder.DropTable(
                name: "UsuarioLogins");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "UsuarioTokens");

            migrationBuilder.DropTable(
                name: "VentasCafe");

            migrationBuilder.DropTable(
                name: "Labores");

            migrationBuilder.DropTable(
                name: "Colaboradores");

            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "RegistrosEnfermedad");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "PeriodosProductivos");

            migrationBuilder.DropTable(
                name: "TiposEnfermedad");

            migrationBuilder.DropTable(
                name: "Fincas");
        }
    }
}
