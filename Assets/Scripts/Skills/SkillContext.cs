using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillContext
{
    public SkillCaster caster; // ½ÃÀüÀÚ
    public Team team; // ½ÃÀüÀÚ ÆÀ
    public Transform firePos; // ¹ß»ç À§Ä¡
    public Transform target; // Å¸±ê
    public Vector2 targetPos; // Å¸±ê ÁÂÇ¥ ½º³À¼¦
}