using System;
using TMPro;
using UnityEngine;

namespace Gnome
{
    public class GameUI : MonoBehaviour
    {
        public RectTransform GnomeDustFill;
        public TMP_Text GnomeDustCounter;

        public void Start()
        {
            SetGnomeDustCount(0);
        }

        public void SetGnomeDustCount(int count)
        {
            GnomeDustFill.anchorMax = new Vector2(count / 69f, 1f);
            GnomeDustCounter.text = $"{count}/69";
        }
    }
}