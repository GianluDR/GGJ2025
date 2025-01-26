using UnityEngine;

public class ObstacleEddy : MonoBehaviour
{
    [SerializeField] public float forzaSpinta = 10f; // Forza con cui la corrente spinge verso il basso

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se l'oggetto che entra nel trigger è il player
        if (other.CompareTag("Player"))
        {
            Debug.Log("TEST");
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Applica una forza verso il basso
                rb.AddForce(Vector3.down * forzaSpinta, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Mantieni la spinta verso il basso mentre il player è nel trigger
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Applica una forza continua verso il basso
                rb.AddForce(Vector3.down * forzaSpinta * Time.deltaTime, ForceMode.Force);
            }
        }
    }
}
