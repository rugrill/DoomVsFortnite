using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public Transform target;
    public Image foreground;
    public Image background;
    public Vector3 offset;


    void LateUpdate() {
        Vector3 direction = (target.position - Camera.main.transform.position).normalized;
        bool isBehind = Vector3.Dot(direction, Camera.main.transform.forward) <= 0;
        foreground.enabled = !isBehind;
        background.enabled = !isBehind;
        transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
    }

    public void SetHealthBarSize(float percentage) {
        float parentWith = GetComponent<RectTransform>().rect.width;
        float with = parentWith * percentage;
        foreground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, with);
    }
}
