using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigShield : MonoBehaviour
{
    Rigidbody2D rb;
    bool installed;
    private SpriteRenderer[] spritesToFlip;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spritesToFlip = GetComponents<SpriteRenderer>();
        // spritesToFlip = GetComponentsInChildren<SpriteRenderer>(true);

    }

    public void Init(Team team, int hp)
    {
        var h = GetComponent<Health>();
        if (h != null)
        {
            h.SetTeam(team);
            h.SetMaxHP(hp, true); // 풀로 채워 배치
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
            
            installed = true;
            if (rb)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }
}
