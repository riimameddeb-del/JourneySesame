using UnityEngine;

// Outil pour jouer un son une fois, où que l'on soit dans le jeu
public static class SonsJeu
{
    public static void Jouer(AudioClip clip, float volume = 0.8f)
    {
        if (clip == null) return;

        // Son "2D" : même volume partout dans le niveau
        GameObject objet = new GameObject("Son_" + clip.name);
        AudioSource source = objet.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.spatialBlend = 0f;
        source.Play();

        Object.Destroy(objet, clip.length + 0.2f);
    }
}