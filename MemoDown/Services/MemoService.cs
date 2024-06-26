﻿using Blazor.Cherrydown.FileUpload;
using MemoDown.Constants;
using MemoDown.Models;
using MemoDown.Options;
using MemoDown.Store;
using Microsoft.Extensions.Options;

namespace MemoDown.Services
{
    public class MemoService
    {
        #region Fields & Properties
        private readonly MemoStore _store;
        private readonly IOptions<MemoDownOptions> _options;
        private MemoItem? _selectedMemo;
        private MemoItem? _selectedSidebarMemo;
        private bool _isSaving;
        private object _locker = new();

        public MemoItem RootMemo => _store.Memo;
        public MemoItem? SelectedMemo => _selectedMemo;
        public MemoItem? SelectedSidebarMemo => _selectedSidebarMemo ?? SelectedMemo?.Parent ?? RootMemo;

        public bool IsSaving {
            get {
                return _isSaving;
            }
            set {
                if (_isSaving != value)
                {
                    _isSaving = value;
                    NotifySavingChanged();
                }
            }
        }

        public event Action OnSelectedMemoChanged;
        public event Action OnSelectedSidebarMemoChanged;
        public event Action OnSavingChanged;

        #endregion

        #region Constructor & Initializer
        public MemoService(MemoStore store, IOptions<MemoDownOptions> options)
        {
            _store = store;
            _options = options;

            OnSelectedMemoChanged = default!;
            OnSelectedSidebarMemoChanged = default!;
            OnSavingChanged = default!;

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
        private void NotifySelectedMemoChanged() => OnSelectedMemoChanged?.Invoke();
        private void NotifySelectedSidebarMemoChanged() => OnSelectedSidebarMemoChanged?.Invoke();
        private void NotifySavingChanged() => OnSavingChanged?.Invoke();

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
                var fs = File.CreateText(fullName);
                fs.Flush();
                fs.Close();
                fs.Dispose();

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
                    // remove uploads
                    var uploadsDir = Path.Combine(_options.Value.MemoDir, GetRelativeUploadsDir(selection.FullPath));
                    if (Directory.Exists(uploadsDir))
                    {
                        Directory.Delete(uploadsDir, true);
                    }

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
                    // remove uploads
                    var uploadsDir = Path.Combine(_options.Value.MemoDir, GetRelativeUploadsDirFromFileFullPath(selection.FullPath));
                    if (Directory.Exists(uploadsDir))
                    {
                        Directory.Delete(uploadsDir, true);
                    }

                    selection.Parent?.Children?.Remove(selection);

                    SetSelectedMemoFromSidebar();
                }
            }
        }

        public void Rename(string oldName, string newName, MemoItem selection)
        {
            void HandleChildren(MemoItem parent, string selectionOldFullPath, string selectionNewFullPath)
            {
                if (parent.Children != null && parent.Children.Any())
                {
                    // 3.1. process children file
                    var subFiles = parent.Children.Where(m => !m.IsDirectory);
                    foreach (var file in subFiles)
                    {
                        var oldFileFullPath = file.FullPath;
                        var newFileFullPath = Path.Combine(selectionNewFullPath, Path.GetRelativePath(selectionOldFullPath, file.FullPath));

                        var oldRelativeUploadsDir = GetRelativeUploadsDirFromFileFullPath(oldFileFullPath);
                        var oldRelativeUrl = GetRelativeUploadsUrl(oldRelativeUploadsDir);
                        var oldUrl = $"/{oldRelativeUrl}/";

                        var newRelativeUploadsDir = GetRelativeUploadsDirFromFileFullPath(newFileFullPath);
                        var newRelativeUrl = GetRelativeUploadsUrl(newRelativeUploadsDir);
                        var newUrl = $"/{newRelativeUrl}/";

                        var content = File.ReadAllText(newFileFullPath);
                        if (content.IndexOf(oldUrl) > 0)
                        {
                            var newContent = content?.Replace(oldUrl, newUrl);
                            File.WriteAllText(newFileFullPath, newContent);
                        }

                        file.FullPath = newFileFullPath;
                    }

                    // 3.2. process children directory
                    var subDirs = parent.Children.Where(m => m.IsDirectory);
                    foreach (var dir in subDirs)
                    {
                        HandleChildren(dir, selectionOldFullPath, selectionNewFullPath);

                        var oldSubDirFullPath = dir.FullPath;
                        var newSubDirFullPath = Path.Combine(selectionNewFullPath, Path.GetRelativePath(selectionOldFullPath, dir.FullPath));
                        dir.FullPath = newSubDirFullPath;
                    }
                }
            }

            lock (_locker)
            {
                if (selection.IsDirectory)
                {
                    if (Directory.Exists(selection.FullPath))
                    {
                        // 1. move dir
                        var oldFullPath = selection.FullPath;
                        // parent path + new
                        var newFullPath = Path.Combine(Path.GetDirectoryName(oldFullPath)!, newName);
                        if (Directory.Exists(oldFullPath))
                        {
                            Directory.Move(oldFullPath, newFullPath);
                        }

                        // 2. move uploads dir
                        var oldUploadsDir = Path.Combine(_options.Value.MemoDir, GetRelativeUploadsDir(oldFullPath));
                        var newUploadsDir = Path.Combine(_options.Value.MemoDir, GetRelativeUploadsDir(newFullPath));
                        if (Directory.Exists(oldUploadsDir))
                        {
                            Directory.Move(oldUploadsDir, newUploadsDir);
                        }

                        // 3. children
                        HandleChildren(selection, oldFullPath, newFullPath);

                        // 4. update memo in memory
                        selection.Name = newName;
                        selection.FullPath = newFullPath;

                        NotifySelectedMemoChanged();

                        NotifySelectedSidebarMemoChanged();
                    }
                }
                else
                {
                    if (File.Exists(selection.FullPath))
                    {
                        var oldFileName = $"{oldName}{MemoConstants.FILE_EXTENSION}";
                        var newFileName = $"{newName}{MemoConstants.FILE_EXTENSION}";

                        // 1. rename memo md file
                        var oldFullPath = selection.FullPath;
                        var newFullPath = Path.Combine(Path.GetDirectoryName(oldFullPath)!, newFileName);

                        File.Move(oldFullPath, newFullPath, true);

                        // 2. rename uploads dir
                        var oldRelativeUploadsDir = GetRelativeUploadsDirFromFileFullPath(oldFullPath);
                        var oldDir = Path.Combine(_options.Value.MemoDir, oldRelativeUploadsDir);

                        var newRelativeUploadsDir = GetRelativeUploadsDirFromFileFullPath(newFullPath);
                        var newDir = Path.Combine(_options.Value.MemoDir, newRelativeUploadsDir);

                        if (Directory.Exists(oldDir))
                        {
                            Directory.Move(oldDir, newDir);
                        }

                        // 3. replace md file contents
                        var oldRelativeUrl = GetRelativeUploadsUrl(oldRelativeUploadsDir);
                        var oldUrl = $"/{oldRelativeUrl}/";

                        var newRelativeUrl = GetRelativeUploadsUrl(newRelativeUploadsDir);
                        var newUrl = $"/{newRelativeUrl}/";

                        var content = File.ReadAllText(newFullPath);
                        if (content.IndexOf(oldUrl) > 0)
                        {
                            var newContent = content?.Replace(oldUrl, newUrl);
                            File.WriteAllText(newFullPath, newContent);
                        }

                        // 4. update memo in memory
                        selection.Name = newFileName;
                        selection.FullPath = newFullPath;

                        NotifySelectedMemoChanged();
                    }
                }
            }
        }

        #endregion

        #region Methods

        public string GetMarkdownContents(MemoItem? memo)
        {
            try
            {
                if (memo != null && !memo.IsDirectory && File.Exists(memo.FullPath))
                {
                    return File.ReadAllText(memo.FullPath);
                }
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
        }

        public void SaveMarkdownContents(MemoItem? memo, string? content)
        {
            lock (_locker)
            {
                if (memo != null && !memo.IsDirectory && File.Exists(memo.FullPath))
                {
                    File.WriteAllText(memo.FullPath, content);
                    //using var fs = new StreamWriter(memo.FullPath);
                    //fs.Write(content);
                    //fs.Flush();
                    //fs.Close();
                    //fs.Dispose();
                }
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
            var fullPath = SelectedMemo!.FullPath;
            var relativeUploadsDir = GetRelativeUploadsDirFromFileFullPath(fullPath);
            var dir = Path.Combine(_options.Value.MemoDir, relativeUploadsDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await using FileStream fs = new(Path.Combine(dir, file.Name), FileMode.Create);
            await file.OpenReadStream(MemoConstants.MaxUploadSize).CopyToAsync(fs); // 25M

            var relativeUrl = GetRelativeUploadsUrl(relativeUploadsDir);
            return new FileUploadResult { FileUri = $"/{relativeUrl}/{file.Name}" };
        }

        public void CleanUpUploads()
        {
            lock (_locker)
            {
                var rootPath = _options.Value.MemoDir;
                var uploadsDir = Path.Combine(rootPath, _options.Value.UploadsRelativePath);

                var mdFileWithUploads = new Dictionary<string, (string dir, List<(string VirtualUrl, string PhysicalPath)> Files)>();
                GetAvaiableMdFileFromUploadsDir(uploadsDir);

                var availableMdFiles = new List<string>();
                GetAvaiableMdFile(_store.Memo);

                // remove dir exists, but md file not there
                var noLongerExists = mdFileWithUploads.Select(kv => kv.Key).Except(availableMdFiles).ToList();
                noLongerExists.ForEach(mdFile =>
                {
                    if (mdFileWithUploads.TryGetValue(mdFile, out var val))
                    {
                        if (Directory.Exists(val.dir))
                        {
                            Directory.Delete(val.dir, true);
                        }
                    }
                });

                // remove md existing, but files changed
                var exists = mdFileWithUploads.Select(kv => kv.Key).Intersect(availableMdFiles).ToList();
                exists.ForEach(mdFile =>
                {
                    if (mdFileWithUploads.TryGetValue(mdFile, out var val))
                    {
                        var contents = File.ReadAllText(mdFile);
                        if (!string.IsNullOrWhiteSpace(contents))
                        {
                            val.Files.ForEach(file =>
                            {
                                if (contents.IndexOf(file.VirtualUrl) < 0)
                                {
                                    if (File.Exists(file.PhysicalPath))
                                    {
                                        File.Delete(file.PhysicalPath);
                                    }
                                }
                            });
                        }
                    }
                });

                void GetAvaiableMdFile(MemoItem memo)
                {
                    if (!memo.IsDirectory)
                    {
                        availableMdFiles.Add(memo.FullPath);
                    }

                    if (memo.IsDirectory)
                    {
                        memo.Children?.ForEach(child => GetAvaiableMdFile(child));
                    }
                }

                void GetAvaiableMdFileFromUploadsDir(string path)
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                            .Select(x => (VirtualUrl: $"/{GetRelativeUploadsUrl(Path.GetRelativePath(rootPath, x))}", PhysicalPath: x))
                            .ToList();

                        var mdFilePath = Path.Combine(rootPath, Path.ChangeExtension(Path.GetRelativePath(uploadsDir, path), MemoConstants.FILE_EXTENSION));

                        if (files.Any())
                        {
                            mdFileWithUploads.Add(mdFilePath, (dir: path, Files: files));
                        }

                        foreach (var subDir in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
                        {
                            GetAvaiableMdFileFromUploadsDir(subDir);
                        }
                    }
                }
            }
        }

        private string GetRelativeUploadsUrl(string url)
        {
            return url.Replace(@"\", "/");
        }

        private string GetRelativeUploadsDir(string absolutePath)
        {
            return Path.Combine(_options.Value.UploadsRelativePath, Path.GetRelativePath(_options.Value.MemoDir, absolutePath));
        }

        private string GetRelativeUploadsDirFromFileFullPath(string fullPath)
        {
            return Path.Combine(_options.Value.UploadsRelativePath,
                Path.GetRelativePath(_options.Value.MemoDir, Path.GetDirectoryName(fullPath)!),
                Path.GetFileNameWithoutExtension(fullPath));
        }
        #endregion
    }
}
