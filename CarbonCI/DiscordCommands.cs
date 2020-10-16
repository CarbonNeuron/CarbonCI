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
using LiteDB;
using Semver;

namespace CarbonCI
{
    public class DiscordCommands
    {
        [Group("locks", CanInvokeWithoutSubcommand = true)] // let's mark this class as a command group
        [Description("Meme commands")] // give it a description for help purposes
        [Hidden] // let's hide this from the eyes of curious users
        [RequireRolesAttribute("Admin+Dev")] // and restrict this to users who have appropriate permissions
        public class LockGroup
        {
            public async Task ExecuteGroupAsync(CommandContext ctx, [Description("Who's locks to lookup")]DiscordMember member)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder();
                embed.WithFooter("Powered by CarbonCI®",
                    "https://cdn.discordapp.com/avatars/759076557750140930/26713c78e70e69f063bda9e918b855bb.png?size=128").WithTimestamp(DateTimeOffset.UtcNow)
                    .WithTitle($"Username locks for **{member.DisplayName}**");
                var col = Program.db.GetCollection<LockedDiscordUser>("LockedDiscordUsers");
                
                if(col.Exists(x => x.guildID == ctx.Guild.Id && x.UserId == member.Id))
                {
                    var thing = col.FindOne(x => x.guildID == ctx.Guild.Id && x.UserId == member.Id);
                    embed.AddField("**Locked to**:", $"{thing.nickToLockTo}");
                    embed.Color = DiscordColor.SpringGreen;
                }
                else
                {
                    embed.Description = "No locks for the specified user!";
                    embed.Color = DiscordColor.Red;
                }
                await ctx.RespondAsync(null, false, embed.Build());

            }

            [Command("clear"), Description("clears a users locks")]
            public async Task UnlockAdd(CommandContext ctx, [Description("Whose nickname should I unlock")] DiscordMember member)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder();
                embed.WithFooter("Powered by CarbonCI®",
                    "https://cdn.discordapp.com/avatars/759076557750140930/26713c78e70e69f063bda9e918b855bb.png?size=128").WithTimestamp(DateTimeOffset.UtcNow);
                var col = Program.db.GetCollection<LockedDiscordUser>("LockedDiscordUsers");
                try
                {
                    var doc = col.FindOne(b => b.UserId == member.Id && b.guildID == ctx.Guild.Id);
                    if (doc is null)
                    {
                        embed.WithTitle($"**Error**");
                        embed.Color = DiscordColor.Red;
                        embed.WithDescription("Doc is null");
                    }
                    else
                    {
                        col.Delete(doc.Id);
                        embed.WithTitle($"**Unlocked {member.Username}!**");
                        embed.Color = DiscordColor.SpringGreen;
                        embed.WithDescription("Successfully unlocked.");
                    }
                    await member.ModifyAsync("");
                }
                catch (Exception b)
                {
                    embed.WithTitle($"**Error**");
                    embed.Color = DiscordColor.Red;
                    embed.WithDescription(b.Message);
                }

                await ctx.RespondAsync(null, false, embed.Build());
            }

            [Command("add"), Description("adds a user to the locks list")]
            public async Task LockAdd(CommandContext ctx, [Description("Whose nickname should I lock?")] DiscordMember member, [RemainingText, Description("Nickname to set them too.")] string nickname)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder();
                embed.WithFooter("Powered by CarbonCI®",
                        "https://cdn.discordapp.com/avatars/759076557750140930/26713c78e70e69f063bda9e918b855bb.png?size=128").WithTimestamp(DateTimeOffset.UtcNow);
                try
                {
                    
                    var col = Program.db.GetCollection<LockedDiscordUser>("LockedDiscordUsers");
                    
                    var h = new LockedDiscordUser {UserId = member.Id, guildID = ctx.Guild.Id, nickToLockTo = nickname};
                    embed.WithColor(DiscordColor.SpringGreen);
                    if (col.Exists(x => x.UserId == member.Id && x.guildID == ctx.Guild.Id))
                    {
                        var a = col.FindOne(x => x.UserId == member.Id && x.guildID == ctx.Guild.Id);
                        a.nickToLockTo = nickname;
                        col.Update(a);
                        embed.WithTitle("**Modified lock**");
                    }
                    else
                    {
                        col.Insert(h);
                        embed.WithTitle("**Locked user nickname**");
                    }

                    embed.AddField("**Locked user:**", member.Username, true);
                    embed.AddField("**Locked to name:**", nickname, true);
                    await member.ModifyAsync(nickname);
                }
                catch (Exception e)
                {
                    embed.WithTitle("**Error!**");
                    embed.WithColor(DiscordColor.Red);
                    embed.Description = e.Message;
                }

                await ctx.RespondAsync(null, false, embed.Build());

            }
        }
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