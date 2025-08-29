using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName = "Skill";
    public float cooldown = 4f;
    public abstract bool Execute(in SkillContext ctx);
}