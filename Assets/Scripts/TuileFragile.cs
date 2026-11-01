using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class TuileFragile : MonoBehaviour
{
    [Header("Images dans l'ordre : propre, fissures, abîmée, très abîmée, effondrée, débris")]
    [SerializeField] private Sprite[] images;

    [Header("Réglages")]
    [Tooltip("Temps entre le premier pas du joueur et la chute (en secondes)")]
    [SerializeField] private float delaiAvantChute = 2f;
    [Tooltip("Force du tremblement avant la chute (0 = aucun)")]
    [SerializeField] private float forceTremblement = 0.015f;
    [SerializeField] private float gravite = 2.5f;
    [SerializeField] private float dureeAvantDisparition = 2.5f;

    [Header("Sons (facultatif)")]
    [Tooltip("Joué au premier pas du joueur, quand la tuile commence à se fissurer")]
    [SerializeField] private AudioClip sonFissures;
    [Tooltip("Joué quand la tuile se casse et tombe")]
    [SerializeField] private AudioClip sonChute;
    [SerializeField, Range(0f, 1f)] private float volumeSons = 0.8f;

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Rigidbody2D rb;
    private Vector3 positionDepart;
    private bool declenchee = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Tant qu'elle ne tombe pas, la tuile est immobile
        rb.bodyType = RigidbodyType2D.Kinematic;
        positionDepart = transform.position;

        if (images != null && images.Length > 0 && images[0] != null)
        {
            sr.sprite = images[0];
        }
    }

    private void OnCollisionEnter2D(Collision2D autre)
    {
        Verifier(autre);
    }

    private void OnCollisionStay2D(Collision2D autre)
    {
        Verifier(autre);
    }

    private void Verifier(Collision2D autre)
    {
        if (declenchee) return;

        // Seul le joueur la fait casser
        if (autre.collider.GetComponentInParent<JoueurController>() == null) return;

        // Seulement si le joueur est DESSUS (ses pieds sont près du haut de la tuile)
        if (autre.collider.bounds.min.y < col.bounds.max.y - 0.2f) return;

        declenchee = true;
        StartCoroutine(Casser());
    }

    private IEnumerator Casser()
    {
        SonsJeu.Jouer(sonFissures, volumeSons);

        int total = (images != null) ? images.Length : 0;

        // La dernière image est celle de la chute : les autres s'enchaînent avant
        int etapes = Mathf.Max(total - 1, 1);
        float dureeEtape = delaiAvantChute / etapes;

        int etape = 0;
        float temps = 0f;

        while (temps < delaiAvantChute)
        {
            temps += Time.deltaTime;

            int voulue = Mathf.Min(Mathf.FloorToInt(temps / dureeEtape), etapes - 1);
            if (voulue != etape)
            {
                etape = voulue;
                if (etape < total && images[etape] != null) sr.sprite = images[etape];
            }

            // La tuile tremble de plus en plus
            float force = forceTremblement * (1f + etape);
            transform.position = positionDepart + (Vector3)(Random.insideUnitCircle * force);

            yield return null;
        }

        transform.position = positionDepart;

        // Chute : dernière image, plus de collision, la gravité la fait tomber
        SonsJeu.Jouer(sonChute, volumeSons);

        if (total > 0 && images[total - 1] != null) sr.sprite = images[total - 1];

        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravite;
        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, dureeAvantDisparition);
    }
}