using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 컴포넌트 참조
    InputHandler inputHandler;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    int hp = 100;
    float moveSpeed = 5f;

    private void Awake()
    {
        // 컴포넌트 참조 초기화
        //inputHandler = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

    }

    #region 입력 이벤트 구독/해제
    private void OnEnable()
    {
        // 입력 이벤트 구독
        inputHandler.OnMove += Move;
        inputHandler.OnAttack += Attack;
    }

    private void OnDisable()
    {
        // 입력 이벤트 구독 해제
        inputHandler.OnMove -= Move;
        inputHandler.OnAttack -= Attack;
    }
    #endregion

    /// <summary>
    /// 이동처리
    /// </summary>
    /// <param name="direction"></param>
    void Move(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed;

        if(direction.x != 0)
        {
            sr.flipX = direction.x < 0;
        }

        anim.SetBool("IsMove", direction != Vector2.zero);
    }

    /// <summary>
    /// 공격처리
    /// </summary>
    void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack");
    }
    
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if(hp <= 0)
        {
            hp = 0;
            //죽었다
            anim.SetTrigger("Die");
        }
        else
        {
            anim.SetTrigger("Hit");
        }
    }

    [ContextMenu("Hit Test")]
    void HitTest()
    {
        TakeDamage(20);
    }
}
