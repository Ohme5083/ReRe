using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReRe.Data.Models;

public class UserModel
{
    public int Id { get; set; }
    [StringLength(40)]
    public string Nom { get; set; } = null!;
    [StringLength(400)]
    public string Prenom { get; set; } = null!;
    public string? Email { get; set; }

    [StringLength(80)]
    public string mot_de_passe { get; set; } = null!;

    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
    public virtual RoleModel? Role { get; set; }
    public List<RessourceModel>? RessourcesLiked { get; set; } = new List<RessourceModel>();
}
