using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BurstLinq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using HeavenStudio.Common;

namespace HeavenStudio.Util
{
    public class SoundByte
    {
        /// <summary>
        /// How many beats before a voice line a game decides it and schedules it. Japanese voices often have to start a
        /// little before the beat to sound on it, which a sound started right on its beat can't do.
        /// </summary>
        public const double VoiceLead = 0.25;

        /// <summary>VoiceLead, but never less than 0.15 seconds (at fast tempos a quarter beat is too short for some voices)</summary>
        public static double VoiceLeadAt(double beat)
        {
            var cond = Conductor.instance;
            if (cond == null) return VoiceLead;
            return Math.Max(VoiceLead, cond.SecsToBeats(0.15, cond.GetBpmAtBeat(beat)));
        }

        static GameObject oneShotAudioSourceObject;
        static AudioSource oneShotAudioSource;
        //static int soundIdx = 0;    Unused value - Marc

        public static Dictionary<string, AudioClip> audioClips { get; private set; } = new Dictionary<string, AudioClip>();

        public enum AudioType
        {
            OGG,
            WAV
        }

        public static Sound GetAvailableScheduledSound()
        {
            // soundIdx++;
            // soundIdx %= GameManager.instance.SoundObjects.Count;
            // GameManager.instance.SoundObjects[soundIdx].Stop();
            // return GameManager.instance.SoundObjects[soundIdx];

            return GameManager.instance.SoundObjects.Get();
        }

        /// <summary>
        /// Ensures that the jukebox and one-shot audio source exist.
        /// </summary>
        public static void BasicCheck()
        {
            if (oneShotAudioSourceObject == null)
            {
                oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                oneShotAudioSource.outputAudioMixerGroup = GlobalGameManager.sfxGroup;
                UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
            }
        }

        /// <summary>
        ///    Stops all currently playing sounds.
        /// </summary>
        public static void KillOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.Stop();
            }
        }

        /// <summary>
        ///    Pauses all currently playing sounds.
        /// </summary>
        public static void PauseOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.Pause();
            }
        }

        /// <summary>
        ///    Unpauses all currently playing sounds.
        /// </summary>
        public static void UnpauseOneShots()
        {
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.UnPause();
            }
        }

        public static void PreloadGameAudioClips(Minigames.Minigame inf)
        {
            if (inf.UsesAssetBundle)
            {
                var cmnAb = inf.GetResourcesAssetBundle();
                if (cmnAb != null)
                {
                    cmnAb.LoadAllAssetsAsync<AudioClip>().completed += (op) =>
                    {
                        foreach (var clip in (op as AssetBundleRequest).allAssets.Cast<AudioClip>())
                        {
                            OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                        }
                    };
                }
                var locAb = inf.GetAudioAssetBundle();
                if (locAb != null)
                {
                    locAb.LoadAllAssetsAsync<AudioClip>().completed += (op) =>
                    {
                        foreach (var clip in (op as AssetBundleRequest).allAssets.Cast<AudioClip>())
                        {
                            OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                        }
                    };
                }
            }
            else
            {
                string path = $"Sfx/games/{inf.name}";
                var clips = Resources.LoadAll<AudioClip>(path);
                foreach (var clip in clips)
                {
                    clip.LoadAudioData();
                    OnResourceLoaded(clip, $"games/{inf.name}/{clip.name}");
                }
            }
        }

        public static void PreloadGameAudioClips(string game)
        {
            var inf = GameManager.instance.GetGameInfo(game);
            PreloadGameAudioClips(inf);
        }

        public static void PreloadAudioClipAsync(string name, string game)
        {
            var inf = GameManager.instance.GetGameInfo(game);
            if (inf != null)
            {
                name = $"games/{name}";
            }
            if (audioClips.ContainsKey(name)) return;
            if (inf.UsesAssetBundle)
            {
                var cmnAb = inf.GetResourcesAssetBundle();
                if (cmnAb != null && cmnAb.Contains(name))
                {
                    var request = cmnAb.LoadAssetAsync<AudioClip>(name);
                    request.completed += (op) =>
                    {
                        OnResourceLoaded((op as ResourceRequest).asset as AudioClip, $"{game}/{name}");
                    };
                }
                else
                {
                    var locAb = inf.GetAudioAssetBundle();
                    if (locAb != null && locAb.Contains(name))
                    {
                        var request = locAb.LoadAssetAsync<AudioClip>(name);
                        request.completed += (op) =>
                        {
                            OnResourceLoaded((op as ResourceRequest).asset as AudioClip, $"{game}/{name}");
                        };
                    }
                }
            }
            else
            {
                PreloadAudioClipAsync($"{game}/{name}");
            }
        }

        public static void PreloadAudioClipAsync(string name)
        {
            if (audioClips.ContainsKey(name)) return;
            string path = $"Sfx/{name}";
            ResourceRequest request = Resources.LoadAsync<AudioClip>(path);
            request.completed += (op) =>
            {
                OnResourceLoaded((op as ResourceRequest).asset as AudioClip, name);
            };
        }

        static void OnResourceLoaded(AudioClip clip, string name)
        {
            if (audioClips.ContainsKey(name))
            {
                audioClips[name] = clip;
            }
            else
            {
                audioClips.Add(name, clip);
            }
        }

        public static void UnloadAudioClips(params string[] names)
        {
            foreach (string s in names)
            {
                if (audioClips.ContainsKey(s)) audioClips.Remove(s);
            }
            Resources.UnloadUnusedAssets();
        }

        public static void UnloadAudioClips()
        {
            audioClips.Clear();
            Resources.UnloadUnusedAssets();
        }

        public static void UnloadAudioClips(string game)
        {
            string[] split;
            foreach (string s in audioClips.Where(x =>
            {
                split = x.Key.Split('/');
                return split.Length > 2 && split[0] == "games" && split[1] == game;
            }).Select(x => x.Key).ToList())
            {
                audioClips.Remove(s);
            }
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        ///    Gets the length of an audio clip
        /// </summary>
        public static double GetClipLength(string name, float pitch = 1f, string game = null)
        {
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
                    var inf = GameManager.instance.GetGameInfo(game);
                    //first try the game's common assetbundle
                    // Debug.Log("Jukebox loading sound " + soundName + " from common");
                    clip = inf.GetResourcesAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    //then the localized one
                    if (clip == null)
                    {
                        // Debug.Log("Jukebox loading sound " + soundName + " from locale");
                        clip = inf.GetAudioAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    }
                }
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = LoadSfx(name);
                }
            }

            if (clip == null)
            {
                Debug.LogError($"Could not load clip {name}");
                return double.NaN;
            }
            return clip.length / pitch;
        }

        /// <summary>
        ///    Gets the length of an audio clip
        ///    Audio clip is fetched from minigame resources
        /// </summary>
        public static double GetClipLengthGame(string name, float pitch = 1f)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (inf != null)
            {
                return GetClipLength($"games/{name}", pitch, inf.UsesAssetBundle ? gameName : null);
            }

            return double.NaN;
        }

        /// <summary>
        ///    Fires a one-shot sound.
        ///    Unpitched, non-scheduled, non-looping sounds are played using a global One-Shot audio source that doesn't create a Sound object.
        ///    Looped sounds return their created Sound object so they can be canceled after creation.
        /// </summary>
        /// <summary>
        ///    Loads a sound effect from Resources/Sfx, preferring the edition's language version (ex. count-ins/jp/one1)
        /// </summary>
        static AudioClip LoadSfx(string name)
        {
            string localized = Localization.LocalizedSfx(name);
            AudioClip clip = localized != name ? Resources.Load<AudioClip>($"Sfx/{localized}") : null;
            if (clip == null) clip = Resources.Load<AudioClip>($"Sfx/{name}");
            return clip;
        }

        public static Sound PlayOneShot(string name, double beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, string game = null, double offset = 0f, bool ignoreConductorPause = false, bool ignoreSwing = false, float panning = 0f)
        {
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
                    var inf = GameManager.instance.GetGameInfo(game);
                    //first try the game's common assetbundle
                    // Debug.Log("Jukebox loading sound " + soundName + " from common");
                    clip = inf.GetResourcesAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    //then the localized one
                    if (clip == null)
                    {
                        // Debug.Log("Jukebox loading sound " + soundName + " from locale");
                        clip = inf.GetAudioAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    }
                }
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = LoadSfx(name);
                }
            }

            if (looping || beat != -1 || pitch != 1f || panning != 0f)
            {
                Sound snd = GetAvailableScheduledSound();

                snd.clip = clip;
                snd.pitch = pitch;
                snd.volume = volume;
                snd.panning = panning;
                snd.looping = looping;
                snd.offset = offset;
                snd.ignoreConductorPause = ignoreConductorPause;
                if (ignoreSwing) snd.beat = Conductor.instance.GetSwungBeat(beat);
                else snd.beat = beat;
                snd.Play();

                return snd;
            }
            else
            {
                if (oneShotAudioSourceObject == null)
                {
                    oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                    oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                    oneShotAudioSource.outputAudioMixerGroup = GlobalGameManager.sfxGroup;
                    UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
                }

                oneShotAudioSource.PlayOneShot(clip, volume);
                // Debug.Log("Played Sound");
                return null;
            }
        }

        /// <summary>
        ///    Schedules a sound to be played at a specific time in seconds.
        /// </summary>
        public static Sound PlayOneShotScheduled(string name, double targetTime, float pitch = 1f, float volume = 1f, bool looping = false, string game = null, bool ignoreConductorPause = false, bool ignoreSwing = false, float panning = 0f)
        {
            Sound snd = GetAvailableScheduledSound();
            AudioClip clip = null;
            string soundName = name.Split('/')[^1];
            if (game != null)
            {
                string cachedName = $"games/{game}/{soundName}";
                if (audioClips.ContainsKey(cachedName))
                {
                    clip = audioClips[cachedName];
                }
                else
                {
                    var inf = GameManager.instance.GetGameInfo(game);
                    //first try the game's common assetbundle
                    // Debug.Log("Jukebox loading sound " + soundName + " from common");
                    clip = inf.GetResourcesAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    //then the localized one
                    if (clip == null)
                    {
                        // Debug.Log("Jukebox loading sound " + soundName + " from locale");
                        clip = inf.GetAudioAssetBundle()?.LoadAsset<AudioClip>(soundName);
                    }
                }
            }

            //can't load from assetbundle, load from resources
            if (clip == null)
            {
                if (audioClips.ContainsKey(name))
                {
                    clip = audioClips[name];
                }
                else
                {
                    // Debug.Log("Jukebox loading sound " + name + " from resources");
                    clip = LoadSfx(name);
                }
            }

            // abort if no clip found

            snd.clip = clip;
            snd.pitch = pitch;
            snd.volume = volume;
            snd.panning = panning;
            snd.looping = looping;

            snd.scheduled = true;
            if (ignoreSwing) snd.scheduledTime = Conductor.instance.GetSwungBeat(targetTime);
            else snd.scheduledTime = targetTime;
            snd.ignoreConductorPause = ignoreConductorPause;
            snd.Play();

            return snd;
        }

        /// <summary>
        ///    Fires a one-shot sound located in minigame resources.
        ///    Unpitched, non-scheduled, non-looping sounds are played using a global One-Shot audio source that doesn't create a Sound object.
        ///    Looped sounds return their created Sound object so they can be canceled after creation.
        /// </summary>
        /// <summary>
        /// Move a sound's beat by a number of straight (unswung) beats. A japanese voice is shifted from where the english
        /// one plays; with swing on, the shift has to be measured from where that sound really lands, or the
        /// syllables between the swung notes stretch and squash.
        /// </summary>
        static double ShiftBeat(double beat, double by, bool ignoreSwing)
        {
            if (by == 0 || ignoreSwing || Conductor.instance == null) return beat + by;  // ignoreSwing: the beat is still straight here
            var cond = Conductor.instance;
            return cond.GetSwungBeat(cond.GetUnSwungBeat(beat) + by);
        }

        public static Sound PlayOneShotGame(string name, double beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, bool forcePlay = false, double offset = 0f, bool ignoreConductorPause = false, bool ignoreSwing = false, float panning = 0f)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (GameManager.instance.currentGame == gameName || forcePlay)
            {
                if (Localization.TryGetVoiceTiming(name, out var timing))
                {
                    if (timing.extra != null && beat >= 0)
                    {
                        foreach (var (sound, extraBeat) in timing.extra)
                        {
                            PlayOneShot($"games/{gameName}/{sound}", ShiftBeat(beat, timing.beat + extraBeat, ignoreSwing), pitch, volume * timing.volume, false, inf.UsesAssetBundle ? gameName : null, 0, ignoreConductorPause, ignoreSwing, panning);
                        }
                    }
                    if (timing.mute) return null;
                    if (beat >= 0) beat = ShiftBeat(beat, timing.beat, ignoreSwing);
                    offset += timing.offset;
                    volume *= timing.volume;
                }
                return PlayOneShot($"games/{name}", beat, pitch, volume, looping, inf.UsesAssetBundle ? gameName : null, offset, ignoreConductorPause, ignoreSwing, panning);
            }

            return null;
        }

        /// <summary>
        ///    Schedules a sound to be played at a specific time in seconds.
        ///    Audio clip is fetched from minigame resources
        /// </summary>
        public static Sound PlayOneShotScheduledGame(string name, double targetTime, float pitch = 1f, float volume = 1f, bool looping = false, bool forcePlay = false, bool ignoreConductorPause = false, bool ignoreSwing = false, float panning = 0f)
        {
            string gameName = name.Split('/')[0];
            var inf = GameManager.instance.GetGameInfo(gameName);
            if (GameManager.instance.currentGame == gameName || forcePlay)
            {
                return PlayOneShotScheduled($"games/{name}", targetTime, pitch, volume, looping, inf.UsesAssetBundle ? gameName : null, ignoreConductorPause, ignoreSwing, panning);
            }

            return null;
        }

        /// <summary>
        ///    Fires a custom one-shot sound.
        ///    The sound is loaded from the remix's resources, and not from HS itself, so a direct audioclip is sent here instead
        /// </summary>
        public static Sound PlayOneShotCustom(AudioClip clip, double beat = -1, float pitch = 1f, float volume = 1f, bool looping = false, string game = null, double offset = 0f, bool ignoreConductorPause = false, bool ignoreSwing = false, float panning = 0f)
        {
            if (looping || beat != -1 || pitch != 1f || panning != 0f)
            {
                Sound snd = GetAvailableScheduledSound();

                snd.clip = clip;
                snd.pitch = pitch;
                snd.volume = volume;
                snd.panning = panning;
                snd.looping = looping;
                snd.offset = offset;
                snd.ignoreConductorPause = ignoreConductorPause;
                if (ignoreSwing) snd.beat = Conductor.instance.GetSwungBeat(beat);
                else snd.beat = beat;
                snd.Play();

                return snd;
            }
            else
            {
                if (oneShotAudioSourceObject == null)
                {
                    oneShotAudioSourceObject = new GameObject("OneShot Audio Source");
                    oneShotAudioSource = oneShotAudioSourceObject.AddComponent<AudioSource>();
                    oneShotAudioSource.outputAudioMixerGroup = GlobalGameManager.sfxGroup;
                    UnityEngine.Object.DontDestroyOnLoad(oneShotAudioSourceObject);
                }

                oneShotAudioSource.PlayOneShot(clip, volume);
                // Debug.Log("Played Sound");
                return null;
            }
        }

        /// <summary>
        /// Stops a looping Sound
        /// </summary>
        public static void KillLoop(Sound source, float fadeTime)
        {
            // Safeguard against previously-destroyed sounds.
            if (source == null)
                return;

            source.KillLoop(fadeTime);
        }

        /// <summary>
        /// Fades a Sound's volume
        /// </summary>
        public static void KillLoop(Sound source, float volume, float duration)
        {
            // Safeguard against previously-destroyed sounds.
            if (source == null)
                return;

            source.FadeTo(volume, duration);
        }

        /// <summary>
        /// Gets a pitch multiplier from semitones.
        /// </summary>
        public static float GetPitchFromSemiTones(int semiTones, bool pitchToMusic)
        {
            var newSemitones = Mathf.Pow(2f, (1f / 12f) * semiTones);
            if (pitchToMusic) newSemitones *= Conductor.instance.musicSource.pitch;
            return newSemitones;
        }
        /// <summary>
        /// Returns the semitones from a pitch.
        /// </summary>
        /// <param name="pitch">The pitch of the sound.</param>
        public static int GetSemitonesFromPitch(float pitch, bool pitchToMusic)
        {
            if (pitchToMusic) return (int)((12f * Mathf.Log(pitch, 2)) / Conductor.instance.musicSource.pitch);
            return (int)(12f * Mathf.Log(pitch, 2));
        }

        /// <summary>
        /// Gets a pitch multiplier from cents.
        /// </summary>
        public static float GetPitchFromCents(int cents, bool pitchToMusic)
        {
            if (pitchToMusic)
            {
                return Mathf.Pow(2f, (1f / 12f) * (cents / 100)) * Conductor.instance.musicSource.pitch;
            }
            else
            {
                return Mathf.Pow(2f, (1f / 12f) * (cents / 100));
            }
        }
    }


    /// <summary>
    /// For a game whose cues play their voice lines from the event's function, right on the cue's beat: a japanese voice
    /// that has to start a little before the beat (see Localization.VoiceTiming) can't, so it sounds late. Call Update()
    /// from the game's Update; while the game plays japanese voices, each cue's voice lines are handed to `schedule` a
    /// little before the cue (SoundByte.VoiceLeadAt; it returns false if it left them for the cue), and the cue's
    /// function asks Claim(beat) whether to leave them out.
    /// </summary>
    public class VoiceLeadCues
    {
        readonly string game;
        readonly HashSet<string> actions;
        readonly Func<Jukebox.RiqEntity, bool> schedule;
        readonly HashSet<double> early = new();
        readonly HashSet<double> claimed = new();
        List<Jukebox.RiqEntity> cues;
        int next;
        bool japanese;

        public VoiceLeadCues(string game, Func<Jukebox.RiqEntity, bool> schedule, params string[] actions)
        {
            this.game = game;
            this.schedule = schedule;
            this.actions = new HashSet<string>(actions.Select(a => $"{game}/{a}"));
        }

        public void Update()
        {
            var cond = Conductor.instance;
            if (cond == null || !cond.isPlaying)
            {
                // stopped: start over next time (the level may be edited in between)
                cues = null;
                early.Clear();
                claimed.Clear();
                return;
            }
            if (cond.isPaused) return;
            double now = cond.songPositionInBeatsAsDouble;
            if (cues == null)
            {
                japanese = Localization.UsesJapaneseVoice(game);
                cues = GameManager.instance.Beatmap.Entities.Where(e => actions.Contains(e.datamodel)).OrderBy(e => e.beat).ToList();
                next = 0;
                while (next < cues.Count && cues[next].beat < now) next++;
            }
            if (!japanese) return;
            while (next < cues.Count && cues[next].beat <= now + SoundByte.VoiceLeadAt(cues[next].beat))
            {
                var e = cues[next++];
                if (e.beat <= now || claimed.Contains(e.beat)) continue;  // too late, the function plays it as usual
                if (schedule(e)) early.Add(e.beat);
            }
        }

        /// <summary>True when the voice lines of the cue at this beat were already scheduled (leave them out).</summary>
        public bool Claim(double beat)
        {
            claimed.Add(beat);
            return early.Contains(beat);
        }
    }
}