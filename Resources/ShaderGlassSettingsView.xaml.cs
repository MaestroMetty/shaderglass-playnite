using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ShaderGlass
{
    public class ProfileItem : INotifyPropertyChanged
    {
        private bool isEnabled;
        public string ProfileName { get; set; }
        public bool IsEnabled 
        { 
            get { return isEnabled; }
            set 
            { 
                isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class ShaderGlassSettingsView : System.Windows.Controls.UserControl
    {
        private readonly ShaderGlassSettings settings;
        private readonly ShaderGlass plugin;
        private static readonly ILogger logger = LogManager.GetLogger();
        private ObservableCollection<ProfileItem> profileItems = new ObservableCollection<ProfileItem>();

        public ShaderGlassSettingsView()
        {
            InitializeComponent();
        }

        public ShaderGlassSettingsView(ShaderGlass plugin) : this()
        {
            this.plugin = plugin;
            this.settings = plugin.settings;
            DataContext = settings;
            LoadProfiles();
            
            // Update profiles when ProfilesPath changes
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProfilesPath")
            {
                LoadProfiles();
            }
        }

        private void LoadProfiles()
        {
            profileItems.Clear();
            
            if (string.IsNullOrWhiteSpace(settings.ProfilesPath) || !Directory.Exists(settings.ProfilesPath))
            {
                return;
            }

            string[] profiles = Directory.GetFiles(settings.ProfilesPath, "*.sgp", SearchOption.TopDirectoryOnly);
            foreach (string profile in profiles.OrderBy(p => Path.GetFileName(p)))
            {
                string profileName = Path.GetFileName(profile);
                bool isIgnored = settings.IgnoredProfiles != null && 
                    settings.IgnoredProfiles.Any(ignored => string.Equals(ignored, profileName, StringComparison.OrdinalIgnoreCase));
                
                var item = new ProfileItem
                {
                    ProfileName = profileName,
                    IsEnabled = !isIgnored
                };
                
                item.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(ProfileItem.IsEnabled))
                    {
                        UpdateIgnoredProfiles();
                    }
                };
                
                profileItems.Add(item);
            }
        }

        private void BrowseExecutableButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = ResourceProvider.GetString("LOCShaderGlassSelectExecutableTitle")
            };

            if (!string.IsNullOrEmpty(settings.ExecutablePath) && File.Exists(settings.ExecutablePath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(settings.ExecutablePath);
                dialog.FileName = Path.GetFileName(settings.ExecutablePath);
            }

            if (dialog.ShowDialog() == true)
            {
                settings.ExecutablePath = dialog.FileName;
            }
        }

        private void BrowseProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = ResourceProvider.GetString("LOCShaderGlassSelectProfilesTitle"),
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = ResourceProvider.GetString("LOCShaderGlassSelectFolder")
            };

            if (!string.IsNullOrEmpty(settings.ProfilesPath) && Directory.Exists(settings.ProfilesPath))
            {
                dialog.InitialDirectory = settings.ProfilesPath;
            }

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FileName;
                
                // If user selected a file, get its directory
                if (File.Exists(selectedPath))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath);
                }
                // If user typed a directory path or navigated to a folder
                else if (!Directory.Exists(selectedPath))
                {
                    // Try to get directory from the path (in case user typed a file path that doesn't exist)
                    var directory = Path.GetDirectoryName(selectedPath);
                    if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    {
                        selectedPath = directory;
                    }
                    // If that doesn't work, use the initial directory (where user navigated to)
                    else if (!string.IsNullOrEmpty(dialog.InitialDirectory) && Directory.Exists(dialog.InitialDirectory))
                    {
                        selectedPath = dialog.InitialDirectory;
                    }
                }

                if (!string.IsNullOrEmpty(selectedPath) && Directory.Exists(selectedPath))
                {
                    settings.ProfilesPath = selectedPath;
                }
            }
        }

        private void RefreshProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            // Update ignored profiles list based on checkbox states
            UpdateIgnoredProfiles();
            
            // Use the plugin's RefreshProfiles method
            plugin.RefreshProfiles();
            
            // Reload the profiles list to reflect any changes
            LoadProfiles();
        }

        private void UpdateIgnoredProfiles()
        {
            if (settings.IgnoredProfiles == null)
            {
                settings.IgnoredProfiles = new System.Collections.Generic.List<string>();
            }

            settings.IgnoredProfiles.Clear();
            foreach (var item in profileItems)
            {
                if (!item.IsEnabled)
                {
                    settings.IgnoredProfiles.Add(item.ProfileName);
                }
            }
        }

        public ObservableCollection<ProfileItem> ProfileItems => profileItems;
    }
}