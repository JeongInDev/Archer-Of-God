using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Health target;  
    [SerializeField] Image  fill;    
    [SerializeField] bool   hideWhenFull = false;

    float baseScaleX = 1f;
    
    void Awake()
    {
        if (!target) target = GetComponentInParent<Health>();
        if (!fill)
        {
            var tf = transform.Find("Health");
            if (tf) fill = tf.GetComponent<Image>();
        }

        baseScaleX = Mathf.Abs(transform.localScale.x); // ★ 추가
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
        // Health가 maxHP를 public로 안 열어놨다면, 필요시 게터 추가해도 OK
        // 여기선 리플렉션 없이 Health에 public 프로퍼티가 있다고 가정:
        // public int MaxHP { get { return maxHP; } }
        return (int)target.GetType().GetProperty("CurrentHP").DeclaringType
            .GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(target);
    }
}
