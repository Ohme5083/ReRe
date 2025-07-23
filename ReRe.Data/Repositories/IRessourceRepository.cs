using ReRe.Data.Models;

namespace ReRe.Data.Repositories
{
    //Possibilités CRUD sur le modèle Ressource
    public interface IRessourceRepository
    {
        Task<IEnumerable<RessourceModel>> Get();
        Task<RessourceModel?> Get(int id);
        Task<RessourceModel?> Create(RessourceModel ressource);
        Task<RessourceModel?> Update(int id, RessourceModel ressource);
        Task<bool> Delete(int id);
    }
}
