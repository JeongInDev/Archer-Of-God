using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] LayerMask damageMask = ~0;

    [Header("Effects")] [SerializeField] GameObject vfxFalled;
    [SerializeField] GameObject vfxSpark;
    [SerializeField] float vfxLifetime = 5.0f;
    private bool vfxPlayed;
    Rigidbody2D rb;
    Collider2D col;

    Team ownerTeam;
    int directHitDamage;
    int tickDamage;
    float tickInterval;
    float duration;
    float tickRadius;

    bool landed;
    bool directHitApplied;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Init(Team ownerTeam, int directHitDamage, int tickDamage, float tickInterval, float duration,
        float tickRadius)
    {
        this.ownerTeam = ownerTeam;
        this.directHitDamage = directHitDamage;
        this.tickDamage = tickDamage;
        this.tickInterval = tickInterval;
        this.duration = duration;
        this.tickRadius = tickRadius;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (landed) return;

        if (col.collider.CompareTag("Ground"))
        {
            Land();
            return;
        }

        if (!directHitApplied && TryGetLivingHealth(col.collider, out var hp) && hp.Team != ownerTeam)
        {
            ApplyDamage(hp, directHitDamage, "SwordRain_Direct");
            directHitApplied = true;
        }
    }

    void Land()
    {
        PlayVFXLocal(vfxFalled, new Vector3(0, -0.5f, 0f));

        landed = true;
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (col) col.enabled = false;

        StartCoroutine(TickRoutine());
    }

    IEnumerator TickRoutine()
    {
        float endAt = Time.time + duration;

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
        PlayVFXLocal(vfxFalled, new Vector3(0, -0.2f, 0f));

        var hits = Physics2D.OverlapCircleAll(transform.position, tickRadius, damageMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (TryGetLivingHealth(hits[i], out var hp) && hp.Team != ownerTeam)
            {
                ApplyDamage(hp, tickDamage, "SwordRain_Tick");
                PlayVFXLocal(vfxFalled, new Vector3(0, -0.5f, 0f));
            }
        }
    }

    bool TryGetLivingHealth(Collider2D c, out Health hp)
    {
        hp = c.GetComponent<Health>();
        if (hp == null) return false;
        if (hp.IsDead) return false;
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

    // ÀÌÆåÆ®
    void PlayVFX(GameObject prefab)
    {
        PlayVFX(prefab, Vector3.zero);
    }

    void PlayVFX(GameObject prefab, Vector3 worldOffset)
    {
        if (vfxPlayed || !prefab) return;
        var pos = (Vector3)transform.position + worldOffset;
        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (vfxLifetime > 0f) Destroy(go, vfxLifetime);
        vfxPlayed = true;
    }

    void PlayVFXLocal(GameObject prefab, Vector3 localOffset)
    {
        PlayVFX(prefab, transform.TransformPoint(localOffset) - transform.position);
    }
}