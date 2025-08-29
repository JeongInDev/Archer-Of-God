using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ----- �̵�/��� -----
    [Header("Movement")] [SerializeField] float moveSpeed = 3f;
    [SerializeField] float arriveEpsilon = 0.3f; // ���� ���� ����
    [SerializeField] Transform leftBound; // �÷��� ���� ��
    [SerializeField] Transform rightBound; // �÷��� ������ ��

    // ���� ����(����) �ð� ����
    [Header("Stop (Attack) Time")]
    [SerializeField] float stopTimeMin = 1f;
    [SerializeField] float stopTimeMax = 4f;

    // ���� ��ų ��� ����
    [Header("Skills During Stop")]
    [SerializeField] float skillIntervalSeconds = 4f; // 4�ʸ��� �õ�
    [SerializeField] bool includeQInRandom = true;    // Q�� �ĺ��� ��������
    [SerializeField] bool includeWInRandom = true;    // Q�� �ĺ��� ��������
    [SerializeField] bool includeEInRandom = true;    // Q�� �ĺ��� ��������
    [SerializeField] bool includeRInRandom = true;    // Q�� �ĺ��� ��������
    
    [Header("Shooting")]
    [SerializeField] private float playerAimOffsetX = -0.5f;
    
    // ���� ����
    enum State { Move, Stop }
    State state = State.Move;

    private SkillCaster caster;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform playerTransform;
    private Vector3 baseScale;

    private float minX, maxX;
    private bool hasBounds;
    private float targetX;              // �̵� ��ǥ X
    private float stopEndTime;        // ���� ���� �ð�
    private float nextSkillTime;      // ���� ��ų �õ� �ð�

    // ���� ��ų ĳ���� ����(�߰�) ����
    bool isCasting = false;        // ��ų ���� ���̸� �̵�/��Ÿ/���� ���� ��� ����
    SkillSlot queuedSkill;              // �̹��� �� ��ų ����
    
    void Awake()
    {
        caster = GetComponent<SkillCaster>();
        rb = GetComponent<Rigidbody2D>(); // ��� OK
        anim = GetComponent<Animator>();
        baseScale = transform.localScale;
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

        // �÷��̾� ���� �ٶ󺸱� (�⺻ ��������Ʈ�� '����' ����)
        var s = baseScale;
        s.x = (playerTransform.position.x < transform.position.x) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        transform.localScale = s;
    }

    // ====== ���� ó�� ======
    void TickMove()
    {
        // ��ǥ���� �̵�
        float dir = Mathf.Sign(targetX - transform.position.x);
        if (Mathf.Abs(targetX - transform.position.x) <= arriveEpsilon) 
            dir = 0f;

        SetMove(dir);

        // ���� �� ����(����) Ÿ�� ����
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
        // ���� �� �ֱ�(�ִϸ����Ͱ� isMoving=0�̸� �ڵ����� ���� �ִϰ� ������ ����)
        SetMove(0f);

        // �� ��ų ĳ���� ���̸� �ƹ� �͵� ���� ����(���� Ÿ�ӵ� ����)
        if (isCasting)
            return;
        
        // ���� �߿��� ��ų �õ�: 4�� ����
        if (Time.time >= nextSkillTime)
        {
            TryCastRandomSkillOnce();                // ������ ��ų 1�� �õ�
            nextSkillTime = Time.time + skillIntervalSeconds; // ���� �õ� ����
        }

        // ���� �ð� ���� �� ���� ��ǥ�� �̵�
        if (Time.time >= stopEndTime)
        {
            state = State.Move;
            PickNewTargetX();
        }
    }

    // ====== �̵�/��ƿ ======
    void SetMove(float dirX)
    {
        // ��� ������ �� ������ Clamp
        float nextX = transform.position.x + dirX * moveSpeed * Time.fixedDeltaTime;
        if (hasBounds)
            nextX = Mathf.Clamp(nextX, minX, maxX);

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
            rb.MovePosition(new Vector2(nextX, rb.position.y));
        else
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

        if (anim)
            anim.SetFloat("isMoving", Mathf.Abs(dirX));
    }

    void PickNewTargetX()
    {
        if (hasBounds)
            targetX = Random.Range(minX, maxX);
        else
            targetX = transform.position.x; // ��谡 ������ ���ڸ�(����)
    }
    
    void TryCastRandomSkillOnce()
    {
        if (!caster || !playerTransform) return;

        // Q/W/E/R �� '��ٿ� ������' �ĺ��� ������
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
        if (count == 0) return;     // ��� ���� ��ų ������ �н�

        var slot = pool[Random.Range(0, count)];
        Debug.Log("���� ��ų ����");
        
        if (slot == SkillSlot.W)
        {
            // Ÿ�� �ʿ� ������ null�� �ֵ� ��. (���� ������ playerTransform �����ص� ����)
            caster.TryCast(SkillSlot.W, playerTransform);

            // ��� �̵� ���� + ���� ��ų Ÿ�̸� ����
            state = State.Move;
            PickNewTargetX();
            nextSkillTime = Time.time + skillIntervalSeconds;
            return;
        }
        
        queuedSkill = slot;
        isCasting = true;
        
        if (anim)
        {
            switch (queuedSkill)
            {
                case SkillSlot.Q: 
                    anim.SetTrigger("SkillQ"); break;
                case SkillSlot.E: 
                    anim.SetTrigger("SkillE"); break;
                case SkillSlot.R: 
                    anim.SetTrigger("SkillR"); break;
            }
        }
    }

    // ====== �ִϸ��̼� �̺�Ʈ ======
    public void AE_Enemy_Shoot()
    {
        if (!caster || !playerTransform) return;
        Vector2 start  = (caster.firePos != null) ? (Vector2)caster.firePos.position : (Vector2)transform.position;
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
    
    // ��ų/���� �ִ� ��(��ų �ִ��� ������ ������ �̺�Ʈ)
    public void AE_Enemy_EndSkill()
    {
        Debug.Log("���� ��ų ��");

        isCasting = false;
        
        state = State.Move;
        PickNewTargetX();
        
        nextSkillTime = Time.time + skillIntervalSeconds;
    }
}