using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace DiscordController
{
    public class DiscordBot
    {
        private DiscordClient discord;
        private CommandsNextModule commands;
        public async Task Bot(string discordBotToken)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = discordBotToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });
            
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ";;"
            });
            
            commands.RegisterCommands<DiscordCommands>();
        }
        public Task StartAsync() => this.discord.ConnectAsync();
        public Task StopAsync() => this.discord.DisconnectAsync();
    }
}