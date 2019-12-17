using System;
using ArgumentClinic;
using Discord;
using Discord.WebSocket;

namespace DiscordChannelArchiver
{
    class Program
    {
        static void Main(string[] args)
        {
            if(new Switch("-h", "--help").ParseArgs(args))
            {
                string helpMsg = $"Required Arguments:\n" +
                    $"  -k --token-type         The type of token you are passing in. Either 'bot' or 'user'\n" +
                    $"  -t --token              The token you want to use\n" +
                    $"Optional Arguments:\n" +
                    $"  -c --clear-messages     Enable to delete all messages by the current user after archiving. Only available to 'user' tokens\n" +
                    $"  -f --download-files     Enable to download all files";
                Console.WriteLine(helpMsg);
                return;
            }

            bool downloadFiles = new Switch("-f", "--download-files").ParseArgs(args);
            bool clearMessages = new Switch("-c", "--clear-messages").ParseArgs(args);
            string TokenType = new Argument<string>("-k", "--token-type", required: true, defaultVal: "user").ParseArgs(args).ToLower();
            string Token = new Argument<string>("-t", "--token", required: true, defaultVal: "").ParseArgs(args);

            if(!TokenType.Equals("user") && clearMessages)
            {
                Console.WriteLine("Cannot clear messages without a user token. Exiting.");
                return;
            }

            DiscordSocketClient client = new DiscordSocketClient();
            if (TokenType.Equals("user"))
            {
                client.LoginAsync(Discord.TokenType.User, Token);
            }
            else if (TokenType.Equals("bot"))
            {
                client.LoginAsync(Discord.TokenType.Bot, Token);
            }
            else
                throw new Exception($"Unsupported token type '{TokenType}'");
            client.StartAsync().Wait();
            while (client.ConnectionState != ConnectionState.Connected)
            {
            }
            System.Threading.Thread.Sleep(2000);
            Console.Clear();
            Console.WriteLine($"Logged in as {client.CurrentUser}!");

            while(true)
            {
                Console.WriteLine("Please enter the type of channel you'd like to archive: (Can be 'channel', 'server', or 'direct')");
                string ChannelType = Console.ReadLine().ToLower();
                ulong ChannelId = 0;

                switch (ChannelType)
                {
                    case "channel":
                       Console.WriteLine("Please enter the Channel ID:");
                       if(!ulong.TryParse(Console.ReadLine(), out ChannelId))
                       {
                            Console.WriteLine("Invalid Channel Id");
                       }
                        break;
                    case "server":
                        Console.WriteLine("Please enter the Server ID:");
                        if (!ulong.TryParse(Console.ReadLine(), out ChannelId))
                        {
                            Console.WriteLine("Invalid Server Id");
                        }
                        break;
                    case "direct":
                        Console.WriteLine("Please enter the User ID:");
                        if (!ulong.TryParse(Console.ReadLine(), out ChannelId))
                        {
                            Console.WriteLine("Invalid User Id");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid Channel Type");
                        continue;
                }
            }
        }
    }
}
