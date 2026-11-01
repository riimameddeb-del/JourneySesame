using UnityEngine;

// Mace qui attend en haut, tremble, tombe d'un coup, puis remonte lentement
[RequireComponent(typeof(Rigidbody2D))]
public class MaceEcrasant : MonoBehaviour
{
    [Header("Mouvement")]
    [Tooltip("Distance de la chute, en unités du jeu (de la position de départ jusqu'au sol)")]
    [SerializeField] private float distanceChute = 3f;
    [Tooltip("Vitesse de la chute : grand = brusque")]
    [SerializeField] private float vitesseChute = 25f;
    [Tooltip("Vitesse de la remontée : petit = tout doucement")]
    [SerializeField] private float vitesseRemontee = 1.2f;

    [Header("Temps (en secondes)")]
    [SerializeField] private float attenteEnHaut = 1.5f;
    [SerializeField] private float dureeTremblement = 0.8f;
    [SerializeField] private float pauseEnBas = 0.6f;
    [Tooltip("Retarde le premier départ, utile pour décaler plusieurs maces")]
    [SerializeField] private float decalageDepart = 0f;

    [Header("Tremblement")]
    [SerializeField] private float forceTremblement = 0.08f;

    private enum Phase { Attente, Tremblement, Chute, PauseBas, Remontee }

    private Phase phase = Phase.Attente;
    private Rigidbody2D rb;
    private Vector2 positionHaut;
    private Vector2 positionBas;
    private float minuteur;
    private float delaiDepart;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        positionHaut = transform.position;
        positionBas = positionHaut + Vector2.down * distanceChute;

        minuteur = attenteEnHaut;
        delaiDepart = decalageDepart;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (delaiDepart > 0f)
        {
            delaiDepart -= dt;
            return;
        }

        switch (phase)
        {
            case Phase.Attente:
                rb.MovePosition(positionHaut);
                minuteur -= dt;
                if (minuteur <= 0f)
                {
                    phase = Phase.Tremblement;
                    minuteur = dureeTremblement;
                }
                break;

            case Phase.Tremblement:
                // Le tremblement s'accentue juste avant la chute
                float avancement = 1f - Mathf.Clamp01(minuteur / Mathf.Max(dureeTremblement, 0.01f));
                float intensite = forceTremblement * (0.5f + avancement);
                Vector2 secousse = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * intensite;
                rb.MovePosition(positionHaut + secousse);

                minuteur -= dt;
                if (minuteur <= 0f) phase = Phase.Chute;
                break;

            case Phase.Chute:
                Vector2 apresChute = Vector2.MoveTowards(rb.position, positionBas, vitesseChute * dt);
                rb.MovePosition(apresChute);

                if (Vector2.Distance(apresChute, positionBas) < 0.001f)
                {
                    phase = Phase.PauseBas;
                    minuteur = pauseEnBas;
                }
                break;

            case Phase.PauseBas:
                rb.MovePosition(positionBas);
                minuteur -= dt;
                if (minuteur <= 0f) phase = Phase.Remontee;
                break;

            case Phase.Remontee:
                Vector2 apresRemontee = Vector2.MoveTowards(rb.position, positionHaut, vitesseRemontee * dt);
                rb.MovePosition(apresRemontee);

                if (Vector2.Distance(apresRemontee, positionHaut) < 0.001f)
                {
                    phase = Phase.Attente;
                    minuteur = attenteEnHaut;
                }
                break;
        }
    }

    // Dans la Scene : montre le trajet de la chute quand l'objet est sélectionné
    private void OnDrawGizmosSelected()
    {
        Vector3 haut = Application.isPlaying ? (Vector3)positionHaut : transform.position;
        Vector3 bas = haut + Vector3.down * distanceChute;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(haut, bas);
        Gizmos.DrawWireSphere(bas, 0.3f);
    }
}