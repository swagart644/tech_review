namespace Stargate.Server.Logging
{
    public interface ILoggingWrapper
    {
        Task Log(string message, bool isSuccess);
    }
}
