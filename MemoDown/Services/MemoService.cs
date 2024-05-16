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
        private MemoItem? _selectedSidebarMemo;

        public MemoItem RootMemo => _store.Memo;
        public MemoItem? SelectedMemo => _selectedMemo;
        public MemoItem? SelectedSidebarMemo => _selectedSidebarMemo ?? SelectedMemo?.Parent ?? RootMemo;

        public event Action OnSelectedMemoChanged;
        public event Action OnSelectedSidebarMemoChanged;

        #region Constructor & Initializer
        public MemoService(MemoStore store, IOptions<MemoDownOptions> options)
        {
            _store = store;
            _options = options;

            OnSelectedMemoChanged = default!;
            OnSelectedSidebarMemoChanged = default!;

            InitializeSelectedMemo(RootMemo);
            if (_selectedMemo == null)
            {
                InitializeSelectedMemo(RootMemo, true);
            }

            _selectedMemo ??= GetSelectedMemoFromSidebar(SelectedSidebarMemo);
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
        #endregion

        #region State Container
        public void NotifySelectedMemoChanged() => OnSelectedMemoChanged?.Invoke();
        public void NotifySelectedSidebarMemoChanged() => OnSelectedSidebarMemoChanged?.Invoke();

        public void SetSelectedMemo(MemoItem? memo)
        {
            _selectedMemo = memo;
            NotifySelectedMemoChanged();
        }

        public void SetSelectedSidebarMemo(MemoItem? memo)
        {
            _selectedSidebarMemo = memo;
            NotifySelectedSidebarMemoChanged();
        }

        public void SetSelectedMemoFromSidebar()
        {
            _selectedMemo = GetSelectedMemoFromSidebar(_selectedSidebarMemo);
            NotifySelectedMemoChanged();
        }

        private MemoItem? GetSelectedMemoFromSidebar(MemoItem? sidebarMemo)
        {
            var seletedMemo = sidebarMemo?.Children?.FirstOrDefault(m => !m.IsDirectory);
            if (seletedMemo == null)
            {
                seletedMemo = sidebarMemo?.Children?.FirstOrDefault();
            }

            SetSelectedMemo(seletedMemo);
            return seletedMemo;
        }

        #endregion

        #region Memu Handler
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

        #endregion
    }
}
