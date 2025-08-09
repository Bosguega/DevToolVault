using System.Linq;
using DevToolVault.Filters;

namespace DevToolVault.Models
{
    public class TreeOptions
    {
        public bool IgnoreEmptyFolders { get; set; }
        public bool ShowFileSize { get; set; }
        public bool ShowSystemFiles { get; set; }
        public bool ShowOnlyCodeFiles { get; set; }
        public string[] IgnorePatterns { get; set; }
        public string[] CodeExtensions { get; set; }

        public static TreeOptions FromFilterProfile(FilterProfile profile)
        {
            return new TreeOptions
            {
                IgnoreEmptyFolders = profile.IgnoreEmptyFolders,
                ShowFileSize = profile.ShowFileSize,
                ShowSystemFiles = profile.ShowSystemFiles,
                ShowOnlyCodeFiles = profile.ShowOnlyCodeFiles,
                IgnorePatterns = profile.IgnorePatterns.ToArray(),
                CodeExtensions = profile.CodeExtensions.ToArray()
            };
        }
    }
}