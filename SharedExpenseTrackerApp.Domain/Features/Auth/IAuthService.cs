using SharedExpenseTrackerApp.Domain.Models.Auth;
using SharedExpenseTrackerApp.Domain.Shared;

namespace SharedExpenseTrackerApp.Domain.Features.Auth;

public interface IAuthService
{
    Task<Result<LoginRespModel>> LoginAsync(LoginReqModel request, CancellationToken ct = default);
    Task<Result<string>> RegisterAsync(RegisterReqModel request, CancellationToken ct = default);
    Task<Result<UserProfileRespModel>> GetCurrentUserAsync(long userId, CancellationToken ct = default);
}
