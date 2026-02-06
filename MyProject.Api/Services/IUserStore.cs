namespace MyProject.Api.Services;

public interface IUserStore
{
    bool TryAdd(string userName, string passwordHash);
    bool TryValidate(string userName, string passwordHash);
    bool Exists(string userName);
    bool TryAddPending(string userName, string passwordHash);
    bool TryApprove(string userName);
    bool TryValidatePending(string userName, string passwordHash);
    bool IsPending(string userName);
    IReadOnlyCollection<string> GetPendingUsers();
}

