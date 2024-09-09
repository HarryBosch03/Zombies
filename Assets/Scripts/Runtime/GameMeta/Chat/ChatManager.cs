using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zombies.Runtime.Player;

namespace Zombies.Runtime.GameMeta.Chat
{
    public class ChatManager : MonoBehaviour
    {
        public TMP_Text messageLogElement;
        public CanvasGroup chatGroup;
        public TMP_InputField inputField;
        public float holdTime;
        public float fadeTime;

        private bool isChatOpen;
        private bool justSubmitted;
        private float fadeTimer;
        private LinkedList<ChatMessage> messageLog = new();

        private static ChatManager privateInstance;
        public static ChatManager instance
        {
            get
            {
                if (privateInstance == null) privateInstance = FindAnyObjectByType<ChatManager>();
                return privateInstance;
            }
        }

        private void OnEnable()
        {
            inputField.onSubmit.AddListener(OnSubmit);
            UpdateUI(false);
        }

        private void OnDisable() { inputField.onSubmit.RemoveListener(OnSubmit); }

        public void OnSubmit(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                SendMessage(PlayerController.localPlayer, new ChatMessage("UNDEF").Body(message));
            }
            SetIsChatOpen(false);
            justSubmitted = true;
            inputField.text = "";
        }

        public void SendMessage(PlayerController sender, ChatMessage message)
        {
            if (message.message.Length > 0 && message.message[0] == '/')
            {
                if (CommandManager.instance.ParseChatCommand(sender, message.message)) return;
            }

            message.Clean();

            messageLog.AddFirst(message);
            UpdateUI(true);
        }

        public void SendLocalSystemMessage(ChatMessage message)
        {
            messageLog.AddFirst(message);
            UpdateUI(true);
        }

        private void UpdateUI(bool showChatUI)
        {
            if (messageLogElement == null) return;

            var str = "";
            var element = messageLog.First;
            while (element != null)
            {
                str = $"{element.Value}\n{str}";
                element = element.Next;
            }

            messageLogElement.text = str;

            if (showChatUI) fadeTimer = holdTime + fadeTime;
        }

        private void Update()
        {
            var kb = Keyboard.current;

            if (!isChatOpen)
            {
                if (kb.enterKey.wasPressedThisFrame && !justSubmitted)
                {
                    SetIsChatOpen(true);
                }

                chatGroup.alpha = Mathf.Clamp01(fadeTimer / fadeTime) * 0.6f;
                fadeTimer -= Time.deltaTime;
            }

            justSubmitted = false;
        }

        private void SetIsChatOpen(bool isChatOpen)
        {
            this.isChatOpen = isChatOpen;
            PlayerController.localPlayer.isControlling = !isChatOpen;

            chatGroup.alpha = 1f;
            chatGroup.blocksRaycasts = isChatOpen;
            chatGroup.interactable = isChatOpen;
            fadeTimer = holdTime + fadeTime;

            if (isChatOpen)
            {
                inputField.Select();
                inputField.text = string.Empty;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}