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
            _logger.LogError($"Nie udało się zalogować do Xl'a. Kod błędu {loginResult}");
        }
        _logger.LogInformation("Zalogowano do Xl'a");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        int logoutResult = _xlApiService.Logout();
        if (logoutResult != 0)
        {
            _logger.LogError($"Nie udało się wylogować z Xl'a. Kod błędu {logoutResult}");
        }
        _logger.LogInformation("Wylogowano z Xl'a");
        return Task.CompletedTask;
    }
}
