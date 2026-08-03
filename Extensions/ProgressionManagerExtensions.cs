namespace MessengerRando.Extensions;

public static class ProgressionManagerExtensions
{
    public static void SetCutsceneAsPlayed<T>(this ProgressionManager manager)
        where T : Cutscene
    {
        manager.SetCutsceneAsPlayed(typeof(T).ToString());
    }
}
