using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Common.Basics;

namespace Common.Exchange
{
    public struct Message
    {
        public Message(Source src, User usr, ulong ChannelId, string content)
        {
            source = src;
            User = usr;
            channelID = (double)ChannelId;
            text = content;
        }
        public Message(Source src, User usr, long ChannelId, string content)
        {
            source = src;
            User = usr;
            channelID = (double)ChannelId;
            text = content;
        }
        public string text { get; init; }
        public User User { get; init; }
        public double channelID { get; init; }
        public Source source { get; init; }
    }

    public struct User
    {
        public User(string name, long id)
        {
            this.name = name;
            this.ID = (double)id;
        }
        public User(string name, ulong id)
        {
            this.name = name;
            this.ID = (double)id;
        }
        public string name { get; init; }
        public double ID { get; init; }
    }

    public struct request
    {
        public request(double channelID, string text)
        {
            this.channelID = channelID;
            this.text = text;
        }
        public double channelID { get; init; }
        public string text { get; init; }
    }

    public abstract class Source
    {
        public virtual int code { get; }
    }

    public class Discord : Source
    {
        public override int code
        {
            get
            {
                return 1;
            }
        }
    }

    public class Telegram : Source
    {
        public override int code
        {
            get
            {
                return 2;
            }
        }
    }

    internal static class Hub
    {
        public static List<int> ids = new List<int>();
        public static Queue<Message> incomming = new Queue<Message>();
        public static Dictionary<int, Queue<request>> ques = new Dictionary<int, Queue<request>>();
        public static Dictionary<int, Dictionary<double, Dictionary<int, double>>> ChannelMap = new Dictionary<int, Dictionary<double, Dictionary<int, double>>>();
        public static Dictionary<string, Channel> MapRequest = new Dictionary<string, Channel>();
    }

    struct Channel
    {
        public Channel(int identifier, Source source)
        {
            src = source;
            id = identifier;
        }
        public Source src { get; init; }
        public double id { get; init; }
    }

    public class Distribution
    {
        public static Distribution Instance = new Distribution();
        private Thread t = new Thread(new ThreadStart(Worker));
        private Distribution()
        {
            Hub.ids.Add(0);
            Hub.ids.Add(new Discord().code);
            Hub.ids.Add(new Telegram().code);
            foreach (int i in Hub.ids)
            {
                if (i > 0)
                {
                    Hub.ques.Add(i, new Queue<request>());
                    Hub.ChannelMap.Add(i, new Dictionary<double, Dictionary<int, double>>());
                }
            }
            t.Start();
        }
        public bool CheckForMessage(Source src, out request req)
        {
            request r;
            if (!Hub.ques.ContainsKey(src.code))
            {
                req = new request(-1, "");
                return false;
            }
            if (Hub.ques[src.code].TryDequeue(out r))
            {
                req = r;
                return true;
            }
            req = new request(-1, "");
            return false;
        }
        public bool enque(Message message)
        {
            if (!Hub.ids.Contains(message.source.code) && message.source.code > 0)
            {
                return false;
            }
            Hub.incomming.Enqueue(message);
            return true;
        }

        private static void Worker()
        {
            while (true)
            {
                Thread.Sleep(100);
                Message msg;
                if (Hub.incomming.TryDequeue(out msg))
                {
                    string text = msg.text;
                    if (text.StartsWith("!map"))
                    {
                        string[] splits = text.Split(" ");
                        if (splits.Length > 1)
                        {

                        }
                        else
                        {
                            string token = Generators.RandomString(20);
                            Hub.ques[msg.source.code].Enqueue(new request(msg.channelID, token));
                        }
                    }
                    Console.WriteLine(msg.text);
                }
            }
        }

        private void functionAddMap(string token, Source src, double channel)
        {
            if (!Hub.MapRequest.ContainsKey(token))
            {
                return;
            }
            if (!Hub.ChannelMap[src.code].ContainsKey(channel))
            {
                Hub.ChannelMap[src.code].Add(channel, new Dictionary<int, double>());
            }
            Hub.ChannelMap[src.code][channel].Add(Hub.MapRequest[token].src.code, Hub.MapRequest[token].id);
            if (!Hub.ChannelMap[Hub.MapRequest[token].src.code].ContainsKey(Hub.MapRequest[token].id))
            {
                Hub.ChannelMap[Hub.MapRequest[token].src.code].Add(Hub.MapRequest[token].id, new Dictionary<int, double>());
            }
            Hub.ChannelMap[Hub.MapRequest[token].src.code][Hub.MapRequest[token].id].Add(src.code, channel);
        }

        public static Dictionary<string, GameServer> GameServers { get; set; } = new Dictionary<string, GameServer>();
    }

    public class GameServer
    {
        public GameServer(string nameC, ulong[] allowedUsersC, ulong[] allowedRolesC, string addressC, ulong[] GuildIDC, string commandnameC)
        {
            name = nameC;
            allowedUsers = allowedUsersC;
            address = addressC;
            allowedRoles = allowedRolesC;
            GuildID = GuildIDC;
            commandname = commandnameC;
        }
        public string name { get; }
        public ulong[] allowedUsers { get; }
        public string address { get; }
        public ulong[] allowedRoles { get; }
        public ulong[] GuildID { get; }
        public string commandname { get; }
    }
}