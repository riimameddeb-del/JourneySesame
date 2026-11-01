using UnityEngine;

// Pour les scies et le mace : tourne sur lui-même et/ou fait des allers-retours
[RequireComponent(typeof(Rigidbody2D))]
public class ObstacleMobile : MonoBehaviour
{
    [Header("Rotation (pour une scie). 0 = ne tourne pas")]
    [SerializeField] private float vitesseRotation = 360f;

    [Header("Va-et-vient autour de la position de départ")]
    [Tooltip("Distance parcourue de chaque côté du point de départ (X = horizontal, Y = vertical)")]
    [SerializeField] private Vector2 amplitude = new Vector2(0f, 1.5f);

    [Tooltip("Nombre d'allers-retours par seconde (0.5 = un aller-retour toutes les 2 secondes)")]
    [SerializeField] private float vitesse = 0.5f;

    [Tooltip("Décale le mouvement (0 à 1) pour que plusieurs obstacles ne bougent pas en même temps")]
    [SerializeField, Range(0f, 1f)] private float decalage = 0f;

    private Rigidbody2D rb;
    private Vector2 depart;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        depart = transform.position;
    }

    private void FixedUpdate()
    {
        float t = Mathf.Sin((Time.time * vitesse + decalage) * 2f * Mathf.PI);
        rb.MovePosition(depart + amplitude * t);

        if (vitesseRotation != 0f)
        {
            rb.MoveRotation(rb.rotation - vitesseRotation * Time.fixedDeltaTime);
        }
    }
}