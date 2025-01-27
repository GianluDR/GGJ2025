using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    [SerializeField]public float jumpForceX = 3f;
    [SerializeField]public float jumpForce = 3f;
    [SerializeField]private bool inAir = false; //NON SERIALIZE
    [Header("Survival Attribute")]
    [SerializeField]private float maxOxygen;
    [SerializeField]private float oxygen;
    [SerializeField]private float oxygenOT = 2f;
    [SerializeField]private float oxygenRechargeOT = 5f;
    [SerializeField]public Slider oxygenSlider;
    //public Slider hungerSlider;
    [Header("Bubble Attribute")]
    [SerializeField]private int bubbleState = 0; //NON SERIALIZE -1 vuln; 0 schien bub; 1 min bub; 2 max bub
    [SerializeField]public float bubbleForce = 0f;
    public float lastImpulseTime = 0f; // Tempo dell'ultimo impulso
    public float impulseInterval = 0.5f; // Intervallo tra ogni scatto (in secondi)
    [Header("Death")]
    [SerializeField] private float deathTime = 3f; // Tempo dopo il quale il giocatore muore
    [SerializeField] public Vector3 StartPos;
    private float deathTimer = 0f; // Timer per il conteggio del tempo
    [Header("Bubble")]
    [SerializeField]public SpriteRenderer bubbleRenderer; // Sprite vulnerabile
    [SerializeField]private Animator bubbleAnimator;
    [SerializeField]public Transform bubblePos; // Sprite vulnerabile
    [SerializeField]public Sprite spriteVUL; // Sprite vulnerabile
    [SerializeField]public Sprite spriteSCB; // Sprite bollaschiena
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
        FindObjectOfType<AudioManager>().Play("Background_Music#2");
        BubbleState();
        FindObjectOfType<AudioManager>().StopPlaying("Background_Music#1");
    }

    

    private void Update()
    {
        if (!inPause)
        {
            oxygen = oxygen - oxygenOT * Time.deltaTime;
            oxygenSlider.value = 1 - oxygen / maxOxygen;

            if(oxygen <= 0){
                bubbleState = -1;
                BubbleState();

                // Incrementa il timer per la morte
                deathTimer += Time.deltaTime;

                if (deathTimer >= deathTime)
                {
                    // Dopo X secondi di ossigeno a 0, il giocatore muore
                    oxygen = maxOxygen;
                    transform.position = StartPos;
                }
            }
            else
            {
                deathTimer = 0f;
            }


            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 is the default layer index
            if (stateInfo.IsName("player_idle"))
            {
                if(bubbleState == 0)
                    bubbleRenderer.enabled = false;
            }
            else
            {
                if(bubbleState == 0)
                    bubbleRenderer.enabled = true;
            }
        }
    }
  

    private void FixedUpdate()
    {
        if (!inPause)
        {  
            if (movementInput.x < 0)
            {
                spriteRenderer.flipX = true; // Flip the sprite to face left
                bubbleRenderer.flipX = true; // Flip the sprite to face left
                if(bubbleState == 0)
                    bubblePos.localPosition = new Vector2(0.22f,0.19f);
            }
            else if (movementInput.x > 0)
            {
                spriteRenderer.flipX = false; // Keep the sprite facing right
                bubbleRenderer.flipX = false; // Flip the sprite to face left
                if(bubbleState == 0)
                    bubblePos.localPosition = new Vector2(-0.22f,0.19f);
            }

            Vector2 direction = new Vector2(movementInput.x, movementInput.y);
            
            rb.AddForce(Vector2.up * bubbleForce, ForceMode2D.Force);
            

            if (inAir)
            {
                if(direction != Vector2.zero){
                    if(Time.time - lastImpulseTime > impulseInterval)
                    {
                        lastImpulseTime = 0f;
                        animator.SetBool("InAir", inAir);
                        animator.SetBool("IsMoving", true);
                        Swim();
                    }
                }
                else
                {
                    animator.SetBool("IsMoving", false);
                }
            }
            else
            {
                if(direction != Vector2.zero){
                    bool success = TryMove(direction);

                    if(!success){
                        success = TryMove(new Vector2(direction.x, 0));
                    }
                    if(!success){
                        success = TryMove(new Vector2(0, direction.y));
                    }
                    animator.SetBool("IsMoving", success);
                    FindObjectOfType<AudioManager>().waitPlaying("Walk");
                }
                else {
                    animator.SetBool("IsMoving", false);
                }
                
                TryMove(direction);
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

    /*private bool TryMove(Vector2 dir)
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
    }*/

    private bool TryMove(Vector2 dir)
    {
        bool tryM = false;

        if (dir != Vector2.zero)
        {
            // Controlla le collisioni
            int count = rb.Cast(
                dir,
                movementFilter,
                castCollisions,
                (moveSpeed * Time.fixedDeltaTime) + collisionOffset
            );

            if (count == 0)
            {
                /*rb.MovePosition(
                    rb.position
                    + dir
                    * moveSpeed
                    * Time.fixedDeltaTime);*/

                // Calcola la forza da applicare
                Vector2 force = dir.normalized * moveSpeed;

                // Applica la forza al rigidbody
                rb.AddForce(force, ForceMode2D.Force);

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

        movementInput = context.ReadValue<Vector2>();

    }

    void Swim()
    {
        //Debug.Log(rb.velocity);

        // Imposta la velocità finale (mantieni la velocità verticale invariata)
        //rb.velocity = new Vector2(0f, 0f);
        ////rb.AddForce(new Vector2(, 0), ForceMode2D.Impulse);
        FindObjectOfType<AudioManager>().waitPlaying("Jump_Swim");
        if(movementInput.y > 0){
            rb.AddForce(new Vector2(movementInput.x * swimmingForce, movementInput.y * swimmingForce * 0.35f), ForceMode2D.Impulse);
        }else{
            rb.AddForce(new Vector2(movementInput.x * swimmingForce, movementInput.y * swimmingForce * 1.15f), ForceMode2D.Impulse);
        }

        lastImpulseTime = Time.time;
    }

        [Header("Walking Speed State")]
    [SerializeField]private float walkingSpeedVUL = 5f;
    [SerializeField]private float walkingSpeedSCB = 5f;
    [SerializeField]private float walkingSpeedMINB = 5f;
    [SerializeField]private float walkingSpeedMAXB = 5f;

    [Header("Swimming Speed State")]
    [SerializeField]private float swimmingForceVUL;
    [SerializeField]private float swimmingForceSCB;
    [SerializeField]private float swimmingForceMINB;
    [SerializeField]private float swimmingForceMAXB;
    public void BubbleState()
    {
        if (bubbleState==-1)//VULNERABILE
        {
            bubbleRenderer.sprite = spriteVUL;
            bubbleRenderer.enabled = false;

            animator.SetBool("NoOxygen", true);
            bubbleForce=(-4f);
            swimmingForce = swimmingForceVUL;
            moveSpeed = walkingSpeedVUL;
            oxygenOT=2f;
        }

        if (bubbleState==0)//BOLLA SCHIENA
        {
            bubblePos.localPosition = new Vector3(-0.22f,0.19f,0f);
            //bubblePos.localScale = new Vector3(1f,1f,1f);
            
            bubbleRenderer.sprite = spriteSCB;
            swimmingForce = swimmingForceSCB;
            moveSpeed = walkingSpeedSCB;
            bubbleForce=(-3f);
            animator.SetBool("NoOxygen", false);
            bubbleAnimator.SetBool("isOnBack", true);
            bubbleAnimator.SetBool("isSmall", false);
            bubbleAnimator.SetBool("isBig", false);
            //VELOCITA LATERALE A TERRA AUMENTATA
            oxygenOT=1.5f;
        }
        else if(bubbleState==1)//BOLLA PICCOLA
        {
            bubblePos.localPosition = new Vector3(0f,0f,0f);
            //bubblePos.localScale = new Vector3(1f,1f,1f);
            bubbleRenderer.enabled = true;
            bubbleRenderer.sprite = spriteMINB;
            swimmingForce = swimmingForceMINB;
            moveSpeed = walkingSpeedMINB;
            bubbleForce=(-2f);
            animator.SetBool("NoOxygen", true);
            bubbleAnimator.SetBool("isOnBack", false);
            bubbleAnimator.SetBool("isSmall", true);
            bubbleAnimator.SetBool("isBig", false);
            //VELOCITA LATERALE A TERRA DIMINUITO
            oxygenOT=1f;
        }
        else if(bubbleState==2)//BOLLA GRANDE
        {
            bubblePos.localPosition = new Vector3(0f,0f,0f);
            //bubblePos.localScale = new Vector3(1.5f,1.5f,1.5f);
            bubbleRenderer.enabled = true;
            bubbleRenderer.sprite = spriteMAXB;
            swimmingForce = swimmingForceMAXB;
            moveSpeed = walkingSpeedMAXB;
            bubbleForce=(5f);
            animator.SetBool("NoOxygen", true);
            bubbleAnimator.SetBool("isOnBack", false);
            bubbleAnimator.SetBool("isSmall", false);
            bubbleAnimator.SetBool("isBig", true);
            //VELOCITA LATERALE A TERRA DIMINUITO
            oxygenOT=1f;
            
        }
    }

    private IEnumerator ChangeColorTemporarily()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer non assegnato!");
            yield break;
        }

        // Cambia il colore a rosso
        spriteRenderer.color = Color.red;

        // Aspetta per mezzo secondo
        yield return new WaitForSeconds(0.5f);

        // Cambia il colore a bianco
        spriteRenderer.color = Color.white;
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
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if(bubbleState>-1&&bubbleState<3)
               BubbleDown();
            else if(bubbleState==-1);//MORTE PERCHè COLPITO DA VULNERABILE
            StartCoroutine(ChangeColorTemporarily());
        }
        if (hit.gameObject.CompareTag("Chele"))
        {
            ObstacleChele obstacle = hit.GetComponent<ObstacleChele>();
            if (obstacle != null)
            {
                if (obstacle != null)
                {
                    if (obstacle.movimentoCoroutine != null)
                    {
                        obstacle.StopCoroutine(obstacle.movimentoCoroutine);
                    }

                    obstacle.movimentoCoroutine = obstacle.StartCoroutine(obstacle.MuoviChildConOscillazione());
                }
            }
        }
        if (hit.gameObject.CompareTag("END"))
        {
            SceneManager.LoadScene("Credits");
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

            oxygen = oxygen + oxygenRechargeOT * Time.deltaTime;
            oxygenSlider.value = oxygen / maxOxygen;

            if(oxygen > 100){
                oxygen = 100;
            }

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
        if (hit.gameObject.CompareTag("Eddy"))
        {
            ObstacleEddy obstacle = hit.GetComponent<ObstacleEddy>();
            if (obstacle != null)
            {
                if (rb != null)
                {
                    Debug.Log(obstacle.transform.localRotation.eulerAngles.z);
                    // Applica la forza verso il basso
                    if(obstacle.transform.localRotation.eulerAngles.z == 90)
                        rb.AddForce(Vector2.up * obstacle.forzaSpinta, ForceMode2D.Impulse);
                    else if(obstacle.transform.localRotation.eulerAngles.z == 180)
                        rb.AddForce(Vector2.left * obstacle.forzaSpinta, ForceMode2D.Impulse);
                    else if(obstacle.transform.localRotation.eulerAngles.z == 270)
                        rb.AddForce(Vector2.down * obstacle.forzaSpinta, ForceMode2D.Impulse);
                    

                }
            }
        }
        if (hit.gameObject.CompareTag("END"))
        {
            SceneManager.LoadScene("Menu");
        }
        
    }

    private void OnTriggerExit2D(Collider2D hit)
    {
        if (hit.gameObject.CompareTag("Air"))
        {
            // Reset
            //rb.velocity = Vector2.zero; 
            initialHeight = 0f;
        }
        if (hit.gameObject.CompareTag("Chele"))
        {
            ObstacleChele obstacle = hit.GetComponent<ObstacleChele>();
            if (obstacle != null)
            {
                if (obstacle != null)
                {
                    // Ferma il movimento e ripristina la posizione iniziale
                    if (obstacle.movimentoCoroutine != null) obstacle.StopCoroutine(obstacle.movimentoCoroutine);
                    obstacle.childObject.localPosition = obstacle.posizioneIniziale;
                }
            }
        }
    }
     
    [Header("Oxygen")]
    public float oxygenToSCBFromZero;
    public float oxygenToSCBFromMax;
    public float oxygenToMAXB;
    public void OnBubbleUP(InputAction.CallbackContext context)
    {
        if (context.started){
            if (bubbleState < 2)
            {
                if(bubbleState==1)
                {
                    //TOGLIERE OSSIGENO PER DA PICCOLA A GRANDE
                    changeOxygen(oxygenToMAXB);
                }
                if(bubbleState==-1)
                {
                    //TIGLIERE OSSIGENO PER DA SENZA A RICREARE LA BOLLA
                    changeOxygen(oxygenToSCBFromZero);
                }
                bubbleState++;
                FindObjectOfType<AudioManager>().Play("Bubble_+");
                BubbleState();
            }
        }
    } 

    void OnBubbleDOWN(InputAction.CallbackContext context)
    {
        if (context.started){
            if (bubbleState > 0)
            {
                bubbleState--;
                if(bubbleState==1)
                {
                    //REFUND OSSIGENO DA BOLLA GRANDE A PICCOLA
                    changeOxygen(oxygenToSCBFromMax);
                }
                FindObjectOfType<AudioManager>().Play("Bubble_-");
                BubbleState();
            }
        }
    }

    private void changeOxygen(float n)
    {
        if(oxygen<5)
        {
            //esplosione
        }
        oxygen = oxygen - n;
    }

    public void OnJump()
    {
        if (!inAir)
        {
            //rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            rb.AddForce(new Vector2(movementInput.x * jumpForceX, jumpForce), ForceMode2D.Impulse);
            //rb.velocity = new Vector2(rb.velocity.x, 20);
        }        
    }
    private float airTimeBuffer = 0.08f; // Tempo in secondi prima di considerare "in aria"
    private float lastGroundedTime;
    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            //lastGroundedTime = Time.time;
            if(inAir)
            { 
                inAir = false;
                animator.SetBool("InAir", inAir);
            }          
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            inAir = true;
            // Inizia a considerare il player in aria, ma verifica nel buffer
            //StartCoroutine(CheckInAir());
        }
    }

    private IEnumerator CheckInAir()
    {
        yield return new WaitForSeconds(airTimeBuffer);

        // Se non è tornato a contatto con il terreno dopo il buffer
        if (Time.time - lastGroundedTime >= airTimeBuffer)
        {
            inAir = true;
            animator.SetBool("InAir", inAir);
        }
    }

    public void BubbleDown()
    {
        FindObjectOfType<AudioManager>().Play("BubblePop");
        bubbleState = -1;
        BubbleState();
        FindObjectOfType<AudioManager>().Play("Hit");
    }

}
