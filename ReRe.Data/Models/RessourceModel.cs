using System.ComponentModel.DataAnnotations.Schema;

namespace ReRe.Data.Models;

public class RessourceModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;

    [ForeignKey(nameof(Type))]
    public int TypeId { get; set; }
    public virtual TypeModel? Type { get; set; }
    public string? Creator { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public virtual ICollection<CommentaireModel>? Commentaires { get; set; }
    public virtual ICollection<UserModel> Utilisateurs { get; set; } = new List<UserModel>();


}
