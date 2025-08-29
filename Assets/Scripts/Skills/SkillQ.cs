using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Skills/Q - Arrow Volley")]
public class SkillQ : SkillData
{
    public float spread = 0.8f;   // 좌우 퍼짐(유닛)
    public int   damageAdd = 0;   // (선택) Q 전용 추가 데미지

    public override bool Execute(in SkillContext ctx)
    {
        // 시작/기본 타깃
        Vector2 start = ctx.firePos ? (Vector2)ctx.firePos.position : (Vector2)ctx.caster.transform.position;
        Vector2 baseTarget = (ctx.target != null) ? (Vector2)ctx.target.position
            : start + Vector2.right * (ctx.caster.transform.localScale.x < 0 ? 1f : -1f);

        // 진행 방향/수직 벡터
        Vector2 dir  = (baseTarget - start).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        // 가운데 + 좌/우 퍼짐으로 3발
        ctx.caster.SpawnArrow(start, baseTarget, 1f, 1f, damageAdd);
        ctx.caster.SpawnArrow(start, baseTarget + perp * spread, 1f, 1f, damageAdd);
        ctx.caster.SpawnArrow(start, baseTarget - perp * spread, 1f, 1f, damageAdd);
        return true;
    }
}
