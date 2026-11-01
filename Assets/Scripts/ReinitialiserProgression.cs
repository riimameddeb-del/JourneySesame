using UnityEngine;

// Outil pour toi (développeuse) : à mettre sur GameManager dans SampleScene.
// Le bouton "Tout rebloquer" apparaît dans l'Inspector. Il n'existe pas dans le jeu.
public class ReinitialiserProgression : MonoBehaviour
{
    [Tooltip("Efface aussi le personnage choisi dans le menu")]
    public bool effacerAussiLePersonnage = true;

    public void ToutReinitialiser()
    {
        // Les niveaux 2 et 3 redeviennent bloqués, le niveau 1 est de nouveau le seul jouable
        LevelProgress.Reinitialiser();

        // Les 3 vies reviennent
        ViesJoueur.Reinitialiser();
        ViesJoueur.rechargementApresMort = false;

        if (effacerAussiLePersonnage)
        {
            PlayerPrefs.DeleteKey("PersonnageChoisi");
        }

        PlayerPrefs.Save();
    }
}