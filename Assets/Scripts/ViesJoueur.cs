// Garde en mémoire le nombre de vies, même quand le niveau recommence
public static class ViesJoueur
{
    public const int ViesMax = 3;

    private static int vies = ViesMax;

    // Vrai quand le niveau recommence parce que le joueur est mort (les vies sont alors conservées)
    public static bool rechargementApresMort = false;

    public static int Vies
    {
        get { return vies; }
    }

    public static void Reinitialiser()
    {
        vies = ViesMax;
    }

    public static void PerdreUneVie()
    {
        if (vies > 0) vies--;
    }
}