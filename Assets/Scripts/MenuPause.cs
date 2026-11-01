using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// À mettre sur l'objet GestionNiveau (ou un objet vide) dans chaque scène de niveau
public class MenuPause : MonoBehaviour
{
    [Header("Panneau de pause (éteint au départ)")]
    [SerializeField] private GameObject panneauPause;

    private JoueurController joueur;
    private bool enPause = false;

    private void Start()
    {
        enPause = false;
        if (panneauPause != null) panneauPause.SetActive(false);
        joueur = FindFirstObjectByType<JoueurController>();
    }

    private void OnDestroy()
    {
        // Sécurité : on ne laisse jamais le jeu en pause en changeant de scène
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (EchapAppuye())
        {
            if (enPause) Reprendre();
            else Pause();
        }
    }

    // ----- À brancher sur les boutons -----

    public void Pause()
    {
        if (enPause || !PauseAutorisee()) return;

        enPause = true;
        Time.timeScale = 0f;

        if (panneauPause != null) panneauPause.SetActive(true);

        // Le personnage ne réagit plus aux touches pendant la pause
        if (joueur != null) joueur.enabled = false;
    }

    public void Reprendre()
    {
        if (!enPause) return;

        enPause = false;
        Time.timeScale = 1f;

        if (panneauPause != null) panneauPause.SetActive(false);

        if (joueur != null) joueur.enabled = true;
    }

    // ----- Intérieur -----

    // Pas de pause si le joueur est mort, a gagné, ou si un écran de fin est affiché
    private bool PauseAutorisee()
    {
        if (Time.timeScale == 0f) return false;

        if (joueur == null) joueur = FindFirstObjectByType<JoueurController>();

        // Quand le joueur est mort ou a gagné, son contrôle est coupé
        if (joueur != null && !joueur.enabled) return false;

        return true;
    }

    private bool EchapAppuye()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return false;
        return k.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}