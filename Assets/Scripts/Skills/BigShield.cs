using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigShield : MonoBehaviour
{
    Rigidbody2D rb;
    bool installed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Team team, int hp)
    {
        var h = GetComponent<Health>();
        if (h != null)
        {
            h.SetTeam(team);
            h.SetMaxHP(hp, true); // Ǯ�� ä�� ��ġ
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
