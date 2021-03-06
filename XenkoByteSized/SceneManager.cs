﻿using System.Threading.Tasks;

using Xenko.Core;
using Xenko.Input;
using Xenko.Engine;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Rendering.Compositing;

namespace XenkoByteSized {
    public class SceneManager : SyncScript {

        private Task<Scene> loadingTask;
        private bool loadingInProgress = false;
        private string currentSceneUrl;
        private Scene currentScene;
        private float timeout;

        public CameraComponent Camera;

        [DataMember(100)]
        [Display("Scenes")]
        public FastList<string> scenes = new FastList<string>();

        [DataMember(200)]
        [Display("Compositors")]
        public FastList<GraphicsCompositor> compositors = new FastList<GraphicsCompositor>();

        private void SwitchToScene(string sceneUrl, GraphicsCompositor compositor) {

            if (timeout >= 0.0f && sceneUrl == currentSceneUrl) {
                return;
            }

            /* don't load ourselves again */
            if (currentScene != null) {
                Content.TryGetAssetUrl(currentScene, out string currentUrl);
                if (currentUrl == sceneUrl) {
                    timeout = 1.0f;
                    return;
                }
            }

            loadingInProgress = true;
            var localLoadingTask = loadingTask = Content.LoadAsync<Scene>(sceneUrl);
            Log.Info($"Loading: {sceneUrl}");

            Script.AddTask(async () => {

                await loadingTask;
                
                if (currentScene != null) {
                    Content.Unload(currentScene);
                    currentScene.Parent = null;
                }

                /* HACK */
                if (Camera.Enabled) {
                    Camera.Enabled = false;
                }

                /* switch compositor, if necessary */
                var currentCompositor = SceneSystem.GraphicsCompositor;
                if (currentCompositor != compositor) {
                    SceneSystem.GraphicsCompositor = compositor;
                }

                currentScene = loadingTask.Result;
                currentScene.Parent = Entity.Scene;
                currentSceneUrl = sceneUrl;
                loadingInProgress = false;

            });

        }

        public override void Update() {

            float delta = (float)Game.UpdateTime.Elapsed.TotalSeconds;
            timeout -= delta;

            /* print info about available scenes to switch to, marking current one */

            var sceneId = 1;
            var curOffset = new Int2((int)Input.Mouse.SurfaceSize.X - 256, 48);

            DebugText.Print("Available Scenes: ", curOffset);
            curOffset.Y += 16;

            foreach (var scn in scenes) {
                if (scn == currentSceneUrl) {
                    DebugText.Print($"* {sceneId}: {scn}", curOffset);
                } else {
                    DebugText.Print($"{sceneId}: {scn}", curOffset);
                }
                curOffset.Y += 16;
                sceneId++;
            }

            /* not handling cancels */
            if (loadingInProgress) { return; }

            for (int i = 0; i < scenes.Count; ++i) {
                if (Input.IsKeyPressed(Keys.D1 + i)) {
                    SwitchToScene(scenes[i], compositors[i]);
                    break;
                }
            }

        }

    }
}
