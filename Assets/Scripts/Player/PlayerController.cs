using UnityEngine;

public class PlayerController : MonoBehaviour, IPausable
{
    private bool isMoving = true;

    private void Update()
    {
        if (isMoving)
        {
            // Example movement logic
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
            transform.Translate(moveDirection * Time.deltaTime * 5f);
        }
    }

    // Pause player-related actions
    public void Pause()
    {
        isMoving = false;
        // Example: Pause animations, if applicable
        // animator.speed = 0f;
    }

    // Resume player-related actions
    public void Unpause()
    {
        isMoving = true;
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
}
