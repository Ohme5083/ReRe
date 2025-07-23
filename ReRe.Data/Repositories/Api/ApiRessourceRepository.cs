using ReRe.Data.Models;
using System.Net.Http.Json;

namespace ReRe.Data.Repositories.Api
{
    public class ApiRessourceRepository : ApiContext, IRessourceRepository
    {
        private string url = "https://localhost:5001/api/ressources";
        public async Task<RessourceModel?> Create(RessourceModel ressource)
        {
            var response = await client.PostAsJsonAsync(url, ressource);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RessourceModel>();
        }

        public async Task<bool> Delete(int id)
        {
            var response = await client.DeleteAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return true;
        }

        public async Task<IEnumerable<RessourceModel>> Get()
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<IEnumerable<RessourceModel>>();
            
        }
        public async Task<RessourceModel?> Get(int id)
        {
            var response = await client.GetAsync($"{url}/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RessourceModel>();
        }

        public async Task<RessourceModel?> Update(int id, RessourceModel ressource)
        {
            var response = await client.PutAsJsonAsync($"{url}/{id}",ressource);
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            return await response.Content.ReadFromJsonAsync<RessourceModel>();
        }
    }
}
