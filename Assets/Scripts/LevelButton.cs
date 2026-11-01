using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

// Sauvegarde de la progression : combien de niveaux ont été réussis
public static class LevelProgress
{
    private const string Cle = "NiveauxReussis";
    private const string CleTransition = "TransitionEnAttente";

    public static event Action ProgressionChangee;

    public static int NiveauxReussis
    {
        get { return PlayerPrefs.GetInt(Cle, 0); }
    }

    // Numéro du niveau réussi dont l'animation n'a pas encore été vue sur la page Level (0 = aucune)
    public static int TransitionEnAttente
    {
        get { return PlayerPrefs.GetInt(CleTransition, 0); }
    }

    // À appeler quand le joueur réussit un niveau : LevelProgress.ValiderNiveau(1);
    public static void ValiderNiveau(int numero)
    {
        // On ne peut valider que le niveau actuellement jouable (ou un niveau déjà réussi)
        if (numero == NiveauxReussis + 1)
        {
            PlayerPrefs.SetInt(Cle, numero);
            PlayerPrefs.SetInt(CleTransition, numero);
            PlayerPrefs.Save();
        }

        if (ProgressionChangee != null) ProgressionChangee();
    }

    // Appelé par les boutons quand ils ont lu l'information : l'animation ne se rejouera pas
    public static void EffacerTransition()
    {
        if (PlayerPrefs.HasKey(CleTransition))
        {
            PlayerPrefs.DeleteKey(CleTransition);
            PlayerPrefs.Save();
        }
    }

    public static void Reinitialiser()
    {
        PlayerPrefs.DeleteKey(Cle);
        PlayerPrefs.DeleteKey(CleTransition);
        PlayerPrefs.Save();

        if (ProgressionChangee != null) ProgressionChangee();
    }
}

// À mettre sur l'objet "Haut" de chaque bouton de niveau (celui qui a le composant Button)
public class LevelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Etat { Bloque, Actif, Complete }

    [Header("Numéro du niveau (1, 2 ou 3)")]
    [SerializeField] private int numeroNiveau = 1;

    [Header("Éléments (trouvés automatiquement si vides)")]
    [SerializeField] private Image haut;
    [SerializeField] private Image baseImage;
    [SerializeField] private TMP_Text numero;
    [SerializeField] private Button bouton;

    [Header("Images du haut")]
    [SerializeField] private Sprite hautBleu;
    [SerializeField] private Sprite hautVert;
    [SerializeField] private Sprite hautGris;

    [Header("Images de la base")]
    [SerializeField] private Sprite baseMarron;
    [SerializeField] private Sprite baseSombre;

    [Header("Taille et position du haut selon l'état")]
    [SerializeField] private Vector2 tailleBleu = new Vector2(150f, 120f);
    [SerializeField] private Vector2 positionBleu = new Vector2(0f, 12f);
    [SerializeField] private Vector2 tailleVert = new Vector2(157f, 190f);
    [SerializeField] private Vector2 positionVert = new Vector2(0f, 47f);
    [SerializeField] private Vector2 tailleGris = new Vector2(150f, 129f);
    [SerializeField] private Vector2 positionGris = new Vector2(0f, 16f);

    [Header("Réglages")]
    [SerializeField] private float enfoncement = 8f;           // de combien le haut descend quand on appuie
    [SerializeField] private float decalageNumeroComplete = -33f; // hauteur du numéro sur le haut vert

    [Header("Animation quand on revient d'une victoire")]
    [Tooltip("Attente après l'affichage de la page, avant que le niveau réussi ne devienne vert")]
    [SerializeField] private float delaiAvantAnimation = 0.9f;
    [Tooltip("Attente (après le début de l'animation) avant que le niveau suivant ne se débloque")]
    [SerializeField] private float delaiAvantDeblocage = 1.0f;
    [Tooltip("Durée du rebond quand l'image change")]
    [SerializeField] private float dureePop = 0.45f;
    [Tooltip("Plus grand = plus de rebond")]
    [SerializeField] private float forceRebond = 2.2f;

    [Header("Quand on clique sur un niveau jouable (on branchera le lancement du niveau ici)")]
    public UnityEvent surClic;

    private Etat etatActuel = Etat.Bloque;
    private Vector2 positionRepos;
    private bool initialise = false;
    private Coroutine animationEnCours;

    private void Awake()
    {
        Initialiser();
    }

    private void OnEnable()
    {
        Initialiser();
        LevelProgress.ProgressionChangee += Rafraichir;
        Rafraichir();
    }

    private void OnDisable()
    {
        LevelProgress.ProgressionChangee -= Rafraichir;
        animationEnCours = null;
    }

    private void Initialiser()
    {
        if (initialise) return;
        initialise = true;

        if (haut == null) haut = GetComponent<Image>();
        if (bouton == null) bouton = GetComponent<Button>();
        if (numero == null) numero = GetComponentInChildren<TMP_Text>(true);

        if (baseImage == null && transform.parent != null)
        {
            Transform b = transform.parent.Find("Base");
            if (b != null) baseImage = b.GetComponent<Image>();
        }

        if (bouton != null)
        {
            // Pas de changement de couleur automatique : on gère nous-mêmes les images
            bouton.transition = Selectable.Transition.None;
            bouton.onClick.AddListener(SurClicBouton);
        }
    }

#if UNITY_EDITOR
    // Remplit les images automatiquement quand on ajoute le script (si elles sont dans Assets/Sprites/Boutons)
    private void Reset()
    {
        string dossier = "Assets/Sprites/Boutons/";
        hautBleu = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dossier + "Bouton_Haut_Bleu.png");
        hautVert = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dossier + "Bouton_Haut_Vert_Etoiles.png");
        hautGris = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dossier + "Bouton_Haut_Gris_Cadenas.png");
        baseMarron = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dossier + "Bouton_Base_Marron.png");
        baseSombre = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(dossier + "Bouton_Base_Sombre.png");
    }
#endif

    private Etat CalculerEtat()
    {
        int reussis = LevelProgress.NiveauxReussis;

        if (numeroNiveau <= reussis) return Etat.Complete;
        if (numeroNiveau == reussis + 1) return Etat.Actif;
        return Etat.Bloque;
    }

    // Met le bouton à jour. Si un niveau vient d'être réussi, on montre d'abord l'ANCIEN état,
    // puis on joue l'animation vers le nouvel état.
    public void Rafraichir()
    {
        Initialiser();

        if (animationEnCours != null)
        {
            StopCoroutine(animationEnCours);
            animationEnCours = null;
        }

        if (haut != null) haut.rectTransform.localScale = Vector3.one;

        Etat etatFinal = CalculerEtat();
        int attente = LevelProgress.TransitionEnAttente;

        // Pas d'animation possible si la page n'est pas affichée
        if (haut == null || !gameObject.activeInHierarchy || attente <= 0)
        {
            AppliquerEtat(etatFinal);
            return;
        }

        if (numeroNiveau == attente)
        {
            // Ce niveau vient d'être réussi : il est encore bleu, puis il devient vert
            AppliquerEtat(Etat.Actif);
            animationEnCours = StartCoroutine(AnimerReussite());
        }
        else if (numeroNiveau == attente + 1)
        {
            // Ce niveau vient d'être débloqué : il est encore verrouillé, puis il s'ouvre
            AppliquerEtat(Etat.Bloque);
            animationEnCours = StartCoroutine(AnimerDeblocage());
        }
        else
        {
            AppliquerEtat(etatFinal);
        }
    }

    // Applique l'apparence d'un état (images, tailles, position, numéro)
    private void AppliquerEtat(Etat etat)
    {
        etatActuel = etat;

        Sprite spriteHaut = hautBleu;
        Sprite spriteBase = baseMarron;
        Vector2 taille = tailleBleu;
        Vector2 position = positionBleu;
        float decalageNumero = 0f;

        if (etat == Etat.Complete)
        {
            spriteHaut = hautVert;
            taille = tailleVert;
            position = positionVert;
            decalageNumero = decalageNumeroComplete;
        }
        else if (etat == Etat.Bloque)
        {
            spriteHaut = hautGris;
            spriteBase = baseSombre;
            taille = tailleGris;
            position = positionGris;
        }

        if (haut != null)
        {
            haut.sprite = spriteHaut;
            haut.preserveAspect = true;
            haut.rectTransform.sizeDelta = taille;
            haut.rectTransform.anchoredPosition = position;
        }

        positionRepos = position;

        if (baseImage != null)
        {
            baseImage.sprite = spriteBase;
            baseImage.preserveAspect = true;
        }

        if (bouton != null) bouton.interactable = (etat != Etat.Bloque);

        if (numero != null)
        {
            numero.text = numeroNiveau.ToString();
            // Le cadenas remplace le numéro quand le niveau est bloqué
            numero.gameObject.SetActive(etat != Etat.Bloque);

            RectTransform rt = numero.rectTransform;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, decalageNumero);
        }
    }

    // ----- Animations de victoire -----

    // Niveau réussi : le haut s'enfonce, puis devient vert avec un rebond
    private IEnumerator AnimerReussite()
    {
        // On attend une image : tous les boutons ont ainsi eu le temps de lire l'information
        yield return null;
        LevelProgress.EffacerTransition();

        if (bouton != null) bouton.interactable = false;

        yield return new WaitForSecondsRealtime(delaiAvantAnimation);

        yield return Enfoncer(0.15f);

        AppliquerEtat(Etat.Complete);
        if (bouton != null) bouton.interactable = false;

        yield return Pop();

        if (bouton != null) bouton.interactable = true;
        animationEnCours = null;
    }

    // Niveau débloqué : le cadenas tremble, puis le bouton devient bleu avec un rebond
    private IEnumerator AnimerDeblocage()
    {
        yield return null;
        LevelProgress.EffacerTransition();

        if (bouton != null) bouton.interactable = false;

        yield return new WaitForSecondsRealtime(delaiAvantAnimation + delaiAvantDeblocage);

        yield return Secouer(0.45f, 7f);

        AppliquerEtat(Etat.Actif);
        if (bouton != null) bouton.interactable = false;

        yield return Pop();

        if (bouton != null) bouton.interactable = true;
        animationEnCours = null;
    }

    private IEnumerator Enfoncer(float duree)
    {
        Vector2 depart = positionRepos;
        Vector2 fin = positionRepos + new Vector2(0f, -enfoncement);
        float temps = 0f;

        while (temps < duree)
        {
            temps += Time.unscaledDeltaTime;
            haut.rectTransform.anchoredPosition = Vector2.Lerp(depart, fin, Mathf.Clamp01(temps / duree));
            yield return null;
        }
    }

    private IEnumerator Secouer(float duree, float amplitude)
    {
        Vector2 depart = positionRepos;
        float temps = 0f;

        while (temps < duree)
        {
            temps += Time.unscaledDeltaTime;

            // Le tremblement diminue petit à petit
            float decalage = Mathf.Sin(temps * 70f) * amplitude * (1f - Mathf.Clamp01(temps / duree));
            haut.rectTransform.anchoredPosition = depart + new Vector2(decalage, 0f);

            yield return null;
        }

        haut.rectTransform.anchoredPosition = depart;
    }

    // Le nouveau haut apparaît en grossissant, avec un petit rebond
    private IEnumerator Pop()
    {
        RectTransform rt = haut.rectTransform;
        float temps = 0f;

        while (temps < dureePop)
        {
            temps += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(temps / dureePop);
            rt.localScale = Vector3.one * EaseOutBack(k);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    // Courbe qui dépasse légèrement 1 puis revient à 1 (effet rebond)
    private float EaseOutBack(float x)
    {
        float c1 = forceRebond;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private void SurClicBouton()
    {
        if (etatActuel == Etat.Bloque) return;
        if (animationEnCours != null) return;

        Debug.Log("Niveau " + numeroNiveau + " lancé");
        surClic.Invoke();
    }

    // ----- Effet "le haut s'enfonce quand on appuie" -----

    public void OnPointerDown(PointerEventData eventData)
    {
        if (etatActuel == Etat.Bloque || haut == null) return;
        if (animationEnCours != null) return;

        haut.rectTransform.anchoredPosition = positionRepos + new Vector2(0f, -enfoncement);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Relacher();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Relacher();
    }

    private void Relacher()
    {
        if (animationEnCours != null) return;
        if (haut != null) haut.rectTransform.anchoredPosition = positionRepos;
    }

    // ----- Pour tester sans rejouer un niveau (menu ⋮ du composant, en mode Play) -----

    [ContextMenu("TEST : valider ce niveau")]
    private void TestValider()
    {
        LevelProgress.ValiderNiveau(numeroNiveau);
    }

    [ContextMenu("TEST : tout réinitialiser")]
    private void TestReinitialiser()
    {
        LevelProgress.Reinitialiser();
    }
}