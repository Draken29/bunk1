using System.Collections.Concurrent;
using System.Linq;

namespace MyProject.Api.Services;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, string> _users = new();
    private readonly ConcurrentDictionary<string, string> _pendingUsers = new();

    public bool TryAdd(string userName, string passwordHash)
    {
        return _users.TryAdd(userName, passwordHash);
    }

    public bool TryValidate(string userName, string passwordHash)
    {
        return _users.TryGetValue(userName, out var storedHash) &&
               storedHash == passwordHash;
    }

    public bool Exists(string userName)
    {
        return _users.ContainsKey(userName);
    }

    public bool TryAddPending(string userName, string passwordHash)
    {
        if (_users.ContainsKey(userName))
        {
            return false;
        }

        return _pendingUsers.TryAdd(userName, passwordHash);
    }

    public bool TryApprove(string userName)
    {
        if (!_pendingUsers.TryRemove(userName, out var passwordHash))
        {
            return false;
        }

        return _users.TryAdd(userName, passwordHash);
    }

    public bool TryValidatePending(string userName, string passwordHash)
    {
        return _pendingUsers.TryGetValue(userName, out var storedHash) &&
               storedHash == passwordHash;
    }

    public bool IsPending(string userName)
    {
        return _pendingUsers.ContainsKey(userName);
    }

    public IReadOnlyCollection<string> GetPendingUsers()
    {
        return _pendingUsers.Keys.ToArray();
    }
}

