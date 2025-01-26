using UnityEngine;
using System.Collections;

public class MovimentoConPausa : MonoBehaviour
{
    [SerializeField] private Vector3 puntoA; // Punto di partenza
    [SerializeField] private Vector3 puntoB; // Punto di arrivo
    [SerializeField] private float velocità = 2f; // Velocità di movimento
    [SerializeField] private float pausa = 1f; // Tempo di pausa in secondi

    private Vector3 target; // Obiettivo corrente

    private void Start()
    {
        // Inizialmente il target è il punto B
        target = puntoB;

        // Avvia la coroutine per il movimento con pausa
        StartCoroutine(MuoviConPausa());
    }

    private IEnumerator MuoviConPausa()
    {
        while (true) // Loop infinito per il movimento continuo
        {
            // Muovi l'oggetto verso il target
            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, velocità * Time.deltaTime);
                yield return null; // Aspetta il frame successivo
            }

            // Cambia il target quando raggiungi il punto
            if (target == puntoA)
            {
                target = puntoB;
            }
            else
            {
                target = puntoA;
            }

            // Pausa prima di riprendere il movimento
            yield return new WaitForSeconds(pausa);
        }
    }
}
