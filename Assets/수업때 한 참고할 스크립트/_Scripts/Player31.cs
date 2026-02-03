using UnityEngine;


public class Player31 : MonoBehaviour, IDamageable13
{
    // 컨트롤러
    //InputHandler inputHandler;
    MoveController13 mc;
    AnimController13 ac;
    AttackSystem13 attSystem;

    // 컴포넌트
    Rigidbody2D rb;
    Animator anim;

    // 플레이어 속성
    int hp = 100;
    float moveSpeed = 5f;

    [SerializeField] Transform attPoint;
    [SerializeField] float attRadius = 0.5f;
    [SerializeField] int attDamage = 10;
    [SerializeField] LayerMask enemyLayer;


    private void Awake()
    {
        // 컴포넌트 할당
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 컨트롤러 생성
        //inputHandler = GetComponent<InputHandler>();
        mc = new MoveController13(rb);
        ac = new AnimController13(anim);
        attSystem = new AttackSystem13(attPoint, attRadius, attDamage, enemyLayer);
    }

    #region 입력 이벤트 구독/해제
    private void OnEnable()
    {
        // 입력 이벤트 구독
       // inputHandler.OnMove += Move;
       // inputHandler.OnAttack += Attack;
    }

    private void OnDisable()
    {
        // 입력 이벤트 구독 해제
       // inputHandler.OnMove -= Move;
       //inputHandler.OnAttack -= Attack;
    }
    #endregion

    #region 이벤트 콜백
    void Move(Vector2 direction)
    {
        // 이동 및 방향 전환
        mc.Move(direction, moveSpeed);
        FlipDirectionX(direction);
        // 애니메이션 처리
        ac.PlayMove(direction != Vector2.zero);
    }

    void FlipDirectionX(Vector2 direction)
    {
        if(direction != Vector2.zero)
        {
            transform.localScale = new Vector3(direction.x < 0 ? -1 : 1, 1, 1);
        }
    }


    void Attack()
    {
        mc.Stop();
        ac.PlayAttack();
    }
    #endregion

    public void AnimEvent()
    {
        attSystem.AnimEvent_CheckCollier();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            //죽었다
            Die();
        }
        else
        {
            ac.PlayHit();
        }
    }

    void Die()
    {
        ac.PlayDie();
    }
  
    [ContextMenu("Hit Test")]
    void HitTest()
    {
        TakeDamage(20);
    }
}
