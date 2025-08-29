using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 이동
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] float arriveEpsilon = 0.3f; // 도착 판정 오차
    [SerializeField] Transform leftBound; // 플랫폼 왼쪽 끝
    [SerializeField] Transform rightBound; // 플랫폼 오른쪽 끝

    // 정지 및 공격
    [Header("Stop (Attack) Time")] [SerializeField]
    float stopTimeMin = 1f;

    [SerializeField] float stopTimeMax = 4f;

    // 스킬
    [Header("Skills During Stop")] [SerializeField]
    float skillIntervalSeconds = 4f; // 4초마다 시도

    [SerializeField] bool includeQInRandom = true; // Q도 후보에 포함할지
    [SerializeField] bool includeWInRandom = true; // W도 후보에 포함할지
    [SerializeField] bool includeEInRandom = true; // E도 후보에 포함할지
    [SerializeField] bool includeRInRandom = true; // R도 후보에 포함할지

    [Header("Shooting")] [SerializeField] private float playerAimOffsetX = -0.5f;

    // 내부 상태
    enum State
    {
        Move,
        Stop
    }

    State state = State.Move;

    private SkillCaster caster;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private Vector3 baseScale;
    private Health health;

    private float minX, maxX;
    private bool hasBounds;
    private float targetX;
    private float stopEndTime;
    private float nextSkillTime;
    private bool isDead;

    // 스킬 상태
    bool isCasting = false;
    SkillSlot queuedSkill;

    void Awake()
    {
        caster = GetComponent<SkillCaster>();
        rb = GetComponent<Rigidbody2D>(); // 없어도 OK
        anim = GetComponent<Animator>();
        baseScale = transform.localScale;
        health = GetComponent<Health>();

        if (health)
            health.OnDied += OnEnemyDied;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerTransform = p.transform;
        if (caster) caster.team = Team.Enemy;

        if (leftBound && rightBound)
        {
            minX = Mathf.Min(leftBound.position.x, rightBound.position.x);
            maxX = Mathf.Max(leftBound.position.x, rightBound.position.x);
            hasBounds = true;
        }

        PickNewTargetX();
        nextSkillTime = Time.time + skillIntervalSeconds;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (playerTransform == null)
            return;

        if (state == State.Move)
            TickMove();
        else
            TickStop();
    }

    void LateUpdate()
    {
        if (playerTransform == null)
            return;

        // 몬스터 시선 처리
        bool isStopped = (state == State.Stop) || isCasting;
        if (isStopped)
        {
            var s = baseScale;
            s.x = (playerTransform.position.x < transform.position.x) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    void TickMove()
    {
        // 목표까지 이동
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (Mathf.Abs(targetX - transform.position.x) <= arriveEpsilon)
            dir = 0f;

        SetMove(dir);

        // 도착 후
        if (dir == 0f)
        {
            state = State.Stop;
            float stopDur = Random.Range(stopTimeMin, stopTimeMax);
            stopEndTime = Time.time + stopDur;
            // 스킬은 전역 타이머(nextSkillTime)가 도달했을 때만 정지 중에 시전
        }
    }

    void TickStop()
    {
        SetMove(0f);

        if (isCasting)
            return;

        // 정지 중에만 스킬 시도
        if (Time.time >= nextSkillTime)
        {
            TryCastRandomSkillOnce();
            nextSkillTime = Time.time + skillIntervalSeconds;
        }

        // 정지 시간 종료 → 다음 목표로 이동
        if (Time.time >= stopEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
    }

    // 이동
    void SetMove(float dirX)
    {
        // 플랫폼 밖으로 못 나가게
        float nextX = transform.position.x + dirX * moveSpeed * Time.fixedDeltaTime;
        if (hasBounds)
            nextX = Mathf.Clamp(nextX, minX, maxX);

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            rb.MovePosition(new Vector2(nextX, rb.position.y));
        else
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);


        // 시선 처리
        if (!Mathf.Approximately(dirX, 0f))
        {
            var s = baseScale;
            s.x = (dirX > 0f) ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (anim)
            anim.SetFloat("isMoving", Mathf.Abs(dirX));
    }

    void PickNewTargetX()
    {
        if (hasBounds)
            targetX = Random.Range(minX, maxX);
        else
            targetX = transform.position.x;
    }

    void TryCastRandomSkillOnce()
    {
        if (!caster || !playerTransform) return;

        // Q/W/E/R 중 쓸 수 있는 놈들만
        SkillSlot[] pool = new SkillSlot[4];
        int count = 0;

        if (includeQInRandom && caster.skillQ && caster.CanCast(SkillSlot.Q))
            pool[count++] = SkillSlot.Q;
        if (includeWInRandom && caster.skillW && caster.CanCast(SkillSlot.W))
            pool[count++] = SkillSlot.W;
        if (includeEInRandom && caster.skillE && caster.CanCast(SkillSlot.E))
            pool[count++] = SkillSlot.E;
        if (includeRInRandom && caster.skillR && caster.CanCast(SkillSlot.R))
            pool[count++] = SkillSlot.R;
        if (count == 0) return; // 사용 가능 스킬 없으면 패스

        var slot = pool[Random.Range(0, count)];
        Debug.Log("몬스터 스킬 실행");

        if (slot == SkillSlot.W)
        {
            caster.TryCast(SkillSlot.W, playerTransform);

            state = State.Move;
            PickNewTargetX();
            nextSkillTime = Time.time + skillIntervalSeconds;
            return;
        }

        queuedSkill = slot;
        isCasting = true;

        if (anim)
        {
            var s = baseScale;
            switch (queuedSkill)
            {
                case SkillSlot.Q:
                    anim.SetTrigger("SkillQ");
                    s.x *= 1f;
                    transform.localScale = s;
                    break;
                case SkillSlot.E:
                    anim.SetTrigger("SkillE");
                    s.x *= 1f;
                    transform.localScale = s;
                    break;
                case SkillSlot.R:
                    anim.SetTrigger("SkillR");
                    s.x *= 1f;
                    transform.localScale = s;
                    break;
            }
        }
    }

    // 애니메이션 이벤트
    public void AE_Enemy_Shoot()
    {
        if (!caster || !playerTransform) return;
        Vector2 start = (caster.firePos != null) ? (Vector2)caster.firePos.position : (Vector2)transform.position;
        Vector2 target = (Vector2)playerTransform.position + new Vector2(playerAimOffsetX, 0f);
        caster.SpawnArrow(start, target);
    }

    public void AE_Enemy_SkillQ()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 Q");

        caster.TryCast(SkillSlot.Q, playerTransform);
    }

    public void AE_Enemy_SkillE()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 E");

        caster.TryCast(SkillSlot.E, playerTransform);
    }

    public void AE_Enemy_SkillR()
    {
        if (!caster) return;

        Debug.Log("몬스터 스킬 R");

        caster.TryCast(SkillSlot.R, playerTransform);
    }

    public void AE_Enemy_EndSkill()
    {
        Debug.Log("몬스터 스킬 끝");

        isCasting = false;

        state = State.Move;
        PickNewTargetX();

        nextSkillTime = Time.time + skillIntervalSeconds;
    }

    void OnDestroy()
    {
        if (health) health.OnDied -= OnEnemyDied;
    }

    void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    void OnEnemyDied(Health _)
    {
        if (isDead) return;
        isDead = true;

        anim?.SetTrigger("isDead");
        DisableAllColliders();

        EnemyRegistry.Unregister(this);
        if (EnemyRegistry.AliveCount == 0)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            var pAnim = playerGO ? playerGO.GetComponent<Animator>() : null;
            pAnim?.SetTrigger("isVictory");
        }
    }

    public void PlayVictory()
    {
        if (!isDead) anim?.SetTrigger("isVictory");
    }

    void DisableAllColliders()
    {
        var cols = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
    }
}