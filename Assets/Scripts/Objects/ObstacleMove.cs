using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    [SerializeField] private Vector3 puntoA; // Punto di partenza
    [SerializeField] private Vector3 puntoB; // Punto di arrivo
    [SerializeField] private float velocità = 2f; // Velocità di movimento

    private Vector3 target; // Obiettivo corrente

    private void Start()
    {
        // Inizialmente il target è il punto B
        target = puntoB;
    }

    private void Update()
    {
        // Muovi l'oggetto verso il target
        transform.position = Vector3.MoveTowards(transform.position, target, velocità * Time.deltaTime);

        // Cambia il target quando l'oggetto raggiunge il punto
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            if (target == puntoA)
            {
                target = puntoB;
            }
            else
            {
                target = puntoA;
            }
        }
    }
}
