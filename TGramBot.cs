using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace SmartScheduler
{
    class TGramBot
    {
        readonly string token = Credentials_Omit.GetTgramToken();

        public TGramBot() { }

        public async Task<Dictionary<string, int>> ScanGroups()
        {
            var bot = new TelegramBotClient(token);
            var updateInfo = await bot.GetUpdatesAsync();
            var results = new Dictionary<string, int>();
            foreach(var update in updateInfo)
            {
                int id = (int)update.Message.Chat.Id;
                string name = update.Message.Chat.Title;
                if (name != null && id != 0)
                {
                    results[name] = id;
                }
            }
            return results;
        }

        public async void SendPhoto(FileStream fs, string message, int sendChatID)
        {
            var bot = new TelegramBotClient(token);
            await bot.SendPhotoAsync(sendChatID, new InputOnlineFile(fs, "Schedule"), message);
            fs.Close();
        }


        //The following is for future updates to this project.
        //The main idea being that the bot could auto send worker time notifications before scheduled time
        /*
        public async Task<string> GetChatNames(int sendChatID)
        {
            var bot = new TelegramBotClient(token);
            var chat = await bot.GetChatAsync(sendChatID);
            return chat.Title;
        }

        public async void SendMessage(string text, int sendChatID)
        {
            var bot = new TelegramBotClient(token);
            var s = await bot.SendTextMessageAsync(sendChatID, text);
        }
        */

    }
}
