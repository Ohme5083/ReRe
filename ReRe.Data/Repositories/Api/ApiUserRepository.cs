using System;
using System.Collections.Generic;
using System.Linq;
using ReRe.Data.Models;
using ReRe.Data.Repositories;
using System.Net.Http.Json;
using ReRe.Data.Role;

namespace ReRe.Data.Repositories.Api
{
    public class ApiUserRepository : ApiContext, IUserRepository
    {
        private string url = "https://localhost:5001/api/Utilisateur";

        public async Task<UserModel?> Create(UserModel user)
        {
            var response = await client.PostAsJsonAsync(url, user);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        public async Task<bool> Delete(int id)
        {
            var response = await client.DeleteAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return true;
        }

        public async Task<IEnumerable<UserModel>> Get()
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserModel>>();

        }
        public async Task<UserModel?> Get(int id)
        {
            var response = await client.GetAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        public async Task<UserModel?> GetByMail(string mail)
        {
            var response = await client.GetAsync($"{url}/Mail/{mail}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }

        public async Task<UserModel?> Update(int id, UserModel user)
        {
            var response = await client.PutAsJsonAsync($"{url}/{id}", user);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }
    }
}
