namespace xasset
{
    public static class DownloadErrors
    {
        public const string NothingToDownload = "Nothing to download.";
        public const string UserCancel = "User cancelled.";
        public const string FileNotExist = "File not exist.";
        public const string DownloadSizeMismatch = "Download size {0} mismatch to {1}";
        public const string DownloadHashMismatch = "Download hash {0} mismatch to {1}";
        public const string FailedToDownloadSomeFiles = "Failed to download {0} files.";
    }
}