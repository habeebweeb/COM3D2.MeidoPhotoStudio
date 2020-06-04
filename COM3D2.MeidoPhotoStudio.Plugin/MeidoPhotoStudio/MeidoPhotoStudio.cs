﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [PluginName("Meido Photo Studio"), PluginVersion("0.0.0")]
    public class MeidoPhotoStudio : PluginBase
    {
        private static MonoBehaviour instance;
        private WindowManager windowManager;
        private MeidoManager meidoManager;
        private EnvironmentManager environmentManager;
        private MessageWindowManager messageWindowManager;
        private Constants.Scene currentScene;
        private bool initialized = false;
        private bool isActive = false;
        private bool uiActive = false;
        private MeidoPhotoStudio()
        {
            MeidoPhotoStudio.instance = this;
        }
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Translation.Initialize("en");
            Constants.Initialize();
        }
        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (currentScene == Constants.Scene.Daily)
            {
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    if (!initialized)
                    {
                        Initialize();
                        windowManager.MainWindowVisible = true;
                    }
                    else
                    {
                        ReturnToMenu();
                    }
                }


                if (isActive)
                {
                    bool qFlag = Input.GetKey(KeyCode.Q);
                    if (!qFlag && Input.GetKeyDown(KeyCode.S))
                    {
                        StartCoroutine(TakeScreenShot());
                    }

                    meidoManager.Update();
                    windowManager.Update();
                    environmentManager.Update();
                }
            }
        }

        private IEnumerator TakeScreenShot()
        {
            // Hide UI and dragpoints
            GameObject editUI = GameObject.Find("/UI Root/Camera");
            GameObject fpsViewer =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/FpsCounter", false);
            GameObject sysDialog =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/SystemDialog", false);
            GameObject sysShortcut =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/SystemShortcut", false);
            if (editUI != null) editUI.SetActive(false);
            fpsViewer.SetActive(false);
            sysDialog.SetActive(false);
            sysShortcut.SetActive(false);
            uiActive = false;

            List<Meido> activeMeidoList = this.meidoManager.ActiveMeidoList;
            bool[] isIK = new bool[activeMeidoList.Count];
            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                isIK[i] = meido.IsIK;
                if (meido.IsIK) meido.SetIKActive(false);
            }

            GizmoRender.UIVisible = false;

            yield return new WaitForEndOfFrame();

            // Take Screenshot
            int[] superSize = new[] { 1, 2, 4 };
            int selectedSuperSize = superSize[(int)GameMain.Instance.CMSystem.ScreenShotSuperSize];

            Application.CaptureScreenshot(Utility.ScreenshotFilename(), selectedSuperSize);
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);

            yield return new WaitForEndOfFrame();

            // Show UI and dragpoints
            uiActive = true;
            if (editUI != null) editUI.SetActive(true);
            fpsViewer.SetActive(GameMain.Instance.CMSystem.ViewFps);
            sysDialog.SetActive(true);
            sysShortcut.SetActive(true);

            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                if (isIK[i]) meido.SetIKActive(true);
            }

            GizmoRender.UIVisible = true;
        }
        private void OnGUI()
        {
            if (uiActive)
            {
                windowManager.OnGUI();
            }
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            currentScene = (Constants.Scene)scene.buildIndex;
        }
        private void ReturnToMenu()
        {
            if (meidoManager.IsBusy) return;
            meidoManager.DeactivateMeidos();
            environmentManager.Deactivate();
            messageWindowManager.Deactivate();

            isActive = false;
            uiActive = false;
            initialized = false;
            windowManager.MainWindowVisible = false;
            GameMain.Instance.SoundMgr.PlayBGM("bgm009.ogg", 1f, true);
            GameObject go = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            go.SetActive(true);
            bool isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") == 3;

            if (isNight)
            {
                GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot_Night");
            }
            else
            {
                GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot");
            }

            GameMain.Instance.MainCamera.Reset(CameraMain.CameraType.Target, true);
            GameMain.Instance.MainCamera.SetTargetPos(new Vector3(0.5609447f, 1.380762f, -1.382336f), true);
            GameMain.Instance.MainCamera.SetDistance(1.6f, true);
            GameMain.Instance.MainCamera.SetAroundAngle(new Vector2(245.5691f, 6.273283f), true);
        }

        private void Initialize()
        {
            initialized = true;
            meidoManager = new MeidoManager();
            meidoManager.BeginCallMeidos += (s, a) => this.uiActive = false;
            meidoManager.EndCallMeidos += (s, a) => this.uiActive = true;
            environmentManager = new EnvironmentManager();
            messageWindowManager = new MessageWindowManager();
            windowManager = new WindowManager(meidoManager, environmentManager, messageWindowManager);

            environmentManager.Initialize();

            isActive = true;
            uiActive = true;

            #region maid stuff
            // if (maid)
            // {
            //     maid.StopKuchipakuPattern();
            //     maid.body0.trsLookTarget = GameMain.Instance.MainCamera.transform;

            //     if (maid.Visible && maid.body0.isLoadedBody)
            //     {
            //         maid.CrossFade("pose_taiki_f.anm", false, true, false, 0f);
            //         maid.SetAutoTwistAll(true);
            //         maid.body0.MuneYureL(1f);
            //         maid.body0.MuneYureR(1f);
            //         maid.body0.jbMuneL.enabled = true;
            //         maid.body0.jbMuneR.enabled = true;
            //     }

            //     maid.body0.SetMask(TBody.SlotID.wear, true);
            //     maid.body0.SetMask(TBody.SlotID.skirt, true);
            //     maid.body0.SetMask(TBody.SlotID.bra, true);
            //     maid.body0.SetMask(TBody.SlotID.panz, true);
            //     maid.body0.SetMask(TBody.SlotID.mizugi, true);
            //     maid.body0.SetMask(TBody.SlotID.onepiece, true);
            //     if (maid.body0.isLoadedBody)
            //     {
            //         for (int i = 0; i < maid.body0.goSlot.Count; i++)
            //         {
            //             List<THair1> fieldValue = Utility.GetFieldValue<TBoneHair_, List<THair1>>(maid.body0.goSlot[i].bonehair, "hair1list");
            //             for (int j = 0; j < fieldValue.Count; ++j)
            //             {
            //                 fieldValue[j].SoftG = new Vector3(0.0f, -3f / 1000f, 0.0f);
            //             }
            //         }
            //     }
            // }
            #endregion

            GameObject dailyPanel = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            dailyPanel.SetActive(false);
        }
    }
}
