using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameMeta.Chat
{
    public class CommandManager : MonoBehaviour
    {
        private List<IChatCommand> commands = new();

        private static CommandManager privateInstance;
        public static CommandManager instance
        {
            get
            {
                if (privateInstance == null) privateInstance = FindAnyObjectByType<CommandManager>();
                return privateInstance;
            }
        }

        private void Awake()
        {
            commands.Clear();
            foreach (var e in GetType().Assembly.GetTypes().Where(e => typeof(IChatCommand).IsAssignableFrom(e) && e.IsClass && !e.IsAbstract))
            {
                var command = Activator.CreateInstance(e) as IChatCommand;
                commands.Add(command);
            }
            var log = $"Registered {commands.Count} commands\n";
            foreach (var e in commands) log += $"/{e.name}\n";
            Debug.Log(log);
        }

        public bool ParseChatCommand(PlayerController sender, string chatMessage)
        {
            var commandName = (string)null;
            var args = new List<string>();
            var buffer = (string)null;
            var inQuotes = false;
            for (var i = 1; i < chatMessage.Length; i++)
            {
                var c = chatMessage[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (inQuotes)
                {
                    buffer += c;
                }
                else if (c == ' ')
                {
                    if (commandName == null) commandName = buffer;
                    else args.Add(buffer);
                    buffer = "";
                }
                else
                {
                    buffer += c;
                }
            }

            if (buffer != null)
            {
                if (commandName == null) commandName = buffer;
                else args.Add(buffer);
            }

            if (commandName == null) return false;

            foreach (var command in commands)
            {
                if (command.name == commandName)
                {
                    try
                    {
                        var msg = command.Perform(sender, args.ToArray());
                        if (!string.IsNullOrEmpty(msg)) ChatManager.instance.SendLocalSystemMessage(ChatMessage.SystemMessage().Body(msg));
                    }
                    catch (Exception e)
                    {
                        ChatManager.instance.SendLocalSystemMessage(ChatMessage.SystemMessage().Body(e.Message));    
                    }
                    return true;
                }
            }

            ChatManager.instance.SendLocalSystemMessage(ChatMessage.SystemMessage().Body($"Command {commandName} does not exist"));
            return true;
        }
    }
}