using Cinema.Domain.Shared;

namespace Cinema.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<Guid>> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<Result<string>> LoginAsync(string email, string password);
}