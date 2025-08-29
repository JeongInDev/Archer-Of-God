using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillContext
{
    public SkillCaster caster;   // 시전자
    public Team team;            // 시전자 팀
    public Transform firePos;    // 발사 위치(없으면 본인 Transform)
    public Transform target;     // 타깃(없으면 null)
    public Vector2 targetPos;    // 타깃 좌표 스냅샷(없으면 0)
}
