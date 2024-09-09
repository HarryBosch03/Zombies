using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using Zombies.Runtime.Player;
using CharacterController = Zombies.Runtime.Player.CharacterController;

namespace Zombies.Runtime.Testing
{
    public class RecoilTesting : MonoBehaviour
    {
        public float inputDuration;
        public float inputDelay;
        public bool aim;
        public bool noRecoil;

        private float clock;
        private CharacterController player;
        private HealthController health;

        private void Start()
        {
            foreach (var enemy in FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None))
            {
                enemy.gameObject.SetActive(false);
            }

            var player = FindAnyObjectByType<PlayerController>();
            player.enabled = false;

            this.player = player.GetComponent<CharacterController>();
            health = player.GetComponent<HealthController>();
        }

        private void Update()
        {
            player.aiming = aim;

            if (clock < inputDuration)
            {
                player.shoot = true;
            }
            else
            {
                player.shoot = false;
                if (clock > inputDuration + inputDelay + inputDelay)
                {
                    clock = 0f;
                }
                if (clock > inputDuration + inputDelay)
                {
                    player.rotation = Vector2.zero;
                }
            }

            if (noRecoil)
            {
                player.rotation = Vector2.zero;
            }

            player.activeWeapon.currentMagazine = player.activeWeapon.magazineSize;

            clock += Time.deltaTime;
        }
    }
}