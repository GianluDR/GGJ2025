using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private static List<IPausable> pausableObjects = new List<IPausable>();
    private static bool isPaused = false;

    // Registers a Pausable object
    public static void RegisterPausable(IPausable pausableObject)
    {
        if (!pausableObjects.Contains(pausableObject))
        {
            pausableObjects.Add(pausableObject);
        }
    }

    // Unregisters a Pausable object
    public static void UnregisterPausable(IPausable pausableObject)
    {
        pausableObjects.Remove(pausableObject);
    }

    // Pauses all registered objects
    public static void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            foreach (var pausable in pausableObjects)
            {
                pausable.Pause();
            }
        }
    }

    // Unpauses all registered objects
    public static void Unpause()
    {
        if (isPaused)
        {
            isPaused = false;
            foreach (var pausable in pausableObjects)
            {
                pausable.Unpause();
            }
        }
    }

    // Toggles between pause and unpause
    public static void TogglePause()
    {
        if (isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }
}
