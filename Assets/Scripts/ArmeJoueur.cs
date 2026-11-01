using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// À mettre sur l'objet Joueur dans chaque niveau : met l'arme choisie dans la main du personnage
public class ArmeJoueur : MonoBehaviour
{
    [Header("Les armes, dans l'ordre : 0 = Bat, 1 = Ruler")]
    [SerializeField] private Sprite[] armes;

    [Tooltip("-1 = utiliser le choix fait dans le menu, 0 ou 1 = forcer l'arme (pour tester)")]
    [SerializeField] private int armeDeTest = -1;

    [Header("Où se trouve l'arme sur le personnage")]
    [Tooltip("Mots cherchés dans le nom des parties du personnage (majuscules ignorées)")]
    [SerializeField] private string[] motsCles = { "weapon", "arme" };
    [Tooltip("Seulement si l'arme n'est pas trouvée toute seule : glisse ici son image (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer emplacementManuel;

    private readonly List<SpriteRenderer> emplacements = new List<SpriteRenderer>();
    private Sprite armeChoisie;

    private IEnumerator Start()
    {
        // On attend une image : le personnage non choisi a alors fini d'être supprimé
        yield return null;

        if (armes == null || armes.Length == 0)
        {
            Debug.LogWarning("ArmeJoueur : la liste des armes est vide.");
            yield break;
        }

        int choix = (armeDeTest >= 0) ? armeDeTest : PlayerPrefs.GetInt("ArmeChoisie", 0);
        choix = Mathf.Clamp(choix, 0, armes.Length - 1);
        armeChoisie = armes[choix];

        TrouverEmplacements();
    }

    private void TrouverEmplacements()
    {
        emplacements.Clear();

        if (emplacementManuel != null)
        {
            emplacements.Add(emplacementManuel);
            return;
        }

        SpriteRenderer[] tous = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in tous)
        {
            string nom = sr.gameObject.name.ToLower();

            foreach (string mot in motsCles)
            {
                if (!string.IsNullOrEmpty(mot) && nom.Contains(mot.ToLower()))
                {
                    emplacements.Add(sr);
                    Debug.Log("ArmeJoueur : emplacement de l'arme trouvé -> " + sr.gameObject.name);
                    break;
                }
            }
        }

        if (emplacements.Count == 0)
        {
            Debug.LogWarning("ArmeJoueur : aucune partie du personnage ne contient \"weapon\" ou \"arme\" dans son nom. Utilise le menu ⋮ du composant → \"Lister les images du personnage\", puis glisse la bonne dans Emplacement Manuel.");
        }
    }

    // LateUpdate : s'exécute après l'Animator, donc l'animation ne remet pas l'ancienne arme
    private void LateUpdate()
    {
        if (armeChoisie == null) return;

        foreach (SpriteRenderer sr in emplacements)
        {
            if (sr != null && sr.sprite != armeChoisie) sr.sprite = armeChoisie;
        }
    }

    // Outil : écrit dans la Console le nom de toutes les images du personnage
    [ContextMenu("Lister les images du personnage")]
    private void ListerLesImages()
    {
        SpriteRenderer[] tous = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in tous)
        {
            string sprite = (sr.sprite != null) ? sr.sprite.name : "(aucune image)";
            Debug.Log(sr.gameObject.name + "  ->  " + sprite, sr.gameObject);
        }
    }
}