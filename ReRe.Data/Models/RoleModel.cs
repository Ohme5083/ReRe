using System.ComponentModel.DataAnnotations;

namespace ReRe.Data.Models
{
    public class RoleModel
    {
        public int Id { get; set; }

        [StringLength(40)]
        public string Libelle { get; set; } = null!;
    }
}
