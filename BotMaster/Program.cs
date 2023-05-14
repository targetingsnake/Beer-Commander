using DiscordBot;
using TelegramBot;
using System.Threading;

namespace BotMaster
{
    public class BotFather
    {

        public static void Main(string[] args)
        {
            config cfg = configuration.data;
            TelegramMaster tmaster = new TelegramMaster(cfg.TelegramToken, cfg.MasterTelegram);
            tmaster.main();
            var t = Task.Run(() => DiscordMaster.Main(cfg.DiscordToken, cfg.MasterDiscord));
            t.Wait();
        }
    }
}