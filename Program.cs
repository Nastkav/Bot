using SkBootsBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static SkBootsBot.Config;

namespace Telegram.Bot.Examples.Echo
{
    public static class Program
    {   

        static Dictionary<string, List<string>> foundBooks = new Dictionary<string, List<string>>();
        private static TelegramBotClient Bot;
        static bool search = false;
        static List<Book> books_search = new List<Book>();
        public static async Task Main()
        {   
            Sqliter.DB_init();

            Bot = new TelegramBotClient(BOT_TOKEN);

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "I don't understand...");
                return;
            }

            switch (message.Text.Split(' ').First())
            {
    
                case "/favourites":
                case "Favourites":
                    await Favourites(message);
                    break;

                case "/bestsellers":
                case "Bestsellers":
                    await Bestsellers(message);
                    break;

                case "/random":
                case "Random":
                    await Random(message);
                    break;

                case "/menu":
                    await ShowMenu(message);
                    break;

                case "/search":
                case "Search":
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Enter search query");
                    search = true;
                    break;

                default:
                    if(!search)
                    await Usage(message);
                    break;
            }

            if(search && message.Text != "/search" && message.Text != "Search")
            {
                await FindBooks(message);
            }


            static async Task Random(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                Thread.Sleep(1000);

                foundBooks.Clear();

                BookApi bookApi = new BookApi(API_TOKEN);
                var booksss = bookApi.RandomBook(0, 10);
                Dictionary<string, string> books = new Dictionary<string, string>();


                foreach (var item in booksss)
                {
                    string link = item.Link;
                    string id = item.Id;
                    string title = item.Title;

                    List<string> TitledLinks = new List<string>();
                    TitledLinks.Add(title);
                    TitledLinks.Add(link);
                    foundBooks.Add(item.Id, TitledLinks);

                    books.Add(title, id);
                    break;

                }

                var inlineKeyboard = new InlineKeyboardMarkup(GetInlineKeyboard_list(books));
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Random book:",
                    replyMarkup: inlineKeyboard
                );
            }

            static async Task Bestsellers(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                Thread.Sleep(1000);

                foundBooks.Clear();

                BookApi bookApi = new BookApi(API_TOKEN);
                var booksss = bookApi.Best(0, 10);
                Dictionary<string, string> books = new Dictionary<string, string>();

                foreach (var item in booksss)
                {
                    string link = item.Link;
                    string id = item.Id;
                    string title = item.Title;

                    List<string> TitledLinks = new List<string>();
                    TitledLinks.Add(title);
                    TitledLinks.Add(link);
                    foundBooks.Add(item.Id, TitledLinks);

                    books.Add(title, id);
                }

                var inlineKeyboard = new InlineKeyboardMarkup(GetInlineKeyboard_list(books));
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Bestsellers:",
                    replyMarkup: inlineKeyboard
                );
            }

            static async Task Favourites(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                Thread.Sleep(1000);

                foundBooks.Clear();

                Dictionary<string, string> books = new Dictionary<string, string>();
                Dictionary<string, List<string>> favouriteBooks = Sqliter.getFavourites(message.From.Id);

                foreach (var book in favouriteBooks)
                {
                    string link = book.Value[1];
                    string id = book.Key;
                    string title = book.Value[0];

                    List<string> TitledLinks = new List<string>();
                    TitledLinks.Add(title);
                    TitledLinks.Add(link);
                    foundBooks.Add(id, TitledLinks);

                    books.Add(title, id);
                }

                var inlineKeyboard = new InlineKeyboardMarkup(GetInlineKeyboard_list(books));
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Favourite books:",
                    replyMarkup: inlineKeyboard
                );

            }
            
            static async Task FindBooks(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                Thread.Sleep(1000);

                if (message == null || message.Type != MessageType.Text)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "I don't understand...");
                    return;
                }

                foundBooks.Clear();

                BookApi bookApi = new BookApi(API_TOKEN);
                try
                {
                    books_search = bookApi.Search(message.Text, 0, 10);
                }
                catch (System.ArgumentNullException)
                {
                    Console.WriteLine("nothing was found");
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Wow, such an empty!\nPlease try another search query");
                    return;
                }

                search = false;

                Dictionary<string, string> books = new Dictionary<string, string>();


                foreach (var item in books_search)
                {
                    string link = item.Link;
                    string id = item.Id;
                    string title = item.Title;

                    List<string> TitledLinks = new List<string>();
                    TitledLinks.Add(title);
                    TitledLinks.Add(link);
                    if (foundBooks.ContainsKey(item.Id)) return;
                    foundBooks.Add(item.Id, TitledLinks);
                    
                    books.Add(title, id);
                }


                var inlineKeyboard = new InlineKeyboardMarkup(GetInlineKeyboard_list(books));
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Found books:",
                    replyMarkup: inlineKeyboard
                );
            }


            static InlineKeyboardButton[][] GetInlineKeyboard_list(Dictionary<string, string> stringArray)
            {
                var keyboardInline = new InlineKeyboardButton[stringArray.Count][];

                for (var i = 0; i < stringArray.Count; i++)
                {
                    var keyboardButtons = new InlineKeyboardButton[1];
                    keyboardButtons[0] = new InlineKeyboardButton
                    {
                        Text = stringArray.ElementAt(i).Key,
                        CallbackData = stringArray.ElementAt(i).Value,
                    };
                    keyboardInline[i] = keyboardButtons;
                }

                return keyboardInline;
            }


           static async Task ShowMenu(Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "Search", "Bestsellers" },
                        new KeyboardButton[] { "Random", "Favourites" },
                    },
                    resizeKeyboard: true
                );

                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose:",
                    replyMarkup: replyKeyboardMarkup

                );
            }


            static async Task Usage(Message message)
            {
                const string usage = "Commands:\n" +
                                        "/help   - show this list\n" +
                                        "/search   - search for books\n" +
                                        "/bestsellers   - show bestsellers\n" +
                                        "/random   - I'm lucky\n" +
                                        "/favourites   - show your favourites\n" +
                                        "/menu - show options menu\n";
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage
                );
            }
        }

        static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            var id = callbackQuery.Data.Split(';').Last();  
            var favourites = Sqliter.getFavourites(callbackQuery.From.Id);
            string title = "";
            string link = "";

            try
            {
                title = foundBooks[id][0];
                link = foundBooks[id][1];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                Console.WriteLine($"User trying to use an old inline keyboard resulted in error:\nThe given key {id} was not present in the dictionary");
                return;
            }

            switch (callbackQuery.Data.Split(';').First())
            {
                case "add":
                if (!Sqliter.checkIfInFavourites(callbackQuery.From.Id, id))
                {   
                    if (Sqliter.getFavourites(callbackQuery.From.Id).Count() == 10)
                    {
                        await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "You can have only up to 10 favourites");
                    }
                    Sqliter.addToFavourites(callbackQuery.From.Id, id, title, link);
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"'{title}' was added to favourites");
                }
                break;
                
                case "del":
                if (Sqliter.checkIfInFavourites(callbackQuery.From.Id, id))
                {   
                    Sqliter.removeFromFavourites(callbackQuery.From.Id, id);
                    await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"'{title}' was deleted from favourites");
                }
                break;

                default:
                var inlineKeyboard = GetInlineKeyboard_actions(callbackQuery, id, link, title);
                await Bot.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Choose action:",
                    replyMarkup: inlineKeyboard
                );
                break;
            }
            


            static InlineKeyboardMarkup GetInlineKeyboard_actions(CallbackQuery callbackQuery, string id, string link, string title)
            {
                string callbackData = "";
                string callbackText = "";
                if (Sqliter.checkIfInFavourites(callbackQuery.From.Id, id))
                {
                    callbackData = $"del;{id}";
                    callbackText = "Delete from favourites";
                }
                    
                else
                {
                    callbackData = $"add;{id}";
                    callbackText = "Add to favourites";
                }

                InlineKeyboardButton btnFav = InlineKeyboardButton.WithCallbackData(callbackText, callbackData);
                InlineKeyboardButton btnUrl = InlineKeyboardButton.WithUrl(title, link);

                List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
                buttons.Add(btnFav);
                buttons.Add(btnUrl);
                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(buttons);

                return keyboard;
            }
        }
    }
}