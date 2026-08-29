using System.Security.Claims;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Alertas;
using FincaNova.Web.Domain.Auditoria;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Enfermedades;
using FincaNova.Web.Domain.Finanzas;
using FincaNova.Web.Domain.Labores;
using FincaNova.Web.Domain.Lotes;
using FincaNova.Web.Domain.Produccion;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Data;

/// <summary>
/// Contexto de EF Core del sistema. Integra ASP.NET Core Identity y todas las
/// entidades de negocio. Completa automáticamente los campos de trazabilidad de
/// <see cref="AuditableEntity"/> en cada guardado (RQNF-015).
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Finca> Fincas => Set<Finca>();
    public DbSet<ConfiguracionFinca> ConfiguracionesFinca => Set<ConfiguracionFinca>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<PeriodoProductivo> PeriodosProductivos => Set<PeriodoProductivo>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<Labor> Labores => Set<Labor>();
    public DbSet<LaborColaborador> LaborColaboradores => Set<LaborColaborador>();
    public DbSet<TipoEnfermedad> TiposEnfermedad => Set<TipoEnfermedad>();
    public DbSet<RegistroEnfermedad> RegistrosEnfermedad => Set<RegistroEnfermedad>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<Recoleccion> Recolecciones => Set<Recoleccion>();
    public DbSet<RegistroProduccion> RegistrosProduccion => Set<RegistroProduccion>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<VentaCafe> VentasCafe => Set<VentaCafe>();
    public DbSet<Insumo> Insumos => Set<Insumo>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ----- Identity: nombres de tabla en español -----
        builder.Entity<ApplicationUser>().ToTable("Usuarios");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UsuarioRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UsuarioClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UsuarioLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RolClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UsuarioTokens");

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Nombre).HasMaxLength(120);
            e.Property(u => u.Apellidos).HasMaxLength(120);
            e.HasOne(u => u.Finca).WithMany().HasForeignKey(u => u.FincaId).OnDelete(DeleteBehavior.SetNull);
        });

        // ----- Finca -----
        builder.Entity<ConfiguracionFinca>()
            .HasOne(c => c.Finca).WithOne(f => f.Configuracion)
            .HasForeignKey<ConfiguracionFinca>(c => c.FincaId)
            .OnDelete(DeleteBehavior.Cascade);

        // ----- Lote -----
        builder.Entity<Lote>(e =>
        {
            e.HasIndex(l => new { l.FincaId, l.Codigo }).IsUnique();
            e.HasOne(l => l.Finca).WithMany(f => f.Lotes)
                .HasForeignKey(l => l.FincaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(l => l.LotePadre).WithMany(l => l.MicroLotes)
                .HasForeignKey(l => l.LotePadreId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PeriodoProductivo>()
            .HasOne(p => p.Finca).WithMany(f => f.PeriodosProductivos)
            .HasForeignKey(p => p.FincaId).OnDelete(DeleteBehavior.Cascade);

        // ----- Colaborador -----
        builder.Entity<Colaborador>().HasIndex(c => c.Identificacion).IsUnique();

        // ----- Labor -----
        builder.Entity<Labor>(e =>
        {
            e.HasOne(l => l.Lote).WithMany().HasForeignKey(l => l.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.PeriodoProductivo).WithMany().HasForeignKey(l => l.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<LaborColaborador>(e =>
        {
            e.HasKey(lc => new { lc.LaborId, lc.ColaboradorId });
            e.HasOne(lc => lc.Labor).WithMany(l => l.Colaboradores).HasForeignKey(lc => lc.LaborId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(lc => lc.Colaborador).WithMany(c => c.Labores).HasForeignKey(lc => lc.ColaboradorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ----- Enfermedades -----
        builder.Entity<RegistroEnfermedad>(e =>
        {
            e.HasOne(r => r.Lote).WithMany().HasForeignKey(r => r.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.TipoEnfermedad).WithMany(t => t.Registros).HasForeignKey(r => r.TipoEnfermedadId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.PeriodoProductivo).WithMany().HasForeignKey(r => r.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Tratamiento>(e =>
        {
            e.HasOne(t => t.RegistroEnfermedad).WithMany(r => r.Tratamientos).HasForeignKey(t => t.RegistroEnfermedadId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Insumo).WithMany().HasForeignKey(t => t.InsumoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ----- Producción -----
        builder.Entity<Recoleccion>(e =>
        {
            e.HasOne(r => r.Lote).WithMany().HasForeignKey(r => r.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.PeriodoProductivo).WithMany().HasForeignKey(r => r.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Colaborador).WithMany().HasForeignKey(r => r.ColaboradorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RegistroProduccion>(e =>
        {
            e.HasOne(r => r.Lote).WithMany().HasForeignKey(r => r.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.PeriodoProductivo).WithMany().HasForeignKey(r => r.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ----- Finanzas -----
        builder.Entity<Gasto>(e =>
        {
            e.HasOne(g => g.Lote).WithMany().HasForeignKey(g => g.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(g => g.PeriodoProductivo).WithMany().HasForeignKey(g => g.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<VentaCafe>(e =>
        {
            e.HasOne(v => v.Lote).WithMany().HasForeignKey(v => v.LoteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.PeriodoProductivo).WithMany().HasForeignKey(v => v.PeriodoProductivoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MovimientoInventario>()
            .HasOne(m => m.Insumo).WithMany(i => i.Movimientos)
            .HasForeignKey(m => m.InsumoId).OnDelete(DeleteBehavior.Cascade);

        // ----- Auditoría -----
        builder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasIndex(a => a.FechaHora);
            e.HasIndex(a => new { a.Entidad, a.EntidadId });
        });

        // ----- Precisión de columnas decimales -----
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampAuditFields()
    {
        var usuario = _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true
            ? _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)
              ?? _httpContextAccessor.HttpContext!.User.Identity!.Name
            : "sistema";
        var ahora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = ahora;
                entry.Entity.CreadoPor = usuario;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaModificacion = ahora;
                entry.Entity.ModificadoPor = usuario;
                entry.Property(nameof(AuditableEntity.FechaCreacion)).IsModified = false;
                entry.Property(nameof(AuditableEntity.CreadoPor)).IsModified = false;
            }
        }
    }
}
