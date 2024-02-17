using Framework.Runtime.Tools;
using UnityEngine;

namespace Framework.Runtime.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private PlayerWeaponManager weaponManager;
        private Arm leftArm, rightArm;

        private void Awake()
        {
            weaponManager = GetComponent<PlayerWeaponManager>();

            var view = transform.Find("View/Arms");
            leftArm = new Arm(view, 'L');
            rightArm = new Arm(view, 'R');
            
            foreach (var c in leftArm.joints[0].GetComponentsInChildren<Transform>())
            {
                c.gameObject.layer = Layers.Viewport;
            }
            
            foreach (var c in rightArm.joints[0].GetComponentsInChildren<Transform>())
            {
                c.gameObject.layer = Layers.Viewport;
            }
        }

        private void Update()
        {
            var leftTarget = (Transform)null;
            var rightTarget = (Transform)null;

            var weapon = weaponManager.CurrentWeapon;
            if (weapon)
            {
                if (weapon.leftHandHold) leftTarget = weapon.leftHandHold;
                if (weapon.rightHandHold) rightTarget = weapon.rightHandHold;
            }
            
            if (leftTarget) leftArm.Solve(leftTarget);
            if (rightTarget) rightArm.Solve(rightTarget);
        }

        public class Arm
        {
            public Transform transform;
            public Transform[] joints;
            private float[] lengths;
            private Vector3 localRoot;

            public Arm(Transform transform, char chirality)
            {
                this.transform = transform;

                
                joints = new Transform[3];
                joints[0] = transform.Find($"Arm.{chirality}");
                joints[1] = joints[0].GetChild(0);
                joints[2] = joints[1].GetChild(0);
                
                localRoot = transform.InverseTransformPoint(joints[0].position);

                lengths = new[]
                {
                    (joints[1].position - joints[0].position).magnitude,
                    (joints[2].position - joints[1].position).magnitude,
                };
            }

            public void Solve(Transform target)
            {
                var root = transform.TransformPoint(localRoot);

                var totalLength = lengths[0] + lengths[1];

                if ((target.position - root).magnitude > totalLength)
                {
                    var d = (target.position - root).normalized;
                    var r = Quaternion.LookRotation(d);
                    
                    joints[0].position = target.position - d * totalLength;
                    joints[0].rotation = r;
                    
                    joints[1].position = target.position - d * lengths[1];
                    joints[1].rotation = r;
                    
                    joints[2].position = target.position;
                    joints[2].rotation = target.rotation;
                    return;
                }
                
                var anchor0 = joints[0].position;
                var anchor1 = anchor0 + Vector3.ClampMagnitude(target.position - anchor0, totalLength);
                
                var lengthA = lengths[0];
                var lengthB = (root - target.position).magnitude;
                var lengthC = lengths[1];

                // Law of Cosines
                var angleC = Mathf.Acos((lengthA * lengthA + lengthB * lengthB - lengthC * lengthC) / (2 * lengthA * lengthB));

                var forward = (anchor1 - anchor0).normalized;
                var right = target.right.normalized;
                var up = Vector3.Cross(forward, right).normalized;
                
                var point1 = anchor0 + (forward * Mathf.Cos(angleC) + -up * Mathf.Sin(angleC)) * lengthA;
                var point2 = anchor1;
                var point0 = anchor0;

                joints[0].rotation = Quaternion.LookRotation(point1 - point0);
                
                joints[1].position = point1;
                joints[1].rotation = Quaternion.LookRotation(point2 - point1);
                
                joints[2].position = point2;
                joints[2].rotation = target ? target.rotation : Quaternion.LookRotation(point2 - point1);
            }
        }
    }
}