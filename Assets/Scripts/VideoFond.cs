using System.IO;
using UnityEngine;
using UnityEngine.Video;

// À mettre sur l'objet qui a le composant Video Player (fond d'écran animé)
// Lit la vidéo depuis le dossier StreamingAssets, ce qui fonctionne dans le navigateur (WebGL)
[RequireComponent(typeof(VideoPlayer))]
public class VideoFond : MonoBehaviour
{
    [Tooltip("Nom exact du fichier vidéo placé dans Assets/StreamingAssets, avec l'extension (ex : fond_accueil.mp4)")]
    [SerializeField] private string nomFichier = "";

    private VideoPlayer lecteur;

    private void Awake()
    {
        lecteur = GetComponent<VideoPlayer>();
        lecteur.playOnAwake = false;
        lecteur.isLooping = true;
        lecteur.skipOnDrop = true;

        // Pas de son dans la vidéo : la musique du jeu suffit
        lecteur.audioOutputMode = VideoAudioOutputMode.None;
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(nomFichier))
        {
            Debug.LogWarning("VideoFond : aucun nom de fichier renseigné sur " + gameObject.name);
            return;
        }

        lecteur.source = VideoSource.Url;
        lecteur.url = Path.Combine(Application.streamingAssetsPath, nomFichier);
        lecteur.Play();
    }
}