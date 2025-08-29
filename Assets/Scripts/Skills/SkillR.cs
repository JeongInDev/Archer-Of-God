using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Skills/R - Falling Sword")]
public class SkillR : SkillData
{
    public GameObject swordPrefab;   // ������ �� ������
    public float dropHeight = 6f;    // ���� �󸶳� ����� ��������
    public float forwardOffset = 0.5f; // ������ �������� �ణ ��ܼ�
    public int   directHitDamage = 15; // ����
    public int   tickDamage      = 5;  // ���� ƽ
    public float tickInterval    = 2f; // 2�� ����
    public float duration        = 6f; // �� �����ð�
    public float tickRadius      = 1.6f;

    public override bool Execute(in SkillContext ctx)
    {
        if (swordPrefab == null || ctx.caster == null) return false;

        // Ÿ�� ������ (�÷��̾�: �ֱ��� ���� �Ѱ��ְ�, ��: �÷��̾� Transform�� �Ѱ��ִ� ���� ����)
        Vector2 basePos = ctx.target ? (Vector2)ctx.target.position
            : (Vector2)ctx.caster.transform.position;

        // ������ ���� �ణ ���� (��������Ʈ�� �⺻ '����' ����)
        float dir = (ctx.caster.transform.localScale.x < 0f) ? +1f : -1f;
        Vector2 spawn = new Vector2(basePos.x + dir * forwardOffset, basePos.y + dropHeight);

        Quaternion baseRot  = swordPrefab.transform.rotation;            // ������ ���� ȸ�� ����
        Quaternion extraRot = Quaternion.Euler(0f, 0f, 180f);            // Z�� 180��
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
