namespace MemoDown.Models
{
    public class MemoItem
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = default!;
        public string FullPath { get; set; } = default!;
        public bool IsDirectory { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public MemoItem? Parent { get; set; }
        public List<MemoItem>? Children { get; set; }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
    }
}
