using UnityEngine;

// À mettre sur l'objet GestionNiveau de chaque scène de niveau
public class SonsNiveau : MonoBehaviour
{
    public static SonsNiveau Instance { get; private set; }

    [Header("Sons du niveau")]
    [SerializeField] private AudioClip sonMortJoueur;
    [SerializeField, Range(0f, 1f)] private float volumeMortJoueur = 0.9f;

    private void Awake()
    {
        Instance = this;
    }

    public void JouerMortJoueur()
    {
        SonsJeu.Jouer(sonMortJoueur, volumeMortJoueur);
    }
}