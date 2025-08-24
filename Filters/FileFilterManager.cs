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
        private List<FilterProfile> _profiles = new();
        private FilterProfile _activeProfile;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                Directory.CreateDirectory(_filtersDirectory);
        }

        private void LoadDefaultFilters()
        {
            // Lista estática de perfis embutidos
            var builtInProfiles = new[]
            {
                new { Name = "Flutter", Description = "Filtro para projetos Flutter",
                      IgnorePatterns = new List<string>
                      {
                          "android","ios","linux","macos","windows","web",
                          "build","dart_tool",".dart_tool",".pub",".flutter-plugins",
                          ".idea",".vscode",".vs",
                          "*.g.dart","*.r.dart","*.gr.dart","*.freezed.dart",
                          "*.inject.dart","*.mocks.dart",
                          "*.apk","*.aab","*.ipa","*.app","*.exe","*.dll",
                          "*.so","*.dylib","*.jar","*.aar","*.framework",
                          ".DS_Store","Thumbs.db","desktop.ini",
                          "*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".dart",".yaml",".yml",".json",".xml",".html",".css",".js",".ts" }
                },
                new { Name = ".NET", Description = "Filtro para projetos .NET",
                      IgnorePatterns = new List<string>
                      {
                          "bin","obj","Debug","Release","x64","x86","AnyCPU",".vs","vs",".vscode","node_modules",
                          "*.exe","*.dll","*.pdb","*.config","*.exe.config",
                          "*.manifest","*.application","*.deploy",
                          ".DS_Store","Thumbs.db","desktop.ini",
                          "*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".cs",".vb",".fs",".xaml",".xml",".json",".config",".cshtml",".razor" }
                },
                new { Name = "Node.js", Description = "Filtro para projetos Node.js",
                      IgnorePatterns = new List<string>
                      {
                          "node_modules",".nyc_output","coverage",".cache","dist","build","out","*.map",
                          ".DS_Store","Thumbs.db","desktop.ini","*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".ts",".tsx",".jsx",".js",".json",".md",".yml",".yaml" }
                },
                new { Name = "Android", Description = "Filtro para projetos Android",
                      IgnorePatterns = new List<string>
                      {
                          "build",".gradle",".idea","captures",".cxx","app/build","app/build/intermediates","app/build/generated",
                          "*.apk","*.aab","*.jar","*.aar","*.dex","*.R.java","*.BuildConfig.java",
                          ".DS_Store","Thumbs.db","desktop.ini","*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".java",".kt",".xml",".gradle",".properties",".json" }
                },
                new { Name = "Web", Description = "Filtro para projetos Web",
                      IgnorePatterns = new List<string>
                      {
                          "dist","build","out",".cache",".tmp","node_modules",".nyc_output","coverage",
                          "*.min.js","*.min.css","*.bundle.js","*.bundle.css","*.map",
                          ".DS_Store","Thumbs.db","desktop.ini","*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".html",".css",".scss",".sass",".less",".js",".ts",".jsx",".tsx",".json",".md",".yml",".yaml" }
                },
                new { Name = "WPF", Description = "Filtro para projetos WPF",
                      IgnorePatterns = new List<string>
                      {
                          "bin","obj","Debug","Release","x64","x86","AnyCPU",".vs","vs",".vscode",
                          "*.exe","*.dll","*.pdb","*.config","*.exe.config",
                          "*.manifest","*.application","*.deploy",
                          "*.Designer.cs","*.g.cs","*.g.i.cs","*.i.cs",
                          ".DS_Store","Thumbs.db","desktop.ini","*.log","*.cache","*.tmp"
                      },
                      CodeExtensions = new List<string> { ".cs",".xaml",".xml",".json",".config",".cshtml",".razor" }
                }
            };

            foreach (var p in builtInProfiles)
            {
                _profiles.Add(new FilterProfile
                {
                    Name = p.Name,
                    Description = p.Description,
                    IgnorePatterns = p.IgnorePatterns,
                    CodeExtensions = p.CodeExtensions,
                    IgnoreEmptyFolders = true,
                    ShowFileSize = false,
                    ShowSystemFiles = false,
                    ShowOnlyCodeFiles = true,
                    IsBuiltIn = true
                });
            }

            _activeProfile = _profiles.FirstOrDefault();
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
                            profile.IsBuiltIn = false;
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

        public IEnumerable<FilterProfile> GetProfiles() => _profiles;

        public FilterProfile GetActiveProfile() => _activeProfile;

        public void SetActiveProfile(FilterProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);
            _activeProfile = profile;
        }

        public void SaveProfile(FilterProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            var fileName = $"{profile.Name}.json";
            var filePath = Path.Combine(_filtersDirectory, fileName);

            var json = JsonSerializer.Serialize(profile, _jsonOptions);
            File.WriteAllText(filePath, json);

            var existing = _profiles.FirstOrDefault(p => p.Name == profile.Name);
            if (existing != null) _profiles.Remove(existing);

            _profiles.Add(profile);
        }

        public void DeleteProfile(FilterProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);
            if (profile.IsBuiltIn) return;

            var fileName = $"{profile.Name}.json";
            var filePath = Path.Combine(_filtersDirectory, fileName);

            if (File.Exists(filePath)) File.Delete(filePath);

            _profiles.Remove(profile);

            if (_activeProfile == profile)
                _activeProfile = _profiles.FirstOrDefault();
        }

        public FilterProfile CreateProfile(string name, string description = "")
        {
            var profile = new FilterProfile
            {
                Name = name,
                Description = description,
                IgnorePatterns = new(_activeProfile.IgnorePatterns),
                CodeExtensions = new(_activeProfile.CodeExtensions),
                IgnoreEmptyFolders = _activeProfile.IgnoreEmptyFolders,
                ShowFileSize = _activeProfile.ShowFileSize,
                ShowSystemFiles = _activeProfile.ShowSystemFiles,
                ShowOnlyCodeFiles = _activeProfile.ShowOnlyCodeFiles,
                IsBuiltIn = false
            };

            return profile;
        }
    }
}
