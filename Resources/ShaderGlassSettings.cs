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

        [DontSerialize]
        public string BrowseButton_Label { get; set; }

        [DontSerialize]
        public string ExecutablePath_Label { get; set; }

        [DontSerialize]
        public string ExecutablePath_Description { get; set; }

        private string executablePath = string.Empty;
        public string ExecutablePath 
        { 
            get { return executablePath; } 
            set { executablePath = value; NotifyPropertyChanged("ExecutablePath"); } 
        }

        [DontSerialize]
        public string ProfilesPath_Label { get; set; }

        [DontSerialize]
        public string ProfilesPath_Description { get; set; }

        private string profilesPath = string.Empty;
        public string ProfilesPath 
        { 
            get { return profilesPath; } 
            set { profilesPath = value; NotifyPropertyChanged("ProfilesPath"); } 
        }

        // Parameterless constructor must exist if you want to use LoadPluginSettings method.
        public ShaderGlassSettings()
        {
        }

        public ShaderGlassSettings(ShaderGlass plugin)
        {
            BrowseButton_Label = "Browse...";
            ExecutablePath_Label = "ShaderGlass Executable Path:";
            ExecutablePath_Description = "Path to the ShaderGlass executable file (\\path\\to\\shaderglass\\ShaderGlass.exe)";
            ProfilesPath_Label = "Profiles Directory Path:";
            ProfilesPath_Description = "Path to the directory containing ShaderGlass profile files (.sgp files)";

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
                errors.Add("ShaderGlass executable path is required.");
            }
            else if (!File.Exists(ExecutablePath))
            {
                errors.Add("ShaderGlass executable path does not exist.");
            }
            else if (!ExecutablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("ShaderGlass executable path must include the .exe file extension.");
            }

            if (string.IsNullOrWhiteSpace(ProfilesPath))
            {
                errors.Add("Profiles directory path is required.");
            }
            else if (!Directory.Exists(ProfilesPath))
            {
                errors.Add("Profiles directory path does not exist.");
            }

            return errors.Count == 0;
        }
    }
}