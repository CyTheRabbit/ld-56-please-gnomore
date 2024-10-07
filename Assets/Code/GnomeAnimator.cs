using UnityEngine;
using UnityEngine.Serialization;

namespace Gnome
{
    public class GnomeAnimator : MonoBehaviour
    {
        public struct WalkAnimationData
        {
            public float Start;
            public bool Flip;
        }

        public struct JumpAnimationData
        {
            public float Height;
            public float Start;
            public float End;
        }

        public struct State
        {
            public float Jump;
            public float Rotation;
        }

        public Transform Body;
        public Rigidbody2D Rigidbody;
        public SpriteRenderer[] SpriteRenderers;
        public float WalkSpeed;
        public float Tilt;
        public float JumpHeight;
        public float SmoothTime;
        
        private WalkAnimationData? walkData;
        private JumpAnimationData? jumpData;
        private State state;
        private State velocity;

        public bool IsMoving;

        public void Update()
        {
            if (IsMoving && walkData is null)
            {
                walkData = new WalkAnimationData
                {
                    Start = Time.time,
                    Flip = Random.value > 0.5f,
                };
            }
            else if (!IsMoving)
            {
                walkData = null;
            }

            var target = new State();
            if (walkData is { } walk)
            {
                var time = Time.time - walk.Start;
                target.Rotation = Mathf.Sin(time * WalkSpeed) * Tilt * (walk.Flip ? -1 : 1);
                target.Jump = Mathf.Sin(time * WalkSpeed * 2) * JumpHeight;
            }

            if (jumpData is { } jump)
            {
                var t = Mathf.InverseLerp(jump.Start, jump.End, Time.time);
                target.Jump = Mathf.Clamp01((t - t * t) * 4) * jump.Height;

                if (Time.time > jump.End)
                {
                    jumpData = null;
                }
            }

            var newState = new State
            {
                Jump = Mathf.SmoothDamp(state.Jump, target.Jump, ref velocity.Jump, SmoothTime),
                Rotation = Mathf.SmoothDamp(state.Rotation, target.Rotation, ref velocity.Rotation, SmoothTime),
            };
            state = newState;

            Body.localRotation = Quaternion.Euler(0, 0, state.Rotation);
            Body.localPosition = Vector3.up * state.Jump;

            var velocityX = Rigidbody.velocity.x;
            if (Mathf.Abs(velocityX) > 0.05f)
            {
                var flipX = velocityX < 0;
                foreach (var sprite in SpriteRenderers)
                {
                    sprite.flipX = flipX;
                }
            }
        }

        public void Jump(float strength)
        {
            jumpData = new JumpAnimationData
            {
                Start = Time.time,
                End = Time.time + strength / 2,
                Height = strength,
            };
        }
    }
}