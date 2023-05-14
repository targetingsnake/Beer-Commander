using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;
using System.Security.AccessControl;

namespace TelegramBot
{
    public class TelegramMaster
    {
        public TelegramMaster(string token, long[] masters)
        {
            botClient = new TelegramBotClient(token);
            _Masters = masters;
        }
        private TelegramBotClient botClient;
        public async void main()
        {

            CancellationTokenSource cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.

            UpdateType[] types = { UpdateType.Message, UpdateType.ChannelPost };
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();


            Console.WriteLine($"Start listening for @{me.Username}");

            bool run = true;
            Common.Exchange.Telegram id = new Common.Exchange.Telegram();

            while (run)
            {
                message t;
                if (ctrl.TryDequeue(out t))
                {
                    if (t.command == "!shutdown")
                    {
                        //run = false;
                    }
                }
                Common.Exchange.request r;
                if (Common.Exchange.Distribution.Instance.CheckForMessage(id, out r))
                {
                    SendMessage(r);
                }
                Thread.Sleep(100);
            }

            // Send cancellation request to stop bot
            cts.Cancel();
            Console.WriteLine("Telegram Bot shutting down");
        }

        private long[] _Masters;

        private Queue<message> ctrl = new Queue<message>();

        struct message
        {
            public message(string cmd, string msg)
            {
                command = cmd;
                Message = msg;
            }
            public string command;
            public string Message;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            if (message.From is not { } sender)
                return;

            long chatId = message.Chat.Id;
            bool masterMsg = false;
            if (_Masters.Contains(sender.Id))
            {
                Console.WriteLine("A Master has written");
                masterMsg = true;
            }

            string textPart = "You said:\n" + messageText + " @" + sender.Username;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {sender.Id}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: textPart,
                cancellationToken: cancellationToken);

            if (messageText.StartsWith("!shutdown") && masterMsg)
            {
                ctrl.Enqueue(new message("!shutdown", messageText));
            }
            EnqueMessage(message);
        }

        private async void SendMessage(Common.Exchange.request req)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            long chatID = (long)req.channelID;
            Console.WriteLine($"Channel id: {chatID} and text: {req.text}");
            Message sentMessage = await botClient.SendTextMessageAsync(
               chatId: chatID,
               text: req.text,
               cancellationToken: cts.Token);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private void EnqueMessage(Message msg)
        {
            if (msg.From is not { } sender)
            {
                throw new ArgumentOutOfRangeException("sender undefinded");
            }
            if (msg.From.Username is not { } username)
            {
                throw new ArgumentOutOfRangeException("sender username undefinded");
            }
            if (msg.Text is not { } messageText)
            {
                throw new ArgumentOutOfRangeException("message has no text");
            }
            Common.Exchange.User usr = new Common.Exchange.User(username, msg.From.Id);
            Common.Exchange.Message message = new Common.Exchange.Message(new Common.Exchange.Telegram(), usr, msg.Chat.Id, msg.Text);
            Common.Exchange.Distribution.Instance.enque(message);
        }
    }
}