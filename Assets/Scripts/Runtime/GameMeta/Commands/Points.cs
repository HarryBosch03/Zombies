using UnityEngine;
using Zombies.Runtime.GameMeta.Chat;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameMeta.Commands
{
    public class Points : IChatCommand
    {
        public string name => "points";
        
        public string Perform(PlayerController sender, string[] args)
        {
            if (args.Length != 2) throw new System.Exception();

            var value = int.Parse(args[1]);
            switch (args[0])
            {
                case "set":
                    sender.points.currentPoints.Value = value;
                    return $"Set {sender.name} points to {value}";
                case "add":
                    sender.points.currentPoints.Value += value;
                    return $"{(value >= 0 ? "Added" : "Removed")} {Mathf.Abs(value)} points to {sender.name}";
                case "remove":
                    sender.points.currentPoints.Value -= value;
                    return $"{(-value >= 0 ? "Added" : "Removed")} {Mathf.Abs(value)} points to {sender.name}";
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}