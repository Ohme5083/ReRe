using ReRe.Data.Models;

namespace ReRe.Data.Repositories
{
    public interface ICommentaireRepository
    {
        Task<IEnumerable<CommentaireModel>> Get();
        Task<CommentaireModel?> Get(int id);
        Task<CommentaireModel?> Create(CommentaireModel commentaire);
        Task<CommentaireModel?> Update(int id, CommentaireModel commentaire);
        Task<bool> Delete(int id);
        Task<IEnumerable<CommentaireModel>> GetByRessourceId(int ressourceId);
    }
}
