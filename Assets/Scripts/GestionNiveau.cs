using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// À mettre sur un objet vide "GestionNiveau" dans chaque scène de niveau
public class GestionNiveau : MonoBehaviour
{
    public static GestionNiveau Instance { get; private set; }

    [Header("Ce niveau")]
    [Tooltip("1, 2 ou 3 : sert à débloquer le niveau suivant quand on gagne")]
    [SerializeField] private int numeroNiveau = 1;
    [Tooltip("Les scènes s'appellent Niveau1, Niveau2, Niveau3...")]
    [SerializeField] private string prefixeScenesNiveaux = "Niveau";

    [Header("Compteur de pièces")]
    [SerializeField] private TMP_Text compteurPieces;
    [Tooltip("{0} = pièces ramassées, {1} = total de pièces du niveau")]
    [SerializeField] private string format = "{0} / {1}";

    [Header("Compteur d'ennemis (facultatif)")]
    [SerializeField] private TMP_Text compteurEnnemis;
    [Tooltip("{0} = ennemis tués, {1} = total d'ennemis du niveau")]
    [SerializeField] private string formatEnnemis = "Ennemis : {0} / {1}";

    [Header("Timer")]
    [Tooltip("Durée du niveau en secondes (120 = 2 minutes)")]
    [SerializeField] private float dureeNiveau = 120f;
    [SerializeField] private TMP_Text texteTemps;
    [Tooltip("Le texte devient rouge quand il reste ce nombre de secondes ou moins")]
    [SerializeField] private float secondesAlerte = 10f;

    [Header("Vies : glisse les 3 cœurs ici, du premier au dernier")]
    [SerializeField] private Image[] coeurs;
    [SerializeField] private Color couleurVieRestante = Color.white;
    [SerializeField] private Color couleurViePerdue = new Color(0.15f, 0.15f, 0.15f, 0.6f);

    [Header("Message à l'écran (ex : il manque des pièces)")]
    [SerializeField] private TMP_Text texteMessage;

    [Header("Écran GAME OVER (éteint au départ)")]
    [SerializeField] private GameObject panneauGameOver;

    [Header("Écran VICTOIRE (éteint au départ)")]
    [SerializeField] private GameObject panneauVictoire;
    [SerializeField] private TMP_Text texteBilan;
    [Tooltip("Nom exact de l'animation de joie dans l'Animator")]
    [SerializeField] private string etatVictoire = "Happy Cas A";
    [SerializeField] private float delaiAvantVictoire = 1.8f;

    [Header("Nom de la scène du menu principal")]
    [SerializeField] private string nomSceneMenu = "SampleScene";

    private int piecesRamassees = 0;
    private int piecesTotal = 0;
    private int ennemisTotal = 0;

    private float tempsRestant = 0f;
    private bool minuteurActif = true;
    private bool niveauTermine = false;
    private bool echecEnCours = false;
    private Color couleurTempsNormale = Color.white;
    private JoueurController joueur;
    private float prochainRafraichissementEnnemis = 0f;

    public int PiecesRamassees
    {
        get { return piecesRamassees; }
    }

    public int PiecesTotal
    {
        get { return piecesTotal; }
    }

    public bool ToutesLesPiecesRamassees
    {
        get { return piecesRamassees >= piecesTotal; }
    }

    public float TempsRestant
    {
        get { return tempsRestant; }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Vies : on repart à 3 sauf si le niveau recommence parce que le joueur est mort
        if (!ViesJoueur.rechargementApresMort) ViesJoueur.Reinitialiser();
        ViesJoueur.rechargementApresMort = false;

        if (panneauGameOver != null) panneauGameOver.SetActive(false);
        if (panneauVictoire != null) panneauVictoire.SetActive(false);
        if (texteMessage != null) texteMessage.text = "";

        // Pièces : le total = la somme des valeurs de toutes les pièces de la scène
        PieceCollectable[] pieces = FindObjectsByType<PieceCollectable>(FindObjectsSortMode.None);

        piecesTotal = 0;
        foreach (PieceCollectable p in pieces)
        {
            piecesTotal += p.Valeur;
        }

        // Ennemis
        ennemisTotal = FindObjectsByType<Ennemi>(FindObjectsSortMode.None).Length;

        // Timer
        tempsRestant = dureeNiveau;
        minuteurActif = true;
        if (texteTemps != null) couleurTempsNormale = texteTemps.color;
        joueur = FindFirstObjectByType<JoueurController>();

        RafraichirPieces();
        RafraichirEnnemis();
        MettreAJourVies();
        AfficherTemps();
    }

    private void OnDestroy()
    {
        // Sécurité : on ne laisse jamais le jeu en pause en changeant de scène
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Le compteur d'ennemis se met à jour 4 fois par seconde
        if (compteurEnnemis != null && Time.unscaledTime >= prochainRafraichissementEnnemis)
        {
            prochainRafraichissementEnnemis = Time.unscaledTime + 0.25f;
            RafraichirEnnemis();
        }

        if (!minuteurActif) return;

        if (joueur == null) joueur = FindFirstObjectByType<JoueurController>();

        // Le temps s'arrête si le joueur est déjà mort
        if (joueur != null)
        {
            SanteJoueur sante = joueur.GetComponent<SanteJoueur>();
            if (sante != null && sante.EstMort) return;
        }

        tempsRestant -= Time.deltaTime;

        if (tempsRestant <= 0f)
        {
            tempsRestant = 0f;
            minuteurActif = false;
            AfficherTemps();
            TempsEcoule();
            return;
        }

        AfficherTemps();
    }

    // ----- Timer -----

    // À appeler quand le temps doit s'arrêter
    public void ArreterTemps()
    {
        minuteurActif = false;
    }

    private void AfficherTemps()
    {
        if (texteTemps == null) return;

        int total = Mathf.CeilToInt(tempsRestant);
        int minutes = total / 60;
        int secondes = total % 60;

        texteTemps.text = minutes.ToString("00") + ":" + secondes.ToString("00");
        texteTemps.color = (tempsRestant <= secondesAlerte) ? Color.red : couleurTempsNormale;
    }

    // Temps écoulé : le joueur perd une vie, comme sur les piques
    private void TempsEcoule()
    {
        if (joueur == null || niveauTermine) return;

        SanteJoueur sante = joueur.GetComponent<SanteJoueur>();
        if (sante == null) sante = joueur.gameObject.AddComponent<SanteJoueur>();

        sante.Mourir();
    }

    // ----- Pièces et ennemis -----

    public void AjouterPieces(int nombre)
    {
        piecesRamassees += nombre;
        RafraichirPieces();
    }

    private void RafraichirPieces()
    {
        if (compteurPieces == null) return;

        // Si le champ est vide, on prend le modèle par défaut
        string modele = string.IsNullOrEmpty(format) ? "{0} / {1}" : format;

        // On remplace {0} et {1} à la main : aucune erreur possible, même si le champ est mal écrit
        compteurPieces.text = modele
            .Replace("{0}", piecesRamassees.ToString())
            .Replace("{1}", piecesTotal.ToString());
    }

    private void RafraichirEnnemis()
    {
        if (compteurEnnemis == null) return;

        int tues = ennemisTotal - Ennemi.NombreVivants();
        if (tues < 0) tues = 0;

        string modele = string.IsNullOrEmpty(formatEnnemis) ? "{0} / {1}" : formatEnnemis;

        compteurEnnemis.text = modele
            .Replace("{0}", tues.ToString())
            .Replace("{1}", ennemisTotal.ToString());
    }

    // ----- Vies -----

    public void MettreAJourVies()
    {
        if (coeurs == null) return;

        for (int i = 0; i < coeurs.Length; i++)
        {
            if (coeurs[i] == null) continue;
            coeurs[i].color = (i < ViesJoueur.Vies) ? couleurVieRestante : couleurViePerdue;
        }
    }

    // ----- Ligne d'arrivée -----

    // Appelé par la ligne d'arrivée quand le joueur la franchit
    public void ArriveeAtteinte(JoueurController j)
    {
        if (niveauTermine || echecEnCours) return;

        SanteJoueur sante = j.GetComponent<SanteJoueur>();
        if (sante != null && sante.EstMort) return;

        int piecesManquantes = piecesTotal - piecesRamassees;
        int ennemisRestants = Ennemi.NombreVivants();

        if (piecesManquantes <= 0 && ennemisRestants <= 0)
        {
            Victoire(j);
        }
        else
        {
            Echec(j, piecesManquantes, ennemisRestants);
        }
    }

    private void Victoire(JoueurController j)
    {
        niveauTermine = true;
        minuteurActif = false;

        // Le joueur s'arrête et fait sa danse de joie
        j.enabled = false;

        Rigidbody2D rb = j.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Animator animator = j.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            int hash = Animator.StringToHash(etatVictoire);
            if (animator.HasState(0, hash)) animator.CrossFade(hash, 0.1f, 0, 0f);
        }

        // On débloque le niveau suivant sur la page Level
        LevelProgress.ValiderNiveau(numeroNiveau);

        StartCoroutine(AfficherVictoire());
    }

    private IEnumerator AfficherVictoire()
    {
        yield return new WaitForSeconds(delaiAvantVictoire);

        if (texteBilan != null)
        {
            int total = Mathf.CeilToInt(tempsRestant);
            string temps = (total / 60).ToString("00") + ":" + (total % 60).ToString("00");

            texteBilan.text = "Pièces : " + piecesRamassees + " / " + piecesTotal
                + "\nEnnemis tués : " + ennemisTotal + " / " + ennemisTotal
                + "\nTemps restant : " + temps;
        }

        if (panneauVictoire != null) panneauVictoire.SetActive(true);
    }

    private void Echec(JoueurController j, int piecesManquantes, int ennemisRestants)
    {
        echecEnCours = true;

        // Message du type "Il manque 2 pièces et 1 ennemi !"
        string manque = "";

        if (piecesManquantes > 0)
        {
            manque += piecesManquantes + (piecesManquantes > 1 ? " pièces" : " pièce");
        }

        if (ennemisRestants > 0)
        {
            if (manque != "") manque += " et ";
            manque += ennemisRestants + (ennemisRestants > 1 ? " ennemis" : " ennemi");
        }

        if (texteMessage != null) texteMessage.text = "Il manque " + manque + " !";

        // Le joueur perd une vie
        SanteJoueur sante = j.GetComponent<SanteJoueur>();
        if (sante == null) sante = j.gameObject.AddComponent<SanteJoueur>();

        sante.Mourir();
    }

    // ----- Game over -----

    public void AfficherGameOver()
    {
        minuteurActif = false;
        if (panneauGameOver != null) panneauGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    // ----- Boutons des écrans de fin -----

    // Bouton "Rejouer" : on recommence le niveau avec 3 vies
    public void Rejouer()
    {
        Time.timeScale = 1f;
        ViesJoueur.Reinitialiser();
        ViesJoueur.rechargementApresMort = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Bouton "Niveau suivant" : s'il n'existe pas, on retourne à la page des niveaux
    public void NiveauSuivant()
    {
        string nomScene = prefixeScenesNiveaux + (numeroNiveau + 1);

        if (!Application.CanStreamedLevelBeLoaded(nomScene))
        {
            RetourAuxNiveaux();
            return;
        }

        Time.timeScale = 1f;
        ViesJoueur.Reinitialiser();
        ViesJoueur.rechargementApresMort = false;
        SceneManager.LoadScene(nomScene);
    }

    // Bouton "Quitter" / "Niveaux" : retour à la page des niveaux
    public void RetourAuxNiveaux()
    {
        Time.timeScale = 1f;
        ViesJoueur.Reinitialiser();
        PageManager.ouvrirPageLevelAuDemarrage = true;
        SceneManager.LoadScene(nomSceneMenu);
    }
}