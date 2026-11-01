using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// S'ajoute tout seul sur le joueur quand il touche un danger
public class SanteJoueur : MonoBehaviour
{
    [Tooltip("Nom exact de l'état de mort dans l'Animator")]
    [SerializeField] private string etatMort = "Death";
    [SerializeField] private float delaiAvantSuite = 2f;

    private bool mort = false;

    public bool EstMort
    {
        get { return mort; }
    }

    public void Mourir()
    {
        if (mort) return;
        mort = true;

        // On coupe les commandes et la physique
        JoueurController controle = GetComponent<JoueurController>();
        if (controle != null) controle.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Son de mort (réglé sur l'objet GestionNiveau, composant Sons Niveau)
        if (SonsNiveau.Instance != null) SonsNiveau.Instance.JouerMortJoueur();

        // Animation de mort : jouée une seule fois, puis figée sur la dernière image
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            int hash = Animator.StringToHash(etatMort);
            if (animator.HasState(0, hash))
            {
                animator.CrossFade(hash, 0.05f, 0, 0f);
                StartCoroutine(FigerALaFin(animator, hash));
            }
        }

        // Une vie en moins
        ViesJoueur.PerdreUneVie();
        if (GestionNiveau.Instance != null) GestionNiveau.Instance.MettreAJourVies();

        StartCoroutine(Suite());
    }

    // Arrête l'animation quand elle arrive à sa fin, pour qu'elle ne boucle pas
    private IEnumerator FigerALaFin(Animator animator, int hash)
    {
        float attente = 0f;

        while (attente < 4f && animator != null)
        {
            attente += Time.deltaTime;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.shortNameHash == hash && info.normalizedTime >= 0.97f)
            {
                animator.speed = 0f;
                yield break;
            }

            yield return null;
        }
    }

    // S'il reste des vies : le niveau recommence. Sinon : GAME OVER.
    private IEnumerator Suite()
    {
        yield return new WaitForSeconds(delaiAvantSuite);

        if (ViesJoueur.Vies > 0)
        {
            ViesJoueur.rechargementApresMort = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (GestionNiveau.Instance != null)
        {
            GestionNiveau.Instance.AfficherGameOver();
        }
        else
        {
            // Sécurité : pas de GestionNiveau dans la scène, on recommence simplement
            ViesJoueur.Reinitialiser();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}