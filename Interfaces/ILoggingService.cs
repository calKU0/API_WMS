namespace APIWMS.Interfaces
{
    public interface ILoggingService
    {
        Task LogErrorAsync(string action, bool success, string errorMessage = null, Dictionary<string, int> fields = null, Exception ex = null);
    }
}
