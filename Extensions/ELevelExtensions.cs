using System;

namespace MessengerRando.Extensions;

public static class ELevelExtensions
{
    extension(ELevel level)
    {
        // This will not work for Surf, but it's fine for now.
        public string SceneName => level + "_Build";

        public static ELevel FromSceneName(string sceneName)
        {
            if (sceneName.EndsWith("_Build"))
                sceneName = sceneName.Substring(0, sceneName.Length - 6);

            foreach (ELevel value in Enum.GetValues(typeof(ELevel)))
                if (value.ToString() == sceneName)
                    return value;

            return ELevel.NONE;
        }
    }
}
