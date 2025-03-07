using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using GrokBot.Models;
using System.Text.Json;

namespace GrokBot.Services
{
    public class ChatStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CHATS_KEY = "grokbot_chats";
        private const int MAX_STORED_CHATS = 10; // 限制存储的聊天数量
        private const int MAX_MESSAGES_PER_CHAT = 20; // 限制每个聊天的消息数量

        public ChatStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<List<Chat>> GetAllChatsAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CHATS_KEY);
                if (string.IsNullOrEmpty(json))
                    return new List<Chat>();

                return JsonSerializer.Deserialize<List<Chat>>(json) ?? new List<Chat>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading chats: {ex.Message}");
                return new List<Chat>();
            }
        }

        public async Task SaveChatAsync(Chat chat)
        {
            try
            {
                var chats = await GetAllChatsAsync();
                
                // 检查是否已存在此聊天
                var existingChat = chats.FirstOrDefault(c => c.Id == chat.Id);
                if (existingChat != null)
                {
                    // 更新现有聊天
                    var index = chats.IndexOf(existingChat);
                    chats[index] = chat;
                }
                else
                {
                    // 添加新聊天
                    chats.Add(chat);
                }
                
                // 限制消息数量，仅保留最新的N条
                if (chat.Messages.Count > MAX_MESSAGES_PER_CHAT)
                {
                    chat.Messages = chat.Messages.GetRange(
                        chat.Messages.Count - MAX_MESSAGES_PER_CHAT, 
                        MAX_MESSAGES_PER_CHAT);
                }
                
                // 仅保留最新的N个聊天记录
                if (chats.Count > MAX_STORED_CHATS)
                {
                    chats = chats.OrderByDescending(c => 
                        c.Messages.Count > 0 ? 
                        c.Messages.Max(m => m.Timestamp) : 
                        DateTime.MinValue)
                        .Take(MAX_STORED_CHATS)
                        .ToList();
                }
                
                var json = JsonSerializer.Serialize(chats);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CHATS_KEY, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving chat: {ex.Message}");
                
                // 如果保存失败，尝试清除存储并重新保存单个聊天
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.clear");
                    var newList = new List<Chat> { chat };
                    var json = JsonSerializer.Serialize(newList);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CHATS_KEY, json);
                }
                catch
                {
                    // 如果仍然失败，只能放弃保存
                }
            }
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
                var json = JsonSerializer.Serialize(chats);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CHATS_KEY, json);
            }
        }
    }
}
