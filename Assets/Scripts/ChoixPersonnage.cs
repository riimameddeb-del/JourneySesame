using UnityEngine;
using UnityEngine.EventSystems;

// À mettre sur l'image d'un personnage dans la page Menu : un clic le choisit
public class ChoixPersonnage : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("0 = Student, 1 = Teacher")]
    [SerializeField] private int index = 0;

    [Tooltip("L'image grossit un peu quand la souris passe dessus (1 = pas d'effet)")]
    [SerializeField] private float agrandissementSurvol = 1.08f;

    private Vector3 echelleDepart;
    private bool echelleMemorisee = false;

    private void Awake()
    {
        MemoriserEchelle();
    }

    private void OnDisable()
    {
        // Quand la carte se cache, on remet la taille normale
        if (echelleMemorisee) transform.localScale = echelleDepart;
    }

    private void MemoriserEchelle()
    {
        if (echelleMemorisee) return;
        echelleDepart = transform.localScale;
        echelleMemorisee = true;
    }

    public void OnPointerClick(PointerEventData donnees)
    {
        string nom = (index == 0) ? "Student" : "Teacher";
        Debug.Log("Personnage choisi : " + nom);

        // On enregistre le choix
        PlayerPrefs.SetInt("PersonnageChoisi", index);
        PlayerPrefs.Save();

        // Puis on passe au choix de l'arme
        MenuChoix menu = FindFirstObjectByType<MenuChoix>();

        if (menu != null)
        {
            menu.MontrerChoixArme();
            return;
        }

        // Sécurité : sans MenuChoix, on garde l'ancien comportement (page Level directe)
        PageManager gestion = FindFirstObjectByType<PageManager>();
        if (gestion != null) gestion.AllerLevel();
    }

    public void OnPointerEnter(PointerEventData donnees)
    {
        MemoriserEchelle();
        transform.localScale = echelleDepart * agrandissementSurvol;
    }

    public void OnPointerExit(PointerEventData donnees)
    {
        MemoriserEchelle();
        transform.localScale = echelleDepart;
    }
}