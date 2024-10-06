using System;
using UnityEngine;

namespace Gnome
{
    public class FistsAnimator : MonoBehaviour
    {
        public Animator Animator;
        public Transform Aim;
        public float AimSmoothTime = 0.25f;

        public event Action PunchLanded;

        private Transform target;
        private Vector3 aimVelocity;
        public bool IsPunching;
        private static readonly int IsPunchingHash = Animator.StringToHash("Punching");

        public void Update()
        {
            if (target != null)
            {
                Aim.position = Vector3.SmoothDamp(Aim.position, target.position, ref aimVelocity, AimSmoothTime);
            }
            Animator.SetBool(IsPunchingHash, IsPunching);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void LandPunch()
        {
            PunchLanded?.Invoke();
        }
    }
}