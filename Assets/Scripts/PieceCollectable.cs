using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

// À mettre sur chaque pièce : elle tourne, et disparaît quand le joueur la touche
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class PieceCollectable : MonoBehaviour
{
    [Header("Animation : glisse les images 1 à 16 (dans n'importe quel ordre, elles sont triées toutes seules)")]
    [SerializeField] private Sprite[] images;
    [SerializeField] private float imagesParSeconde = 14f;
    [Tooltip("Chaque pièce commence son tour à un moment différent")]
    [SerializeField] private bool decalerAnimation = true;

    [Header("Valeur et son")]
    [SerializeField] private int valeur = 1;
    [SerializeField] private AudioClip sonRamassage;
    [SerializeField, Range(0f, 1f)] private float volumeSon = 0.8f;

    private SpriteRenderer sr;
    private CircleCollider2D col;
    private bool ramassee = false;
    private float decalage = 0f;

    public int Valeur
    {
        get { return valeur; }
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        TrierImages();

        if (decalerAnimation) decalage = Random.value * 10f;
    }

    private void Update()
    {
        if (ramassee || images == null || images.Length == 0) return;

        int index = Mathf.FloorToInt((Time.time + decalage) * imagesParSeconde) % images.Length;
        if (images[index] != null) sr.sprite = images[index];
    }

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

    private void OnTriggerEnter2D(Collider2D autre)
    {
        if (ramassee) return;
        if (autre.GetComponentInParent<JoueurController>() == null) return;

        ramassee = true;
        col.enabled = false;

        JouerSon();

        if (GestionNiveau.Instance != null)
        {
            GestionNiveau.Instance.AjouterPieces(valeur);
        }

        StartCoroutine(Disparaitre());
    }

    private void JouerSon()
    {
        if (sonRamassage == null) return;

        // Son "2D" : même volume où que soit la pièce
        GameObject objetSon = new GameObject("SonPiece");
        AudioSource source = objetSon.AddComponent<AudioSource>();
        source.clip = sonRamassage;
        source.volume = volumeSon;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(objetSon, sonRamassage.length + 0.1f);
    }

    // Petit effet : la pièce monte, grossit et s'efface
    private IEnumerator Disparaitre()
    {
        float duree = 0.25f;
        float temps = 0f;

        Vector3 depart = transform.position;
        Vector3 echelleDepart = transform.localScale;
        Color couleur = sr.color;

        while (temps < duree)
        {
            temps += Time.deltaTime;
            float k = Mathf.Clamp01(temps / duree);

            transform.position = depart + Vector3.up * (0.6f * k);
            transform.localScale = echelleDepart * (1f + 0.4f * k);

            couleur.a = 1f - k;
            sr.color = couleur;

            yield return null;
        }

        Destroy(gameObject);
    }
}