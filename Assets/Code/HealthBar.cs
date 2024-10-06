using UnityEngine;

namespace Gnome
{
    public class HealthBar : MonoBehaviour
    {
        public RectTransform Root;
        public RectTransform Fill;
        public PunchableDecoration Target;

        public void Update()
        {
            Root.gameObject.SetActive(Target.Health < Target.MaxHealth);
            Fill.anchorMax = new Vector2((float)Target.Health / Target.MaxHealth, 1);
        }
    }
}