using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace DevToolVault.Filters
{
    public class FileFilterManager
    {
        private readonly string _filtersDirectory;
        private List<FilterProfile> _profiles = new List<FilterProfile>();
        private FilterProfile _activeProfile;

        public FileFilterManager(string filtersDirectory = null)
        {
            _filtersDirectory = filtersDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DevToolVault", "Filters");

            EnsureDirectoryExists();
            LoadDefaultFilters();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_filtersDirectory))
            {
                Directory.CreateDirectory(_filtersDirectory);
            }
        }

        private void LoadDefaultFilters()
        {
            // Carrega perfis padrão
            var defaultProfile = new FilterProfile
            {
                Name = "Default",
                Description = "Filtro padrão para projetos .NET",
                IgnorePatterns = new List<string>
                {
                    ".git", ".svn", ".hg", ".bzr", "_darcs",
                    "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU",
                    "node_modules", "bower_components", "jspm_packages",
                    ".nuget", "packages", ".vscode", ".idea", ".vs",
                    "*.tmp", "*.temp", "*.log", "*.cache", "*.bak", "*.swp",
                    "Thumbs.db", "desktop.ini", ".DS_Store", ".AppleDouble",
                    ".LSOverride", "Icon\r", "$RECYCLE.BIN", "System Volume Information",
                    "build", "dist", "out", "target", "coverage", ".gradle",
                    ".gradletasknamecache", "gradlew", "gradlew.bat",
                    ".mvn", "mvnw", "mvnw.cmd",
                    ".idea", "*.iml", ".vscode", "*.sublime-project", "*.sublime-workspace",
                    ".cxx", "captures", "local.properties", "*.apk", "*.aab",
                    "*.dll", "*.exe", "*.so", "*.dylib", "*.pdb", "*.xml",
                    "*.config", "*.settings", "*.user", "*.suo", "*.userosscache"
                },
                CodeExtensions = new List<string>
                {
                    ".cs", ".vb", ".fs", ".cpp", ".c", ".h", ".hpp", ".java", ".py",
                    ".js", ".ts", ".jsx", ".tsx", ".html", ".css", ".scss", ".sass",
                    ".less", ".php", ".rb", ".go", ".rs", ".swift", ".kt", ".scala",
                    ".clj", ".cljs", ".edn", ".r", ".m", ".sh", ".sql", ".xml", ".json",
                    ".yaml", ".yml", ".toml", ".ini", ".cfg", ".conf", ".properties",
                    ".md", ".txt", ".dockerfile", ".gitignore", ".gitattributes"
                }
            };

            _profiles.Add(defaultProfile);
            _activeProfile = defaultProfile;

            // Tenta carregar perfis personalizados
            LoadCustomProfiles();
        }

        private void LoadCustomProfiles()
        {
            try
            {
                var filterFiles = Directory.GetFiles(_filtersDirectory, "*.json");
                foreach (var file in filterFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var profile = JsonSerializer.Deserialize<FilterProfile>(json);
                        if (profile != null)
                        {
                            _profiles.Add(profile);
                        }
                    }
                    catch
                    {
                        // Ignora arquivos inválidos
                    }
                }
            }
            catch
            {
                // Ignora erros ao carregar perfis
            }
        }

        public IEnumerable<FilterProfile> GetProfiles()
        {
            return _profiles;
        }

        public FilterProfile GetActiveProfile()
        {
            return _activeProfile;
        }

        public void SetActiveProfile(FilterProfile profile)
        {
            _activeProfile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public void SaveProfile(FilterProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var fileName = $"{profile.Name}.json";
            var filePath = Path.Combine(_filtersDirectory, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(filePath, json);

            // Atualiza a lista se já existir
            var existing = _profiles.FirstOrDefault(p => p.Name == profile.Name);
            if (existing != null)
            {
                _profiles.Remove(existing);
            }
            _profiles.Add(profile);
        }

        public void DeleteProfile(FilterProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var fileName = $"{profile.Name}.json";
            var filePath = Path.Combine(_filtersDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _profiles.Remove(profile);

            // Se deletou o perfil ativo, volta para o padrão
            if (_activeProfile == profile)
            {
                _activeProfile = _profiles.FirstOrDefault(p => p.Name == "Default");
            }
        }

        public FilterProfile CreateProfile(string name, string description = "")
        {
            var profile = new FilterProfile
            {
                Name = name,
                Description = description,
                IgnorePatterns = new List<string>(_activeProfile.IgnorePatterns),
                CodeExtensions = new List<string>(_activeProfile.CodeExtensions),
                IgnoreEmptyFolders = _activeProfile.IgnoreEmptyFolders,
                ShowFileSize = _activeProfile.ShowFileSize,
                ShowSystemFiles = _activeProfile.ShowSystemFiles,
                ShowOnlyCodeFiles = _activeProfile.ShowOnlyCodeFiles
            };

            return profile;
        }
    }
}