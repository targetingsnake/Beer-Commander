using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.Json;
using Discord;
using System.Data;
using Common.Exchange;
using Discord.Rest;

namespace BotMaster
{
    internal class configuration
    {
        private static config readConfig()
        {
            string filename = "";
            if (File.Exists("overwrite.json"))
            {
                filename = "overwrite.json";
                Console.WriteLine("Overwrite found");
            }
            else if (File.Exists("config.json"))
            {
                filename = "config.json";
                Console.WriteLine("standard config found");
            }
            if (filename == "")
            {
                throw new FileNotFoundException("Configuration File not found");
            }
            string jsonString = File.ReadAllText(filename);
            InternalConfig cfg = JsonSerializer.Deserialize<InternalConfig>(jsonString)!;

            if (cfg.TelegramToken is null || cfg.DiscordToken is null || cfg.SQlPassword is null
                || cfg.SQlServer is null || cfg.SQlUser is null || cfg.MasterDiscord is null || cfg.MasterTelegram is null 
                || cfg.Hosts is null)
            {
                throw new DataException();
            }

            Console.WriteLine($"DB-Server: {cfg.SQlServer}");
            Console.WriteLine($"DB-User: {cfg.SQlUser}");
            Console.WriteLine($"DB-PW: {cfg.SQlPassword}");
            Console.WriteLine($"DC-Token: {cfg.DiscordToken}");
            Console.WriteLine($"TG-Token: {cfg.TelegramToken}");
            string DcMaster = "";
            foreach (ulong t in cfg.MasterDiscord)
            {
                if (DcMaster != "")
                {
                    DcMaster += ", ";
                }
                DcMaster += t.ToString();
            }
            Console.WriteLine($"DC-Master: {DcMaster}");
            string TgMaster = "";
            foreach (long t in cfg.MasterTelegram)
            {
                if (TgMaster != "")
                {
                    TgMaster += ", ";
                }
                TgMaster += t.ToString();
            }
            Console.WriteLine($"TG-master: {TgMaster}");
            Console.WriteLine("Hosts:");
            foreach (string h in cfg.Hosts.Keys)
            {
                string allowedUsers = "";
                foreach (ulong user in cfg.Hosts[h].allowedUsers)
                {
                    allowedUsers += "\r\n\t\t\t- " + user.ToString();
                }
                string allowedRoles = "";
                foreach (ulong role in cfg.Hosts[h].allowedRoles)
                {
                    allowedRoles += "\r\n\t\t\t- " + role.ToString();
                }
                string servers = "";
                foreach (ulong s in cfg.Hosts[h].GuildID)
                {
                    servers += "\r\n\t\t\t- " + s.ToString();
                }
                Console.WriteLine($"\t{h}: \r\n\t\tdisplayname: {cfg.Hosts[h].name}\r\n\t\tallowedUsers: {allowedUsers}\r\n\t\tallowedRoles: {allowedRoles}\r\n\t\thostaddress: " +
                    $"{cfg.Hosts[h].address}\r\n\t\tallowedServers: {servers}");
            }

            config Fcfg = new config(cfg.SQlPassword, cfg.SQlUser, cfg.SQlServer, cfg.DiscordToken, cfg.TelegramToken, cfg.MasterTelegram, cfg.MasterDiscord, cfg.Hosts);

            return Fcfg;
        }

        private static config _cfg = readConfig();

        public static config data
        {
            get
            {
                return _cfg;
            }
        }
    }

    public class InternalConfig
    {
        public string? SQlPassword { get; set; }
        public string? SQlUser { get; set; }
        public string? SQlServer { get; set; }
        public string? DiscordToken { get; set; }
        public string? TelegramToken { get; set; }
        public IList<long>? MasterTelegram { get; set; }
        public IList<ulong>? MasterDiscord { get; set; }
        public Dictionary<string, InternalServer>? Hosts { get; set; }
    }

    public class InternalServer
    {
        public string? name { get; set; }
        public ulong[]? allowedUsers { get; set; }
        public ulong[]? allowedRoles { get; set; }  
        public ulong[]? GuildID { get; set; }
        public string? address { get; set; }
    }

    public class config
    {
        public config(string sQlPassword, string sQlUser, string sQlServer, string discordToken, string telegramToken, IList<long> TgMaster, IList<ulong> DcMaster, Dictionary<string, InternalServer> gameservers)
        {
            SQlPassword = sQlPassword;
            SQlUser = sQlUser;
            SQlServer = sQlServer;
            DiscordToken = discordToken;
            TelegramToken = telegramToken;
            long[] tgm = new long[TgMaster.Count];
            ulong[] dcm = new ulong[DcMaster.Count];
            foreach(string k in gameservers.Keys)
            {
                if (gameservers[k].name is null || gameservers[k].allowedUsers is null || gameservers[k].address is null || gameservers[k].allowedRoles is null ||
                    gameservers[k].GuildID is null  || gameservers is null || gameservers[k] is null)
                {
                    throw new ArgumentException("Null argument");
                }
                GameServer g = new GameServer(gameservers[k].name, gameservers[k].allowedUsers, gameservers[k].allowedRoles, gameservers[k].address, gameservers[k].GuildID, k);
                Distribution.GameServers.Add(k, g);
            }
            for (int i = 0; i < TgMaster.Count; i++)
            {
                tgm[i] = TgMaster[i];
            }
            MasterTelegram = tgm;
            for(int j = 0; j < DcMaster.Count; j++)
            {
                dcm[j] = DcMaster[j];
            }
            MasterDiscord = dcm;
        }

        public string SQlPassword { get; init; }
        public string SQlUser { get; init; }
        public string SQlServer { get; init; }
        public string DiscordToken { get; init; }
        public string TelegramToken { get; init; }
        public long[] MasterTelegram { get; init; }
        public ulong[] MasterDiscord { get; init; }
    }
}
