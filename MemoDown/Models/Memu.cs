using Microsoft.AspNetCore.Components.Web;

namespace MemoDown.Models
{
    public enum MemuEnum
    {
        CreateNote = 1,
        DeleteNote,
        CreateDiretory,
        DeleteDiretory,
        Delete,
        Rename
    }

    public class MemoContextMenuArgs
    {
        public MouseEventArgs MouseArgs { get; set; } = default!;
        public MemoItem Memo { get; set; } = default!;
    }
}
