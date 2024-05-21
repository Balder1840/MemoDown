using Blazor.Cherrydown.FileUpload;
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
        public MemoItem? CreateFile(MemoItem? selection, string? newFileName, bool createInParent = false)
        {
            string? dir = null;
            MemoItem? newMemo = null;
            newFileName ??= MemoConstants.NEW_FILE;

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
            var fileName = $"{newFileName}{MemoConstants.FILE_EXTENSION}";
            var fullName = Path.Combine(dir, fileName);
            while (File.Exists(fullName))
            {
                fileIndex++;
                fileName = $"{newFileName}{fileIndex}{MemoConstants.FILE_EXTENSION}";
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

        public MemoItem? CreateDirectory(MemoItem? selection, string? newDirName, bool createInParent = false)
        {
            string? dir = null;
            MemoItem? newMemo = null;
            newDirName ??= MemoConstants.NEW_DIRECTORY;

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
            var dirName = newDirName;
            var fullName = Path.Combine(dir, dirName);
            while (Directory.Exists(fullName))
            {
                dirIndex++;
                dirName = $"{newDirName}{dirIndex}";
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

        public void Delete(MemoItem selection)
        {
            if (selection.IsDirectory)
            {
                if (Directory.Exists(selection.FullPath))
                {
                    Directory.Delete(selection.FullPath, true);

                    selection.Parent?.Children?.Remove(selection);

                    if (selection.Id == _selectedSidebarMemo?.Id)
                    {
                        SetSelectedSidebarMemo(selection.Parent);
                    }
                    else
                    {
                        NotifySelectedSidebarMemoChanged();
                    }
                    SetSelectedMemoFromSidebar();
                }
            }
            else
            {
                if (File.Exists(selection.FullPath))
                {
                    File.Delete(selection.FullPath);
                    selection.Parent?.Children?.Remove(selection);

                    SetSelectedMemoFromSidebar();
                }
            }

        }

        #endregion

        #region Methods
        public string GetMarkdownContents(MemoItem? memo)
        {
            try
            {
                return memo == null || memo.IsDirectory ? string.Empty : File.ReadAllText(memo.FullPath);
            }
            catch (IOException ex)
            {
                return string.Empty;
            }
        }

        public async Task SaveMarkdownContents(MemoItem? memo, string? content)
        {
            if (memo != null && !memo.IsDirectory && File.Exists(memo.FullPath))
            {
                await using var fs = new StreamWriter(memo.FullPath);
                await fs.WriteAsync(content);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }

        public IEnumerable<MemoItem>? SearchMemo(string? searchTerm = null)
        {
            if (SelectedSidebarMemo?.Children != null)
            {
                foreach (var child in SelectedSidebarMemo.Children)
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        if (child.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return child;
                        }
                        else if (!child.IsDirectory && File.Exists(child.FullPath))
                        {
                            using var sr = new StreamReader(child.FullPath);
                            string? line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (!string.IsNullOrWhiteSpace(line) && line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return child;
                                    yield break;
                                }
                            }

                            sr.Close();
                            sr.Dispose();
                        }
                    }
                    else
                    {
                        yield return child;
                    }
                }
            }
        }

        public async Task<FileUploadResult> SaveUploadFile(IBrowserFile file)
        {
            var fileNameWithoutExtension = SelectedMemo!.Name.TrimEnd(MemoConstants.FILE_EXTENSION.ToCharArray());
            var dir = Path.Combine(_options.Value.MemoDir, _options.Value.UploadsDir, fileNameWithoutExtension);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await using FileStream fs = new(Path.Combine(dir, file.Name), FileMode.Create);
            await file.OpenReadStream(25 * 1024 * 1024).CopyToAsync(fs); // 25M

            return new FileUploadResult { FileUri = $"{_options.Value.UploadsVirtualPath}/{fileNameWithoutExtension}/{file.Name}" };
        }
        #endregion
    }
}
