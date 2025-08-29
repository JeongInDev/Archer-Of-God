using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Skills/R - Falling Sword")]
public class SkillR : SkillData
{
    public GameObject swordPrefab;   // 떨어질 검 프리팹
    public float dropHeight = 6f;    // 위로 얼마나 띄워서 생성할지
    public float forwardOffset = 0.5f; // 시전자 앞쪽으로 약간 당겨서
    public int   directHitDamage = 15; // 직격
    public int   tickDamage      = 5;  // 주위 틱
    public float tickInterval    = 2f; // 2초 간격
    public float duration        = 6f; // 총 유지시간
    public float tickRadius      = 1.6f;

    public override bool Execute(in SkillContext ctx)
    {
        if (swordPrefab == null || ctx.caster == null) return false;

        // 타깃 스냅샷 (플레이어: 최근접 적을 넘겨주고, 적: 플레이어 Transform을 넘겨주는 구조 권장)
        Vector2 basePos = ctx.target ? (Vector2)ctx.target.position
            : (Vector2)ctx.caster.transform.position;

        // 시전자 기준 약간 앞쪽 (스프라이트가 기본 '왼쪽' 가정)
        float dir = (ctx.caster.transform.localScale.x < 0f) ? +1f : -1f;
        Vector2 spawn = new Vector2(basePos.x + dir * forwardOffset, basePos.y + dropHeight);

        Quaternion baseRot  = swordPrefab.transform.rotation;            // 프리팹 원래 회전 유지
        Quaternion extraRot = Quaternion.Euler(0f, 0f, 180f);            // Z축 180°
        var go = GameObject.Instantiate(swordPrefab, spawn, baseRot * extraRot);
        var sr = go.GetComponent<Sword>();
        if (sr != null)
        {
            sr.Init(
                ownerTeam: ctx.team,
                directHitDamage: directHitDamage,
                tickDamage: tickDamage,
                tickInterval: tickInterval,
                duration: duration,
                tickRadius: tickRadius
            );
        }
        return true;
    }
}
