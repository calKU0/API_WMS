using System.Threading;
using System.Threading.Tasks;
using APIWMS.Data.Enums;
using APIWMS.Interfaces;
using Microsoft.Extensions.Hosting;

public class LoginService : IHostedService
{
    private readonly IXlApiService _xlApiService;
    private readonly ILogger _logger;

    public LoginService(IXlApiService xlApiService, ILogger<LoginService> logger)
    {
        _xlApiService = xlApiService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var loginResult = _xlApiService.Login();
        if (loginResult != 0)
        {
            _logger.LogError($"Error when trying to log in to XL. Error code {loginResult}");
        }
        _logger.LogInformation("Logged in to XL");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        int logoutResult = _xlApiService.Logout();
        if (logoutResult != 0)
        {
            _logger.LogError($"Error when tring to log out from XL. Error code: {logoutResult}");
        }
        _logger.LogInformation("Logged out from XL");
        return Task.CompletedTask;
    }
}
