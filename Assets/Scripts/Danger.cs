using UnityEngine;

// À mettre sur tout ce qui tue le joueur au contact : piques, scies, mace, zone sous le niveau
public class Danger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D autre)
    {
        Toucher(autre);
    }

    private void OnTriggerStay2D(Collider2D autre)
    {
        Toucher(autre);
    }

    private void OnCollisionEnter2D(Collision2D autre)
    {
        Toucher(autre.collider);
    }

    private void Toucher(Collider2D autre)
    {
        JoueurController joueur = autre.GetComponentInParent<JoueurController>();
        if (joueur == null) return;

        SanteJoueur sante = joueur.GetComponent<SanteJoueur>();
        if (sante == null) sante = joueur.gameObject.AddComponent<SanteJoueur>();

        sante.Mourir();
    }
}