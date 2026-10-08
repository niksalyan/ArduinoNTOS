using NTOSDev.Controls;
using NTOSDev.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev
{
    public partial class ProjectProperties : DockContent
    {
        
        private const string ConfigFileName = "config.json";

        private string? _rootPath;
        private ProjectPropertiesModel _properties = new();

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public string? RootPath
        {
            get => _rootPath;

            private set
            {
                if (string.Equals(_rootPath, value, StringComparison.OrdinalIgnoreCase))
                    return;

                _rootPath = value;

                LoadConfig();
            }
        }

        public ProjectPropertiesModel Properties
        {
            get => _properties;

            private set
            {
                _properties = value ?? new ProjectPropertiesModel();

                propertyGrid.SelectedObject = _properties;
                propertyGrid.Refresh();
            }
        }

        public ProjectProperties()
        {
            InitializeComponent();

            propertyGrid.SelectedObject = Properties;
        }

        public ProjectProperties LoadFolder(string path)
        {
            RootPath = path;
            return this;
        }

        private string? ConfigPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RootPath))
                    return null;

                return Path.Combine(RootPath, ConfigFileName);
            }
        }

        private void LoadConfig()
        {
            // Always start from a clean model.
            Properties = new ProjectPropertiesModel();

            var configPath = ConfigPath;

            if (string.IsNullOrWhiteSpace(configPath))
                return;

            try
            {
                if (!Directory.Exists(RootPath))
                    return;

                if (!File.Exists(configPath))
                {
                    // No config yet.
                    // Keep default properties and create it immediately.
                    SaveConfig();
                    return;
                }

                var json = File.ReadAllText(configPath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    SaveConfig();
                    return;
                }

                var loaded = JsonSerializer.Deserialize<ProjectPropertiesModel>(
                    json,
                    _jsonOptions);

                if (loaded != null)
                {
                    Properties = loaded;
                }
                else
                {
                    SaveConfig();
                }
            }
            catch
            {
                // A broken config must never prevent the project from loading.
                // Keep default properties.
                Properties = new ProjectPropertiesModel();
            }
        }

        private void SaveConfig()
        {
            var configPath = ConfigPath;

            if (string.IsNullOrWhiteSpace(configPath))
                return;

            try
            {
                if (!Directory.Exists(RootPath))
                    return;

                var json = JsonSerializer.Serialize(
                    Properties,
                    _jsonOptions);

                var tempPath = configPath + ".tmp";

                File.WriteAllText(tempPath, json);

                // Replace the existing file atomically where possible.
                if (File.Exists(configPath))
                {
                    File.Replace(
                        tempPath,
                        configPath,
                        null);
                }
                else
                {
                    File.Move(tempPath, configPath);
                }
            }
            catch
            {
                // Config persistence should never crash NTOSDev.
                //
                // The in-memory Properties object remains valid even if
                // the file cannot be written.
            }
        }

        private void propertyGrid_PropertyValueChanged(
            object? s,
            PropertyValueChangedEventArgs e)
        {
            SaveConfig();
        }
    }
}