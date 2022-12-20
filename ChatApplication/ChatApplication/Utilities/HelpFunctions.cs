using ChatApplication.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ChatApplication.Assets
{
    public static class HelpFunctions
    {
        /* 
         * Used to determine which chat should be loaded into the chat window
         */
        public static ObservableCollection<Message> DetermineChat(string chatTime, User _user)
        {
            ObservableCollection<Message> Messages = new ObservableCollection<Message>();

            if (chatTime != null)
            {
                foreach (Chat c in GetChatList())
                {
                    if (c.time.ToString() == chatTime)
                    {
                        foreach (string s in c.messages)
                        {
                            Message message = new Message() { Msg = s, Username = _user.Name };
                            Messages.Add(message);
                        }
                        break;
                    }
                }
            }
            return Messages;
        }

        /*
         * Used to filter the history list with LINQ
         */
        public static IEnumerable<Chat> SearchForUser(string searchWord)
        {

            return GetChatList().Where(Chat => Chat.partner != null && Chat.partner.Contains(searchWord)).OrderBy(chat => chat.time).Reverse().ToList();
        }

        /*
         * Used to get the history file and deserialize it from JSON
         */
        public static List<Chat> GetChatList()
        {
            string filePath = GetFile();
            var deserializedChatList = new List<Chat>();

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                deserializedChatList = JsonConvert.DeserializeObject<List<Chat>>(json);
            }

            deserializedChatList = deserializedChatList.Where(chat => chat != null).OrderBy(chat => chat.time).ToList();
            deserializedChatList.Reverse();

            return deserializedChatList;
        }

        /*
         * Used to create a file if it's not created before.
         * Will also instantiate a JSON array which will be filled with content when chats are being produced.
         */
        public static void CreateFile()
        {
            if (!Directory.Exists(GetFolder()))
            {
                Directory.CreateDirectory(GetFolder());
                using (StreamWriter w = File.AppendText(GetFile()))
                {
                    w.WriteLine("[]");
                }
            }
        }

        /*
         * Used to get the save file content
         */
        public static string GetFile()
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"History\chatData.json");
        }

        /*
         * Used to get the save file folder
         */
        public static string GetFolder()
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"History\");
        }


    }
}
