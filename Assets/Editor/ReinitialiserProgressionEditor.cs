using UnityEditor;
using UnityEngine;

// Ajoute le gros bouton rouge dans l'Inspector du composant ReinitialiserProgression
[CustomEditor(typeof(ReinitialiserProgression))]
public class ReinitialiserProgressionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Ce bouton remet tout à zéro : niveaux 2 et 3 bloqués, 3 vies, et personnage effacé si la case est cochée. Il fonctionne même sans lancer le jeu.",
            MessageType.Info);

        Color ancienneCouleur = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.55f, 0.45f);

        if (GUILayout.Button("Tout rebloquer (recommencer le jeu)", GUILayout.Height(36)))
        {
            bool ok = EditorUtility.DisplayDialog(
                "Recommencer tout le jeu ?",
                "Tous les niveaux seront de nouveau bloqués, sauf le niveau 1. Cette action est immédiate.",
                "Oui, tout effacer",
                "Annuler");

            if (ok)
            {
                ((ReinitialiserProgression)target).ToutReinitialiser();
                Debug.Log("Progression effacée : seul le niveau 1 est jouable.");
            }
        }

        GUI.backgroundColor = ancienneCouleur;
    }

    // Même chose depuis la barre de menu : Outils → Recommencer tout le jeu
    [MenuItem("Outils/Recommencer tout le jeu (rebloquer les niveaux)")]
    private static void RecommencerDepuisLeMenu()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Recommencer tout le jeu ?",
            "Tous les niveaux seront de nouveau bloqués, sauf le niveau 1.",
            "Oui, tout effacer",
            "Annuler");

        if (!ok) return;

        LevelProgress.Reinitialiser();
        ViesJoueur.Reinitialiser();
        ViesJoueur.rechargementApresMort = false;
        PlayerPrefs.DeleteKey("PersonnageChoisi");
        PlayerPrefs.Save();

        Debug.Log("Progression effacée : seul le niveau 1 est jouable.");
    }
}