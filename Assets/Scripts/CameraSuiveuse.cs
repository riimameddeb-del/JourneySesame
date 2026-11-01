using UnityEngine;

// À mettre sur Main Camera : suit le joueur sans sortir des limites du niveau
[RequireComponent(typeof(Camera))]
public class CameraSuiveuse : MonoBehaviour
{
    [Header("Cible (laisse vide : le joueur est trouvé tout seul)")]
    [SerializeField] private Transform cible;

    [Header("Suivi")]
    [Tooltip("Temps de lissage : petit = la caméra colle au joueur, grand = elle glisse doucement")]
    [SerializeField] private float douceur = 0.15f;
    [Tooltip("Décale la caméra par rapport au joueur (X positif = on voit plus loin vers la droite)")]
    [SerializeField] private Vector2 decalage = new Vector2(0f, 0f);
    [Tooltip("Coche pour que la caméra suive aussi les sauts et les chutes")]
    [SerializeField] private bool suivreVertical = false;

    [Header("Limites du niveau")]
    [SerializeField] private bool limiterAuNiveau = true;
    [Tooltip("Option 1 : glisse ici le décor (DecorationBaground ou le Canvas) : la caméra reste dans son rectangle")]
    [SerializeField] private RectTransform decor;

    [Header("Option 2 : limites à la main (utilisées si le champ Décor est vide)")]
    [Tooltip("Bord GAUCHE du niveau (coordonnée X dans le monde)")]
    [SerializeField] private float limiteGauche = -8.9f;
    [Tooltip("Bord DROIT du niveau")]
    [SerializeField] private float limiteDroite = 8.9f;
    [Tooltip("Bord BAS du niveau (utile seulement si Suivre Vertical est coché)")]
    [SerializeField] private float limiteBas = -5f;
    [Tooltip("Bord HAUT du niveau (utile seulement si Suivre Vertical est coché)")]
    [SerializeField] private float limiteHaut = 5f;

    private Camera cam;
    private Vector3 vitesse = Vector3.zero;
    private float departY;
    private float departZ;
    private Rect zone;

    private void Start()
    {
        cam = GetComponent<Camera>();
        departY = transform.position.y;
        departZ = transform.position.z;

        CalculerZone();
        TrouverCible();

        // Au départ, la caméra se place directement sur le joueur (sans glisser)
        if (cible != null)
        {
            transform.position = Limiter(PositionVoulue());
        }
    }

    private void LateUpdate()
    {
        if (cible == null)
        {
            TrouverCible();
            if (cible == null) return;
        }

        Vector3 voulue = Limiter(PositionVoulue());
        transform.position = Vector3.SmoothDamp(transform.position, voulue, ref vitesse, douceur);
    }

    private void TrouverCible()
    {
        if (cible != null) return;

        JoueurController joueur = FindFirstObjectByType<JoueurController>();
        if (joueur != null) cible = joueur.transform;
    }

    private Vector3 PositionVoulue()
    {
        float x = cible.position.x + decalage.x;
        float y = suivreVertical ? cible.position.y + decalage.y : departY;
        return new Vector3(x, y, departZ);
    }

    // Calcule le rectangle du niveau : soit depuis le décor, soit depuis les valeurs à la main
    private void CalculerZone()
    {
        if (decor != null)
        {
            Vector3[] coins = new Vector3[4];
            decor.GetWorldCorners(coins);

            float minX = coins[0].x, maxX = coins[0].x, minY = coins[0].y, maxY = coins[0].y;
            for (int i = 1; i < 4; i++)
            {
                minX = Mathf.Min(minX, coins[i].x);
                maxX = Mathf.Max(maxX, coins[i].x);
                minY = Mathf.Min(minY, coins[i].y);
                maxY = Mathf.Max(maxY, coins[i].y);
            }

            zone = new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        else
        {
            zone = new Rect(limiteGauche, limiteBas, limiteDroite - limiteGauche, limiteHaut - limiteBas);
        }
    }

    // Empêche la caméra de montrer ce qui est en dehors du niveau
    private Vector3 Limiter(Vector3 p)
    {
        if (!limiterAuNiveau) return p;

        float demiHauteur = cam.orthographicSize;
        float demiLargeur = demiHauteur * cam.aspect;

        float minX = zone.xMin + demiLargeur;
        float maxX = zone.xMax - demiLargeur;

        // Si le niveau est plus étroit que l'écran, on centre la caméra
        p.x = (minX > maxX) ? zone.center.x : Mathf.Clamp(p.x, minX, maxX);

        if (suivreVertical)
        {
            float minY = zone.yMin + demiHauteur;
            float maxY = zone.yMax - demiHauteur;
            p.y = (minY > maxY) ? zone.center.y : Mathf.Clamp(p.y, minY, maxY);
        }

        return p;
    }

    // Dans la Scene : montre le rectangle du niveau quand la caméra est sélectionnée
    private void OnDrawGizmosSelected()
    {
        Rect r;

        if (decor != null)
        {
            Vector3[] coins = new Vector3[4];
            decor.GetWorldCorners(coins);
            r = new Rect(coins[0].x, coins[0].y, coins[2].x - coins[0].x, coins[2].y - coins[0].y);
        }
        else
        {
            r = new Rect(limiteGauche, limiteBas, limiteDroite - limiteGauche, limiteHaut - limiteBas);
        }

        Gizmos.color = Color.yellow;
        Vector3 a = new Vector3(r.xMin, r.yMin, 0f);
        Vector3 b = new Vector3(r.xMax, r.yMin, 0f);
        Vector3 c = new Vector3(r.xMax, r.yMax, 0f);
        Vector3 d = new Vector3(r.xMin, r.yMax, 0f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }
}