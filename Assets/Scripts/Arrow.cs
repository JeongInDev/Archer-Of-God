using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Arrow : MonoBehaviour
{
    [Header("Curve")]
    // 0→1 구간에서 양끝 값이 0인 커브 (기본값: 부드러운 포물선)
    // public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    [SerializeField] AnimationCurve heightCurve =
        new AnimationCurve(
            new Keyframe(0f,   0f, 0f, 0f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f,   0f, 0f, 0f)
        );
    
    [Header("ArrowData")]
    [SerializeField] float maxHeight   = 2f;     // 최고점
    [SerializeField] float arrowSpeed = 8f;     // ★ 화살 속도(유닛/초) ? 일정하게 유지
    [SerializeField] float minDuration = 1f;     // 근거리 감성(최소 비행시간)
    [SerializeField] float maxDuration = 3f;     // 원거리 감성(최대 비행시간)
    [SerializeField] float fadeTime    = 2f;     // Ground 맞았을 때 페이드
    [SerializeField] int damage = 10;         // 화살 기본 데미지
    [SerializeField] Team ownerTeam = Team.Player; // 발사자 팀(아군 오발 방지용 선택)
    
    // ★ 새로 추가: 충돌이 없으면 n초 후 자동 삭제
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
        
        // ★ 거리/속도 → 비행시간 계산(원하는 감성 범위로 클램프)
        float dist = Vector2.Distance(start, target);
        float raw  = dist / Mathf.Max(0.001f, arrowSpeed);
        duration   = Mathf.Clamp(raw, minDuration, maxDuration);
        
        
        StopAllCoroutines();
        // ★ 자동 삭제 타이머 시작 (충돌 발생 시 정지)
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

            // 진행 방향 회전
            Vector2 delta = pos - prev;
            if (delta.sqrMagnitude > 1e-6f) transform.right = delta.normalized;
            prev = pos;

            yield return null;
        }

        // flying = false;                 // 충돌이 없으면 목표 지점 도달 후 파괴
        // Destroy(gameObject);
    }
    
    public void MulTravelSpeed(float mul) { arrowSpeed *= mul; }
    public void SetDamageAdd(int add)     { damage += add; }   // damage 필드가 없다면 int damage = 10; 같은 기본값 추가
    public void SetOwnerTeam(Team t)      { ownerTeam = t; } // 이 메서드도 하나 추가 권장
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ★ 충돌이 발생했으니 자동 삭제 타이머 중지
        if (autoKillCo != null) { StopCoroutine(autoKillCo); autoKillCo = null; }

        if (!flying) return;

        if (other.CompareTag("Enemy"))
        {
            // 1) IDamageable 우선
            IDamageable dmg;
            if (other.TryGetComponent<IDamageable>(out dmg))
            {
                var info = new DamageInfo(damage, ownerTeam, "Arrow", transform.position, this);
                dmg.ApplyDamage(info);
            }
            else
            {
                // 2) IDamageable이 없다면 Health 직접 찾기(폴백)
                Health hp;
                if (other.TryGetComponent<Health>(out hp))
                {
                    var info = new DamageInfo(damage, ownerTeam, "Arrow", transform.position, this);
                    hp.ApplyDamage(info);
                }
            }

            Destroy(gameObject); // 적 맞으면 화살 파괴(연출은 취향대로)
            return;
        }
        else if (other.CompareTag("Ground"))
        {
            flying = false;
            StopAllCoroutines();        // 그 자리에서 멈춤
            if (_collider2D) _collider2D.enabled = false;
            StartCoroutine(FadeAndDie());
        }
    }
    
    IEnumerator AutoDestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // 5초 동안 아무 충돌도 없었다면 자동 삭제
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
}
