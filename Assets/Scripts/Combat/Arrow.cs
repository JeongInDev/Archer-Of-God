using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Arrow : MonoBehaviour
{
    [Header("Curve")]
    [SerializeField] private AnimationCurve heightCurve =
        new AnimationCurve(
            new Keyframe(0f,   0f, 0f, 0f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f,   0f, 0f, 0f)
        );
    
    [Header("ArrowData")]
    [SerializeField] private float maxHeight   = 2f;            // 최고점
    [SerializeField] private float arrowSpeed = 8f;             // 화살 속도
    [SerializeField] private float minDuration = 1f;            // 최소 비행시간
    [SerializeField] private float maxDuration = 3f;            // 최대 비행시간
    [SerializeField] private float fadeTime    = 2f;            // 땅맞았을때 맞았을 때 페이드 시간
    [SerializeField] private int damage = 10;                   // 화살 기본 데미지
    [SerializeField] private Team ownerTeam = Team.Player;      // 팀
    
    [Header("Effects")]
    [SerializeField] GameObject vfxOnCharacter;   // 적/플레이어 등 캐릭터에 맞았을 때
    [SerializeField] GameObject vfxOnGround;  // Ground 등에 박혔을 때
    [SerializeField] float vfxLifetime  = 1.0f; // VFX 프리팹이 자기파괴 안할 때 대비
    private bool vfxPlayed;
    
    [SerializeField] private float autoDestroyAfter = 5f;
    private Coroutine autoKillCo;
    
    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider2D;
    private SpriteRenderer[] _spriteRenderers;

    private Vector2 start;
    private Vector2 target;
    private float duration;
    private bool flying;
    
    private void Awake()
    {
        _rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        _collider2D = GetComponentInChildren<Collider2D>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }
    
    public void Launch(Vector2 s, Vector2 t)
    {
        start = s; 
        target = t;
        float dist = Vector2.Distance(start, target);
        float raw  = dist / Mathf.Max(0.001f, arrowSpeed);
        duration   = Mathf.Clamp(raw, minDuration, maxDuration);
        
        StopAllCoroutines();
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
            float u = Mathf.Clamp01(t / duration);  // 0..1
            float h = Mathf.Lerp(0f, maxHeight, heightCurve.Evaluate(u));
            Vector2 pos = Vector2.Lerp(start, target, u) + Vector2.up * h;

            transform.position = pos;

            // 진행 방향 회전
            Vector2 delta = pos - prev;
            if (delta.sqrMagnitude > 1e-6f) transform.right = delta.normalized;
            prev = pos;

            yield return null;
        }
    }
    
    public void MulTravelSpeed(float mul) { arrowSpeed *= mul; }
    public void SetDamageAdd(int add)     { damage += add; }
    public void SetOwnerTeam(Team t)      { ownerTeam = t; }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"Arrow hit {other.name} tag:{other.tag}");

        if (autoKillCo != null) { StopCoroutine(autoKillCo); autoKillCo = null; }
        if (!flying) return;

        if (other.CompareTag("Ground"))
        {
            PlayVFXLocal(vfxOnGround, new Vector3(0.5f, 0, 0)); 
            flying = false;
            StopAllCoroutines();
            if (_collider2D) _collider2D.enabled = false;
            StartCoroutine(FadeAndDie());
            return;
        }

        IDamageable dmg = null;
        Health hp = null;

        if (!other.TryGetComponent<IDamageable>(out dmg))
            other.TryGetComponent<Health>(out hp);
        if (dmg == null && hp == null) return;

        // 팀 판정 같은 팀이면 무시
        Team targetTeam = (dmg != null) ? dmg.Team : hp.Team;
        if (targetTeam == ownerTeam) 
            return;

        PlayVFXLocal(vfxOnCharacter, new Vector3(0.5f, 0, 0)); 
        
        // 데미지
        var info = new DamageInfo(damage, ownerTeam, "Arrow", transform.position, this);
        if (dmg != null)        
            dmg.ApplyDamage(info);
        else 
            hp.ApplyDamage(info);

        Destroy(gameObject);
    }
    
    IEnumerator AutoDestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (this) Destroy(gameObject);
        Debug.Log("화살 자동 삭제!");
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
