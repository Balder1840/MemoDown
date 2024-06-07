namespace MemoDown.Options
{
    public class MemoDownOptions
    {
        private const string Memo = "memo";
        private const string DefaultUploadsDir = "uploads";

        public string MemoDir { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Memo);
        public int AutoSavingIntervalSecond { get; set; } = 30;
        public string UploadsDir { get; set; } = DefaultUploadsDir;
        public string UploadsVirtualPath { get; set; } = DefaultUploadsDir;

        public Account Account { get; set; } = new Account();
    }

    public class Account
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
