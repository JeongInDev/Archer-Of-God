using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Health target;
    [SerializeField] Image fill;
    [SerializeField] bool hideWhenFull = false;

    float baseScaleX = 1f;

    void Awake()
    {
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (target)
            target.OnDied += OnTargetDied;
    }

    private void LateUpdate()
    {
        if (!target || !fill) return;

        float ratio = Mathf.Clamp01((float)target.CurrentHP / Mathf.Max(1, targetMaxHP()));
        fill.fillAmount = ratio;

        if (hideWhenFull)
            fill.transform.parent.gameObject.SetActive(ratio < 0.999f);

        var parent = transform.parent;
        if (parent)
        {
            var s = transform.localScale;
            float parentWorldSign = Mathf.Sign(parent.lossyScale.x);
            s.x = parentWorldSign * baseScaleX;
            transform.localScale = s;
        }
    }

    int targetMaxHP()
    {
        return (int)target.GetType().GetProperty("CurrentHP").DeclaringType
            .GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(target);
    }

    void OnDestroy()
    {
        if (target) target.OnDied -= OnTargetDied;
    }

    void OnTargetDied(Health _)
    {
        gameObject.SetActive(false);
    }
}