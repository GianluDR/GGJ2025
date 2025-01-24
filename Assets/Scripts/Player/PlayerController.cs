using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, IPausable
{
    

    [Header("Player Gameplay Attribute")]
    [SerializeField]private bool inPause = false; //NON SERIALIZE
    [SerializeField]private float speed = 5f; //NON SERIALIZE
    [SerializeField]private Vector2 movementInput; //NON SERIALIZE
    [SerializeField]private float collisionOffset = 0.005f; //NON SERIALIZE
    [SerializeField]private float moveSpeed = 5f; //NON SERIALIZE

    private Rigidbody2D rb;

    Animator animator;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
    }

    private void Update()
    {
        
    }

   private void FixedUpdate()
    {
        if (!inPause)
        {   
            Vector2 direction = new Vector2(movementInput.x, movementInput.y);
            
            if(direction != Vector2.zero){
                bool success = TryMove(direction);

                if(!success){
                    success = TryMove(new Vector2(direction.x, 0));
                }
                if(!success){
                    success = TryMove(new Vector2(0, direction.y));
                }
                animator.SetBool("IsMoving", success);
            }

            else {
                animator.SetBool("IsMoving", false);
            }

            TryMove(direction);
            
        }
    }

    // Pause player-related actions
    public void Pause()
    {
        inPause = true;
        // Example: Pause animations, if applicable
        // animator.speed = 0f;
    }

    // Resume player-related actions
    public void Unpause()
    {
        inPause = false;
        // Example: Resume animations
        // animator.speed = 1f;
    }

    private void OnEnable()
    {
        PauseManager.RegisterPausable(this);
    }

    private void OnDisable()
    {
        PauseManager.UnregisterPausable(this);
    }

    private bool TryMove(Vector2 dir)
    {
        bool tryM = false;

        if (dir != Vector2.zero)
        {
            int count = rb.Cast(
                    dir,
                    movementFilter,
                    castCollisions,
                    (moveSpeed
                        * Time.fixedDeltaTime)
                        + collisionOffset);

            if (count == 0)
            {
                rb.MovePosition(
                    rb.position
                    + dir
                    * moveSpeed
                    * Time.fixedDeltaTime);
                tryM = true;
                    
            } 
            else
            {
                tryM = false;
            }
            
        }else
            {
                tryM = false;
            }
        return tryM;
    }

    // Called by Unity Event
    public void OnMove(InputAction.CallbackContext context)
    {
        // read only if started or in process
        if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started)
        {
            movementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            movementInput = Vector2.zero;
        }
    }
}
