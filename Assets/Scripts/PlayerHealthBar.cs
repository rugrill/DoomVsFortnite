using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image foreground;
    public Image background;
    public void SetPlayerHealthBarSize(float percentage) {
        float parentWith = GetComponent<RectTransform>().rect.width;
        float with = parentWith * percentage;
        foreground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, with);
    }
}
