using TMPro;
using UnityEngine;

// À mettre sur l'objet "Menu" (la racine de la page Menu) : gère les deux étapes du choix
public class MenuChoix : MonoBehaviour
{
    [Header("Le titre qui change")]
    [SerializeField] private TMP_Text titre;
    [SerializeField] private string texteChoixPersonnage = "Choose your character";
    [SerializeField] private string texteChoixArme = "Choose your weapon";

    [Header("Étape 1 : tout ce qui sert à choisir le personnage (cartes, images, noms)")]
    [SerializeField] private GameObject[] elementsPersonnages;

    [Header("Étape 2 : tout ce qui sert à choisir l'arme")]
    [SerializeField] private GameObject[] elementsArmes;

    private int etape = 1;

    private void OnEnable()
    {
        // Chaque fois que la page Menu s'ouvre, on repart du choix du personnage
        MontrerChoixPersonnage();
    }

    public void MontrerChoixPersonnage()
    {
        etape = 1;
        Appliquer();
    }

    public void MontrerChoixArme()
    {
        etape = 2;
        Appliquer();
    }

    private void Appliquer()
    {
        bool etapePersonnage = (etape == 1);

        Activer(elementsPersonnages, etapePersonnage);
        Activer(elementsArmes, !etapePersonnage);

        if (titre != null)
        {
            titre.text = etapePersonnage ? texteChoixPersonnage : texteChoixArme;
        }
    }

    private void Activer(GameObject[] objets, bool actif)
    {
        if (objets == null) return;

        foreach (GameObject objet in objets)
        {
            if (objet != null) objet.SetActive(actif);
        }
    }

    // À brancher sur le bouton "Return" de la page Menu
    public void Retour()
    {
        if (etape == 2)
        {
            MontrerChoixPersonnage();
            return;
        }

        PageManager gestion = FindFirstObjectByType<PageManager>();
        if (gestion != null) gestion.AllerAccueil();
    }
}