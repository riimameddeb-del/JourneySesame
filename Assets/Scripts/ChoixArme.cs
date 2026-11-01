using UnityEngine;
using UnityEngine.EventSystems;

// À mettre sur la carte d'une arme dans la page Menu : un clic la choisit puis ouvre la page Level
public class ChoixArme : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("0 = Bat, 1 = Ruler")]
    [SerializeField] private int index = 0;

    [Tooltip("La carte grossit un peu quand la souris passe dessus (1 = pas d'effet)")]
    [SerializeField] private float agrandissementSurvol = 1.08f;

    private Vector3 echelleDepart;
    private bool echelleMemorisee = false;

    private void Awake()
    {
        MemoriserEchelle();
    }

    private void OnDisable()
    {
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
        string nom = (index == 0) ? "Bat" : "Ruler";
        Debug.Log("Arme choisie : " + nom);

        PlayerPrefs.SetInt("ArmeChoisie", index);
        PlayerPrefs.Save();

        PageManager gestion = FindFirstObjectByType<PageManager>();
        if (gestion != null) gestion.AllerLevel();
        else Debug.LogWarning("ChoixArme : aucun PageManager trouvé dans la scène (GameManager).");
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