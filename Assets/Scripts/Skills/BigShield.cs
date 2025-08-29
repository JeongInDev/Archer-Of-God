using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigShield : MonoBehaviour
{
    Rigidbody2D rb;
    bool installed;
    private SpriteRenderer[] spritesToFlip;
    Health health;

    [Header("Effects")]
    [SerializeField] GameObject vfxOnGround;  
    [SerializeField] float vfxLifetime  = 1.0f;
    private bool vfxPlayed;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spritesToFlip = GetComponents<SpriteRenderer>();
        health = GetComponent<Health>();
        if (health != null) health.OnDied += OnDied;
    }

    public void Init(Team team, int hp)
    {
        var h = GetComponent<Health>();
        if (h != null)
        {
            h.SetTeam(team);
            h.SetMaxHP(hp, true);
        }

        // 적팀이 쓴 스킬이라면 반대로 뒤집기
        if (team == Team.Enemy)
        {
            foreach (var sr in spritesToFlip)
            {
                if (!sr) continue;
                sr.flipX = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (installed) return;

        if (col.collider.CompareTag("Ground"))
        {
            PlayVFXLocal(vfxOnGround, new Vector3(0, -0.5f, 0f));
            installed = true;
            if (rb)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    void OnDestroy()
    {
        if (health != null) health.OnDied -= OnDied;
    }

    void OnDied(Health _)
    {
        Destroy(gameObject);
    }
    
    // 이펙트
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