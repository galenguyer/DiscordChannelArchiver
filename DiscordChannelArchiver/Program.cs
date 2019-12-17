﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArgumentClinic;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordChannelArchiver
{
    class Program
    {
        static bool downloadFiles;
        static bool clearMessages;
        static string TokenType;
        static string Token;
        static DiscordSocketClient client = new DiscordSocketClient();

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

            downloadFiles = new Switch("-f", "--download-files").ParseArgs(args);
            clearMessages = new Switch("-c", "--clear-messages").ParseArgs(args);
            TokenType = new Argument<string>("-k", "--token-type", required: true, defaultVal: "user").ParseArgs(args).ToLower();
            Token = new Argument<string>("-t", "--token", required: true, defaultVal: "").ParseArgs(args);

            if(!TokenType.Equals("user") && clearMessages)
            {
                Console.WriteLine("Cannot clear messages without a user token. Exiting.");
                return;
            }

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

        public void GetDirectMessages(ulong UserId)
        {
            try
            {
                var channel = client.GetUser(UserId).GetOrCreateDMChannelAsync().Result;
                var messages = DownloadMessagesFromChannel(channel as ITextChannel);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine("Cancelling operation");
            }
        }

        public void GetGuildMessages(ulong GuildId)
        {
            try
            {
                var guild = client.GetGuild(GuildId);
                var channels = guild.TextChannels;
                foreach(var channel in channels)
                {
                    GetChannelMessages(channel.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine("Cancelling operation");
            }
        }

        public void GetChannelMessages(ulong ChannelId)
        {
            try
            {
                var channel = client.GetChannel(ChannelId) as SocketTextChannel;
                var messages = DownloadMessagesFromChannel(channel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine("Cancelling operation");
            }
        }

        public List<IMessage> DownloadMessagesFromChannel(ITextChannel channel)
        {
            List<IMessage> messages = channel.GetMessagesAsync().Flatten().Result.ToList();
            List<IMessage> tempMessages = new List<IMessage>();
            Console.Write($"Getting Messages... {messages.Count}");
            do
            {
                try
                {
                    tempMessages = channel.GetMessagesAsync(messages.Last(), Direction.Before).Flatten().Result.ToList();
                    messages.AddRange(tempMessages);
                    messages = messages.OrderByDescending(m => m.Id).ToList();
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"Getting Messages... {messages.Count}");
                }
                catch
                {

                }
            } while (tempMessages.Count == 100);

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"Getting Messages... {messages.Count} Done!");
            messages = messages.OrderBy(m => m.Id).Distinct().ToList();
            SaveMessagesToFiles(messages, channel);
            return messages;
        }

        public void SaveMessagesToFiles(List<IMessage> messages, ITextChannel channel)
        {
            Directory.CreateDirectory($"{channel.Id}-data");

            // Save all messages in a simple format
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var msg in messages)
            {
                stringBuilder.AppendLine($"{msg.Author.Username}: {msg.Content}");
                if (msg.Attachments.Any())
                    foreach (var attachment in msg.Attachments)
                        stringBuilder.AppendLine($"\t{msg.Id}-{attachment.Filename}");
            }
            File.WriteAllText($"{channel.Id}-data/{channel.Id}-messages.txt", stringBuilder.ToString());

            // Save everything as JSON
            var saveMessages = messages.Select(m => new
            {
                m.Id,
                Author = new
                {
                    m.Author.Id,
                    m.Author.Username,
                    m.Author.Discriminator
                },
                m.Content,
                Attatchments = m.Attachments,
                Timestamp = m.CreatedAt
            });
            dynamic guild = new { };
            try
            {
                guild = new
                {
                    Id = (channel as SocketGuildChannel).Guild.Id,
                    Name = (channel as SocketGuildChannel).Guild.Name,
                };
            }
            catch { }
            var saveChannel = new
            {
                Guild = guild,
                Id = channel.Id,
                Name = channel.Name,
                PinnedMessages = channel.GetPinnedMessagesAsync().Result.Select(m => new
                {
                    m.Id,
                    Author = new
                    {
                        m.Author.Id,
                        m.Author.Username,
                        m.Author.Discriminator
                    },
                    m.Content,
                    Attatchments = m.Attachments,
                    Timestamp = m.CreatedAt
                }),
                Messages = saveMessages,
            };
            File.WriteAllText($"{channel.Id}-data/{channel.Id}-data.txt", JsonConvert.SerializeObject(saveMessages, Formatting.Indented));
        }
    }
}
