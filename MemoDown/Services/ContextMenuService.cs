using MemoDown.Constants;
using MemoDown.Models;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
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

        void OnMenuItemClick(MenuItemEventArgs args, MemoItem? selection)
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
