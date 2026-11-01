using System.Collections.Generic;
using UnityEngine;

public class PopInUI : MonoBehaviour
{
    [Header("Éléments à animer (optionnel : trouvés automatiquement)")]
    [SerializeField] private Transform titre;
    [SerializeField] private Transform[] boutons;

    [Header("Réglages du pop")]
    [SerializeField] private float delaiInitial = 0.5f;
    [SerializeField] private float dureePop = 0.7f;
    [SerializeField] private float delaiEntreElements = 0.3f;
    [SerializeField] private float force = 2.5f; // plus grand = plus de rebond
    [SerializeField] private bool fondu = true;  // apparition progressive en plus du pop

    [Header("Pulsation du titre (après le pop)")]
    [SerializeField] private bool pulsationTitre = true;
    [SerializeField] private float amplitudePulsation = 0.08f; // 0.08 = grossit de 8 %
    [SerializeField] private float vitessePulsation = 2f;      // plus grand = plus rapide

    private class Element
    {
        public Transform cible;
        public Vector3 tailleOrigine;
        public CanvasGroup groupe;
        public float debut;
        public bool estTitre;
    }

    private readonly List<Element> elements = new List<Element>();
    private float temps;
    private bool animationEnCours = false;
    private bool premiereImage = true;

    private void Awake()
    {
        ConstruireListe();
        CacherTout(); // évite un flash des éléments avant le début de l'animation
    }

    private void OnEnable()
    {
        ConstruireListe();
        temps = 0f;
        premiereImage = true;
        animationEnCours = true;
        CacherTout();
    }

    private void OnDisable()
    {
        // On remet tout à l'état normal si la page est cachée
        foreach (Element e in elements)
        {
            if (e.cible != null) e.cible.localScale = e.tailleOrigine;
            if (e.groupe != null) e.groupe.alpha = 1f;
        }
        animationEnCours = false;
    }

    private void CacherTout()
    {
        foreach (Element e in elements)
        {
            if (e.cible != null) e.cible.localScale = Vector3.zero;
            if (e.groupe != null && fondu) e.groupe.alpha = 0f;
        }
    }

    private void ConstruireListe()
    {
        // Liste déjà construite : on la réutilise
        if (elements.Count > 0)
        {
            RecalculerDebuts();
            return;
        }

        // Titre
        Transform t = titre;
        if (t == null) t = ChercherParNom(transform, "TitreImage");
        if (t != null) AjouterElement(t, true);
        else Debug.LogWarning("PopInUI : TitreImage introuvable.");

        // Boutons : champs renseignés, sinon par nom
        int avant = elements.Count;
        if (boutons != null)
        {
            foreach (Transform b in boutons)
            {
                if (b != null) AjouterElement(b, false);
            }
        }

        if (elements.Count == avant)
        {
            string[] noms = { "PlayButton", "MenuButton", "QuitButton" };
            foreach (string nom in noms)
            {
                Transform b = ChercherParNom(transform, nom);
                if (b != null) AjouterElement(b, false);
            }
        }

        RecalculerDebuts();
    }

    private void AjouterElement(Transform cible, bool estTitre)
    {
        // Un Animator ou Animation sur l'objet (ou ses enfants) écraserait la taille : on les désactive
        foreach (Animator anim in cible.GetComponentsInChildren<Animator>(true))
        {
            anim.enabled = false;
        }

        foreach (Animation legacy in cible.GetComponentsInChildren<Animation>(true))
        {
            legacy.enabled = false;
        }

        Element e = new Element();
        e.cible = cible;
        e.tailleOrigine = cible.localScale;
        e.estTitre = estTitre;

        // Sécurité : si la taille d'origine est nulle, on prend 1
        if (e.tailleOrigine == Vector3.zero) e.tailleOrigine = Vector3.one;

        // CanvasGroup pour le fondu (ajouté automatiquement si absent)
        e.groupe = cible.GetComponent<CanvasGroup>();
        if (e.groupe == null) e.groupe = cible.gameObject.AddComponent<CanvasGroup>();

        elements.Add(e);
    }

    private void RecalculerDebuts()
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].debut = delaiInitial + i * delaiEntreElements;
        }
    }

    private Transform ChercherParNom(Transform racine, string nom)
    {
        foreach (Transform enfant in racine)
        {
            if (enfant.name == nom) return enfant;
            Transform trouve = ChercherParNom(enfant, nom);
            if (trouve != null) return trouve;
        }
        return null;
    }

    // LateUpdate : s'exécute après tout le reste, donc plus rien ne peut écraser la taille
    private void LateUpdate()
    {
        if (!animationEnCours) return;

        // On ignore la première image (souvent très longue à cause du chargement)
        // et on limite chaque pas de temps pour ne jamais "sauter" l'animation
        if (premiereImage)
        {
            premiereImage = false;
        }
        else
        {
            temps += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        }

        foreach (Element e in elements)
        {
            if (e.cible == null) continue;

            float k = (temps - e.debut) / dureePop;

            if (k <= 0f)
            {
                // Pas encore apparu
                e.cible.localScale = Vector3.zero;
                if (fondu) e.groupe.alpha = 0f;
            }
            else if (k < 1f)
            {
                // Phase de pop (rebond)
                e.cible.localScale = e.tailleOrigine * EaseOutBack(k);
                if (fondu) e.groupe.alpha = Mathf.Clamp01(k * 3f);
            }
            else if (e.estTitre && pulsationTitre)
            {
                // Après le pop : le titre grossit un peu puis rétrécit, en boucle
                float tempsPulsation = temps - (e.debut + dureePop);
                float facteur = 1f + amplitudePulsation * (0.5f - 0.5f * Mathf.Cos(tempsPulsation * vitessePulsation * Mathf.PI));
                e.cible.localScale = e.tailleOrigine * facteur;
                e.groupe.alpha = 1f;
            }
            else
            {
                e.cible.localScale = e.tailleOrigine;
                e.groupe.alpha = 1f;
            }
        }
    }

    // Courbe qui dépasse légèrement 1 puis revient à 1 (effet rebond)
    private float EaseOutBack(float x)
    {
        float c1 = force;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}