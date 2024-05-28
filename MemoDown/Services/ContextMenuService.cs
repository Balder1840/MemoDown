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
        private readonly NotificationService _notificationService;
        private readonly RadzenContextMenuService _contextMenuService;
        private readonly MemoService _memoService;
        private readonly DialogService _dialogService;

        public ContextMenuService(NotificationService notificationService,
            RadzenContextMenuService contextMenuService,
            MemoService memoService,
            DialogService dialogService)
        {
            _notificationService = notificationService;
            _contextMenuService = contextMenuService;
            _memoService = memoService;
            _dialogService = dialogService;
        }

        public void HandleSidebarBtnAddClick(MouseEventArgs args)
        {
            _contextMenuService.Open(args,
            new List<ContextMenuItem> {
                new ContextMenuItem(){ Text = MemoConstants.NEW_FILE, Value = MemuEnum.CreateNote, Image="/images/markdown_20x20.png", ImageStyle="margin-right:0.5rem;" },
                new ContextMenuItem(){ Text = MemoConstants.NEW_DIRECTORY, Value = MemuEnum.CreateDiretory, Icon = "folder" } },
            async menuArgs => await OnMenuItemClick(menuArgs, _memoService.SelectedSidebarMemo));
        }

        public void HandleOpenContextMenu(MemoContextMenuArgs args)
        {
            RenderFragment<RadzenContextMenuService> rf = value => __builder2 =>
            {
                __builder2.OpenComponent<RadzenMenu>(545);
                __builder2.AddComponentParameter(546, "Click", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, async delegate (MenuItemEventArgs arg)
                {
                    await OnContextMenuClick(arg, args.Memo);
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

        private async Task OnContextMenuClick(MenuItemEventArgs args, MemoItem selection)
        {
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
                            await _memoService.Rename(initialName, name, selection);
                            _notificationService.Notify(NotificationSeverity.Success, "重命名成功！");
                        }
                        break;
                    }
                default:
                    _notificationService.Notify(NotificationSeverity.Error, "未知菜单类型！");
                    break;
            }

            _contextMenuService.Close();
        }

        private async Task OnMenuItemClick(MenuItemEventArgs args, MemoItem? selection)
        {
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

            _contextMenuService.Close();
        }
    }
}
