using Coravel.Invocable;

namespace MemoDown.Services.Background
{
    public class GithubAutoSyncService : IInvocable
    {
        private readonly GithubSyncService _githubSyncService;
        private readonly ILogger _logger;

        public GithubAutoSyncService(GithubSyncService githubSyncService, ILogger<GithubAutoSyncService> logger)
        {
            _githubSyncService = githubSyncService;
            _logger = logger;
        }

        public Task Invoke()
        {
            try
            {
                return _githubSyncService.SyncToGithub($"synchronized at {DateTime.Now:HH:mm:ss, dddd, MMMM d, yyyy}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync to github");
            }

            return Task.CompletedTask;
        }
    }
}
