using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SavePosePane : BasePane
    {
        private MeidoManager meidoManager;
        private Button savePoseButton;
        private Button deletePoseButton;
        private TextField poseNameTextField;
        private ComboBox categoryComboBox;
        private string categoryHeader;
        private string nameHeader;

        public SavePosePane(MeidoManager meidoManager)
        {
            Constants.customPoseChange += (s, a) =>
            {
                this.categoryComboBox.SetDropdownItems(Constants.CustomPoseGroupList.ToArray());
            };

            this.meidoManager = meidoManager;

            this.categoryHeader = Translation.Get("posePane", "categoryHeader");
            this.nameHeader = Translation.Get("posePane", "nameHeader");

            this.savePoseButton = new Button(Translation.Get("posePane", "saveButton"));
            this.savePoseButton.ControlEvent += OnSavePose;

            this.deletePoseButton = new Button(Translation.Get("posePane", "deleteButton"));
            this.categoryComboBox = new ComboBox(Constants.CustomPoseGroupList.ToArray());
            this.poseNameTextField = new TextField();
            this.poseNameTextField.ControlEvent += OnSavePose;
        }

        protected override void ReloadTranslation()
        {
            this.categoryHeader = Translation.Get("posePane", "categoryHeader");
            this.nameHeader = Translation.Get("posePane", "nameHeader");
            this.savePoseButton.Label = Translation.Get("posePane", "saveButton");
            this.deletePoseButton.Label = Translation.Get("posePane", "deleteButton");
        }

        public override void Draw()
        {
            MiscGUI.Header(categoryHeader);
            this.categoryComboBox.Draw();

            MiscGUI.Header(nameHeader);
            GUILayout.BeginHorizontal();
            this.poseNameTextField.Draw();
            this.savePoseButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private void OnSavePose(object sender, EventArgs args)
        {
            byte[] anmBinary = this.meidoManager.ActiveMeido.SerializePose();
            Constants.AddPose(anmBinary, this.poseNameTextField.Value, this.categoryComboBox.Value);
            this.poseNameTextField.Value = String.Empty;
        }
    }
}
