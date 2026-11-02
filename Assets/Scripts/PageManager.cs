using UnityEngine;
using UnityEngine.SceneManagement;

public class PageManager : MonoBehaviour
{
    // Quand on revient d'un niveau, on veut tomber sur la page Level et pas sur l'accueil
    public static bool ouvrirPageLevelAuDemarrage = false;

    [Header("Les 3 pages (les objets racines)")]
    [SerializeField] private GameObject pageAccueil;
    [SerializeField] private GameObject pageMenu;
    [SerializeField] private GameObject pageLevel;

    [Header("Noms des scènes des niveaux (Niveau1, Niveau2, Niveau3 dans cet ordre)")]
    [SerializeField] private string[] scenesNiveaux = { "Niveau1", "Niveau2", "Niveau3" };

    private void Start()
    {
        if (ouvrirPageLevelAuDemarrage)
        {
            ouvrirPageLevelAuDemarrage = false;
            AllerLevel();
        }
        else
        {
            AllerAccueil();
        }
    }

    private void AfficherSeulement(GameObject page)
    {
        if (pageAccueil != null) pageAccueil.SetActive(pageAccueil == page);
        if (pageMenu != null) pageMenu.SetActive(pageMenu == page);
        if (pageLevel != null) pageLevel.SetActive(pageLevel == page);
    }

    // ----- À brancher sur les boutons -----

    public void AllerAccueil()
    {
        AfficherSeulement(pageAccueil);
    }

    public void AllerMenu()
    {
        AfficherSeulement(pageMenu);
    }

    public void AllerLevel()
    {
        AfficherSeulement(pageLevel);
    }

    // numero = 1, 2 ou 3 : charge la scène du niveau correspondant
    public void AllerNiveau(int numero)
    {
        int index = numero - 1;

        if (scenesNiveaux == null || index < 0 || index >= scenesNiveaux.Length)
        {
            Debug.LogWarning("PageManager : aucun nom de scène pour le niveau " + numero + ".");
            return;
        }

        string nomScene = scenesNiveaux[index];

        if (!Application.CanStreamedLevelBeLoaded(nomScene))
        {
            Debug.LogWarning("PageManager : la scène \"" + nomScene + "\" est introuvable. Il faut l'ajouter dans File > Build Profiles > Scene List.");
            return;
        }

        SceneManager.LoadScene(nomScene);
    }

    // 0 = Student, 1 = Teacher : on retient le choix puis on va à la page des niveaux
    public void ChoisirPersonnage(int index)
    {
        PlayerPrefs.SetInt("PersonnageChoisi", index);
        PlayerPrefs.Save();
        AllerLevel();
    }

    // Branché sur le bouton Quit de l'accueil.
    // Dans le navigateur, recharge la page ; dans l'éditeur, arrête le jeu ; sinon quitte l'application.
    public void QuitterJeu()
    {
        global::QuitterJeu.Quitter();
    }
}