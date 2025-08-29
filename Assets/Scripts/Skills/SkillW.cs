using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Skills/W - HeavyArrowBuff")]
public class SkillW : SkillData
{
    public float duration = 5f; // 버프 지속 시간
    public float speedMul = 0.6f; // 얼마나 더 느려질지
    public float scaleMul = 1.6f; // 얼마나 더 커질지
    public int damageAdd = 5; // 얼마나 더 세질지

    public override bool Execute(in SkillContext ctx)
    {
        if (ctx.caster == null) return false;
        ctx.caster.ApplyWBuff(duration, speedMul, scaleMul, damageAdd);
        return true;
    }
}