/// <summary>Shared sword progression boundary for merge logic and its UI.</summary>
public static class SwordRules
{
    public const int MaxLevel = 10;

    // Retains the existing piece progression: 2, 2, 3, 4, ...
    // The sentinel is only for display at the cap; merge callers also guard MaxLevel.
    public static int RequiredPieces(int level) => level >= MaxLevel ? 9999 : level <= 2 ? 2 : level;
}
