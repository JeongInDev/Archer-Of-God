using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillContext
{
    public SkillCaster caster;   // ������
    public Team team;            // ������ ��
    public Transform firePos;    // �߻� ��ġ(������ ���� Transform)
    public Transform target;     // Ÿ��(������ null)
    public Vector2 targetPos;    // Ÿ�� ��ǥ ������(������ 0)
}
