using UnityEngine;

// Remet la progression à zéro à chaque lancement du jeu dans le navigateur (WebGL).
// Ce script n'a pas besoin d'être placé sur un objet : il s'exécute tout seul, une seule fois, au démarrage.
public static class ResetAuDemarrage
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Reinitialiser()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
#endif
}