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

    private void LateUpdate()
    {
        if (!target || !fill) return;

        float ratio = Mathf.Clamp01((float)target.CurrentHP / Mathf.Max(1, targetMaxHP()));
        fill.fillAmount = ratio;

        if (hideWhenFull)
            fill.transform.parent.gameObject.SetActive(ratio < 0.999f);

        // 부모가 좌/우 반전(scale.x 음수)될 때 HP바가 뒤집히지 않게 고정
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
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
