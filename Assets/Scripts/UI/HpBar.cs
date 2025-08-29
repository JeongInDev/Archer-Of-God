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

        // �θ� ��/�� ����(scale.x ����)�� �� HP�ٰ� �������� �ʰ� ����
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }
    
    int targetMaxHP()
    {
        // Health�� maxHP�� public�� �� ������ٸ�, �ʿ�� ���� �߰��ص� OK
        // ���⼱ ���÷��� ���� Health�� public ������Ƽ�� �ִٰ� ����:
        // public int MaxHP { get { return maxHP; } }
        return (int)target.GetType().GetProperty("CurrentHP").DeclaringType
            .GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(target);
    }
}
