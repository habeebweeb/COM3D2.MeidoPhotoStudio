using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    internal abstract class DragPointMeido : DragPoint
    {
        protected const int jointUpper = 0;
        protected const int jointMiddle = 1;
        protected const int jointLower = 2;
        protected Meido meido;
        protected Maid maid;
        protected bool isPlaying;
        private bool isBone;
        public bool IsBone
        {
            get => isBone;
            set
            {
                if (value != isBone)
                {
                    isBone = value;
                    ApplyDragType();
                }
            }
        }

        public virtual void Initialize(Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(position, rotation);
            this.meido = meido;
            maid = meido.Maid;
            isPlaying = !meido.Stop;
        }

        public override void AddGizmo(float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            base.AddGizmo(scale, mode);
            Gizmo.GizmoDrag += (s, a) =>
            {
                meido.Stop = true;
                isPlaying = false;
            };
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
            isPlaying = !meido.Stop;
        }

        protected void InitializeIK(TBody.IKCMO iKCmo, Transform upper, Transform middle, Transform lower)
        {
            iKCmo.Init(upper, middle, lower, maid.body0);
        }

        protected void Porc(TBody.IKCMO ikCmo, Transform upper, Transform middle, Transform lower)
        {
            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            ikCmo.Porc(upper, middle, lower, CursorPosition(), Vector3.zero, ikData);
        }
    }
}
