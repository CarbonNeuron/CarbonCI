using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Semver;

namespace CarbonCI
{
    public class DiscordCommands
    {
        [Group("build")] // let's mark this class as a command group
        [Description("automated build and deployment commands.")] // give it a description for help purposes
        [Hidden] // let's hide this from the eyes of curious users
        [RequireRolesAttribute("Developer")] // and restrict this to users who have appropriate permissions
        public class ExampleGrouppedCommands
        {
            [Command("patch"), Description("Builds a patch")]
            public async Task patch(CommandContext ctx)
            {
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
                // let's trigger a typing indicator to let
                // users know we're working
                await ctx.TriggerTypingAsync();
                var latest = await Program.getLatestVersion();
                var message = await ctx.RespondAsync($"Building patch {latest.incrementPatch()} from commits...");
                var tt = await Program.runBuildScript(latest.incrementPatch());
                await ctx.TriggerTypingAsync();
                if (tt)
                {
                    Dictionary<String, Stream> fileDict = new Dictionary<string, Stream>();
                    fileDict.Add("AmongUsCapture-x32.exe", File.OpenRead(Path.Combine(x86ApplicationOutput).Replace("file:\\", "")));
                    fileDict.Add("AmongUsCapture-x64.exe", File.OpenRead(Path.Combine(x64ApplicationOutput).Replace("file:\\", "")));
                    await message.DeleteAsync();
                    await ctx.RespondWithFilesAsync(fileDict, $"Build {latest.incrementPatch()} success!");
                }
                else
                {
                    MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(Settings.LogHolder));
                    await ctx.RespondWithFileAsync(stream, "log.txt");
                }
            }
            
            [Command("minor"), Description("Builds a minor release")]
            public async Task minor(CommandContext ctx)
            {
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
                // let's trigger a typing indicator to let
                // users know we're working
                await ctx.TriggerTypingAsync();
                var latest = await Program.getLatestVersion();
                var message = await ctx.RespondAsync($"Building minor {latest.incrementMinor()} from commits...");
                var tt = await Program.runBuildScript(latest.incrementMinor());
                await ctx.TriggerTypingAsync();
                if (tt)
                {
                    Dictionary<String, Stream> fileDict = new Dictionary<string, Stream>();
                    fileDict.Add("AmongUsCapture-x32.exe", File.OpenRead(Path.Combine(x86ApplicationOutput).Replace("file:\\", "")));
                    fileDict.Add("AmongUsCapture-x64.exe", File.OpenRead(Path.Combine(x64ApplicationOutput).Replace("file:\\", "")));
                    await message.DeleteAsync();
                    await ctx.RespondWithFilesAsync(fileDict, $"Build {latest.incrementMinor()} success!");
                }
                else
                {
                    MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(Settings.LogHolder));
                    await message.ModifyAsync($"Build {latest.incrementMinor()} failed uh oh.!");
                    await ctx.RespondWithFileAsync(stream, "log.txt");
                }
            }
            
            [Command("major"), Description("Builds a major release")]
            public async Task major(CommandContext ctx)
            {
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
                // let's trigger a typing indicator to let
                // users know we're working
                await ctx.TriggerTypingAsync();
                var latest = await Program.getLatestVersion();
                var message = await ctx.RespondAsync($"Building major {latest.incrementMajor()} from commits...");
                var tt = await Program.runBuildScript(latest.incrementMajor());
                await ctx.TriggerTypingAsync();
                if (tt)
                {
                    Dictionary<String, Stream> fileDict = new Dictionary<string, Stream>();
                    fileDict.Add("AmongUsCapture-x32.exe", File.OpenRead(Path.Combine(x86ApplicationOutput).Replace("file:\\", "")));
                    fileDict.Add("AmongUsCapture-x64.exe", File.OpenRead(Path.Combine(x64ApplicationOutput).Replace("file:\\", "")));
                    await message.DeleteAsync();
                    await ctx.RespondWithFilesAsync(fileDict, $"Build {latest.incrementMajor()} success!");
                }
                else
                {
                    MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(Settings.LogHolder));
                    await message.ModifyAsync($"Build {latest.incrementMajor()} failed uh oh.!");
                    await ctx.RespondWithFileAsync(stream, "log.txt");
                }
            }
            [Command("alpha"), Description("Builds a prerelease")]
            public async Task alpha(CommandContext ctx)
            {
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
                // let's trigger a typing indicator to let
                // users know we're working
                await ctx.TriggerTypingAsync();
                var latest = await Program.getLatestVersion();
                var message = await ctx.RespondAsync($"Building alpha {latest.incrementAlpha()} from commits...");
                var tt = await Program.runBuildScript(latest.incrementAlpha());
                await ctx.TriggerTypingAsync();
                if (tt)
                {
                    Dictionary<String, Stream> fileDict = new Dictionary<string, Stream>();
                    fileDict.Add("AmongUsCapture-x32.exe", File.OpenRead(Path.Combine(x86ApplicationOutput).Replace("file:\\", "")));
                    fileDict.Add("AmongUsCapture-x64.exe", File.OpenRead(Path.Combine(x64ApplicationOutput).Replace("file:\\", "")));
                    await message.DeleteAsync();
                    await ctx.RespondWithFilesAsync(fileDict, $"Build {latest.incrementAlpha()} success!");
                }
                else
                {
                    MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(Settings.LogHolder));
                    await message.ModifyAsync($"Build {latest.incrementAlpha()} failed uh oh.!");
                    await ctx.RespondWithFileAsync(stream, "log.txt");
                }
            }
            // all the commands will need to be executed as <prefix>build <command> <arguments>
        }
    }
}