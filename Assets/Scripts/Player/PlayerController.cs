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
    [Header("Bubble Attribute")]
    [SerializeField]private bool inBubble = false; //NON SERIALIZE
    [SerializeField]public float bubbleForce = 10f;
    [Header("Sprites")]
    [SerializeField]public Sprite spriteA; // Sprite iniziale
    [SerializeField]public Sprite spriteB; // Sprite finale

    private Rigidbody2D rb;

    Animator animator;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteA != null)
        {
            spriteRenderer.sprite = spriteA;
        }
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (!inPause)
        {   

            if (inBubble)
            {
                rb.AddForce(Vector2.up * bubbleForce, ForceMode2D.Force); // Forza costante verso l'alto
            }

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
            
        }
        else
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

    public void OnBubble()
    {
        if (spriteRenderer == null) return;

        // Cambia lo sprite in base al parametro
        if (!inBubble)
        {
            inBubble = true;
            spriteRenderer.sprite = spriteB; // Cambia a spriteB
        }
        else
        {
            inBubble = false;
            spriteRenderer.sprite = spriteA; // Torna a spriteA

            rb.velocity = Vector2.zero; 
        }
    }

    public float initialHeight = 0;
    public float forceMultiplier = 50f; // Moltiplicatore per aumentare la forza
    public float exponentialGrowth = 10f; // Crescita esponenziale della forza
    //public float maxHeight = 1f;
    private void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.gameObject.CompareTag("Air"))
        {
            // Calcola la proporzionalità in base all'altezza del giocatore
            initialHeight = transform.position.y;
            Debug.Log(initialHeight);
            
        }
    }

    private void OnTriggerStay2D(Collider2D hit)
    {
        // Controllo per il trigger "Air"
        if (hit.gameObject.CompareTag("Air"))
        {
            // Calcola la differenza di altezza rispetto all'altezza iniziale
            float currentHeight = transform.position.y;
            Debug.Log(initialHeight);
            Debug.Log(currentHeight);
            float heightDifference = currentHeight - initialHeight;

            /*if (heightDifference > maxHeight)
            {
                float force = 100 * Mathf.Pow(heightDifference, 500); // Forza aumenta vertiginosamente
                rb.AddForce(new Vector2(0, -force), ForceMode2D.Force);
            }*/


            // Aumenta la forza in base alla differenza di altezza
            if (heightDifference > 0)
            {
                float force = forceMultiplier * Mathf.Pow(heightDifference, exponentialGrowth); // Forza aumenta vertiginosamente
                rb.AddForce(new Vector2(0, -force), ForceMode2D.Force);
            }

            // Incrementa una statistica del player
            /*PlayerStats playerStats = hit.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.IncreaseStat("SomeStat", 10); // Modifica il nome della statistica e il valore da incrementare
            }*/
        }
    }

    private void OnTriggerExit2D(Collider2D hit)
    {
        if (hit.gameObject.CompareTag("Air"))
        {
            // Reset
            rb.velocity = Vector2.zero; 
            initialHeight = 0f;
        }
    }



}
