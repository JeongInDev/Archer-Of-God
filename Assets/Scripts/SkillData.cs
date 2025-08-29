using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName = "Skill";
    public float cooldown = 4f;         // 발동 성공 시 시작할 쿨타임
    public abstract bool Execute(in SkillContext ctx); // true면 성공(쿨다운 시작)
}
