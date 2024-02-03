using FishNet.Object;
using UnityEngine;

namespace Framework.Runtime.Npc.Movement
{
    public class NpcMovement : NetworkBehaviour
    {
        public Vector3? targetPosition;
        public Vector3? targetDirection;
    }
}