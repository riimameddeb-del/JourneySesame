using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class JoueurController : MonoBehaviour
{
    [Header("Les deux personnages (enfants de Joueur)")]
    [SerializeField] private GameObject persoStudent;
    [SerializeField] private GameObject persoTeacher;

    [Tooltip("-1 = utiliser le choix fait dans le menu, 0 = Student, 1 = Teacher (pour tester)")]
    [SerializeField] private int personnageDeTest = -1;

    [Header("Mouvement")]
    [SerializeField] private float vitesseMarche = 3.5f;
    [SerializeField] private float vitesseCourse = 6f;
    [Tooltip("Coche pour que le personnage coure par défaut (Maj = marcher)")]
    [SerializeField] private bool courirParDefaut = false;
    [SerializeField] private float forceSaut = 12f;
    [SerializeField] private float gravite = 3f;

    [Header("Direction")]
    [Tooltip("Coche si le personnage regarde vers la droite quand il est posé tel quel")]
    [SerializeField] private bool regardeADroiteParDefaut = true;

    [Header("Collider automatique")]
    [SerializeField] private bool ajusterColliderAuPerso = true;
    [SerializeField] private float facteurLargeur = 0.6f;

    [Tooltip("Remonte le bas de la boîte (en unités du jeu). Si le personnage flotte au-dessus du sol, augmente : 0.05, 0.1, 0.15... S'il s'enfonce, mets une valeur négative.")]
    [SerializeField] private float rognerLeBas = 0f;

    [Header("Attaque au corps à corps (touche F)")]
    [Tooltip("Longueur de la zone touchée devant le personnage (en unités du jeu)")]
    [SerializeField] private float porteeAttaque = 1.3f;
    [Tooltip("Moment (en secondes après l'appui sur F) où le coup commence à toucher")]
    [SerializeField] private float debutCoup = 0.1f;
    [Tooltip("Moment (en secondes après l'appui sur F) où le coup s'arrête")]
    [SerializeField] private float finCoup = 0.4f;
    [SerializeField] private int degatsAttaque = 1;

    [Header("Animations : noms EXACTS des états dans l'Animator")]
    [SerializeField] private string etatRepos = "Idle";
    [SerializeField] private string etatMarche = "Walk";
    [SerializeField] private string etatCourse = "Run";
    [SerializeField] private string etatSautDebut = "Jump P1 (Start)";
    [SerializeField] private string etatSautMontee = "Jump P2 (In Air)";
    [SerializeField] private string etatChute = "Jump P3 (Fall)";
    [SerializeField] private string etatAtterrissage = "Landing";
    [SerializeField] private string etatAttaqueMelee = "Attack Melee";
    [SerializeField] private string etatAttaqueDistance = "Attack Ranged";

    [Header("Durées (en secondes)")]
    [SerializeField] private float dureeTransition = 0.08f;
    [SerializeField] private float dureeDebutSaut = 0.12f;
    [SerializeField] private float dureeAtterrissage = 0.12f;
    [SerializeField] private float dureeAttaque = 0.5f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private GameObject persoActif;
    private Animator animator;
    private string etatCourant = "";
    private readonly HashSet<string> avertissements = new HashSet<string>();
    private readonly HashSet<Ennemi> ennemisTouches = new HashSet<Ennemi>();

    private float horizontal = 0f;
    private bool veutCourir = false;
    private bool sautDemande = false;
    private bool auSol = false;
    private float tempsEnLair = 0f;
    private bool regardeVersLaDroite = true;

    private float minuteurDebutSaut = 0f;
    private float minuteurAtterrissage = 0f;
    private float minuteurAttaque = 0f;
    private string etatAttaqueEnCours = "";

    // Vrai pendant que le joueur frappe au corps à corps (les ennemis ne le blessent pas à ce moment)
    public bool EnAttaque
    {
        get { return minuteurAttaque > 0f && etatAttaqueEnCours == etatAttaqueMelee; }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // Réglages physiques faits automatiquement
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravite;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Pas de frottement : le personnage ne reste pas collé aux murs
        PhysicsMaterial2D sansFrottement = new PhysicsMaterial2D("SansFrottement");
        sansFrottement.friction = 0f;
        sansFrottement.bounciness = 0f;
        col.sharedMaterial = sansFrottement;

        regardeVersLaDroite = regardeADroiteParDefaut;

        ChoisirPersonnage();

        if (persoActif != null) animator = persoActif.GetComponentInChildren<Animator>();

        if (ajusterColliderAuPerso) AjusterCollider();
    }

    private void ChoisirPersonnage()
    {
        int choix = personnageDeTest >= 0 ? personnageDeTest : PlayerPrefs.GetInt("PersonnageChoisi", 0);

        GameObject choisi = (choix == 0) ? persoStudent : persoTeacher;
        GameObject autre = (choix == 0) ? persoTeacher : persoStudent;

        // On supprime le personnage non choisi au lieu de l'éteindre :
        // c'est plus propre pour le système d'animation 2D
        if (autre != null) Destroy(autre);

        if (choisi == null)
        {
            Debug.LogWarning("JoueurController : le personnage choisi n'est pas renseigné.");
            return;
        }

        choisi.SetActive(true);
        persoActif = choisi;
    }

    // Adapte la boîte de collision à la taille du personnage affiché
    private void AjusterCollider()
    {
        if (persoActif == null) return;

        SpriteRenderer[] sprites = persoActif.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length == 0) return;

        Bounds zone = sprites[0].bounds;
        for (int i = 1; i < sprites.Length; i++)
        {
            zone.Encapsulate(sprites[i].bounds);
        }

        // On remonte le bas de la boîte pour que les pieds touchent vraiment le sol
        float bas = zone.min.y + rognerLeBas;
        float haut = zone.max.y;
        if (haut - bas < 0.1f) bas = haut - 0.1f;

        Vector3 centreMonde = new Vector3(zone.center.x, (haut + bas) / 2f, zone.center.z);

        Vector3 echelle = transform.lossyScale;
        float largeur = (zone.size.x * facteurLargeur) / Mathf.Max(Mathf.Abs(echelle.x), 0.0001f);
        float hauteur = (haut - bas) / Mathf.Max(Mathf.Abs(echelle.y), 0.0001f);

        col.size = new Vector2(largeur, hauteur);
        col.offset = transform.InverseTransformPoint(centreMonde);
    }

    private void Update()
    {
        horizontal = LireHorizontal();
        veutCourir = courirParDefaut ? !ShiftAppuye() : ShiftAppuye();

        if (SautAppuye()) sautDemande = true;

        // Les minuteurs descendent
        minuteurDebutSaut -= Time.deltaTime;
        minuteurAtterrissage -= Time.deltaTime;
        minuteurAttaque -= Time.deltaTime;

        // Attaques (une seule à la fois)
        if (minuteurAttaque <= 0f)
        {
            if (AttaqueMeleeAppuyee()) LancerAttaque(etatAttaqueMelee);
            else if (AttaqueDistanceAppuyee()) LancerAttaque(etatAttaqueDistance);
        }

        // Pendant le coup de poing, on regarde si un ennemi est touché
        if (EnAttaque)
        {
            float ecoule = dureeAttaque - minuteurAttaque;
            if (ecoule >= debutCoup && ecoule <= finCoup) AppliquerCoup();
        }

        // Le personnage se retourne par rotation (plus sûr qu'une échelle négative)
        if (persoActif != null && Mathf.Abs(horizontal) > 0.01f)
        {
            bool versLaDroite = horizontal > 0f;
            regardeVersLaDroite = versLaDroite;

            bool garderOrientation = (versLaDroite == regardeADroiteParDefaut);
            persoActif.transform.localRotation = Quaternion.Euler(0f, garderOrientation ? 0f : 180f, 0f);
        }

        MettreAJourAnimation();
    }

    private void FixedUpdate()
    {
        // Détection du sol et de l'atterrissage
        bool maintenant = EstAuSol();

        if (maintenant)
        {
            if (!auSol && tempsEnLair > 0.2f) minuteurAtterrissage = dureeAtterrissage;
            tempsEnLair = 0f;
        }
        else
        {
            tempsEnLair += Time.fixedDeltaTime;
        }
        auSol = maintenant;

        // Déplacement
        float vitesse = veutCourir ? vitesseCourse : vitesseMarche;
        rb.linearVelocity = new Vector2(horizontal * vitesse, rb.linearVelocity.y);

        // Saut
        if (sautDemande && auSol)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forceSaut);
            minuteurDebutSaut = dureeDebutSaut;
            auSol = false;
            tempsEnLair = 0f;
        }
        sautDemande = false;
    }

    private bool EstAuSol()
    {
        Bounds b = col.bounds;
        Vector2 centre = new Vector2(b.center.x, b.min.y - 0.03f);
        Vector2 taille = new Vector2(b.size.x * 0.9f, 0.08f);

        Collider2D[] touches = Physics2D.OverlapBoxAll(centre, taille, 0f);
        foreach (Collider2D c in touches)
        {
            if (c.isTrigger) continue;
            if (c.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }

    // ----- Attaque -----

    private void LancerAttaque(string nomEtat)
    {
        etatAttaqueEnCours = nomEtat;
        minuteurAttaque = dureeAttaque;
        etatCourant = ""; // force l'animation à repartir du début
        ennemisTouches.Clear();
    }

    // Touche les ennemis dans la zone devant le personnage (chaque ennemi une seule fois par coup)
    private void AppliquerCoup()
    {
        float sens = regardeVersLaDroite ? 1f : -1f;
        Bounds b = col.bounds;

        Vector2 centre = new Vector2(b.center.x + sens * (b.extents.x + porteeAttaque * 0.5f), b.center.y);
        Vector2 taille = new Vector2(porteeAttaque, b.size.y);

        Collider2D[] touches = Physics2D.OverlapBoxAll(centre, taille, 0f);
        foreach (Collider2D c in touches)
        {
            Ennemi ennemi = c.GetComponentInParent<Ennemi>();
            if (ennemi == null || ennemi.EstMort) continue;

            if (ennemisTouches.Add(ennemi))
            {
                ennemi.PrendreDegats(degatsAttaque);
            }
        }
    }

    // ----- Animations -----

    private void MettreAJourAnimation()
    {
        if (animator == null) return;

        string voulu;

        if (minuteurAttaque > 0f)
        {
            voulu = etatAttaqueEnCours;
        }
        else if (!auSol)
        {
            if (minuteurDebutSaut > 0f) voulu = etatSautDebut;
            else if (rb.linearVelocity.y > 0.5f) voulu = etatSautMontee;
            else voulu = etatChute;
        }
        else if (minuteurAtterrissage > 0f)
        {
            voulu = etatAtterrissage;
        }
        else if (Mathf.Abs(horizontal) > 0.01f)
        {
            voulu = veutCourir ? etatCourse : etatMarche;
        }
        else
        {
            voulu = etatRepos;
        }

        if (voulu == etatCourant) return;

        int hash = Animator.StringToHash(voulu);

        if (!animator.HasState(0, hash))
        {
            if (avertissements.Add(voulu))
            {
                Debug.LogWarning("JoueurController : l'état \"" + voulu + "\" n'existe pas dans l'Animator. Utilise le menu ⋮ du composant → \"Afficher les états de l'Animator\" pour voir les bons noms.");
            }
            return;
        }

        animator.CrossFade(hash, dureeTransition, 0, 0f);
        etatCourant = voulu;
    }

    // ----- Clavier -----

    private float LireHorizontal()
    {
        float h = 0f;
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (k.leftArrowKey.isPressed || k.aKey.isPressed) h -= 1f;
            if (k.rightArrowKey.isPressed || k.dKey.isPressed) h += 1f;
        }
#else
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h += 1f;
#endif
        return h;
    }

    // Saut : espace, flèche haut, ou Z / W
    private bool SautAppuye()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        return k.spaceKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.W);
#endif
    }

    // Courir : Maj
    private bool ShiftAppuye()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        return k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }

    // Attaque au corps à corps : F
    private bool AttaqueMeleeAppuyee()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        return k.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }

    // Attaque à distance (animation seulement) : G
    private bool AttaqueDistanceAppuyee()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        return k.gKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.G);
#endif
    }

#if UNITY_EDITOR
    // Outil : affiche dans la Console tous les états et paramètres de l'Animator des 2 personnages
    [ContextMenu("Afficher les états de l'Animator")]
    private void AfficherEtatsAnimator()
    {
        GameObject[] persos = { persoStudent, persoTeacher };

        foreach (GameObject p in persos)
        {
            if (p == null) continue;

            Animator a = p.GetComponentInChildren<Animator>(true);
            if (a == null)
            {
                Debug.Log(p.name + " : aucun Animator trouvé.");
                continue;
            }

            RuntimeAnimatorController brut = a.runtimeAnimatorController;
            UnityEditor.Animations.AnimatorController ctrl = brut as UnityEditor.Animations.AnimatorController;

            if (ctrl == null && brut is AnimatorOverrideController)
            {
                ctrl = ((AnimatorOverrideController)brut).runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            }

            if (ctrl == null)
            {
                Debug.Log(p.name + " : contrôleur d'animation introuvable.");
                continue;
            }

            List<string> etats = new List<string>();
            foreach (UnityEditor.Animations.ChildAnimatorState e in ctrl.layers[0].stateMachine.states)
            {
                etats.Add(e.state.name);
            }

            List<string> parametres = new List<string>();
            foreach (AnimatorControllerParameter prm in ctrl.parameters)
            {
                parametres.Add(prm.name + " (" + prm.type + ")");
            }

            Debug.Log(p.name + " | ÉTATS : " + string.Join(" | ", etats));
            Debug.Log(p.name + " | PARAMÈTRES : " + (parametres.Count > 0 ? string.Join(" | ", parametres) : "aucun"));
        }
    }
#endif
}