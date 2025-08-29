using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private Team team = Team.Enemy;
    [SerializeField] private int maxHP = 100;
    public int CurrentHP { get; private set; }
    public event Action<Health> OnDied;
    bool dead;  
    
    public Team Team => team;
    public bool IsAlive => CurrentHP > 0;
    public int MaxHP => maxHP;
    public bool IsDead => dead;
    
    void Awake() => CurrentHP = maxHP;

    public void SetTeam(Team t) { team = t; }
    public void SetMaxHP(int newMax, bool refill)
    {
        maxHP = Mathf.Max(1, newMax);
        CurrentHP = refill ? maxHP : Mathf.Min(CurrentHP, maxHP);
    }
    
    public void ApplyDamage(DamageInfo info)
    {
        if (!IsAlive) return;
        if (info.sourceTeam == team) return; // 아군 공격 무시(원하면 끄기)

        CurrentHP -= Mathf.Max(0, info.amount);
        
        Debug.Log($"{name} took {info.amount} dmg → HP {CurrentHP}/{maxHP}");
        
        if (CurrentHP <= 0) Die();
    }

    void Die()
    {
        if (dead) return;                // ★ 중복 방지
        dead = true;

        OnDied?.Invoke(this);             // ★ 먼저 알리고
    }
}
