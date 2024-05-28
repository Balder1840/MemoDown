using Microsoft.AspNetCore.Components.Web;

#pragma warning disable CS8618
namespace MemoDown.Components.Shared
{
    public class MemoMenuItemEventArgs : MouseEventArgs
    {
        //
        // Summary:
        //     Gets text of the clicked item.
        public string Text { get; internal set; }

        //
        // Summary:
        //     Gets the value of the clicked item.
        public object Value { get; internal set; }

        //
        // Summary:
        //     Gets the path path of the clicked item.
        public string Path { get; internal set; }
    }
}
