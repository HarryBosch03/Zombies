using System.Text.RegularExpressions;
using UnityEngine;

namespace Zombies.Runtime.GameMeta.Chat
{
    public class ChatMessage
    {
        public string sender;
        public string message;
        
        private Color color = UnityEngine.Color.white;

        public static ChatMessage SystemMessage() => new ChatMessage("System");

        public ChatMessage(string sender)
        {
            this.sender = sender;
        }
        
        public ChatMessage Body(string message)
        {
            this.message += message;
            return this;
        }
        
        public ChatMessage Color(Color color)
        {
            this.color = color;
            return this;
        }
        
        public override string ToString() => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>[@{sender}] {message}</color>";

        public void Clean()
        {
            var bannedCharacters = new[]
            {
                '<', '>'
            };

            foreach (var c in bannedCharacters)
            {
                message = message.Replace(c.ToString(), "");
            }
        }
    }
}