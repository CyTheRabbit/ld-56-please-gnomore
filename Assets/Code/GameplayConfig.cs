using UnityEngine;

namespace Gnome
{
    [CreateAssetMenu]
    public class GameplayConfig : ScriptableObject
    {
        public float JoinRadius = 3;
        public int FollowPriority = 0;
        [Range(0, 1)]
        public float Overtaking = 0.25f;
        public float AvoidanceDistance = 3;
        public float SocialDistance = 2;
        public float GrumpyDistance = 3;
        public float GrumpyHeatUp = 3;
        public float GrumpyCoolDown = 3;
        [Space]
        public float FollowDistanceFromLeader = 1f;
        public float CrowdSpread = 0.15f;
    }
}