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
    [SerializeField]public float swimmingForce = 2f; // Forza di accelerazione per il nuoto
    [SerializeField]public float maxSwimmingSpeed = 5f; // Velocità massima mentre nuota
    [SerializeField]private bool inAir = false; //NON SERIALIZE
    [Header("Bubble Attribute")]
    [SerializeField]private int bubbleState = 0; //NON SERIALIZE -1 no bub 0 min bub 1 max bub
    [SerializeField]public float bubbleForce = 10f;
    public float lastImpulseTime = 0f; // Tempo dell'ultimo impulso
    public float impulseInterval = 0.5f; // Intervallo tra ogni scatto (in secondi)
    [Header("Sprites")]
    [SerializeField]public Sprite spriteNOB; // Sprite no bolla
    [SerializeField]public Sprite spriteMINB; //Sprite bolla piccola
    [SerializeField]public Sprite spriteMAXB; //Sprite bolla grande
    private Rigidbody2D rb;
    [SerializeField] private InputAction BubbleUP;
    [SerializeField] private InputAction BubbleDOWN;
    Animator animator;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private ContactFilter2D movementFilter;
    private SpriteRenderer spriteRenderer;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteNOB != null)
        {
            spriteRenderer.sprite = spriteNOB;
        }
    }

    private void Update()
    {

    }
  

    private void FixedUpdate()
    {
        if (!inPause)
        {   
            Vector2 direction = new Vector2(movementInput.x, movementInput.y);
            
            if (inBubble)
            {
                rb.AddForce(Vector2.up * bubbleForce, ForceMode2D.Force); // Forza costante verso l'alto
            }

            if (inAir)
            {
                if(direction != Vector2.zero){
                    if(Time.time - lastImpulseTime > impulseInterval)
                    {
                        lastImpulseTime = 0f;
                        Swim();
                    }
                }
            }
            else
            {
                if(direction != Vector2.zero){
                    /*bool success = TryMove(direction);

                    if(!success){
                        success = TryMove(new Vector2(direction.x, 0));
                    }
                    if(!success){
                        success = TryMove(new Vector2(0, direction.y));
                    }*/
                    animator.SetBool("IsMoving", /*success*/true);
                }
                else {
                    animator.SetBool("IsMoving", false);
                }
                
                TryMove(new Vector2(direction.x,direction.y));
            }
            
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

        BubbleUP.Enable();
        BubbleUP.started+= OnBubbleUP;
        BubbleDOWN.Enable();
        BubbleDOWN.started+= OnBubbleDOWN;
    }

    private void OnDisable()
    {
        PauseManager.UnregisterPausable(this);

        BubbleUP.started-= OnBubbleUP;
        BubbleUP.Disable();
        BubbleDOWN.started-= OnBubbleDOWN;
        BubbleDOWN.Disable();
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

    void Swim()
    {
        // Imposta la velocità finale (mantieni la velocità verticale invariata)
        rb.velocity = new Vector2(0f, 0f);
        rb.AddForce(new Vector2(movementInput.x * swimmingForce, 0), ForceMode2D.Impulse);
        if(movementInput.y > 0)
            rb.AddForce(new Vector2(0, movementInput.y * swimmingForce * 0.35f), ForceMode2D.Impulse);
        else
            rb.AddForce(new Vector2(0, movementInput.y * swimmingForce * 1.15f), ForceMode2D.Impulse);
            
        lastImpulseTime = Time.time;
    }

    public void BubbleState()
    {
        // Cambia lo sprite in base al parametro
        if (bubbleState<0)//NO BOLLA
        {
            spriteRenderer.sprite = spriteNOB;
        }
        else if(bubbleState==0)//BOLLA PICCOLA
        {
            spriteRenderer.sprite = spriteMINB;
            rb.velocity = Vector2.zero; 
        }
        else if(bubbleState>0)//BOLLA GRANDE
        {
            spriteRenderer.sprite = spriteMAXB;
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
            
        }
    }

    private void OnTriggerStay2D(Collider2D hit)
    {
        // Controllo per il trigger "Air"
        if (hit.gameObject.CompareTag("Air"))
        {
            // Calcola la differenza di altezza rispetto all'altezza iniziale
            float currentHeight = transform.position.y;
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
     
    public void OnBubbleUP(InputAction.CallbackContext context)
    {
        if (context.started){
            if (bubbleState < 1)
            {
                bubbleState++;
                BubbleState();
            }
        }
    } 
    

    void OnBubbleDOWN(InputAction.CallbackContext context)
    {
        if (context.started){
            if (bubbleState > -1)
                {
                    bubbleState--;
                    BubbleState();
                }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            inAir = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            inAir = true;
        }  
    }

}
