using UnityEngine;

// À mettre sur la ligne d'arrivée à la fin du niveau
[RequireComponent(typeof(BoxCollider2D))]
public class LigneArrivee : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D autre)
    {
        JoueurController joueur = autre.GetComponentInParent<JoueurController>();
        if (joueur == null) return;

        if (GestionNiveau.Instance != null)
        {
            GestionNiveau.Instance.ArriveeAtteinte(joueur);
        }
    }
}