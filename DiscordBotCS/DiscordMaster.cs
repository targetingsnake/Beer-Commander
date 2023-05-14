using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Discord.Webhook;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Exchange;
//using System.Xml;

namespace DiscordBot
{
    public class DiscordMaster
    {
        public static Task Main(string token, ulong[] masters) => new DiscordMaster().MainAsync(token, masters);

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private DiscordSocketClient _client = new DiscordSocketClient();

        private ulong[] _Masters = { };

        private async Task MainAsync(string token, ulong[] masters)
        {
            Console.WriteLine("Initialising");

            _Masters = masters;

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task Client_Ready()
        {
            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            //var guild = _client.GetGuild(guildId);

            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            //var guildCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            //guildCommand.WithName("infome");

            // Descriptions can have a max length of 100.
            //guildCommand.WithDescription("");
            try
            {
                List<ApplicationCommandProperties> applicationCommandProperties = new();

                var globalCommand_infome = new SlashCommandBuilder();
                globalCommand_infome.WithName("infome");
                globalCommand_infome.WithDescription("Answers with user information");
                applicationCommandProperties.Add(globalCommand_infome.Build());
            
                // Let's do our global command

                Dictionary<ulong, List<GameServer>> guildServers = new Dictionary<ulong, List<GameServer>>();
                Dictionary<ulong, SlashCommandBuilder> guildCommands = new Dictionary<ulong, SlashCommandBuilder>();
                foreach(GameServer g in Distribution.GameServers.Values)
                {
                    foreach(ulong guid in g.GuildID)
                    {
                        if (!guildServers.ContainsKey(guid))
                        {
                            guildServers.Add(guid, new List<GameServer>());
                        }
                        guildServers[guid].Add(g);
                    }
                }
                foreach (ulong guid in guildServers.Keys)
                {
                    List<ApplicationCommandProperties> applicationCommandPropertiesGuild = new();
                    var serverOption = new SlashCommandOptionBuilder()
                        .WithName("server")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithDescription("The server you want to do the action to.")
                        .WithRequired(true);
                    foreach (GameServer g in guildServers[guid])
                    {
                        serverOption.AddChoice(g.name, g.commandname);
                    }

                    var globalCommand_start = new SlashCommandBuilder();
                    globalCommand_start.WithName("start");
                    globalCommand_start.WithDescription("Starts Gameserver");
                    globalCommand_start.AddOption(serverOption);
                    applicationCommandPropertiesGuild.Add(globalCommand_start.Build());

                    var globalCommand_stop = new SlashCommandBuilder();
                    globalCommand_stop.WithName("stop");
                    globalCommand_stop.WithDescription("Stops Gameserver");
                    globalCommand_stop.AddOption(serverOption);
                    applicationCommandPropertiesGuild.Add(globalCommand_stop.Build());

                    var globalCommand_update = new SlashCommandBuilder();
                    globalCommand_update.WithName("update");
                    globalCommand_update.WithDescription("Updates Gameserver");
                    globalCommand_update.AddOption(serverOption);
                    applicationCommandPropertiesGuild.Add(globalCommand_update.Build());

                    var globalCommand_status = new SlashCommandBuilder();
                    globalCommand_status.WithName("status");
                    globalCommand_status.WithDescription("Gibt Status des Gameserver zurück");
                    globalCommand_status.AddOption(serverOption); 
                    applicationCommandPropertiesGuild.Add(globalCommand_status.Build());

                    var globalCommand_test = new SlashCommandBuilder();
                    globalCommand_test.WithName("test");
                    globalCommand_test.WithDescription("Tested die Verbindung zum Gameserver");
                    globalCommand_test.AddOption(serverOption);
                    applicationCommandPropertiesGuild.Add(globalCommand_test.Build());

                    SocketGuild guildSrc = _client.GetGuild(guid);
                    await guildSrc.BulkOverwriteApplicationCommandAsync(applicationCommandPropertiesGuild.ToArray());
                }


                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                //await guild.CreateApplicationCommandAsync(guildCommand.Build());

                // With global commands we don't need the guild.
                await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
                //await _client.CreateGlobalApplicationCommandAsync(globalCommand_infome.Build());

                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }


        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            EmbedBuilder emb = new EmbedBuilder();
            Embed[] embeds = new Embed[1];
            if (_Masters.Contains(command.User.Id))
            {
                Console.WriteLine("DC Master has wridden.");
            }
            switch (command.CommandName)
            {
                case "infome":
                    emb.WithAuthor(command.User.Username, command.User.GetAvatarUrl());
                    emb.WithDescription(command.User.Mention);
                    emb.WithTitle("Userinfo");
                    EmbedFieldBuilder field = new EmbedFieldBuilder();
                    field.WithName("ID");
                    field.WithValue(command.User.Id);
                    emb.WithFields(field);
                    embeds[0] = emb.Build();
                    await command.RespondAsync("", embeds);
                    break;
                case "start":
                    executeGameserverCMD(command, "start");
                    break;
                case "stop":
                    executeGameserverCMD(command, "stop");
                    break;
                case "update":
                    executeGameserverCMD(command, "update");
                    break;
                case "status":
                    executeGameserverCMD(command, "status");
                    break;
                case "test":
                    executeGameserverCMD(command, "test");
                    break;
                default:
                    await command.RespondAsync($"You executed {command.Data.Name}");
                    break;
            }
        }

        private async void executeGameserverCMD(SocketSlashCommand command, string gameservercmd)
        {
            EmbedBuilder emb = new EmbedBuilder();
            Embed[] embeds = new Embed[1];
            SocketSlashCommandDataOption[] options = command.Data.Options.ToArray();
            string path = "";
            ulong[] allowedUsers = new ulong[0];
            ulong[] allowedRoles = new ulong[0];
            foreach (SocketSlashCommandDataOption option in options)
            {
                if (option.Name == "server")
                {
                    path = Distribution.GameServers[(string)option.Value].address;
                    allowedUsers = Distribution.GameServers[(string)option.Value].allowedUsers;
                    allowedRoles = Distribution.GameServers[(string)option.Value].allowedRoles;
                }
            }
            if (command.IsDMInteraction)
            {
                await command.RespondAsync("Operation not permited in DM");
                return;
            }
            SocketGuild guild = _client.GetGuild((ulong) command.GuildId);
            SocketGuildUser usr = guild.GetUser(command.User.Id);
            SocketRole[] roles = usr.Roles.ToArray();
            bool roleFound = false;
            foreach ( var role in roles )
            {
                if (allowedRoles.Contains(role.Id))
                {
                    roleFound = true;
                }
            }
            if ((!allowedUsers.Contains(command.User.Id) && !roleFound && !_Masters.Contains(command.User.Id)) || path == "")
            {
                emb.WithAuthor(command.User.Username, command.User.GetAvatarUrl());
                emb.WithTitle("Server gestartet");
                emb.AddField("Fehler", "Unauthorized");
                emb.AddField("Status", "Server nicht gestartet");
                embeds[0] = emb.Build();
                await command.RespondAsync("", embeds);
                return;
            }
            if ( gameservercmd == "start" || gameservercmd == "stop" || gameservercmd == "update")
            {
                await command.RespondAsync("Command erhalten, nach Beendigunge wird eine Nachricht geposted", ephemeral: true);
            }
            Dictionary<string, string> result = Common.HttpRequest.SendGetRequest(path + "/" + gameservercmd);
            emb.WithAuthor(command.User.Username, command.User.GetAvatarUrl());
            emb.WithTitle("Server gestartet");
            foreach (string k in result.Keys)
            {
                if (result[k].Length > 900)
                {
                    Console.WriteLine(k + ": " + result[k]);

                    //emb.AddField(k, "```" +  Common.functions.RemoveColorPrefixes(result[k].Substring(0, 900)) + "```", inline: false);
                    //string r = Common.functions.RemoveColorPrefixes(result[k]);
                    string r = result[k];
                    string[] lines = Common.functions.SplitStringOnNewlines(r, 900);
                    foreach (string line in lines)
                    {
                        //Console.WriteLine(k + ": " + result[k]);

                        emb.AddField(k, "```" + line + "```", inline: false);
                    }
                }
                else
                {
                    string r = Common.functions.RemoveAnsiEscapeCodes(result[k]);
                    r = Common.functions.RemoveColorPrefixes(r);
                    Console.WriteLine(k + ": " + result[k]);
                    emb.AddField(k, "```" + r + "```", inline: false);
                }
            }
            embeds[0] = emb.Build();
            if (gameservercmd == "start" || gameservercmd == "stop" || gameservercmd == "update")
            {
                await command.FollowupAsync("", embeds);
                return;
            }
            await command.RespondAsync("", embeds);
        }
    }
}
