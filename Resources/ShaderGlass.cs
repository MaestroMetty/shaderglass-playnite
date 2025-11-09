using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ShaderGlass
{
    public class ShaderGlass : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public ShaderGlassSettings settings { get; private set; }

        // Track ShaderGlass processes per game
        private Dictionary<Guid, Process> shaderGlassProcesses = new Dictionary<Guid, Process>();

        public override Guid Id { get; } = Guid.Parse("b3ea67cd-a2e7-4f91-88c4-8486e31fc900");

        public ShaderGlass(IPlayniteAPI api) : base(api)
        {
            settings = new ShaderGlassSettings(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Check if game has ShaderGlass tag and launch ShaderGlass if needed
            if (args.Game?.TagIds == null || args.Game.TagIds.Count == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.ExecutablePath) || !File.Exists(settings.ExecutablePath))
            {
                logger.Warn("ShaderGlass executable path is not configured or does not exist.");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.ProfilesPath) || !Directory.Exists(settings.ProfilesPath))
            {
                logger.Warn("ShaderGlass profiles directory path is not configured or does not exist.");
                return;
            }

            // Get all tags for the game
            var gameTags = PlayniteApi.Database.Tags.Where(t => args.Game.TagIds.Contains(t.Id)).ToList();
            
            // Look for ShaderGlass tags
            string profileName = null;
            bool hasProfileSet = false;
            bool noFullscreen = false;
            bool pausedMode = false;

            foreach (var tag in gameTags)
            {
                if (tag.Name.StartsWith("[ShaderGlass]", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract content from tag
                    // Format: "[ShaderGlass] <profile-name>", "[ShaderGlass] NoFullscreen", or "[ShaderGlass] PausedMode"
                    // Since we already checked it starts with "[ShaderGlass]", just remove the prefix
                    string content = tag.Name.Substring("[ShaderGlass] ".Length).Trim();
                    
                    if (content.Equals("NoFullscreen", StringComparison.OrdinalIgnoreCase))
                    {
                        noFullscreen = true;
                    }
                    else if (content.Equals("PausedMode", StringComparison.OrdinalIgnoreCase))
                    {
                        pausedMode = true;
                    }
                    else if (string.IsNullOrWhiteSpace(profileName) &&!hasProfileSet)
                    {
                        // Use first profile name found (skip subsequent profile tags if user incorrectly sets multiple)
                        profileName = content;
                        hasProfileSet = true;
                    }
                }
            }

            // Launch ShaderGlass if we found a profile name
            // Note: NoFullscreen tag can exist alone if there's another tag with profile name
            if (!string.IsNullOrWhiteSpace(profileName))
            {
                LaunchShaderGlass(args.Game.Id, profileName, noFullscreen, pausedMode);
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Close ShaderGlass process if it was started for this game
            if (shaderGlassProcesses.ContainsKey(args.Game.Id))
            {
                var process = shaderGlassProcesses[args.Game.Id];
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error closing ShaderGlass process for game {args.Game.Name}");
                }
                finally
                {
                    if (process != null)
                    {
                        process.Dispose();
                    }
                    shaderGlassProcesses.Remove(args.Game.Id);
                }
            }
        }

        private void LaunchShaderGlass(Guid gameId, string profileName, bool noFullscreen, bool pausedMode)
        {
            try
            {
                // Ensure profile name has .sgp extension
                if (!profileName.EndsWith(".sgp", StringComparison.OrdinalIgnoreCase))
                {
                    profileName = profileName + ".sgp";
                }

                // Construct full path to profile file
                var profilePath = Path.Combine(settings.ProfilesPath, profileName);
                
                // Validate profile file exists
                if (!File.Exists(profilePath))
                {
                    logger.Warn($"ShaderGlass profile file not found: {profilePath}");
                    return;
                }

                var executablePath = settings.ExecutablePath;
                var executableDir = Path.GetDirectoryName(executablePath);
                var arguments = noFullscreen ? $"\"{profilePath}\"" : $"-f \"{profilePath}\"";
                if (pausedMode)
                {
                    arguments += " -p";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = executableDir,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                //TO-DO: Check if adding delay when its paused fixes weird behavior
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    shaderGlassProcesses[gameId] = process;
                    logger.Info($"Launched ShaderGlass for game {gameId} with profile {profilePath} (NoFullscreen: {noFullscreen}) (PausedMode: {pausedMode})");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error launching ShaderGlass for game {gameId} with profile {profileName}");
            }
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public void RefreshProfiles()
        {
            if (string.IsNullOrWhiteSpace(settings.ProfilesPath) || !Directory.Exists(settings.ProfilesPath))
            {
                return;
            }

            string[] shaderGlassProfiles = Directory.GetFiles(settings.ProfilesPath, "*.sgp", SearchOption.TopDirectoryOnly);
            Tag[] currentProfileTags = PlayniteApi.Database.Tags.Where(t => t.Name.StartsWith("[ShaderGlass]", StringComparison.OrdinalIgnoreCase)).ToArray();
            
            // Ensure IgnoredProfiles list exists
            if (settings.IgnoredProfiles == null)
            {
                settings.IgnoredProfiles = new List<string>();
            }

            // Create progress window
            var progressWindow = PlayniteApi.Dialogs.CreateWindow(new Playnite.SDK.WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = false,
                ShowCloseButton = false
            });
            var progressControl = new RefreshProfilesProgressWindow();
            progressWindow.Content = progressControl;
            progressWindow.Title = ResourceProvider.GetString("LOCShaderGlassRefreshingProfiles");
            progressWindow.Height = 160;
            progressWindow.Width = 400;
            progressWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
            progressWindow.ShowInTaskbar = false;
            progressWindow.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            progressWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            progressWindow.Show();
            
            // Process all profiles in directory
            int totalProfiles = shaderGlassProfiles.Length;
            int currentIndex = 0;
            int tagsAdded = 0;
            int tagsRemoved = 0;
            int ignoredCount = 0;
            
            foreach (string profile in shaderGlassProfiles)
            {
                currentIndex++;
                string profileFileName = Path.GetFileName(profile);
                string profileNameWithoutExtension = Path.GetFileNameWithoutExtension(profileFileName);
                
                // Update progress
                progressControl.UpdateProgress(currentIndex, totalProfiles, 
                    string.Format(ResourceProvider.GetString("LOCShaderGlassProcessingProfile"), profileNameWithoutExtension));
                System.Windows.Forms.Application.DoEvents();
                
                // Check if profile is ignored
                bool isIgnored = settings.IgnoredProfiles.Any(ignored => 
                    string.Equals(ignored, profileFileName, StringComparison.OrdinalIgnoreCase));
                
                if (isIgnored)
                {
                    ignoredCount++;
                }
                
                // Find existing tag for this profile
                var existingTag = currentProfileTags.FirstOrDefault(t => 
                    t.Name.Equals($"[ShaderGlass] {profileNameWithoutExtension}", StringComparison.OrdinalIgnoreCase));
                
                if (existingTag != null)
                {
                    // Tag exists - remove it if profile is ignored
                    if (isIgnored)
                    {
                        PlayniteApi.Database.Tags.Remove(existingTag);
                        tagsRemoved++;
                        logger.Info($"Removed ShaderGlass tag for ignored profile: {existingTag.Name}");
                    }
                }
                else
                {
                    // Tag doesn't exist - create it if profile is NOT ignored
                    if (!isIgnored)
                    {
                        var newTag = new Tag
                        {
                            Name = $"[ShaderGlass] {profileNameWithoutExtension}"
                        };
                        PlayniteApi.Database.Tags.Add(newTag);
                        tagsAdded++;
                        logger.Info($"Created ShaderGlass tag for profile: {newTag.Name}");
                    }
                }
            }

            // Close progress window
            progressWindow.Close();
            
            // Show notification with results
            string notificationText = string.Format(ResourceProvider.GetString("LOCShaderGlassRefreshNotification"), 
                tagsAdded, ignoredCount, totalProfiles);
            PlayniteApi.Notifications.Add("ShaderGlassRefresh", notificationText, Playnite.SDK.NotificationType.Info);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
            RefreshProfiles();
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ShaderGlassSettingsView(this);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCShaderGlassRefreshProfiles"),
                    MenuSection = "@ShaderGlass",
                    Action = (menuArgs) =>
                    {
                        RefreshProfiles();
                    }
                }
            };
        }
    }
}