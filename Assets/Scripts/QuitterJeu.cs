using System.Runtime.InteropServices;
using UnityEngine;

// À appeler à la place de Application.Quit().
// Dans le navigateur (WebGL), recharge la page pour revenir à l'écran d'accueil.
public static class QuitterJeu
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RechargerPage();
#endif

    public static void Quitter()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RechargerPage();
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}