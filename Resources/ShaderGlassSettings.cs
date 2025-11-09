using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ShaderGlass
{
    public class ShaderGlassSettings : ISettings, INotifyPropertyChanged
    {
        private readonly ShaderGlass plugin;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string executablePath = string.Empty;
        public string ExecutablePath 
        { 
            get { return executablePath; } 
            set { executablePath = value; NotifyPropertyChanged("ExecutablePath"); } 
        }

        private string profilesPath = string.Empty;
        public string ProfilesPath 
        { 
            get { return profilesPath; } 
            set { profilesPath = value; NotifyPropertyChanged("ProfilesPath"); } 
        }

        private List<string> ignoredProfiles = new List<string>();
        public List<string> IgnoredProfiles 
        { 
            get { return ignoredProfiles; } 
            set { ignoredProfiles = value ?? new List<string>(); NotifyPropertyChanged("IgnoredProfiles"); } 
        }

        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public ShaderGlassSettings()
        {
        }

        public ShaderGlassSettings(ShaderGlass plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;
            Load();
        }

        private void Load()
        {
            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ShaderGlassSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                if (savedSettings.ExecutablePath != null)
                {
                    ExecutablePath = savedSettings.ExecutablePath;
                }
                if (savedSettings.ProfilesPath != null)
                {
                    ProfilesPath = savedSettings.ProfilesPath;
                }
                if (savedSettings.IgnoredProfiles != null)
                {
                    IgnoredProfiles = savedSettings.IgnoredProfiles;
                }
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to settings.
            Load();
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings.
            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ExecutablePath))
            {
                errors.Add(ResourceProvider.GetString("LOCShaderGlassExecutablePathRequired"));
            }
            else if (!File.Exists(ExecutablePath))
            {
                errors.Add(ResourceProvider.GetString("LOCShaderGlassExecutablePathNotExist"));
            }
            else if (!ExecutablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(ResourceProvider.GetString("LOCShaderGlassExecutablePathMustBeExe"));
            }

            if (string.IsNullOrWhiteSpace(ProfilesPath))
            {
                errors.Add(ResourceProvider.GetString("LOCShaderGlassProfilesPathRequired"));
            }
            else if (!Directory.Exists(ProfilesPath))
            {
                errors.Add(ResourceProvider.GetString("LOCShaderGlassProfilesPathNotExist"));
            }

            return errors.Count == 0;
        }
    }
}