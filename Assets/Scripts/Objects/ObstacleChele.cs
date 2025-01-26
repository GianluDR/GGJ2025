using UnityEngine;
using System.Collections;

public class ObstacleChele : MonoBehaviour
{
    [SerializeField] public Transform childObject; // Il child GameObject da spostare
    [SerializeField] private Vector3 puntoA; // Primo punto designato
    [SerializeField] private Vector3 puntoB; // Secondo punto designato
    [SerializeField] private float velocitàNormale = 2f; // Velocità normale per la risalita
    [SerializeField] private float accelerazioneDiscesa = 9.8f; // Accelerazione in discesa
    [SerializeField] private float pausa = 1f; // Tempo di pausa in secondi

    private Vector3 target; // Obiettivo corrente
    private float velocitàCorrente; // Velocità attuale
    private bool inDiscesa; // Determina se il movimento è in discesa
    private bool inMovimento; // Indica se il movimento è attivo
    public Vector3 posizioneIniziale; // Posizione iniziale del child
    public Coroutine movimentoCoroutine; // La coroutine del movimento

    private void Start()
    {
        if (childObject == null)
        {
            Debug.LogError("ChildObject non assegnato!");
            enabled = false;
            return;
        }

        // Inizializza le variabili
        posizioneIniziale = childObject.localPosition;
        target = puntoB;
        velocitàCorrente = velocitàNormale;
        inDiscesa = true;
        inMovimento = false; // Parte in attesa di trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ferma il movimento e ripristina la posizione iniziale
            if (movimentoCoroutine != null) StopCoroutine(movimentoCoroutine);
            childObject.localPosition = posizioneIniziale;
        }
    }

    public IEnumerator MuoviChildConOscillazione()
    {
        inMovimento = true;

        while (true) // Loop infinito per l'oscillazione continua
        {
            if (inMovimento)
            {
                // Se siamo in discesa, acceleriamo
                if (inDiscesa)
                {
                    velocitàCorrente += accelerazioneDiscesa * Time.deltaTime;
                }

                // Muovi il child verso il target
                childObject.localPosition = Vector3.MoveTowards(childObject.localPosition, target, velocitàCorrente * Time.deltaTime);

                // Controlla se abbiamo raggiunto il target
                if (Vector3.Distance(childObject.localPosition, target) < 0.1f)
                {
                    inMovimento = false; // Ferma il movimento per la pausa

                    // Cambia il target e imposta velocità
                    if (target == puntoB)
                    {
                        target = puntoA;
                        velocitàCorrente = velocitàNormale;
                        inDiscesa = false; // Ora stiamo risalendo
                    }
                    else
                    {
                        target = puntoB;
                        inDiscesa = true; // Ora stiamo scendendo
                    }

                    // Pausa
                    yield return new WaitForSeconds(pausa);
                    inMovimento = true; // Riprendi il movimento
                }
            }

            yield return null; // Aspetta il frame successivo
        }
    }
}
