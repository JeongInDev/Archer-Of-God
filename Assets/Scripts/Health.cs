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
    public int MaxHP => maxHP;
    
    void Awake() => CurrentHP = maxHP;

    public void ApplyDamage(DamageInfo info)
    {
        if (!IsAlive) return;
        if (info.sourceTeam == team) return; // �Ʊ� ���� ����(���ϸ� ����)

        CurrentHP -= Mathf.Max(0, info.amount);
        
        Debug.Log($"{name} took {info.amount} dmg �� HP {CurrentHP}/{maxHP}");
        
        if (CurrentHP <= 0) Die();
    }

    void Die()
    {
        gameObject.SetActive(false); // �ϴ� ��Ȱ�� ó��(������ ���߿�)
    }
}
