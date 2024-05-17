using Radzen;
using RadzenDialogService = Radzen.DialogService;

namespace MemoDown.Services
{
    public class DialogService
    {

        private readonly RadzenDialogService _dialogService;

        public DialogService(RadzenDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public Task<bool?> ConfirmDelete(string? content = null)
        {
            return _dialogService.Confirm($"确定删除{content}吗？", "提示", new ConfirmOptions()
            {
                OkButtonText = "确定",
                CancelButtonText = "取消",
                CloseDialogOnEsc = false,
                ShowClose = false,
                CloseDialogOnOverlayClick = false,
            });
        }
    }
}
