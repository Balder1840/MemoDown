﻿namespace MemoDown.Options
{
    public class MemoDownOptions
    {
        private const string Memo = "memo";
        private const string DefaultUploadsDir = "memo";

        public string MemoDir { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Memo);
        public int AutoSavingIntervalSecond { get; set; } = 30;
        public string UploadsDir { get; set; } = DefaultUploadsDir;
    }
}
