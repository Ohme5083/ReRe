using ReRe.Data.Models;

namespace ReRe.Data.Role
{
    public interface IRoleRepository
    {
        Task<IEnumerable<RoleModel>> Get();
        Task<RoleModel?> Get(int id);
        Task<RoleModel?> Create(RoleModel role);
        Task<RoleModel?> Update(int id, RoleModel role);
        Task<bool> Delete(int id);
    }
}
