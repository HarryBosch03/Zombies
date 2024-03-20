using UnityEngine;

namespace Framework.Runtime.Animation
{
    public class IKNode : MonoBehaviour
    {
        private IKNode next;
        private IKNode last;
        private float length;

        protected virtual void Awake()
        {
            var child = transform.GetChild(0);
            if (child) next = child.GetComponent<IKNode>();
            if (next)
            {
                length = (next.transform.position - transform.position).magnitude;
            }

            var parent = transform.parent;
            if (parent) last = parent.GetComponent<IKNode>();
        }

        public void SolveForward()
        {
            
        }

        public void SolveBack()
        {
            
        }
    }
}