using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gnome
{
    public class EmotionPerception : MonoBehaviour
    {
        public BoxCollider2D Trigger;
        public float BaseFrequency = 4.5f;
        public float RandomFrequency = 6f;
        public float Talkativeness = 0.2f;
        public float TalkativenessLimit = 4f;

        private readonly HashSet<Crowd> crowds = new();

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<GnomeAgent>() is { Crowd: { } crowd } gnome)
            {
                if (crowds.Add(crowd))
                {
                    StartCoroutine(CrowdEmotionsRoutine(crowd));
                }
            }
        }

        private IEnumerator CrowdEmotionsRoutine(Crowd crowd)
        {
            yield return new WaitForSeconds(0.5f);

            while (crowd.Members.Count > 0)
            {
                var visibleMembers = crowd.Members
                    .Where(member => Trigger.OverlapPoint(member.Position)
                                     && member.Behaviour is not GnomeLeaderBehaviour)
                    .ToArray();
                if (visibleMembers.Length != 0)
                {
                    var member = visibleMembers.RandomElement();
                    member.Wish();
                }

                yield return new WaitForSeconds(
                    BaseFrequency
                    + RandomFrequency * Random.value
                    - Mathf.Clamp(
                        Talkativeness * crowd.Members.Count,
                        min: 0,
                        max: TalkativenessLimit));
            }
        }
    }
}