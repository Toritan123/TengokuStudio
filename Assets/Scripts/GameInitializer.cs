using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.IO.Compression;
using System.Text;

using UnityEngine;
using UnityEngine.Audio;

using Jukebox;

namespace HeavenStudio
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] RenderTexture gameRenderTexture;
        [SerializeField] RenderTexture vfxRenderTexture;
        [SerializeField] RenderTexture bgRenderTexture;
        [SerializeField] RenderTexture overlayRenderTexture;

        [SerializeField] HeavenStudio.Editor.Editor editorGO;
        [SerializeField] String debug_cmdFile;

        [SerializeField] GameManager gameManager;

        [SerializeField] GameObject MainCamera;
        [SerializeField] GameObject CursorCamera;
        [SerializeField] GameObject VfxCamera;
        [SerializeField] GameObject BgCamera;
        [SerializeField] GameObject OverlayCamera;
        [SerializeField] GameObject StaticCamera;
        [SerializeField] GameObject Cursor;

        [SerializeField] GameObject Profiler;

        public bool debugUI;

        public bool playOnStart = false;
        public bool fromCmd = false;

        private void Start()
        {
            string input = "";
            if (debug_cmdFile != string.Empty)
            {
                if (debug_cmdFile.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(debug_cmdFile))
                    {
                        input = debug_cmdFile;
                        fromCmd = true;
                        playOnStart = true;
                    }
                }
            }
            else if (GlobalGameManager.PlayOpenFilePath is not null or "")
            {
                if (editorGO == null && GlobalGameManager.PlayOpenFilePath.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    if (File.Exists(GlobalGameManager.PlayOpenFilePath) && Path.GetExtension(GlobalGameManager.PlayOpenFilePath) == ".riq")
                    {
                        input = GlobalGameManager.PlayOpenFilePath;
                        fromCmd = true;
                        playOnStart = true;
                    }
                }

                if (!GlobalGameManager.returnToLibrary)
                {
                    GlobalGameManager.instance.ClearOpenFile();
                }
            }

            GameObject Games = new GameObject();
            Games.name = "Games";

            gameManager.TogglePlayMode(playOnStart);

            gameManager.GamesHolder = Games;
            gameManager.CircleCursor = Cursor.transform.GetChild(0).GetComponent<CircleCursor>();
            gameManager.GameCamera = MainCamera.GetComponent<Camera>();
            gameManager.CursorCam = CursorCamera.GetComponent<Camera>();
            gameManager.OverlayCamera = OverlayCamera.GetComponent<Camera>();
            gameManager.StaticCamera = StaticCamera.GetComponent<Camera>();

            if (!debugUI && Profiler != null)
            {
                Profiler.GetComponent<DebugUI>().enabled = false;
                Profiler.transform.GetChild(0).gameObject.SetActive(false);
            }

            GameObject Conductor = new GameObject();
            Conductor.name = "Conductor";
            AudioSource source = Conductor.AddComponent<AudioSource>();
            Conductor cond = Conductor.AddComponent<Conductor>();
            cond.musicSource = source;
            source.priority = 255;
            source.outputAudioMixerGroup = Settings.GetMusicMixer();
            cond.gameManager = gameManager;

            gameManager.conductor = cond;

            GlobalGameManager.GameRenderTexture = gameRenderTexture;
            GlobalGameManager.VfxRenderTexture = vfxRenderTexture;
            GlobalGameManager.BGRenderTexture = bgRenderTexture;
            GlobalGameManager.OverlayRenderTexture = overlayRenderTexture;
            GlobalGameManager.ResetGameRenderTexture();

            if (editorGO == null)
            {
                bool success = OpenCmdRemix(input);
                gameManager.Init(success);
            }
            else
            {
                editorGO.Init();
            }
        }

        public bool OpenCmdRemix(string path)
        {
            try
            {
                string tmpDir = RiqFileHandler.ExtractRiq(path);
                Debug.Log("Imported RIQ successfully!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Log($"Error importing RIQ: {e.Message}");
                return false;
            }
        }
    }
}