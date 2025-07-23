using ReRe.Data.Models;

namespace ReRe.Data.Role
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> Get();
        Task<UserModel?> Get(int id);
        Task<UserModel?> GetByMail(string mail);
        Task<UserModel?> Create(UserModel role);
        Task<UserModel?> Update(int id, UserModel user);
        Task<bool> Delete(int id);
    }
}
