using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillContext
{
    public SkillCaster caster; // ������
    public Team team; // ������ ��
    public Transform firePos; // �߻� ��ġ
    public Transform target; // Ÿ��
    public Vector2 targetPos; // Ÿ�� ��ǥ ������
}