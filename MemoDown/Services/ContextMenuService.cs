using MemoDown.Constants;
using MemoDown.Extensions;
using MemoDown.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using RadzenContextMenuService = Radzen.ContextMenuService;

namespace MemoDown.Services
{
    public class ContextMenuService
    {
        #region Properties & Fields
        private readonly NotificationService _notificationService;
        private readonly RadzenContextMenuService _contextMenuService;
        private readonly MemoService _memoService;
        private readonly DialogService _dialogService;
        private readonly GithubSyncService _githubSyncService;
        private readonly RenderFragment<RadzenContextMenuService> _profileContextMenuFragment;
        #endregion

        #region Constructor
        public ContextMenuService(NotificationService notificationService,
            RadzenContextMenuService contextMenuService,
            MemoService memoService,
            DialogService dialogService,
            GithubSyncService githubSyncService)
        {
            _notificationService = notificationService;
            _contextMenuService = contextMenuService;
            _memoService = memoService;
            _dialogService = dialogService;
            _githubSyncService = githubSyncService;

            _profileContextMenuFragment = value => __builder2 =>
            {
                __builder2.OpenComponent<RadzenMenu>(644);
                __builder2.AddComponentParameter(645, "Click", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<MenuItemEventArgs, Task>)OnProfileContextMenuItemClick)));
                __builder2.AddAttribute(646, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder3)
                {
                    __builder3.OpenComponent<RadzenMenuItem>(647);
                    __builder3.AddComponentParameter(648, "Value", RuntimeHelpers.TypeCheck((object)MemuEnum.Signout));
                    __builder3.AddAttribute(649, "Template", (RenderFragment)delegate (RenderTreeBuilder __builder4)
                    {
                        __builder4.OpenElement(650, "form");
                        __builder4.AddAttribute(651, "method", "post");
                        __builder4.AddAttribute(652, "action", "/logout");
                        __builder4.AddAttribute(653, "b-wfm830zd58");
                        __builder4.OpenComponent<RadzenButton>(654);
                        __builder4.AddComponentParameter(655, "Icon", "logout");
                        __builder4.AddComponentParameter(656, "Text", RuntimeHelpers.TypeCheck(MemoConstants.SINGOUT));
                        __builder4.AddComponentParameter(657, "ButtonStyle", RuntimeHelpers.TypeCheck(ButtonStyle.Light));
                        __builder4.AddComponentParameter(658, "ButtonType", RuntimeHelpers.TypeCheck(ButtonType.Submit));
                        __builder4.AddComponentParameter(659, "class", "profile-menu-item-button");
                        __builder4.CloseComponent();
                        __builder4.CloseElement();
                    });
                    __builder3.CloseComponent();
                    __builder3.AddMarkupContent(660, "\r\n        ");
                    __builder3.OpenComponent<RadzenMenuItem>(661);
                    __builder3.AddComponentParameter(662, "Text", RuntimeHelpers.TypeCheck(MemoConstants.SYNC_TO_GITHUB));
                    __builder3.AddComponentParameter(663, "Image", "images/github-mark.svg");
                    __builder3.AddComponentParameter(664, "Value", RuntimeHelpers.TypeCheck(MemuEnum.Sync));
                    __builder3.AddComponentParameter(665, "ImageStyle", "width:20px;height:20px;margin-right:0.5rem;");
                    __builder3.CloseComponent();
                });
                __builder2.CloseComponent();
            };
        }
        #endregion

        #region Public Methods
        public void OnSidebarBtnAddClick(MouseEventArgs args)
        {
            _contextMenuService.Open(args,
            new List<ContextMenuItem> {
                new ContextMenuItem(){ Text = MemoConstants.NEW_FILE, Value = MemuEnum.CreateNote, Image="/images/markdown_20x20.png", ImageStyle="margin-right:0.5rem;" },
                new ContextMenuItem(){ Text = MemoConstants.NEW_DIRECTORY, Value = MemuEnum.CreateDiretory, Icon = "folder" } },
            async menuArgs => await OnSidebarAddBtnContextMenuItemClick(menuArgs, _memoService.SelectedSidebarMemo));
        }

        public void OpenMemoContextMenu(MemoContextMenuArgs args)
        {
            RenderFragment<RadzenContextMenuService> rf = value => __builder2 =>
            {
                __builder2.OpenComponent<RadzenMenu>(545);
                __builder2.AddComponentParameter(546, "Click", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, async delegate (MenuItemEventArgs arg)
                {
                    await OnMemoContextMenuItemClick(arg, args.Memo);
                })));
                __builder2.AddAttribute(547, "ChildContent", (RenderFragment)delegate (RenderTreeBuilder __builder3)
                {
                    __builder3.OpenComponent<RadzenMenuItem>(548);
                    __builder3.AddComponentParameter(549, "Text", RuntimeHelpers.TypeCheck(MemoConstants.NEW_FILE));
                    __builder3.AddComponentParameter(550, "Value", RuntimeHelpers.TypeCheck((object)MemuEnum.CreateNote));
                    __builder3.AddComponentParameter(551, "Image", "/images/markdown_20x20.png");
                    __builder3.AddComponentParameter(552, "ImageStyle", "margin-right:0.5rem;");
                    __builder3.CloseComponent();
                    __builder3.AddMarkupContent(553, "\r\n");
                    __builder3.OpenComponent<RadzenMenuItem>(554);
                    __builder3.AddComponentParameter(555, "Text", RuntimeHelpers.TypeCheck(MemoConstants.NEW_DIRECTORY));
                    __builder3.AddComponentParameter(556, "Value", RuntimeHelpers.TypeCheck((object)MemuEnum.CreateDiretory));
                    __builder3.AddComponentParameter(557, "Icon", "folder");
                    __builder3.CloseComponent();
                    __builder3.AddMarkupContent(558, "\r\n<hr class='mt-0 mb-0'>\r\n");
                    __builder3.OpenComponent<RadzenMenuItem>(559);
                    __builder3.AddComponentParameter(560, "Text", RuntimeHelpers.TypeCheck(MemoConstants.RENAME));
                    __builder3.AddComponentParameter(561, "Value", RuntimeHelpers.TypeCheck((object)MemuEnum.Rename));
                    __builder3.AddComponentParameter(562, "Icon", "drive_file_rename_outline");
                    __builder3.CloseComponent();
                    __builder3.AddMarkupContent(563, "\r\n<hr class='mt-0 mb-0'>\r\n");
                    __builder3.OpenComponent<RadzenMenuItem>(564);
                    __builder3.AddComponentParameter(565, "Text", RuntimeHelpers.TypeCheck(MemoConstants.DELETE));
                    __builder3.AddComponentParameter(566, "Value", RuntimeHelpers.TypeCheck((object)MemuEnum.Delete));
                    __builder3.AddComponentParameter(567, "Icon", "delete");
                    __builder3.CloseComponent();
                });
                __builder2.CloseComponent();
            };

            _contextMenuService.Open(args.MouseArgs, rf);
        }

        public void OpenProfileContextMenu(MouseEventArgs args)
        {
            _contextMenuService.Open(args, _profileContextMenuFragment);
        }
        #endregion

        #region Private Methods
        private async Task OnMemoContextMenuItemClick(MenuItemEventArgs args, MemoItem selection)
        {
            _contextMenuService.Close();

            var menu = (MemuEnum)args.Value;

            switch (menu)
            {
                case MemuEnum.CreateNote:
                    {
                        var name = await _dialogService.ShowNamingDialog();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var memo = _memoService.CreateFile(selection, name, !selection.IsDirectory);
                            _memoService.SetSelectedSidebarMemo(memo?.Parent);
                            _memoService.SetSelectedMemo(memo);
                            _notificationService.Notify(NotificationSeverity.Success, "创建成功！");
                        }
                        break;
                    }
                case MemuEnum.CreateDiretory:
                    {
                        var name = await _dialogService.ShowNamingDialog(category: "文件夹");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var memo = _memoService.CreateDirectory(selection, name, !selection.IsDirectory);
                            _memoService.SetSelectedSidebarMemo(memo);
                            _memoService.SetSelectedMemo(memo);
                            _notificationService.Notify(NotificationSeverity.Success, "创建成功！");
                        }
                        break;
                    }
                case MemuEnum.Delete:
                    {
                        var confirmation = selection.IsDirectory ? $"文件夹{selection.Name}及其子文件" : $"文件{selection.Name}";
                        var confirmResult = await _dialogService.ConfirmDelete(confirmation);
                        if (confirmResult.HasValue && confirmResult.Value)
                        {
                            _memoService.Delete(selection);
                            _notificationService.Notify(NotificationSeverity.Success, "删除成功！");
                        }
                        break;
                    }
                case MemuEnum.Rename:
                    {
                        var category = selection.IsDirectory ? "文件夹" : "笔记";
                        var initialName = selection.IsDirectory ? selection.Name : selection.Name.TrimEnd(MemoConstants.FILE_EXTENSION);
                        var name = await _dialogService.ShowNamingDialog(operation: "重命名", category: category, initialName: initialName);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _memoService.Rename(initialName, name, selection);
                            _notificationService.Notify(NotificationSeverity.Success, "重命名成功！");
                        }
                        break;
                    }
                default:
                    _notificationService.Notify(NotificationSeverity.Error, "未知菜单类型！");
                    break;
            }
        }

        private async Task OnProfileContextMenuItemClick(MenuItemEventArgs args)
        {
            _contextMenuService.Close();

            var menu = (MemuEnum)args.Value;
            switch (menu)
            {
                case MemuEnum.Sync:
                    {
                        _dialogService.ShowLoading();
                        await _githubSyncService.SyncToGithub($"synchronized at {DateTime.Now:HH:mm:ss, dddd, MMMM d, yyyy}");

                        _dialogService.CloseLoading();
                        _notificationService.Notify(NotificationSeverity.Success, "同步至Github成功！");
                        break;
                    }
                default:
                    _notificationService.Notify(NotificationSeverity.Error, "未知菜单类型！");
                    break;
            }
        }

        private async Task OnSidebarAddBtnContextMenuItemClick(MenuItemEventArgs args, MemoItem? selection)
        {
            _contextMenuService.Close();

            var menu = (MemuEnum)args.Value;

            switch (menu)
            {
                case MemuEnum.CreateNote:
                    {
                        var name = await _dialogService.ShowNamingDialog();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var memo = _memoService.CreateFile(selection, name);
                            _memoService.SetSelectedMemo(memo);

                            _notificationService.Notify(NotificationSeverity.Success, "创建成功！");
                        }
                        break;
                    }
                case MemuEnum.CreateDiretory:
                    {
                        var name = await _dialogService.ShowNamingDialog(category: "文件夹");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var memo = _memoService.CreateDirectory(selection, name);
                            _memoService.SetSelectedSidebarMemo(memo);
                            _memoService.SetSelectedMemo(memo);

                            _notificationService.Notify(NotificationSeverity.Success, "创建成功！");
                        }
                        break;
                    }
                default:
                    _notificationService.Notify(NotificationSeverity.Error, "未知菜单类型！");
                    break;
            }
        }
        #endregion
    }
}
