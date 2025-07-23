using Microsoft.AspNetCore.Http;
using System.Text;

namespace ReRe.Test
{
    public class DummySession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }
    }

    public static class SessionExtensions
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
            if (session.TryGetValue(key, out var value) && value.Length == 4)
            {
                return BitConverter.ToInt32(value, 0);
            }
            return null;
        }
    }
}
