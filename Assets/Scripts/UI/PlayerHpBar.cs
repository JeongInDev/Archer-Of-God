using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] Health target;
    [SerializeField] Image fill;
    [SerializeField] bool smooth = true;
    [SerializeField] float smoothSpeed = 8f;

    float currentRatio = 1f;

    void Awake()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.GetComponent<Health>();
        }

        if (fill == null)
        {
            var tf = transform.Find("Health");
            if (tf) fill = tf.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (!target || !fill) return;

        float targetRatio = Mathf.Clamp01((float)target.CurrentHP / Mathf.Max(1, target.MaxHP));
        currentRatio = smooth
            ? Mathf.MoveTowards(currentRatio, targetRatio, smoothSpeed * Time.deltaTime)
            : targetRatio;

        fill.fillAmount = currentRatio;
    }
}