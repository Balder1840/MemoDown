using Coravel.Invocable;

namespace MemoDown.Services.Background
{
    public class GithubAutoSyncService : IInvocable
    {
        private readonly GithubSyncService _githubSyncService;

        public GithubAutoSyncService(GithubSyncService githubSyncService)
        {
            _githubSyncService = githubSyncService;
        }

        public Task Invoke()
        {
            return _githubSyncService.SyncToGithub($"synchronized at {DateTime.Now:HH:mm:ss, dddd, MMMM d, yyyy}");
        }
    }
}
