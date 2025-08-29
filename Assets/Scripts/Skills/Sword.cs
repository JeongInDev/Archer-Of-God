using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] LayerMask damageMask = ~0; // 필요 시 레이어 제한
    
    Rigidbody2D rb;
    Collider2D  col;

    Team ownerTeam;
    int   directHitDamage;
    int   tickDamage;
    float tickInterval;
    float duration;
    float tickRadius;

    bool landed;
    bool directHitApplied;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        // 프리팹 권장값:
        // rb.bodyType = Dynamic, rb.gravityScale = 6~10, col.isTrigger = true
    }

    public void Init(Team ownerTeam, int directHitDamage, int tickDamage, float tickInterval, float duration, float tickRadius)
    {
        this.ownerTeam       = ownerTeam;
        this.directHitDamage = directHitDamage;
        this.tickDamage      = tickDamage;
        this.tickInterval    = tickInterval;
        this.duration        = duration;
        this.tickRadius      = tickRadius;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (landed) return;

        // 1) 땅에 꽂히기
        if (col.collider.CompareTag("Ground"))
        {
            Land();
            return;
        }

        // 2) 낙하 중 직격 데미지(한 번만)
        if (!directHitApplied && TryGetLivingHealth(col.collider, out var hp) && hp.Team != ownerTeam)
        {
            ApplyDamage(hp, directHitDamage, "SwordRain_Direct");
            directHitApplied = true;
        }
    }

    void Land()
    {
        landed = true;
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // 자리에 고정
        }
        if (col) col.enabled = false; // 더 이상 트리거 불필요

        // 착지 즉시 1틱 + 이후 주기 틱
        StartCoroutine(TickRoutine());
    }
    
    IEnumerator TickRoutine()
    {
        float endAt = Time.time + duration;

        // 착지 즉시 틱 1회
        DoTick();

        while (Time.time + 0.001f < endAt)
        {
            yield return new WaitForSeconds(tickInterval);
            DoTick();
        }

        Destroy(gameObject);
    }

    void DoTick()
    {
        // 주변 피해
        var hits = Physics2D.OverlapCircleAll(transform.position, tickRadius, damageMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (TryGetLivingHealth(hits[i], out var hp) && hp.Team != ownerTeam)
            {
                ApplyDamage(hp, tickDamage, "SwordRain_Tick");
            }
        }
    }

    bool TryGetLivingHealth(Collider2D c, out Health hp)
    {
        hp = c.GetComponent<Health>();
        if (hp == null) return false;
        if (hp.IsDead)  return false;
        return true;
    }

    void ApplyDamage(Health hp, int amount, string id)
    {
        var info = new DamageInfo(amount, ownerTeam, id, (Vector2)transform.position, this);
        hp.ApplyDamage(info);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tickRadius);
    }
#endif
}
