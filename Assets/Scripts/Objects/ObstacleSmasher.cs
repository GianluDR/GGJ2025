using UnityEngine;
using System.Collections;

public class ObstacleSmasher : MonoBehaviour
{
    [SerializeField] private Vector3 puntoA; // Punto di partenza
    [SerializeField] private Vector3 puntoB; // Punto di arrivo
    [SerializeField] private float velocitàNormale = 2f; // Velocità normale per la risalita
    [SerializeField] private float accelerazioneDiscesa = 9.8f; // Accelerazione in discesa
    [SerializeField] private float pausa = 1f; // Tempo di pausa in secondi

    private Vector3 target; // Obiettivo corrente
    private float velocitàCorrente; // Velocità attuale
    private bool inDiscesa; // Determina se l'oggetto sta scendendo
    private bool inMovimento; // Indica se l'oggetto si sta muovendo

    private void Start()
    {
        // Imposta il primo target e lo stato iniziale
        target = puntoB;
        velocitàCorrente = velocitàNormale;
        inDiscesa = true; // Partiamo con la discesa
        inMovimento = true; // L'oggetto inizia in movimento

        // Avvia la coroutine per il movimento
        StartCoroutine(MuoviConPausa());
    }

    private IEnumerator MuoviConPausa()
    {
        while (true) // Loop infinito per il movimento continuo
        {
            if (inMovimento)
            {
                // Se siamo in discesa, acceleriamo
                if (inDiscesa)
                {
                    velocitàCorrente += accelerazioneDiscesa * Time.deltaTime;
                }

                // Muovi l'oggetto verso il target
                transform.position = Vector3.MoveTowards(transform.position, target, velocitàCorrente * Time.deltaTime);

                // Controlla se abbiamo raggiunto il target
                if (Vector3.Distance(transform.position, target) < 0.1f)
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
