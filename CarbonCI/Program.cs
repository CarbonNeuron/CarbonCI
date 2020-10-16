using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using LiteDB;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using Octokit;
using Semver;
using LogLevel = DSharpPlus.LogLevel;

namespace CarbonCI
{
    internal static class Program
    {
        static string Owner = "denverquane";
        static DiscordClient discord;
        static CommandsNextModule commands;
        static GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("CarbonCI"));
        public static LiteDatabase db = new LiteDatabase(@"MyData.db");
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            //Do some sync stuff before starting our async method
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            await setupDiscord();
            await setupGithub();
            //var latest = await getLatestVersion();
            discord.GuildMemberUpdated += DiscordOnGuildMemberUpdated;
            //var tt = await runBuildScript(latest.incrementPatch());
            _logger.Info($"idle");
            await Task.Delay(-1); //Never returns.
        }

        private static async Task DiscordOnGuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            try
            {
                var col = db.GetCollection<LockedDiscordUser>("LockedDiscordUsers");
                if (col.Exists(q => q.UserId == e.Member.Id))
                {
                    try
                    {
                        var thing = col.FindOne(q => q.guildID == e.Guild.Id && q.UserId == e.Member.Id);
                        if (thing.nickToLockTo != e.NicknameAfter)
                        {
                            await e.Member.ModifyAsync(nickname: thing.nickToLockTo);
                        }
                    }
                    catch (Exception b)
                    {
                        _logger.Error(b);
                    }
                }
            }
            catch (Exception j)
            {
                _logger.Error(j);
            }
        }


        public static async Task<bool> runBuildScript(SemVersion BuildVersion)
        {
            try
            {
                string[] FolderPath =
                {
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),
                    @"Script\", @"amonguscapture"
                };
                string[] x86ApplicationOutput =
                {
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),
                    @"Script\", @"amonguscapture\", @"AmongUsCapture-x32.exe"
                };
                string[] x64ApplicationOutput =
                {
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),
                    @"Script\", @"amonguscapture\", @"AmongUsCapture-x64.exe"
                };
                string[] paths =
                {
                    System.IO.Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase),
                    @"Script\buildScript.bat"
                };
                if (Directory.Exists(Path.Combine(FolderPath).Replace("file:\\", ""))) //If it didn't clean up last time
                {
                    try
                    {
                        _logger.Debug("Deleting old files.");
                        Directory.Delete(Path.Combine(FolderPath).Replace("file:\\", ""), true);
                    } 
                    catch(System.UnauthorizedAccessException)
                    {
                    }
                    
                }

                var version = BuildVersion;
                Process proc = new Process();
                
                var finalpath = Path.Combine(paths).Replace("file:\\", "");
                _logger.Info(finalpath);
                string preRelease = "";
                if (version.getAlphaNumber() != 0)
                {
                    preRelease = "--pre-release";
                }
                proc.StartInfo.FileName = finalpath;
                proc.StartInfo.Arguments =
                    $"{version.Major}.{version.Minor}.{version.Patch}.{version.getAlphaNumber()} {version.ToString()} {preRelease}";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();
                string err = proc.StandardError.ReadToEnd();
                _logger.Error(err);
                //Process should be finished now?
                Settings.LogHolder = err;
                if (File.Exists(Path.Combine(x86ApplicationOutput).Replace("file:\\", "")) && File.Exists(Path.Combine(x64ApplicationOutput).Replace("file:\\", "")))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<SemVersion> getLatestVersion()
        {
            var release = await gitHub.Repository.Release.GetLatest(Owner, "amonguscapture");
            return SemVersion.TryParse(release.TagName, out var version) ? version : new SemVersion(0, 0, 0);
        }

        private static async Task setupGithub()
        {
            var basicAuth =
                new Credentials(Settings.PSettings.GithubUsername,
                    Settings.PSettings.GithubPassword); // NOTE: not real credentials
            gitHub.Credentials = basicAuth;
            _logger.Info($"Logged into github.");
        }

        private static async Task setupDiscord()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Settings.PSettings.DiscordToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ".."
            });
            commands.RegisterCommands<DiscordCommands>();

            await discord.ConnectAsync();
        }
    }

    public class LockedDiscordUser
    {
        public ObjectId Id { get; set; }
        public ulong UserId { get; set; }
        public string nickToLockTo { get; set; }
        
        public ulong guildID { get; set; }
    }
}