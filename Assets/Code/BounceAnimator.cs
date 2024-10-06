using UnityEngine;

namespace Gnome
{
    public class BounceAnimator : MonoBehaviour
    {
        public Transform Body;
        public float InertiaStrength;
        public float DragStrength;
        public float OffsetLimit;
        public float TiltLimit;
        public float OffsetMagnitude;
        public float TiltMagnitude;
        public float OffsetBumpStrength;
        public float TiltBumpStrength;

        private Vector2 offset;
        private Vector2 velocity;
        private float tilt;
        private float angularVelocity;

        public void Update()
        {
            var acceleration = CalculateForces();
            var newVelocity = velocity + acceleration * Time.deltaTime;
            offset = Vector2.ClampMagnitude((velocity + newVelocity) / 2 * Time.deltaTime, OffsetLimit);
            velocity = newVelocity;

            var angularAcceleration = CalculateAngularForces();
            var newAngularVelocity = angularVelocity + angularAcceleration * Time.deltaTime;
            tilt = Mathf.Clamp((angularVelocity + newAngularVelocity) / 2 * Time.deltaTime, -TiltLimit, TiltLimit);
            angularVelocity = newAngularVelocity;

            Body.localPosition = offset * OffsetMagnitude;
            Body.localRotation = Quaternion.Euler(0, 0, -tilt * TiltMagnitude);
        }

        public void Bump(Vector2 force)
        {
            velocity += force * OffsetBumpStrength;
            angularVelocity += force.x * TiltBumpStrength;
        }

        private Vector2 CalculateForces()
        {
            var inertia = offset * -InertiaStrength;
            var drag = velocity * -DragStrength;
            return drag + inertia;
        }

        private float CalculateAngularForces()
        {
            var inertia = tilt * -InertiaStrength;
            var drag = angularVelocity * -DragStrength;
            return drag + inertia;
        }
    }
}