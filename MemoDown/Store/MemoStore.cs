using MemoDown.Models;
using MemoDown.Options;
using Microsoft.Extensions.Options;

namespace MemoDown.Store
{
    public class MemoStore
    {
        private readonly IOptions<MemoDownOptions> _options;
        private MemoItem _rootMemo;
        public MemoItem Memo => _rootMemo;

        public MemoStore(IOptions<MemoDownOptions> options)
        {
            _options = options;
            _rootMemo = null!;
        }

        public void Initialzie()
        {
            var memoDir = _options.Value.MemoDir;
            if (!string.IsNullOrWhiteSpace(memoDir) && Directory.Exists(memoDir))
            {
                var currentDir = new DirectoryInfo(memoDir);

                _rootMemo = new MemoItem
                {
                    Name = currentDir.Name,
                    FullPath = currentDir.FullName,
                    IsDirectory = true,
                    CreatedAt = currentDir.CreationTime,
                    LastModifiedAt = currentDir.LastWriteTime,
                    Parent = null,
                };

                _rootMemo.Children = GetChildrenMemosFromDir(memoDir, _rootMemo);
            }
        }

        private List<MemoItem> GetChildrenMemosFromDir(string dir, MemoItem? parent)
        {
            var memos = new List<MemoItem>();
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return memos;
            }

            var subDirs = Directory.GetDirectories(dir, "*", searchOption: SearchOption.TopDirectoryOnly);
            foreach (var subDir in subDirs.Where(_ => !_.EndsWith("_docs", StringComparison.OrdinalIgnoreCase)))
            {
                var dirInfo = new DirectoryInfo(subDir);
                var memo = new MemoItem
                {
                    Name = dirInfo.Name,
                    FullPath = dirInfo.FullName,
                    IsDirectory = true,
                    CreatedAt = dirInfo.CreationTime,
                    LastModifiedAt = dirInfo.LastWriteTime,
                    Parent = parent,
                };

                memo.Children = GetChildrenMemosFromDir(subDir, memo);

                memos.Add(memo);
            }

            var files = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var memo = new MemoItem
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    IsDirectory = false,
                    CreatedAt = fileInfo.CreationTime,
                    LastModifiedAt = fileInfo.LastWriteTime,
                    Parent = parent,
                };

                memos.Add(memo);
            }

            return memos;
        }
    }
}
