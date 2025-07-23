using ReRe.Data.Models;
using ReRe.Data.Repositories;
using ReRe.Data.Role;
using System.Net.Http.Json;

namespace ReRe.Data.Repositories.Api
{
    public class ApiRoleRepository : ApiContext, IRoleRepository
    {
        private string url = "https://localhost:5001/api/Role";

        public async Task<RoleModel?> Create(RoleModel role)
        {
            var response = await client.PostAsJsonAsync(url, role);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RoleModel>();
        }

        public async Task<bool> Delete(int id)
        {
            var response = await client.DeleteAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return true;
        }

        public async Task<IEnumerable<RoleModel>> Get()
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<IEnumerable<RoleModel>>();

        }
        public async Task<RoleModel?> Get(int id)
        {
            var response = await client.GetAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RoleModel>();
        }

        public async Task<RoleModel?> Update(int id, RoleModel role)
        {
            var response = await client.PutAsJsonAsync($"{url}/{id}", role);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RoleModel>();
        }
    }
}
