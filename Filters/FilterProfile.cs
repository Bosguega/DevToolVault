using System.Collections.Generic;

namespace DevToolVault.Filters
{
    public class FilterProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> IgnorePatterns { get; set; } = new List<string>();
        public List<string> CodeExtensions { get; set; } = new List<string>();
        public bool IgnoreEmptyFolders { get; set; } = true;
        public bool ShowFileSize { get; set; } = false;
        public bool ShowSystemFiles { get; set; } = false;
        public bool ShowOnlyCodeFiles { get; set; } = false;

        // Indica se é um perfil embutido (não deve ser excluído)
        public bool IsBuiltIn { get; set; } = false;
    }
}