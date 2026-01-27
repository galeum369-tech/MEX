//using UnityEngine;

//public class PlayerController : MonoBehaviour
//{
//    //컴포넌트 참조
//    InputHandler inputHandler;
//    Rigidbody2D rb;
//    Animator anim;
//    SpriteRenderer sr;

//    int hp = 100;
//    float moveSpeed = 5;

//    private void Awake()
//    {
//        //컴포넌트 참조 초기화
//        inputHandler = GetComponent<InputHandler>();
//        rb = GetComponent<Rigidbody2D>();
//        anim = GetComponent<Animator>();
//        sr = GetComponent<SpriteRenderer>();
//    }

//    #region 인에이블 디스에이블
//    private void OnEnable()
//    {
//        //입력 이벤트 구독
//        inputHandler.OnMove += Move;
//        inputHandler.OnAttack += Attack;
//    }

//    private void OnDisable()
//    {
//        inputHandler.OnMove -= Move;
//        inputHandler.OnAttack -= Attack;
//    }
//    #endregion

//    /// <summary>
//    /// 이동 처리
//    /// </summary>
//    /// <param name="direction"></param>
//    void Move(Vector2 direction)
//    {
//        rb.linearVelocity = direction * moveSpeed;

//        if (direction.x != 0)
//        {
//            sr.flipX = direction.x < 0;
//        }

//        anim.SetBool("isMove", direction != Vector2.zero);
//    }

//    /// <summary>
//    /// 공격처리
//    /// </summary>
//    void Attack()
//    {
//        rb.linearVelocity = Vector2.zero;

//        anim.SetTrigger("Attack");

//    }

//    public void TakeDamage(int damage)
//    {
//        hp -= damage;
//        if (hp <= 0)
//        {
//            hp = 0;
//            //죽음
//            anim.SetTrigger("Die");
//        }
//        else
//        {
//            anim.SetTrigger("Hit");
//        }
//    }

//    [ContextMenu("Hit Test")]
//    void HitTest()
//    {
//        TakeDamage(25);
//    }
//}
