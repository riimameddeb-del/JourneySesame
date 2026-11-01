using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

// Ennemi qui patrouille de gauche à droite. Le joueur le tue avec la touche F.
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Ennemi : MonoBehaviour
{
    [Header("Vie")]
    [SerializeField] private int vie = 1;

    [Header("Patrouille")]
    [Tooltip("Distance parcourue de chaque côté du point de départ (en unités du jeu)")]
    [SerializeField] private float distancePatrouille = 2f;
    [SerializeField] private float vitesse = 1.5f;

    [Header("Direction")]
    [Tooltip("Coche si le dessin regarde vers la droite quand il est posé tel quel")]
    [SerializeField] private bool regardeADroiteParDefaut = true;
    [Tooltip("Seulement si l'ennemi est fait de plusieurs parties : glisse ici l'objet à retourner. Sinon laisse vide.")]
    [SerializeField] private Transform visuel;

    [Header("Animation par images (facultatif, pour un simple sprite)")]
    [SerializeField] private Sprite[] images;
    [SerializeField] private float imagesParSeconde = 8f;

    [Header("Sons (facultatif)")]
    [Tooltip("Joué quand l'ennemi est touché mais ne meurt pas")]
    [SerializeField] private AudioClip sonDegats;
    [Tooltip("Joué quand l'ennemi meurt")]
    [SerializeField] private AudioClip sonMort;
    [SerializeField, Range(0f, 1f)] private float volumeSons = 0.8f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer srRacine;
    private SpriteRenderer[] rendus;
    private Color[] couleursOrigine;

    private float departX;
    private int sens = 1;
    private bool mort = false;
    private float decalage = 0f;

    public bool EstMort
    {
        get { return mort; }
    }

    // Nombre d'ennemis encore vivants dans la scène (pour la ligne d'arrivée)
    public static int NombreVivants()
    {
        Ennemi[] tous = FindObjectsByType<Ennemi>(FindObjectsSortMode.None);

        int vivants = 0;
        foreach (Ennemi e in tous)
        {
            if (!e.mort) vivants++;
        }
        return vivants;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        srRacine = GetComponent<SpriteRenderer>();

        rendus = GetComponentsInChildren<SpriteRenderer>();
        couleursOrigine = new Color[rendus.Length];
        for (int i = 0; i < rendus.Length; i++)
        {
            couleursOrigine[i] = rendus[i].color;
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col.isTrigger = true;

        departX = transform.position.x;
        decalage = Random.value * 5f;

        TrierImages();
    }

    private void Update()
    {
        // Animation par images (si renseignées)
        if (mort || srRacine == null || images == null || images.Length == 0) return;

        int index = Mathf.FloorToInt((Time.time + decalage) * imagesParSeconde) % images.Length;
        if (images[index] != null) srRacine.sprite = images[index];
    }

    private void FixedUpdate()
    {
        if (mort) return;

        float x = rb.position.x + sens * vitesse * Time.fixedDeltaTime;

        if (x >= departX + distancePatrouille)
        {
            x = departX + distancePatrouille;
            sens = -1;
        }
        else if (x <= departX - distancePatrouille)
        {
            x = departX - distancePatrouille;
            sens = 1;
        }

        rb.MovePosition(new Vector2(x, rb.position.y));
        Orienter();
    }

    private void Orienter()
    {
        bool versLaDroite = sens > 0;
        bool garder = (versLaDroite == regardeADroiteParDefaut);

        if (visuel != null)
        {
            visuel.localRotation = Quaternion.Euler(0f, garder ? 0f : 180f, 0f);
        }
        else if (srRacine != null)
        {
            srRacine.flipX = !garder;
        }
    }

    // ----- Contact avec le joueur -----

    private void OnTriggerEnter2D(Collider2D autre)
    {
        ToucherJoueur(autre);
    }

    private void OnTriggerStay2D(Collider2D autre)
    {
        ToucherJoueur(autre);
    }

    private void ToucherJoueur(Collider2D autre)
    {
        if (mort) return;

        JoueurController joueur = autre.GetComponentInParent<JoueurController>();
        if (joueur == null) return;

        // Le joueur est en train de frapper : son coup passe en premier
        if (joueur.EnAttaque) return;

        SanteJoueur sante = joueur.GetComponent<SanteJoueur>();
        if (sante == null) sante = joueur.gameObject.AddComponent<SanteJoueur>();

        sante.Mourir();
    }

    // ----- Dégâts -----

    public void PrendreDegats(int degats)
    {
        if (mort) return;

        vie -= degats;

        if (vie <= 0)
        {
            Mourir();
        }
        else
        {
            SonsJeu.Jouer(sonDegats, volumeSons);
            StartCoroutine(Clignoter());
        }
    }

    private void Mourir()
    {
        mort = true;
        col.enabled = false;

        SonsJeu.Jouer(sonMort, volumeSons);

        // L'ennemi est éjecté vers le haut puis tombe en tournant
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(-sens * 2f, 6f);
        rb.angularVelocity = 360f * -sens;

        StartCoroutine(Disparaitre());
    }

    private IEnumerator Clignoter()
    {
        for (int i = 0; i < rendus.Length; i++)
        {
            if (rendus[i] != null) rendus[i].color = Color.red;
        }

        yield return new WaitForSeconds(0.12f);

        for (int i = 0; i < rendus.Length; i++)
        {
            if (rendus[i] != null) rendus[i].color = couleursOrigine[i];
        }
    }

    private IEnumerator Disparaitre()
    {
        float duree = 1.2f;
        float temps = 0f;

        while (temps < duree)
        {
            temps += Time.deltaTime;
            float k = Mathf.Clamp01(temps / duree);

            for (int i = 0; i < rendus.Length; i++)
            {
                if (rendus[i] == null) continue;
                Color c = couleursOrigine[i];
                c.a = 1f - k;
                rendus[i].color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // ----- Outils -----

    // Trie les images par leur numéro : "image 2" avant "image 10"
    private void TrierImages()
    {
        if (images == null || images.Length < 2) return;

        System.Array.Sort(images, (a, b) => NumeroDe(a).CompareTo(NumeroDe(b)));
    }

    private int NumeroDe(Sprite s)
    {
        if (s == null) return int.MaxValue;

        Match m = Regex.Match(s.name, @"\d+");
        if (!m.Success) return int.MaxValue;

        return int.Parse(m.Value);
    }

    // Dans la Scene : montre la zone de patrouille quand l'ennemi est sélectionné
    private void OnDrawGizmosSelected()
    {
        float x0 = Application.isPlaying ? departX : transform.position.x;
        Vector3 gauche = new Vector3(x0 - distancePatrouille, transform.position.y, 0f);
        Vector3 droite = new Vector3(x0 + distancePatrouille, transform.position.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gauche, droite);
        Gizmos.DrawWireSphere(gauche, 0.15f);
        Gizmos.DrawWireSphere(droite, 0.15f);
    }
}