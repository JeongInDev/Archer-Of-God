using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // �̵�
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] float arriveEpsilon = 0.3f; // ���� ���� ����
    [SerializeField] Transform leftBound; // �÷��� ���� ��
    [SerializeField] Transform rightBound; // �÷��� ������ ��

    // ���� �� ����
    [Header("Stop (Attack) Time")] [SerializeField]
    float stopTimeMin = 1f;

    [SerializeField] float stopTimeMax = 4f;

    // ��ų
    [Header("Skills During Stop")] [SerializeField]
    float skillIntervalSeconds = 4f; // 4�ʸ��� �õ�

    [SerializeField] bool includeQInRandom = true; // Q�� �ĺ��� ��������
    [SerializeField] bool includeWInRandom = true; // W�� �ĺ��� ��������
    [SerializeField] bool includeEInRandom = true; // E�� �ĺ��� ��������
    [SerializeField] bool includeRInRandom = true; // R�� �ĺ��� ��������

    [Header("Shooting")] [SerializeField] private float playerAimOffsetX = -0.5f;

    // ���� ����
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

    // ��ų ����
    bool isCasting = false;
    SkillSlot queuedSkill;

    void Awake()
    {
        caster = GetComponent<SkillCaster>();
        rb = GetComponent<Rigidbody2D>(); // ��� OK
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

        // ���� �ü� ó��
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
        // ��ǥ���� �̵�
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (Mathf.Abs(targetX - transform.position.x) <= arriveEpsilon)
            dir = 0f;

        SetMove(dir);

        // ���� ��
        if (dir == 0f)
        {
            state = State.Stop;
            float stopDur = Random.Range(stopTimeMin, stopTimeMax);
            stopEndTime = Time.time + stopDur;
            // ��ų�� ���� Ÿ�̸�(nextSkillTime)�� �������� ���� ���� �߿� ����
        }
    }

    void TickStop()
    {
        SetMove(0f);

        if (isCasting)
            return;

        // ���� �߿��� ��ų �õ�
        if (Time.time >= nextSkillTime)
        {
            TryCastRandomSkillOnce();
            nextSkillTime = Time.time + skillIntervalSeconds;
        }

        // ���� �ð� ���� �� ���� ��ǥ�� �̵�
        if (Time.time >= stopEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
    }

    // �̵�
    void SetMove(float dirX)
    {
        // �÷��� ������ �� ������
        float nextX = transform.position.x + dirX * moveSpeed * Time.fixedDeltaTime;
        if (hasBounds)
            nextX = Mathf.Clamp(nextX, minX, maxX);

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            rb.MovePosition(new Vector2(nextX, rb.position.y));
        else
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);


        // �ü� ó��
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

        // Q/W/E/R �� �� �� �ִ� ��鸸
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
        if (count == 0) return; // ��� ���� ��ų ������ �н�

        var slot = pool[Random.Range(0, count)];
        Debug.Log("���� ��ų ����");

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

    // �ִϸ��̼� �̺�Ʈ
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

        Debug.Log("���� ��ų Q");

        caster.TryCast(SkillSlot.Q, playerTransform);
    }

    public void AE_Enemy_SkillE()
    {
        if (!caster) return;

        Debug.Log("���� ��ų E");

        caster.TryCast(SkillSlot.E, playerTransform);
    }

    public void AE_Enemy_SkillR()
    {
        if (!caster) return;

        Debug.Log("���� ��ų R");

        caster.TryCast(SkillSlot.R, playerTransform);
    }

    public void AE_Enemy_EndSkill()
    {
        Debug.Log("���� ��ų ��");

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