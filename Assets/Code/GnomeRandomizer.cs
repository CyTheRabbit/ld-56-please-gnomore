using UnityEngine;

namespace Gnome
{
    public class GnomeRandomizer : MonoBehaviour
    {
        public Transform Body;
        public SpriteRenderer BodyRenderer;
        public SpriteRenderer HeadRenderer;
        public SpriteRenderer FaceRenderer;
        public SpriteRenderer HatRenderer;
        public SpriteRenderer BeardRenderer;
        [Space]
        public float WidthMin;
        public float WidthMax;
        public float HeightMin;
        public float HeightMax;
        [Space]
        public Gradient HatColor;
        public Gradient BeardColor;
        public Gradient BodyColor;
        public Gradient HeadColor;
        [Space]
        public Sprite[] HatVariants;
        public Sprite[] BeardVariants;
        public Sprite[] FaceVariants;

        public void Start()
        {
            Body.localScale = new Vector3(Random.Range(WidthMin, WidthMax), Random.Range(HeightMin, HeightMax), 1);

            BodyRenderer.color = BodyColor.Evaluate(Random.value);
            HeadRenderer.color = HeadColor.Evaluate(Random.value);
            FaceRenderer.sprite = RandomElement(FaceVariants);
            HatRenderer.color = HatColor.Evaluate(Random.value);
            HatRenderer.sprite = RandomElement(HatVariants);
            BeardRenderer.color = BeardColor.Evaluate(Random.value);
            BeardRenderer.sprite = RandomElement(BeardVariants);
        }

        private static T RandomElement<T>(T[] variants) => variants[Random.Range(0, variants.Length)];
    }
}