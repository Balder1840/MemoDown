using MemoDown.Constants;
using MemoDown.Extensions;
using MemoDown.Options;
using Microsoft.Extensions.Options;
using Octokit;
using System.Security.Cryptography;
using System.Text;

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
                        var blob = await CreateBlob(File.ReadAllText(change.FullPath));
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
        private const int ApiCallLimit = 5000;
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
            var tree = await _githubClient!.Git.Tree.GetRecursive(_repoOwner, _repoName, _branch);
            return tree.Tree;
        }

        private Task<Reference> GetHeadRef()
        {
            return _githubClient!.Git.Reference.Get(_repoOwner, _repoName, _headRef);
        }

        private Task<Commit> GetCommit(string sha)
        {
            return _githubClient!.Git.Commit.Get(_repoOwner, _repoName, sha);
        }

        private Task<BlobReference> CreateBlob(string content)
        {
            var blob = new NewBlob { Encoding = EncodingType.Utf8, Content = content };
            return _githubClient!.Git.Blob.Create(_repoOwner, _repoName, blob);
        }
        private Task<BlobReference> CreateBlob(byte[] array)
        {
            var blob = new NewBlob { Encoding = EncodingType.Base64, Content = Convert.ToBase64String(array) };
            return _githubClient!.Git.Blob.Create(_repoOwner, _repoName, blob);
        }

        private Task<TreeResponse> CreateTree(GitReference baseTree, IEnumerable<NewTreeItem> treeItems)
        {
            var newTree = new NewTree { BaseTree = baseTree.Sha };

            foreach (var item in treeItems)
            {
                newTree.Tree.Add(item);
            }

            return _githubClient!.Git.Tree.Create(_repoOwner, _repoName, newTree);
        }

        private Task<Commit> CreateCommit(string commitMessage, string treeSha, string parentSha)
        {
            var newCommit = new NewCommit(commitMessage, treeSha, parentSha);
            return _githubClient!.Git.Commit.Create(_repoOwner, _repoName, newCommit);
        }

        // Update HEAD with the commit
        private Task<Reference> UpdateHeadRef(string commitSha)
        {
            return _githubClient!.Git.Reference.Update(_repoOwner, _repoName, _headRef, new ReferenceUpdate(commitSha));
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
