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
using System.Text.RegularExpressions;
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
                    var match = Regex.Match(tag.Name, @"\[ShaderGlass\]\s*(.+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var content = match.Groups[1].Value.Trim();
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

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
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
    }
}