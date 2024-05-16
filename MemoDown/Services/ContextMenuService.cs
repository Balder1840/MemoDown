using MemoDown.Constants;
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

        public ContextMenuService(NotificationService notificationService,
            RadzenContextMenuService contextMenuService,
            MemoService memoService)
        {
            _notificationService = notificationService;
            _contextMenuService = contextMenuService;
            _memoService = memoService;
        }

        public void HandleSidebarBtnAddClick(MouseEventArgs args)
        {
            _contextMenuService.Open(args,
            new List<ContextMenuItem> {
                new ContextMenuItem(){ Text = MemoConstants.NEW_FILE, Value = MemuEnum.CreateNote, Image="/images/markdown_20x20.png", ImageStyle="margin-right:0.5rem;" },
                new ContextMenuItem(){ Text = MemoConstants.NEW_DIRECTORY, Value = MemuEnum.CreateDiretory, Icon = "folder" } },
            menuArgs => OnMenuItemClick(menuArgs, _memoService.SelectedSidebarMemo));
        }

        public void HandleOpenContextMenu(MemoContextMenuArgs args)
        {
            RenderFragment<RadzenContextMenuService> rf = value => __builder2 =>
            {
                __builder2.OpenComponent<RadzenMenu>(545);
                __builder2.AddComponentParameter(546, "Click", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, delegate (MenuItemEventArgs arg)
                {
                    OnContextMenuClick(arg, args.Memo);
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

        private void OnContextMenuClick(MenuItemEventArgs args, MemoItem memo)
        {
            _notificationService.Notify(NotificationSeverity.Success, $"调用成功！{args.Text}-{memo.FullPath}");
            if (!args.Value.Equals(3) && !args.Value.Equals(4))
            {
                _contextMenuService.Close();
            }
        }

        private void OnMenuItemClick(MenuItemEventArgs args, MemoItem? selection)
        {
            var menu = (MemuEnum)args.Value;

            switch (menu)
            {
                case MemuEnum.CreateNote:
                    {
                        var memo = _memoService.CreateFile(selection);
                        _memoService.SetSelectedMemo(memo);
                        break;
                    }
                case MemuEnum.CreateDiretory:
                    {
                        var memo = _memoService.CreateDirectory(selection);
                        _memoService.SetSelectedSidebarMemo(memo);
                        _memoService.SetSelectedMemo(memo);
                        break;
                    }
                default:
                    _notificationService.Notify(NotificationSeverity.Error, "未知菜单类型！");
                    break;
            }

            _notificationService.Notify(NotificationSeverity.Success, "创建成功！");

            _contextMenuService.Close();
        }
    }
}
