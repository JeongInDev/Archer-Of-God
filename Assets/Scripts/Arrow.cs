using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Arrow : MonoBehaviour
{
    [Header("Curve")]
    // 0��1 �������� �糡 ���� 0�� Ŀ�� (�⺻��: �ε巯�� ������)
    // public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    [SerializeField] AnimationCurve heightCurve =
        new AnimationCurve(
            new Keyframe(0f,   0f, 0f, 0f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f,   0f, 0f, 0f)
        );
    
    [Header("ArrowData")]
    [SerializeField] float maxHeight   = 2f;     // �ְ���
    [SerializeField] float arrowSpeed = 8f;     // �� ȭ�� �ӵ�(����/��) ? �����ϰ� ����
    [SerializeField] float minDuration = 1f;     // �ٰŸ� ����(�ּ� ����ð�)
    [SerializeField] float maxDuration = 3f;     // ���Ÿ� ����(�ִ� ����ð�)
    [SerializeField] float fadeTime    = 2f;     // Ground �¾��� �� ���̵�
    [SerializeField] int damage = 10;         // ȭ�� �⺻ ������
    [SerializeField] Team ownerTeam = Team.Player; // �߻��� ��(�Ʊ� ���� ������ ����)
    
    // �� ���� �߰�: �浹�� ������ n�� �� �ڵ� ����
    [SerializeField] float autoDestroyAfter = 5f;
    Coroutine autoKillCo;
    
    Rigidbody2D _rigidbody2D;
    Collider2D _collider2D;
    SpriteRenderer[] _spriteRenderers;

    private Vector2 start;
    private Vector2 target;
    private float duration;
    bool flying;
    
    private void Awake()
    {
        _rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        _collider2D = GetComponentInChildren<Collider2D>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }
    
    public void Launch(Vector2 s, Vector2 t)
    {
        start = s; target = t;
        
        // �� �Ÿ�/�ӵ� �� ����ð� ���(���ϴ� ���� ������ Ŭ����)
        float dist = Vector2.Distance(start, target);
        float raw  = dist / Mathf.Max(0.001f, arrowSpeed);
        duration   = Mathf.Clamp(raw, minDuration, maxDuration);
        
        
        StopAllCoroutines();
        // �� �ڵ� ���� Ÿ�̸� ���� (�浹 �߻� �� ����)
        if (autoKillCo != null) StopCoroutine(autoKillCo);
        autoKillCo = StartCoroutine(AutoDestroyAfter(autoDestroyAfter));
        
        StartCoroutine(Fly());
    }
    
    IEnumerator Fly()
    {
        flying = true;
        float t = 0f;
        Vector2 prev = start;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);                 // 0..1
            float h = Mathf.Lerp(0f, maxHeight, heightCurve.Evaluate(u));
            Vector2 pos = Vector2.Lerp(start, target, u) + Vector2.up * h;

            transform.position = pos;

            // ���� ���� ȸ��
            Vector2 delta = pos - prev;
            if (delta.sqrMagnitude > 1e-6f) transform.right = delta.normalized;
            prev = pos;

            yield return null;
        }

        // flying = false;                 // �浹�� ������ ��ǥ ���� ���� �� �ı�
        // Destroy(gameObject);
    }
    
    public void MulTravelSpeed(float mul) { arrowSpeed *= mul; }
    public void SetDamageAdd(int add)     { damage += add; }   // damage �ʵ尡 ���ٸ� int damage = 10; ���� �⺻�� �߰�
    public void SetOwnerTeam(Team t)      { ownerTeam = t; } // �� �޼��嵵 �ϳ� �߰� ����
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // �� �浹�� �߻������� �ڵ� ���� Ÿ�̸� ����
        if (autoKillCo != null) { StopCoroutine(autoKillCo); autoKillCo = null; }

        if (!flying) return;

        if (other.CompareTag("Enemy"))
        {
            // 1) IDamageable �켱
            IDamageable dmg;
            if (other.TryGetComponent<IDamageable>(out dmg))
            {
                var info = new DamageInfo(damage, ownerTeam, "Arrow", transform.position, this);
                dmg.ApplyDamage(info);
            }
            else
            {
                // 2) IDamageable�� ���ٸ� Health ���� ã��(����)
                Health hp;
                if (other.TryGetComponent<Health>(out hp))
                {
                    var info = new DamageInfo(damage, ownerTeam, "Arrow", transform.position, this);
                    hp.ApplyDamage(info);
                }
            }

            Destroy(gameObject); // �� ������ ȭ�� �ı�(������ ������)
            return;
        }
        else if (other.CompareTag("Ground"))
        {
            flying = false;
            StopAllCoroutines();        // �� �ڸ����� ����
            if (_collider2D) _collider2D.enabled = false;
            StartCoroutine(FadeAndDie());
        }
    }
    
    IEnumerator AutoDestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // 5�� ���� �ƹ� �浹�� �����ٸ� �ڵ� ����
        if (this) Destroy(gameObject);

        Debug.Log("ȭ�� �ڵ� ����!");
    }
    
    IEnumerator FadeAndDie()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            foreach (var sr in _spriteRenderers)
            {
                var c = sr.color; c.a = a; sr.color = c;
            }
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
