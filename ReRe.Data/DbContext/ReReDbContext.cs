using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReRe.Data.Models;


namespace ReRe.Data.DbContext;
public class ReReDbContext : IdentityDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TypeModel>().HasData(
        new TypeModel { Id = 1, Libelle = "Privée" },
        new TypeModel { Id = 2, Libelle = "Partagée" },
        new TypeModel { Id = 3, Libelle = "Publique" }
        );

        modelBuilder.Entity<RoleModel>().HasData(
        new RoleModel { Id = 1, Libelle = "Utilisateur" },
        new RoleModel { Id = 2, Libelle = "Modérateur" },
        new RoleModel { Id = 3, Libelle = "Administrateur" }
        );

        // Définir la table d'association entre Ressource et Utilisateur
        modelBuilder.Entity<RessourceModel>()
            .HasMany(r => r.Utilisateurs)
            .WithMany(u => u.RessourcesLiked)
            .UsingEntity(j => j.ToTable("RessourceUtilisateurs")); // nom de table d'association personnalisable
    }
    public virtual DbSet<CommentaireModel> Commentaires { get; set; } = null!;
    public virtual DbSet<RessourceModel> Ressources { get; set; } = null!;
    public virtual DbSet<UserModel> Utilisateurs { get; set; } = null!;
    public virtual DbSet<RoleModel> RoleModels { get; set; } = null!;
    public virtual DbSet<TypeModel> TypeModel { get; set; } = null!;

    public ReReDbContext(DbContextOptions<ReReDbContext> options)
        : base(options)
    {
    }
}
