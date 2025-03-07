using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using GrokBot.Models;
using System.Text.Json;

namespace GrokBot.Services
{
    public class ChatStorageService
    {
        private readonly ILocalStorageService _localStorage;
        private const string CHATS_KEY = "grokbot_chats";

        public ChatStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<List<Chat>> GetAllChatsAsync()
        {
            try
            {
                if (await _localStorage.ContainKeyAsync(CHATS_KEY))
                {
                    return await _localStorage.GetItemAsync<List<Chat>>(CHATS_KEY) ?? new List<Chat>();
                }
                
                return new List<Chat>();
            }
            catch (Exception)
            {
                return new List<Chat>();
            }
        }

        public async Task SaveChatAsync(Chat chat)
        {
            var chats = await GetAllChatsAsync();
            var existingChat = chats.FirstOrDefault(c => c.Id == chat.Id);
            
            if (existingChat != null)
            {
                // Update existing chat
                var index = chats.IndexOf(existingChat);
                chats[index] = chat;
            }
            else
            {
                // Add new chat
                chats.Add(chat);
            }
            
            await _localStorage.SetItemAsync(CHATS_KEY, chats);
        }

        public async Task<Chat?> GetChatByIdAsync(string id)
        {
            var chats = await GetAllChatsAsync();
            return chats.FirstOrDefault(c => c.Id == id);
        }

        public async Task DeleteChatAsync(string id)
        {
            var chats = await GetAllChatsAsync();
            var chatToRemove = chats.FirstOrDefault(c => c.Id == id);
            
            if (chatToRemove != null)
            {
                chats.Remove(chatToRemove);
                await _localStorage.SetItemAsync(CHATS_KEY, chats);
            }
        }
    }
}
