using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/E - BigShield")]
public class SkillE : SkillData
{
    public GameObject shieldPrefab;
    public float dropHeight = 6f; // 위로 얼마나 띄워서 떨어뜨릴지
    public float forwardOffset = 0.2f; // 시전자 앞쪽으로 얼마만큼
    public int maxHp = 100;

    public override bool Execute(in SkillContext ctx)
    {
        if (shieldPrefab == null || ctx.caster == null) return false;

        Vector2 start = ctx.firePos
            ? (Vector2)ctx.firePos.position
            : (Vector2)ctx.caster.transform.position;

        float dir = (ctx.caster.transform.localScale.x < 0f) ? +1f : -1f;
        Vector2 spawn = new Vector2(start.x + dir * forwardOffset, start.y + dropHeight);
        var go = GameObject.Instantiate(shieldPrefab, spawn, Quaternion.identity);

        // 팀/체력 주입
        var b = go.GetComponent<BigShield>();
        if (b != null)
        {
            b.Init(ctx.team, maxHp);
        }
        else
        {
            var h = go.GetComponent<Health>();
            if (h != null)
            {
                h.SetTeam(ctx.team);
                h.SetMaxHP(maxHp, true);
            }
        }

        return true;
    }
}