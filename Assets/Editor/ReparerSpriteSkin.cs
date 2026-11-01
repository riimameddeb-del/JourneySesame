using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D.Animation;

// Outil de l'éditeur : désactive les Sprite Skin qui n'ont aucun os
// (ils font planter le calcul de déformation des personnages)
public static class ReparerSpriteSkin
{
    [MenuItem("Outils/Désactiver les Sprite Skin sans os")]
    private static void Reparer()
    {
        SpriteSkin[] skins = Object.FindObjectsByType<SpriteSkin>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int desactives = 0;

        foreach (SpriteSkin skin in skins)
        {
            if (!skin.enabled) continue;

            bool sansOs = (skin.boneTransforms == null || skin.boneTransforms.Length == 0);
            if (!sansOs) continue;

            Undo.RecordObject(skin, "Désactiver Sprite Skin sans os");
            skin.enabled = false;
            EditorUtility.SetDirty(skin);
            desactives++;

            Debug.Log("Sprite Skin désactivé sur : " + skin.gameObject.name, skin.gameObject);
        }

        if (desactives > 0) EditorSceneManager.MarkAllScenesDirty();

        Debug.Log("Outil terminé : " + desactives + " Sprite Skin désactivé(s) sur " + skins.Length + " trouvé(s).");
    }
}