using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = MessengerRando.Utils.Logger;
using Random = System.Random;

namespace MessengerRando.GameOverrideManagers;

public static class RandoMusicManager
{
    private static readonly Logger logger = Logger.GetLogger(typeof(RandoMusicManager));
    private static Random random;
    public static bool ShuffleMusic;

    public static Dictionary<string, AudioObjectDefinition> NameToMusic;
    public static List<ShopJukeboxTrack> ShopTracks;
    public static List<LevelJukeboxTrack> LevelTracks;

    public static void BuildMusicLibrary()
    {
        try
        {
            random = new Random();
            ShopTracks = Manager<AudioManager>.Instance.ShopJukeboxTracks;
            LevelTracks = Manager<AudioManager>.Instance.LevelJukeboxTracks;
        }
        catch (Exception e)
        {
            logger.Exception(e);
        }
    }

    public static MusicAudioObject OnPlayMusic(
        On.AudioManager.orig_PlayMusic orig,
        AudioManager self,
        AudioObjectDefinition audioObjectDefinition,
        bool loop,
        float fadeInDuration,
        float playbackTime,
        GameObject customAudioObject)
    {
        if (audioObjectDefinition == null)
            return null;
#if DEBUG
        logger.Log("Playing music {0}", audioObjectDefinition.GetInstanceID());
#endif
        if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_05_B_SunkenShrine))
        {
            self.levelMusicShuffle = false;
        }
        else
        {
            self.levelMusicShuffle = ShuffleMusic;
        }
        if (!audioObjectDefinition.IsMusic() || !ShuffleMusic ||
            !Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_13_TowerOfTimeHQ))
        {
            return orig(self, audioObjectDefinition, loop, fadeInDuration, playbackTime, customAudioObject);
        }
        var newAudio = self.GetRandomLevelTrack();
        var dimension = random.Next(0, 2);
        audioObjectDefinition = dimension == 0 ? newAudio.track_8 : newAudio.track_16;
        self.StopMusic();
        return orig(self, audioObjectDefinition, loop, fadeInDuration, playbackTime, customAudioObject);
    }
}