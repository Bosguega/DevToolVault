namespace DevToolVault.Models
{
    public class TreeOptions
    {
        public bool IgnoreEmptyFolders { get; set; }
        public bool ShowFileSize { get; set; }
        public bool ShowSystemFiles { get; set; }
        public string[] IgnorePatterns { get; set; }
    }
}