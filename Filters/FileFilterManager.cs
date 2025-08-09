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
            // Perfil para projetos Flutter
            var flutterProfile = new FilterProfile
            {
                Name = "Flutter",
                Description = "Filtro para projetos Flutter",
                IgnorePatterns = new List<string>
    {
        // Diretórios de plataforma (geralmente não editados diretamente)
        "android", "ios", "linux", "macos", "windows", "web",
        
        // Diretórios gerados
        "build", "dart_tool", ".dart_tool", ".pub", ".flutter-plugins",
        
        // Diretórios de IDE
        ".idea", ".vscode", ".vs",
        
        // Arquivos gerados
        "*.g.dart", "*.r.dart", "*.gr.dart", "*.freezed.dart",
        "*.inject.dart", "*.mocks.dart",
        
        // Outros arquivos não editáveis
        "*.apk", "*.aab", "*.ipa", "*.app", "*.exe", "*.dll",
        "*.so", "*.dylib", "*.jar", "*.aar", "*.framework",
        
        // Arquivos de sistema
        ".DS_Store", "Thumbs.db", "desktop.ini",
        
        // Logs e cache
        "*.log", "*.cache", "*.tmp"
    },
                CodeExtensions = new List<string>
    {
        ".dart", ".yaml", ".yml", ".json", ".xml", ".html", ".css", ".js", ".ts"
    },
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = true
            };

            // Perfil para projetos .NET
            var dotnetProfile = new FilterProfile
            {
                Name = ".NET",
                Description = "Filtro para projetos .NET",
                IgnorePatterns = new List<string>
                {
                    // Diretórios gerados
                    "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU",
                    ".vs", "vs", ".vscode", "node_modules",
                    
                    // Arquivos gerados
                    "*.exe", "*.dll", "*.pdb", "*.config", "*.exe.config",
                    "*.manifest", "*.application", "*.deploy",
                    
                    // Arquivos de sistema
                    ".DS_Store", "Thumbs.db", "desktop.ini",
                    
                    // Logs e cache
                    "*.log", "*.cache", "*.tmp"
                },
                CodeExtensions = new List<string>
                {
                    ".cs", ".vb", ".fs", ".xaml", ".xml", ".json", ".config", ".cshtml", ".razor"
                },
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = true
            };

            // Perfil para projetos Node.js
            var nodejsProfile = new FilterProfile
            {
                Name = "Node.js",
                Description = "Filtro para projetos Node.js",
                IgnorePatterns = new List<string>
                {
                    // Diretórios gerados
                    "node_modules", ".nyc_output", "coverage", ".cache",
                    "dist", "build", "out",
                    
                    // Arquivos gerados
                    "*.js", "*.map", "*.d.ts",
                    
                    // Arquivos de sistema
                    ".DS_Store", "Thumbs.db", "desktop.ini",
                    
                    // Logs e cache
                    "*.log", "*.cache", "*.tmp"
                },
                CodeExtensions = new List<string>
                {
                    ".ts", ".tsx", ".jsx", ".js", ".json", ".md", ".yml", ".yaml"
                },
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = true
            };

            // Perfil para projetos Android (Java/Kotlin)
            var androidProfile = new FilterProfile
            {
                Name = "Android",
                Description = "Filtro para projetos Android",
                IgnorePatterns = new List<string>
                {
                    // Diretórios gerados
                    "build", ".gradle", ".idea", "captures", ".cxx",
                    "app/build", "app/build/intermediates", "app/build/generated",
                    
                    // Arquivos gerados
                    "*.apk", "*.aab", "*.jar", "*.aar", "*.dex",
                    "*.R.java", "*.BuildConfig.java",
                    
                    // Arquivos de sistema
                    ".DS_Store", "Thumbs.db", "desktop.ini",
                    
                    // Logs e cache
                    "*.log", "*.cache", "*.tmp"
                },
                CodeExtensions = new List<string>
                {
                    ".java", ".kt", ".xml", ".gradle", ".properties", ".json"
                },
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = true
            };

            // Perfil para projetos Web (HTML/CSS/JS)
            var webProfile = new FilterProfile
            {
                Name = "Web",
                Description = "Filtro para projetos Web",
                IgnorePatterns = new List<string>
                {
                    // Diretórios gerados
                    "dist", "build", "out", ".cache", ".tmp",
                    "node_modules", ".nyc_output", "coverage",
                    
                    // Arquivos gerados
                    "*.min.js", "*.min.css", "*.bundle.js", "*.bundle.css",
                    "*.map",
                    
                    // Arquivos de sistema
                    ".DS_Store", "Thumbs.db", "desktop.ini",
                    
                    // Logs e cache
                    "*.log", "*.cache", "*.tmp"
                },
                CodeExtensions = new List<string>
                {
                    ".html", ".css", ".scss", ".sass", ".less", ".js", ".ts", ".jsx", ".tsx",
                    ".json", ".md", ".yml", ".yaml"
                },
                IgnoreEmptyFolders = true,
                ShowFileSize = false,
                ShowSystemFiles = false,
                ShowOnlyCodeFiles = true
            };

            _profiles.Add(flutterProfile);
            _profiles.Add(dotnetProfile);
            _profiles.Add(nodejsProfile);
            _profiles.Add(androidProfile);
            _profiles.Add(webProfile);

            // Define o primeiro perfil como ativo por padrão
            _activeProfile = _profiles.FirstOrDefault();

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
                _activeProfile = _profiles.FirstOrDefault();
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