using UnityEngine;
using Zombies.Runtime.Enemies.Common;
using Zombies.Runtime.Health;
using Zombies.Runtime.Player;
using CharacterController = Zombies.Runtime.Player.CharacterController;

namespace Zombies.Runtime.Testing
{
    public class DamageTester : MonoBehaviour
    {
        public DamageArgs damage = new DamageArgs(10);
        public GameObject damager;
        public float delay = 1f;

        public bool move;
        public float movementRange = 2f;

        private float clock;
        private int movementSign;
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

            movementSign = 1;
        }

        private void Update()
        {
            clock += Time.deltaTime;
            if (clock > delay)
            {
                clock -= delay;
                var report = health.TakeDamage(damage.UpdateWithContext(damager, player.transform.position, Vector3.zero, null));
                if (report.wasLethal) health.gameObject.SetActive(true);
            }

            if (player.transform.position.x > movementRange) movementSign = -1;
            if (player.transform.position.x < -movementRange) movementSign = 1;

            if (move)
            {
                player.moveDirection = Vector3.right * movementSign;
            }
            else
            {
                player.moveDirection = Vector3.zero;
                player.transform.position = Vector3.zero;
            }
        }
    }
}