using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
namespace Infraestructura.Data;
public class PuntoHalconDbContext : DbContext
{
 public PuntoHalconDbContext(DbContextOptions<PuntoHalconDbContext> options) : base(options) { }
 public DbSet<Usuario> Usuarios => Set<Usuario>();
 public DbSet<Evidencia> Evidencias => Set<Evidencia>();
 public DbSet<Mision> Misiones => Set<Mision>();
 public DbSet<Insignia> Insignias => Set<Insignia>();
 public DbSet<Progreso> Progresos => Set<Progreso>();
 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  base.OnModelCreating(modelBuilder);
  modelBuilder.Entity<Usuario>().HasIndex(x => x.Correo).IsUnique();
  modelBuilder.Entity<Evidencia>().HasOne(x => x.Usuario).WithMany(x => x.Evidencias).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
  modelBuilder.Entity<Progreso>().HasOne(x => x.Mision).WithMany(x => x.Progresos).HasForeignKey(x => x.MisionId).OnDelete(DeleteBehavior.Cascade);
  modelBuilder.Entity<Progreso>().HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
  modelBuilder.Entity<Insignia>().HasOne(x => x.Mision).WithMany(x => x.Insignias).HasForeignKey(x => x.MisionId).OnDelete(DeleteBehavior.SetNull);
 }
}
