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

        public void TogglePhysics()
        {
            _rigidbody.isKinematic = !(_door is not KeycardDoor && _door.DoorState == EDoorState.Open);
        }

        private void Awake()
        {
            _door = GetComponent<Door>();
            _rigidbody = GetComponent<Rigidbody>();
            _hingeJoint = GetComponent<HingeJoint>();
            
            TogglePhysics();

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
            if (obj != this)
            {
                return;
            }

            TogglePhysics();
        }
    }
}
