namespace MessengerRando.Extensions;

public static class ELevelExtensions
{
    extension(ELevel level)
    {
        public string SceneName => level + "_Build";
    }
}
