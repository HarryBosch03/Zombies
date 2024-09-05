using UnityEngine;

namespace Zombies.Runtime.GameMeta
{
    public class JoinTeam : MonoBehaviour
    {
        public string team;

        private void OnEnable()
        {
            Teams.JoinTeam(team, gameObject);
        }

        private void OnDisable()
        {
            Teams.LeaveTeam(team, gameObject);
        }
    }
}