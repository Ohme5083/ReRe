namespace ReRe.Data.Models
{
    public class TypeModel
    {
        public int Id { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public virtual ICollection<RessourceModel>? Ressources { get; set; }
    }
}
