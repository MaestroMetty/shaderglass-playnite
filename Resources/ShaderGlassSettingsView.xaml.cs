using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ShaderGlass
{
    public partial class ShaderGlassSettingsView : System.Windows.Controls.UserControl
    {
        private readonly ShaderGlassSettings settings;

        public ShaderGlassSettingsView()
        {
            InitializeComponent();
        }

        public ShaderGlassSettingsView(ShaderGlass plugin) : this()
        {
            this.settings = plugin.settings;
            DataContext = settings;
        }

        private void BrowseExecutableButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select ShaderGlass executable"
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
                Title = "Select Profiles Directory (you can type the path in the address bar)",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select Folder"
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
    }
}