namespace Application.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> GetBotReplyAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
