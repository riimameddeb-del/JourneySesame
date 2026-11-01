using UnityEngine;
using UnityEngine.SceneManagement;

// Musique du jeu : continue sans coupure d'une scène à l'autre
public class MusiqueJeu : MonoBehaviour
{
    public static MusiqueJeu Instance { get; private set; }

    [Header("Musiques (tu peux mettre la même pour les deux)")]
    [SerializeField] private AudioClip musiqueMenu;
    [SerializeField] private AudioClip musiqueNiveaux;
    [Tooltip("Nom de la scène du menu principal")]
    [SerializeField] private string nomSceneMenu = "SampleScene";

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource source;

    private void Awake()
    {
        // Une seule musique dans tout le jeu : si une autre existe déjà, celle-ci disparaît
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;

        SceneManager.sceneLoaded += QuandSceneChargee;
    }

    private void Start()
    {
        if (Instance != this) return;
        ChoisirMusique(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= QuandSceneChargee;
    }

    private void Update()
    {
        // Permet de régler le volume en direct pendant le jeu
        if (source != null) source.volume = volume;
    }

    private void QuandSceneChargee(Scene scene, LoadSceneMode mode)
    {
        ChoisirMusique(scene.name);
    }

    private void ChoisirMusique(string nomScene)
    {
        AudioClip voulue = (nomScene == nomSceneMenu) ? musiqueMenu : musiqueNiveaux;

        // Si une des deux est vide, on prend l'autre
        if (voulue == null) voulue = (musiqueMenu != null) ? musiqueMenu : musiqueNiveaux;
        if (voulue == null) return;

        // Même musique que celle déjà en cours : on ne la relance pas
        if (source.clip == voulue && source.isPlaying) return;

        source.clip = voulue;
        source.Play();
    }
}