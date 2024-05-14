using MemoDown.Constants;
using MemoDown.Models;
using MemoDown.Options;
using MemoDown.Store;
using Microsoft.Extensions.Options;

namespace MemoDown.Services
{
    public class MemoService
    {
        private readonly MemoStore _store;
        private readonly IOptions<MemoDownOptions> _options;
        private MemoItem? _selectedMemo;

        public MemoItem? SelectedMemo => _selectedMemo;
        public MemoItem? SelectedSidebarMemo => SelectedMemo?.Parent;
        public MemoService(MemoStore store, IOptions<MemoDownOptions> options)
        {
            _store = store;
            _options = options;

            InitializeSelectedMemo(RootMemo);
            if (_selectedMemo == null)
            {
                InitializeSelectedMemo(RootMemo, true);
            }
        }

        public MemoItem RootMemo => _store.Memo;

        public void SetSelectedMemo(MemoItem? memo)
        {
            _selectedMemo = memo;
        }

        public MemoItem? GetSelectedMemoFromSidebar(MemoItem? sidebarMemo)
        {
            var seletedMemo = sidebarMemo?.Children?.FirstOrDefault(m => !m.IsDirectory);
            if (seletedMemo == null)
            {
                seletedMemo = sidebarMemo?.Children?.FirstOrDefault();
            }

            SetSelectedMemo(seletedMemo);
            return seletedMemo;
        }

        private void InitializeSelectedMemo(MemoItem memo, bool includingDirectory = false)
        {
            if (memo == null || memo.Children == null)
            {
                return;
            }

            if (memo.Children.Any(m => includingDirectory || !m.IsDirectory))
            {
                _selectedMemo = memo.Children.First(m => includingDirectory || !m.IsDirectory);
                return;
            }

            if (memo.Children.Any(m => m.IsDirectory))
            {
                InitializeSelectedMemo(memo.Children.First(m => m.IsDirectory), includingDirectory);
            }
        }

        public MemoItem? CreateFile(MemoItem? selection, bool createInParent = false)
        {
            string? dir = null;
            MemoItem? newMemo = null;

            if (createInParent)
            {
                dir = selection?.Parent?.FullPath;
            }
            else
            {
                dir = selection?.FullPath;
            }

            dir ??= _options.Value.MemoDir;

            var fileIndex = 0;
            var fileName = $"{MemoConstants.NEW_FILE}{MemoConstants.FILE_EXTENSION}";
            var fullName = Path.Combine(dir, fileName);
            while (File.Exists(fullName))
            {
                fileIndex++;
                fileName = $"{MemoConstants.NEW_FILE}{fileIndex}{MemoConstants.FILE_EXTENSION}";
                fullName = Path.Combine(dir, fileName);
            }

            if (!File.Exists(fullName))
            {
                File.Create(fullName);
                if (createInParent)
                {
                    newMemo = new MemoItem
                    {
                        Name = fileName,
                        FullPath = fullName,
                        IsDirectory = false,
                        Parent = selection?.Parent,
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now,
                    };
                    if (selection != null && selection.Parent != null && selection?.Parent?.Children == null)
                    {
                        selection!.Parent.Children = new List<MemoItem>();
                    }
                    selection?.Parent?.Children?.Add(newMemo);
                }
                else
                {
                    newMemo = new MemoItem
                    {
                        Name = fileName,
                        FullPath = fullName,
                        IsDirectory = false,
                        Parent = selection,
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now,
                    };

                    if (selection != null && selection?.Children == null)
                    {
                        selection!.Children = new List<MemoItem>();
                    }
                    selection?.Children?.Add(newMemo);
                }
            }

            return newMemo;
        }

        public MemoItem? CreateDirectory(MemoItem? selection, bool createInParent = false)
        {
            string? dir = null;
            MemoItem? newMemo = null;

            if (createInParent)
            {
                dir = selection?.Parent?.FullPath;
            }
            else
            {
                dir = selection?.FullPath;
            }

            dir ??= _options.Value.MemoDir;

            var dirIndex = 0;
            var dirName = MemoConstants.NEW_DIRECTORY;
            var fullName = Path.Combine(dir, dirName);
            while (Directory.Exists(fullName))
            {
                dirIndex++;
                dirName = $"{MemoConstants.NEW_DIRECTORY}{dirIndex}";
                fullName = Path.Combine(dir, dirName);
            }

            if (!Directory.Exists(fullName))
            {
                Directory.CreateDirectory(fullName);
                if (createInParent)
                {
                    newMemo = new MemoItem
                    {
                        Name = dirName,
                        FullPath = fullName,
                        IsDirectory = true,
                        Parent = selection?.Parent,
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now,
                    };
                    if (selection != null && selection.Parent != null && selection?.Parent?.Children == null)
                    {
                        selection!.Parent.Children = new List<MemoItem>();
                    }
                    selection?.Parent?.Children?.Add(newMemo);
                }
                else
                {
                    newMemo = new MemoItem
                    {
                        Name = dirName,
                        FullPath = fullName,
                        IsDirectory = true,
                        Parent = selection,
                        CreatedAt = DateTime.Now,
                        LastModifiedAt = DateTime.Now,
                    };
                    if (selection != null && selection?.Children == null)
                    {
                        selection!.Children = new List<MemoItem>();
                    }
                    selection?.Children?.Add(newMemo);
                }
            }

            return newMemo;
        }
    }
}
