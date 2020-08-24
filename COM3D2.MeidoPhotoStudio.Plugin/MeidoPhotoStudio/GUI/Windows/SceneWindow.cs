using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneWindow : BaseWindow
    {
        private SceneManager sceneManager;
        private SceneManagerTitleBarPane titleBar;
        private SceneManagerDirectoryPane directoryList;
        private SceneManagerScenePane sceneGrid;
        private Rect resizeHandleRect;
        private bool resizing;
        private float resizeHandleSize = 15f;
        private bool visible;
        public override bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (visible && !sceneManager.Initialized) sceneManager.Initialize();
            }
        }

        public SceneWindow(SceneManager sceneManager)
        {
            windowRect.width = Screen.width * 0.65f;
            windowRect.height = Screen.height * 0.75f;
            windowRect.x = Screen.width * 0.5f - windowRect.width * 0.5f;
            windowRect.y = Screen.height * 0.5f - windowRect.height * 0.5f;

            resizeHandleRect = new Rect(0f, 0f, resizeHandleSize, resizeHandleSize);


            this.sceneManager = sceneManager;
            SceneModalWindow sceneModalWindow = new SceneModalWindow(this.sceneManager);

            this.titleBar = AddPane(new SceneManagerTitleBarPane(sceneManager));
            this.titleBar.closeChange += (s, a) => Visible = false;

            this.directoryList = AddPane(new SceneManagerDirectoryPane(sceneManager, sceneModalWindow));

            this.sceneGrid = AddPane(new SceneManagerScenePane(sceneManager, sceneModalWindow));

            this.sceneGrid.SetParent(this);
        }

        public override void GUIFunc(int id)
        {
            HandleResize();
            Draw();
            if (!resizing) GUI.DragWindow();
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.F8)) Visible = !Visible;
        }

        public override void Deactivate()
        {
            Visible = false;
        }

        public override void Draw()
        {
            GUI.enabled = !SceneManager.Busy && !Modal.Visible;

            GUILayout.BeginArea(new Rect(10f, 10f, windowRect.width - 20f, windowRect.height - 20f));

            titleBar.Draw();

            GUILayout.BeginHorizontal();
            directoryList.Draw();
            sceneGrid.Draw();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            GUI.Box(resizeHandleRect, GUIContent.none);
        }

        private void HandleResize()
        {
            resizeHandleRect.x = windowRect.width - resizeHandleSize;
            resizeHandleRect.y = windowRect.height - resizeHandleSize;

            if (!resizing && Input.GetMouseButton(0) && resizeHandleRect.Contains(Event.current.mousePosition))
            {
                resizing = true;
            }

            if (resizing)
            {
                float rectWidth = Event.current.mousePosition.x;
                float rectHeight = Event.current.mousePosition.y;

                float minWidth = Utility.GetPix(
                    SceneManagerDirectoryPane.listWidth
                    + (int)(SceneManager.sceneDimensions.x * SceneManagerScenePane.thumbnailScale)
                );

                windowRect.width = Mathf.Max(minWidth, rectWidth);
                windowRect.height = Mathf.Max(300, rectHeight);

                if (!Input.GetMouseButton(0)) resizing = false;
            }
        }
    }
}
