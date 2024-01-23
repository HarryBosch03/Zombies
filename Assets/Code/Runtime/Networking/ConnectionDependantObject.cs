using FishNet.Connection;
using FishNet.Object;

namespace Framework.Runtime.Networking
{
    public class ConnectionDependantObject : NetworkBehaviour
    {
        public bool enableIfClient;
        public bool enableIfServer;
        public bool enableIfOwner;
        public bool invert;

        private void UpdateState()
        {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var active = false;
            
            // ReSharper disable once ConvertIfToOrExpression
            if (IsServer && enableIfServer) active = true;
            else if (IsClient && enableIfClient) active = true;
            else if (IsOwner && enableIfOwner) active = true;
            
            gameObject.SetActive(active != invert);
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner) => UpdateState();
        public override void OnOwnershipServer(NetworkConnection prevOwner) => UpdateState();
    }
}