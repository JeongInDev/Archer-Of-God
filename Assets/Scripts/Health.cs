using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private Team team = Team.Enemy;
    [SerializeField] private int maxHP = 100;
    public int CurrentHP { get; private set; }
    public Team Team => team;
    public bool IsAlive => CurrentHP > 0;

    void Awake() => CurrentHP = maxHP;

    public void ApplyDamage(DamageInfo info)
    {
        if (!IsAlive) return;
        if (info.sourceTeam == team) return; // �Ʊ� ���� ����(���ϸ� ����)

        CurrentHP -= Mathf.Max(0, info.amount);
        if (CurrentHP <= 0) Die();
    }

    void Die()
    {
        gameObject.SetActive(false); // �ϴ� ��Ȱ�� ó��(������ ���߿�)
    }
}
