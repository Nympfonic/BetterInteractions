using Arys.BetterInteractions.Helper;
using EFT.Interactive;
using UnityEngine;

namespace Arys.BetterInteractions.Components
{
    [DisallowMultipleComponent]
    public class BetterInteractionsPhysicsDoor : MonoBehaviour
    {
        private Door _door;
        private Rigidbody _rigidbody;
        private HingeJoint _hingeJoint;

        public void TogglePhysics(EDoorState doorState)
        {
            _rigidbody.isKinematic = doorState != EDoorState.Open;
        }

        private void Awake()
        {
            _door = GetComponent<Door>();
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            // Rigidbody needs to be added to Tarkov's managed rigidbodies otherwise they will not work
            PhysicsHelper.SupportRigidbody(_rigidbody, 0f);
            _hingeJoint = gameObject.AddComponent<HingeJoint>();
            
            TogglePhysics(_door.DoorState);

            _door.OnDoorStateChanged += DoorStateChangedPhysicsCheck;

            //_rigidbody.mass = 2f;
            //_rigidbody.drag = 1f;
            _rigidbody.useGravity = false;
            _rigidbody.maxDepenetrationVelocity = 15f;

            _hingeJoint.useSpring = false;
            _hingeJoint.useMotor = false;
            _hingeJoint.useLimits = true;
            _hingeJoint.breakForce = 25f;
            _hingeJoint.limits = new JointLimits()
            {
                min = _door.CloseAngle,
                max = _door.OpenAngle,
                bounciness = 0.2f,
                bounceMinVelocity = 10f
            };
        }

        private void OnDestroy()
        {
            _door.OnDoorStateChanged -= DoorStateChangedPhysicsCheck;
        }

        private void DoorStateChangedPhysicsCheck(WorldInteractiveObject obj, EDoorState prevState, EDoorState nextState)
        {
            if (obj != _door)
            {
                return;
            }

            TogglePhysics(nextState);
        }
    }
}
