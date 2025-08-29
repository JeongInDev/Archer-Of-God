using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="Skills/W - HeavyArrowBuff")]
public class SkillW : SkillData
{
    public float duration = 5f;
    public float speedMul = 0.6f;  // ������
    public float scaleMul = 1.6f;  // ũ��
    public int   damageAdd = 5;    // ���� �� ��

    public override bool Execute(in SkillContext ctx)
    {
        if (ctx.caster == null) return false;
        ctx.caster.ApplyWBuff(duration, speedMul, scaleMul, damageAdd);
        return true;
    }
}
