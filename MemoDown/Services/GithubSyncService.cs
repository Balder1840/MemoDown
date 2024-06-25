using MemoDown.Constants;
using MemoDown.Extensions;
using MemoDown.Options;
using Microsoft.Extensions.Options;
using Octokit;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;

namespace MemoDown.Services
{
    public class GithubSyncService
    {
        private string? _repoName;
        private string? _repoOwner;
        private string? _branch;
        private string _headRef;
        private readonly GitHubClient _githubClient;
        private readonly IOptions<MemoDownOptions> _options;
        private readonly PartitionedRateLimiter<string> _rateLimiter;
        private const string RateLimiterPartitionKey = "MEMODOWN";

        public GithubSyncService(IOptions<MemoDownOptions> options)
        {
            _options = options;

            _repoName = options.Value.Github.RepoName;
            _repoOwner = options.Value.Github.RepoOwner;
            _branch = options.Value.Github.Branch;
            _headRef = options.Value.Github.HeadRef;

            var credential = new Credentials(options.Value.Github.PAT);
            _githubClient = new GitHubClient(new ProductHeaderValue(MemoDownOptions.Key));
            _githubClient.Credentials = credential;

            // github allows 900r/min, 5000r/hour
            _rateLimiter = PartitionedRateLimiter.CreateChained(
                    // 5000r/1hour
                    PartitionedRateLimiter.Create<string, string>(_ => RateLimitPartition.GetFixedWindowLimiter(RateLimiterPartitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5000,
                        QueueLimit = 5000,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true,
                        Window = TimeSpan.FromHours(1),
                    })),
                    // 4r/1s
                    PartitionedRateLimiter.Create<string, string>(_ => RateLimitPartition.GetTokenBucketLimiter(RateLimiterPartitionKey, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 1,
                        QueueLimit = 5000,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromMilliseconds(250),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true,
                    }))
                //PartitionedRateLimiter.Create<string, string>(_ => RateLimitPartition.GetSlidingWindowLimiter(RateLimiterPartitionKey, _ => new SlidingWindowRateLimiterOptions
                //{
                //    PermitLimit = 4,
                //    QueueLimit = 5000,
                //    SegmentsPerWindow = 4,
                //    Window = TimeSpan.FromSeconds(1),
                //    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                //    AutoReplenishment = true,
                //}))
                );
        }

        public async Task SyncToGithub(string commitMessage)
        {
            var localFiles = GetLocalFiles().ToList();

            var remoteFiles = await GetRemoteFiles();

            var deleteFiles = remoteFiles.ExceptBy(localFiles.Select(l => l.RelateivePath), r => r.RelateivePath).ForEach(d => d.Mode = BlobObjectMode.Deleted);
            var newFiles = localFiles.ExceptBy(remoteFiles.Select(r => r.RelateivePath), l => l.RelateivePath).ForEach(d => d.Mode = BlobObjectMode.Added);
            var changedFiles = localFiles.Where(l => remoteFiles.Any(r => r.RelateivePath == l.RelateivePath && r.Sha != l.Sha)).ForEach(d => d.Mode = BlobObjectMode.Modified);

            var changes = new List<BlobObject>();
            changes.AddRange(deleteFiles);
            changes.AddRange(newFiles);
            changes.AddRange(changedFiles);

            if (changes.Any())
            {
                await SyncToGithubInternal(commitMessage, changes);
            }
        }

        #region Local
        private IEnumerable<BlobObject> GetLocalFiles()
        {
            var memoDir = _options.Value.MemoDir;

            if (Directory.Exists(memoDir))
            {
                return Directory.GetFiles(memoDir, "*", SearchOption.AllDirectories)
                    .Select(f => new BlobObject
                    {
                        Extension = Path.GetExtension(f),
                        FullPath = f,
                        Name = Path.GetFileName(f),
                        RelateivePath = GetLinuxStyleRelativePath(memoDir, f),
                        Sha = GetFileSha(f)
                    })
                    .OrderBy(f => f.RelateivePath);
            }

            return Array.Empty<BlobObject>();
        }

        private const char LinuxStyleDirectorySeparatorChar = '/';
        private string GetLinuxStyleRelativePath(string basePath, string path)
        {
            var relativePath = Path.GetRelativePath(basePath, path);
            return string.Join(LinuxStyleDirectorySeparatorChar, relativePath.Split(Path.DirectorySeparatorChar));
        }

        private string GetFileSha(string file)
        {
            var fileBytes = File.ReadAllBytes(file).AsSpan().TrimStart(Encoding.UTF8.Preamble);

            var fileSize = fileBytes.Length;

            var prefix = $"blob {fileSize}\0";
            var prefixBytes = Encoding.UTF8.GetBytes(prefix);

            using var sha1 = SHA1.Create();
            sha1.TransformBlock(prefixBytes, 0, prefixBytes.Length, null, 0);
            sha1.TransformFinalBlock(fileBytes.ToArray(), 0, fileSize);

            string hash = BitConverter.ToString(sha1.Hash!).Replace("-", "").ToLowerInvariant();
            return hash;

        }
        #endregion

        #region Github

        private async Task SyncToGithubInternal(string commitMessage, IReadOnlyList<BlobObject> changes)
        {
            var remaining = _githubClient!.GetLastApiInfo()?.RateLimit.Remaining;

            var headRef = await GetHeadRef();
            var lastestCommit = await GetCommit(headRef.Object.Sha);

            //1. Create the blob(s) corresponding to your file(s), and then build the sub trees         
            var subTrees = new List<NewTreeItem>();
            foreach (var change in changes)
            {
                if (change.Mode == BlobObjectMode.Added || change.Mode == BlobObjectMode.Modified)
                {
                    if (change.Extension == MemoConstants.FILE_EXTENSION)
                    {
                        //Stopwatch sw = Stopwatch.StartNew();
                        //sw.Start();
                        var blob = await CreateBlob(File.ReadAllText(change.FullPath));
                        //sw.Stop();
                        var tree = new NewTreeItem
                        {
                            Path = change.RelateivePath,
                            Type = TreeType.Blob,
                            Mode = DefaultMode,
                            Sha = blob.Sha,
                        };
                        subTrees.Add(tree);
                    }
                    else
                    {
                        var blob = await CreateBlob(File.ReadAllBytes(change.FullPath));
                        var tree = new NewTreeItem
                        {
                            Path = change.RelateivePath,
                            Type = TreeType.Blob,
                            Mode = DefaultMode,
                            Sha = blob.Sha,
                        };
                        subTrees.Add(tree);
                    }
                }
                else if (change.Mode == BlobObjectMode.Deleted)
                {
                    var tree = new NewTreeItem
                    {
                        Path = change.RelateivePath,
                        Type = TreeType.Blob,
                        Mode = DefaultMode,
                        Sha = string.Empty, // indicate delete
                    };
                    subTrees.Add(tree);
                }
            }

            //2. Create a new tree
            var newTree = await CreateTree(lastestCommit.Tree, subTrees);

            //3. Create the commit with the SHAs of the tree and the reference of master branch
            var commit = await CreateCommit(commitMessage, newTree.Sha, headRef.Object.Sha);

            //4. Update the HEAD reference of main branch with the SHA of the commit
            await UpdateHeadRef(commit.Sha);
        }

        private const string DefaultMode = "100644";
        private async Task<IEnumerable<BlobObject>> GetRemoteFiles()
        {
            var trees = await GetRemoteTrees();
            return trees.Where(t => t.Type.Value == TreeType.Blob).Select(t => new BlobObject
            {
                Extension = Path.GetExtension(t.Path),
                FullPath = t.Path,
                Name = Path.GetFileName(t.Path),
                RelateivePath = t.Path,
                Sha = t.Sha
            });
        }

        private async Task<IReadOnlyList<TreeItem>> GetRemoteTrees()
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            var tree = await _githubClient!.Git.Tree.GetRecursive(_repoOwner, _repoName, _branch);
            return tree.Tree;
        }

        private async Task<Reference> GetHeadRef()
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            return await _githubClient!.Git.Reference.Get(_repoOwner, _repoName, _headRef);
        }

        private async Task<Commit> GetCommit(string sha)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            return await _githubClient!.Git.Commit.Get(_repoOwner, _repoName, sha);
        }

        private async Task<BlobReference> CreateBlob(string content)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            var blob = new NewBlob { Encoding = EncodingType.Utf8, Content = content };
            return await _githubClient!.Git.Blob.Create(_repoOwner, _repoName, blob);
        }
        private async Task<BlobReference> CreateBlob(byte[] array)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            var blob = new NewBlob { Encoding = EncodingType.Base64, Content = Convert.ToBase64String(array) };
            return await _githubClient!.Git.Blob.Create(_repoOwner, _repoName, blob);
        }

        private async Task<TreeResponse> CreateTree(GitReference baseTree, IEnumerable<NewTreeItem> treeItems)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);

            var newTree = new NewTree { BaseTree = baseTree.Sha };

            foreach (var item in treeItems)
            {
                newTree.Tree.Add(item);
            }

            return await _githubClient!.Git.Tree.Create(_repoOwner, _repoName, newTree);
        }

        private async Task<Commit> CreateCommit(string commitMessage, string treeSha, string parentSha)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            var newCommit = new NewCommit(commitMessage, treeSha, parentSha);
            return await _githubClient!.Git.Commit.Create(_repoOwner, _repoName, newCommit);
        }

        // Update HEAD with the commit
        private async Task<Reference> UpdateHeadRef(string commitSha)
        {
            using var _ = await _rateLimiter.AcquireAsync(RateLimiterPartitionKey, 1);
            return await _githubClient!.Git.Reference.Update(_repoOwner, _repoName, _headRef, new ReferenceUpdate(commitSha));
        }

        #endregion

        #region Model
        internal class BlobObject
        {
            public string Name { get; set; } = default!;
            public string FullPath { get; set; } = default!;
            public string RelateivePath { get; set; } = default!;
            public string Extension { get; set; } = default!;
            public string Sha { get; set; } = default!;

            public BlobObjectMode Mode { get; set; }
        }

        internal enum BlobObjectMode
        {
            Added = 1,
            Modified = 2,
            Deleted = 3
        }
        #endregion
    }
}
