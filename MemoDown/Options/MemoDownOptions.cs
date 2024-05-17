namespace MemoDown.Options
{
    public class MemoDownOptions
    {
        private const string Memo = "memo";
        public string MemoDir { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Memo);

        public int AutoSavingIntervalSecond { get; set; } = 30;
    }
}
