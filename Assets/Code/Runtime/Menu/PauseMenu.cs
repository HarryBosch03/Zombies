using System.Collections.Generic;
using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zombies.Runtime.Player;
using Zombies.Runtime.Utility;

namespace Zombies.Runtime.Menu
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        public InputAction pauseAction;

        private Transform menuParent;
        private Button buttonPrefab;
        private List<Button> buttons = new();
        
        private void Awake()
        {
            buttonPrefab = transform.Find<Button>("Pad/MenuButton");
            buttonPrefab.gameObject.SetActive(false);
            
            pauseAction.performed += OnPauseActionPerformed;
            pauseAction.Enable();
            
            Open(false);
        }

        private void StartServer()
        {
            InstanceFinder.ServerManager.StartConnection();
            StartClient();
        }

        private void StartClient()
        {
            InstanceFinder.ClientManager.StartConnection();
            Open(false);
        }

        private void OnDestroy()
        {
            pauseAction.performed -= OnPauseActionPerformed;
        }

        private void OnPauseActionPerformed(InputAction.CallbackContext obj) => Open(!gameObject.activeSelf);

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Open(bool state)
        {
            BuildMenu();
            
            gameObject.SetActive(state);
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;

            PlayerController.EnableInput(!state);
        }

        private void BuildMenu()
        {
            ClearMenu();

            AddButton("Resume", () => Open(false));
            AddButton("Reload Scene", ReloadScene);
            AddButton("Start Server", StartServer);
            AddButton("Start Client", StartClient);
            AddButton("Quit", Quit);
        }
        
        private void ClearMenu()
        {
            foreach (var button in buttons)
            {
                Destroy(button.gameObject);
            }
            buttons.Clear();
        }

        private void AddButton(string name, UnityAction callback)
        {
            var button = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            button.gameObject.SetActive(true);

            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = name.ToUpper();
            
            button.onClick.AddListener(callback);
            
            buttons.Add(button);
        }

        public void SetupButton(Button button, string label, UnityAction callback)
        {
            button.transform.SetAsLastSibling();
            button.name = label;
            button.GetComponentInChildren<TMP_Text>().text = label;
            button.onClick.AddListener(callback);
        }
    }
}