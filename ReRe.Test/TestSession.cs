using Microsoft.AspNetCore.Http;
using System.Text;

namespace ReRe.Test
{
    /// <summary>
    /// Implémentation de session pour les tests
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) 
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) 
            => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, out byte[]? value) 
            => _sessionStorage.TryGetValue(key, out value);
    }

    /// <summary>
    /// Extensions pour faciliter l'utilisation des sessions dans les tests
    /// </summary>
    public static class TestSessionExtensions
    {
        public static void SetString(this ISession session, string key, string value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(value));
        }

        public static string? GetString(this ISession session, string key)
        {
            if (session.TryGetValue(key, out var value))
            {
                return Encoding.UTF8.GetString(value);
            }
            return null;
        }

        public static void SetInt32(this ISession session, string key, int value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static int? GetInt32(this ISession session, string key)
        {
            if (session.TryGetValue(key, out var value) && value != null && value.Length >= 4)
            {
                // Correction : s'assurer que nous lisons correctement les 4 bytes
                return BitConverter.ToInt32(value, 0);
            }
            return null;
        }
    }
}
