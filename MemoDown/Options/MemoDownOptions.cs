using System.ComponentModel.DataAnnotations;

namespace MemoDown.Options
{
    public class MemoDownOptions
    {
        private const string Memo = "memo";
        private const string DefaultUploadsDir = "uploads";

        public const string Key = "MemoDown";

        public string MemoDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Memo);

        [Required(ErrorMessage = "AutoSavingIntervalSecond is required.")]
        public int AutoSavingIntervalSecond { get; set; } = 30;

        public string UploadsRelativePath { get; set; } = DefaultUploadsDir;
        public string UploadsVirtualPath { get; set; } = DefaultUploadsDir;

        public Account Account { get; set; } = new Account();

        public CloudflareTurnstile CloudflareTurnstile { get; set; } = new CloudflareTurnstile();
    }

    public class Account
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class CloudflareTurnstile
    {
        public bool Enable { get; set; }
        public string? SiteKey { get; set; }
        public string? SecretKey { get; set; }
    }
}