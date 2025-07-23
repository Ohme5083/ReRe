using System.ComponentModel.DataAnnotations.Schema;

namespace ReRe.Data.Models;

public class CommentaireModel
{
    public int Id { get; set; }

    [ForeignKey(nameof(UserModel))]
    public int? UserId { get; set; }
    public virtual UserModel? UserModel { get; set; }
    public string? Comment { get; set; }

    [ForeignKey(nameof(RessourceModel))]
    public int RessourceId { get; set; }
    public virtual RessourceModel? RessourceModel { get; set; }

    [ForeignKey(nameof(CommentaireParent))]
    public int? CommentaireId { get; set; }
    public virtual CommentaireModel? CommentaireParent { get; set; }
    public virtual ICollection<CommentaireModel>? CommentairesEnfants { get; set; }
    public DateTime Date { get; set; }
}
