using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool faceMoveDirection;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    public float currentspeed;
    public Animator animator;
    public ParticleSystem spit;
    public ParticleSystem spam;
    public ParticleSystem forwards;
    public ParticleSystem forwardspread;
    private GameUI gameUI;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        gameUI = FindAnyObjectByType<GameUI>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            animator.SetTrigger("Attack");
            StartCoroutine(Attack(4f));
        }
    }

    void FixedUpdate()
    {   
        currentspeed = moveInput.y + moveInput.x;
        animator.SetFloat("speed", currentspeed);
        animator.SetBool("Grounded", isGrounded);
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveInput.y + right * moveInput.x;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = movement.x * moveSpeed;
        velocity.z = movement.z * moveSpeed;
        animator.SetFloat("speed", movement.magnitude);
        
        rb.linearVelocity = velocity;

        if (faceMoveDirection && movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement,Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            
        }
    }

    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
        animator.SetBool("isGrounded", isGrounded);
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    public IEnumerator Attack(float delay)
    {
        faceMoveDirection = false;
        animator.SetTrigger("Attack");
        moveInput = Vector2.zero;
        moveSpeed = 0f;
        yield return new WaitForSeconds(2f);
        spit.Play();
        spam.Play();
        forwards.Play();
        forwardspread.Play();
        yield return new WaitForSeconds(1.6f);
        moveSpeed = 20f;
        forwardspread.Stop();
        forwards.Stop();
        spit.Stop();
        spam.Stop();
        faceMoveDirection = true;

    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Coin coin = collision.gameObject.GetComponent<Coin>();

            if (coin != null)
            {
                gameUI.AddScore(coin.GetScoreValue());
                Destroy(coin.gameObject);
            }
        }
    }
}
