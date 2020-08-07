using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal enum AttachPoint
    {
        None, Head, Neck, UpperArmL, UpperArmR, ForearmL, ForearmR, MuneL, MuneR, HandL, HandR,
        Pelvis, ThighL, ThighR, CalfL, CalfR, FootL, FootR
    }

    internal class MeidoDragPointManager
    {
        private enum Bone
        {
            Head, HeadNub, ClavicleL, ClavicleR,
            UpperArmL, UpperArmR, ForearmL, ForearmR,
            HandL, HandR, IKHandL, IKHandR,
            MuneL, MuneSubL, MuneR, MuneSubR,
            Neck, Spine, Spine0a, Spine1, Spine1a, ThighL, ThighR,
            Pelvis, Hip,
            CalfL, CalfR, FootL, FootR,
            // Dragpoint specific
            Cube, Body, Torso,
            // Fingers
            Finger0L, Finger01L, Finger02L, Finger0NubL,
            Finger1L, Finger11L, Finger12L, Finger1NubL,
            Finger2L, Finger21L, Finger22L, Finger2NubL,
            Finger3L, Finger31L, Finger32L, Finger3NubL,
            Finger4L, Finger41L, Finger42L, Finger4NubL,
            Finger0R, Finger01R, Finger02R, Finger0NubR,
            Finger1R, Finger11R, Finger12R, Finger1NubR,
            Finger2R, Finger21R, Finger22R, Finger2NubR,
            Finger3R, Finger31R, Finger32R, Finger3NubR,
            Finger4R, Finger41R, Finger42R, Finger4NubR,
            // Toes
            Toe0L, Toe01L, Toe0NubL,
            Toe1L, Toe11L, Toe1NubL,
            Toe2L, Toe21L, Toe2NubL,
            Toe0R, Toe01R, Toe0NubR,
            Toe1R, Toe11R, Toe1NubR,
            Toe2R, Toe21R, Toe2NubR
        }
        private static readonly Dictionary<AttachPoint, Bone> PointToBone = new Dictionary<AttachPoint, Bone>()
        {
            [AttachPoint.Head] = Bone.Head,
            [AttachPoint.Neck] = Bone.HeadNub,
            [AttachPoint.UpperArmL] = Bone.UpperArmL,
            [AttachPoint.UpperArmR] = Bone.UpperArmR,
            [AttachPoint.ForearmL] = Bone.ForearmL,
            [AttachPoint.ForearmR] = Bone.ForearmR,
            [AttachPoint.MuneL] = Bone.MuneL,
            [AttachPoint.MuneR] = Bone.MuneR,
            [AttachPoint.HandL] = Bone.HandL,
            [AttachPoint.HandR] = Bone.HandR,
            [AttachPoint.Pelvis] = Bone.Pelvis,
            [AttachPoint.ThighL] = Bone.ThighL,
            [AttachPoint.ThighR] = Bone.ThighR,
            [AttachPoint.CalfL] = Bone.CalfL,
            [AttachPoint.CalfR] = Bone.CalfR,
            [AttachPoint.FootL] = Bone.FootL,
            [AttachPoint.FootR] = Bone.FootR,
        };
        private static bool cubeActive;
        public static bool CubeActive
        {
            get => cubeActive;
            set
            {
                if (value != cubeActive)
                {
                    cubeActive = value;
                    CubeActiveChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static bool cubeSmall = false;
        public static bool CubeSmall
        {
            get => cubeSmall;
            set
            {
                if (value != cubeSmall)
                {
                    cubeSmall = value;
                    CubeSmallChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static EventHandler CubeActiveChange;
        private static EventHandler CubeSmallChange;
        private Meido meido;
        private Maid maid;
        private Dictionary<Bone, Transform> BoneTransform;
        private Dictionary<Bone, DragPointMeido> DragPoints;
        private DragPointBody dragBody;
        private DragPointBody dragCube;
        public event EventHandler<MeidoUpdateEventArgs> SelectMaid;
        private bool isBone = false;
        public bool IsBone
        {
            get => isBone;
            set
            {
                if (isBone != value)
                {
                    isBone = value;
                    foreach (DragPointMeido dragPoint in DragPoints.Values)
                    {
                        dragPoint.IsBone = isBone;
                    }
                }
            }
        }
        private bool active = true;
        public bool Active
        {
            get => active;
            set
            {
                if (active != value)
                {
                    active = value;
                    foreach (DragPointMeido dragPoint in DragPoints.Values)
                    {
                        dragPoint.gameObject.SetActive(active);
                    }
                    DragPointHead head = (DragPointHead)DragPoints[Bone.Head];
                    head.gameObject.SetActive(true);
                    head.IsIK = !active;
                }
            }
        }

        public MeidoDragPointManager(Meido meido)
        {
            this.meido = meido;
            this.maid = meido.Maid;
            this.meido.BodyLoad += Initialize;
        }

        public Transform GetAttachPointTransform(AttachPoint point)
        {
            if (point == AttachPoint.None) return null;
            return BoneTransform[PointToBone[point]];
        }

        public void Destroy()
        {
            foreach (DragPointMeido dragPoint in DragPoints.Values)
            {
                GameObject.Destroy(dragPoint.gameObject);
            }
            GameObject.Destroy(dragCube.gameObject);
            GameObject.Destroy(dragBody.gameObject);
            BoneTransform.Clear();
            DragPoints.Clear();
            CubeActiveChange -= OnCubeActive;
            CubeSmallChange -= OnCubeSmall;
        }

        private void Initialize(object sender, EventArgs args)
        {
            meido.BodyLoad -= Initialize;
            CubeActiveChange += OnCubeActive;
            CubeSmallChange += OnCubeSmall;
            InitializeBones();
            InitializeDragPoints();
        }

        private void InitializeDragPoints()
        {
            DragPoints = new Dictionary<Bone, DragPointMeido>();

            dragCube = DragPoint.Make<DragPointBody>(
                PrimitiveType.Cube, Vector3.one * 0.12f, DragPoint.Blue
            );
            dragCube.Initialize(() => maid.transform.position, () => Vector3.zero);
            dragCube.Set(maid.transform);

            dragCube.IsCube = true;
            dragCube.ConstantScale = true;
            dragCube.Select += OnSelectBody;
            dragCube.EndScale += OnSetDragPointScale;
            dragCube.gameObject.SetActive(CubeActive);

            dragBody = DragPoint.Make<DragPointBody>(
                PrimitiveType.Capsule, new Vector3(0.2f, 0.3f, 0.24f), DragPoint.LightBlue
            );
            dragBody.Initialize(
                () => new Vector3(
                    (BoneTransform[Bone.Hip].position.x + BoneTransform[Bone.Spine0a].position.x) / 2f,
                    (BoneTransform[Bone.Spine1].position.y + BoneTransform[Bone.Spine0a].position.y) / 2f,
                    (BoneTransform[Bone.Spine0a].position.z + BoneTransform[Bone.Hip].position.z) / 2f
                ),
                () => new Vector3(
                    BoneTransform[Bone.Spine0a].eulerAngles.x,
                    BoneTransform[Bone.Spine0a].eulerAngles.y,
                    BoneTransform[Bone.Spine0a].eulerAngles.z + 90f
                )
            );
            dragBody.Set(maid.transform);
            dragBody.Select += OnSelectBody;
            dragBody.EndScale += OnSetDragPointScale;

            // Head Dragpoint
            DragPointHead dragHead = DragPoint.Make<DragPointHead>(
                PrimitiveType.Sphere, new Vector3(0.2f, 0.24f, 0.2f), DragPoint.LightBlue
            );
            dragHead.Initialize(meido,
                () => new Vector3(
                    BoneTransform[Bone.Head].position.x,
                    (BoneTransform[Bone.Head].position.y * 1.2f + BoneTransform[Bone.HeadNub].position.y * 0.8f) / 2f,
                    BoneTransform[Bone.Head].position.z
                ),
                () => new Vector3(
                    BoneTransform[Bone.Head].eulerAngles.x,
                    BoneTransform[Bone.Head].eulerAngles.y,
                    BoneTransform[Bone.Head].eulerAngles.z + 90f
                )
            );
            dragHead.Set(BoneTransform[Bone.Neck]);
            dragHead.Select += OnSelectFace;

            DragPoints[Bone.Head] = dragHead;

            // Torso Dragpoint
            Transform spineTrans1 = BoneTransform[Bone.Spine1];
            Transform spineTrans2 = BoneTransform[Bone.Spine1a];

            DragPointTorso dragTorso = DragPoint.Make<DragPointTorso>(
                PrimitiveType.Capsule, new Vector3(0.2f, 0.19f, 0.24f), DragPoint.LightBlue
            );
            dragTorso.Initialize(meido,
                () => new Vector3(
                    spineTrans1.position.x,
                    spineTrans2.position.y,
                    spineTrans1.position.z - 0.05f
                ),
                () => new Vector3(
                    spineTrans1.eulerAngles.x,
                    spineTrans1.eulerAngles.y,
                    spineTrans1.eulerAngles.z + 90f
                )
            );
            dragTorso.Set(BoneTransform[Bone.Spine1a]);

            DragPoints[Bone.Torso] = dragTorso;

            // Pelvis Dragpoint
            Transform pelvisTrans = BoneTransform[Bone.Pelvis];
            Transform spineTrans = BoneTransform[Bone.Spine];

            DragPointPelvis dragPelvis = DragPoint.Make<DragPointPelvis>(
                PrimitiveType.Capsule, new Vector3(0.2f, 0.15f, 0.24f), DragPoint.LightBlue
            );
            dragPelvis.Initialize(meido,

                () => new Vector3(
                    pelvisTrans.position.x,
                    (pelvisTrans.position.y + spineTrans.position.y) / 2f,
                    pelvisTrans.position.z
                ),
                () => new Vector3(
                    pelvisTrans.eulerAngles.x + 90f,
                    pelvisTrans.eulerAngles.y + 90f,
                    pelvisTrans.eulerAngles.z
                )
            );
            dragPelvis.Set(BoneTransform[Bone.Pelvis]);

            DragPoints[Bone.Pelvis] = dragPelvis;

            InitializeMuneDragPoint(left: true);
            InitializeMuneDragPoint(left: false);

            DragPointChain[] armDragPointL = MakeIKChain(BoneTransform[Bone.HandL]);
            DragPoints[Bone.UpperArmL] = armDragPointL[0];
            DragPoints[Bone.ForearmL] = armDragPointL[1];
            DragPoints[Bone.HandL] = armDragPointL[2];

            DragPointChain[] armDragPointR = MakeIKChain(BoneTransform[Bone.HandR]);
            DragPoints[Bone.UpperArmR] = armDragPointR[0];
            DragPoints[Bone.ForearmR] = armDragPointR[1];
            DragPoints[Bone.HandR] = armDragPointR[2];

            DragPointChain[] legDragPointL = MakeIKChain(BoneTransform[Bone.FootL]);
            DragPoints[Bone.CalfL] = legDragPointL[0];
            DragPoints[Bone.FootL] = legDragPointL[1];

            DragPointChain[] legDragPointR = MakeIKChain(BoneTransform[Bone.FootR]);
            DragPoints[Bone.CalfR] = legDragPointR[0];
            DragPoints[Bone.FootR] = legDragPointR[1];

            InitializeSpineDragPoint(
                Bone.Neck, Bone.Spine, Bone.Spine0a, Bone.Spine1, Bone.Spine1a, Bone.Hip, Bone.ThighL, Bone.ThighR
            );

            InitializeFingerDragPoint(Bone.Finger0L, Bone.Finger4R);
            InitializeFingerDragPoint(Bone.Toe0L, Bone.Toe2R);
        }

        private void InitializeMuneDragPoint(bool left)
        {
            Bone mune = left ? Bone.MuneL : Bone.MuneR;
            Bone sub = left ? Bone.MuneSubL : Bone.MuneSubR;
            DragPointChain muneDragPoint = DragPoint.Make<DragPointChain>(
                PrimitiveType.Sphere, Vector3.one * 0.12f, DragPoint.LightBlue
            );
            muneDragPoint.Initialize(meido,
                () => (BoneTransform[mune].position + BoneTransform[sub].position) / 2f,
                () => Vector3.zero
            );
            muneDragPoint.Set(BoneTransform[sub]);
            DragPoints[mune] = muneDragPoint;
        }

        private DragPointChain[] MakeIKChain(Transform lower)
        {
            Vector3 limbDragPointSize = Vector3.one * 0.12f;
            // Ignore Thigh transform when making a leg IK chain
            bool isLeg = lower.name.EndsWith("Foot");
            DragPointChain[] dragPoints = new DragPointChain[isLeg ? 2 : 3];
            for (int i = dragPoints.Length - 1; i >= 0; i--)
            {
                Transform joint = lower;
                dragPoints[i] = DragPoint.Make<DragPointChain>(
                    PrimitiveType.Sphere, limbDragPointSize, DragPoint.LightBlue
                );
                dragPoints[i].Initialize(meido, () => joint.position, () => Vector3.zero);
                dragPoints[i].Set(joint);
                dragPoints[i].AddGizmo();
                lower = lower.parent;
            }
            return dragPoints;
        }

        private void InitializeFingerDragPoint(Bone start, Bone end)
        {
            Vector3 fingerDragPointSize = Vector3.one * 0.015f;
            int joints = BoneTransform[start].name.Split(' ')[2].StartsWith("Finger") ? 4 : 3;
            for (Bone bone = start; bone <= end; bone += joints)
            {
                for (int i = 1; i < joints; i++)
                {
                    Transform trans = BoneTransform[bone + i];
                    DragPointFinger chain = DragPoint.Make<DragPointFinger>(
                        PrimitiveType.Sphere, fingerDragPointSize, DragPoint.Blue
                    );
                    chain.Initialize(meido, () => trans.position, () => Vector3.zero);
                    chain.Set(trans);
                    DragPoints[bone + i] = chain;
                }
            }
        }

        private void InitializeSpineDragPoint(params Bone[] bones)
        {
            Vector3 spineDragPointSize = Vector3.one * 0.045f;
            foreach (Bone bone in bones)
            {
                Transform spine = BoneTransform[bone];
                PrimitiveType primitive = bone == Bone.Hip ? PrimitiveType.Cube : PrimitiveType.Sphere;
                DragPointSpine dragPoint = DragPoint.Make<DragPointSpine>(
                    primitive, spineDragPointSize, DragPoint.LightBlue
                );
                dragPoint.Initialize(meido,
                    () => spine.position,
                    () => Vector3.zero
                );
                dragPoint.Set(spine);
                dragPoint.AddGizmo();
                DragPoints[bone] = dragPoint;
            }
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            dragCube.gameObject.SetActive(CubeActive);
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            dragCube.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnSetDragPointScale(object sender, EventArgs args)
        {
            this.SetDragPointScale(maid.transform.localScale.x);
        }

        private void OnSelectBody(object sender, EventArgs args)
        {
            SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.ActiveSlot, fromMaid: true, isBody: true));
        }

        private void OnSelectFace(object sender, EventArgs args)
        {
            SelectMaid?.Invoke(this, new MeidoUpdateEventArgs(meido.ActiveSlot, fromMaid: true, isBody: false));
        }

        private void SetDragPointScale(float scale)
        {
            foreach (DragPointMeido dragPoint in DragPoints.Values)
            {
                dragPoint.DragPointScale = scale;
            }
            dragBody.DragPointScale = scale;
        }

        private void InitializeBones()
        {
            // TODO: Move to external file somehow
            Transform transform = maid.body0.m_Bones.transform;
            BoneTransform = new Dictionary<Bone, Transform>()
            {
                [Bone.Head] = CMT.SearchObjName(transform, "Bip01 Head"),
                [Bone.Neck] = CMT.SearchObjName(transform, "Bip01 Neck"),
                [Bone.HeadNub] = CMT.SearchObjName(transform, "Bip01 HeadNub"),
                [Bone.IKHandL] = CMT.SearchObjName(transform, "_IK_handL"),
                [Bone.IKHandR] = CMT.SearchObjName(transform, "_IK_handR"),
                [Bone.MuneL] = CMT.SearchObjName(transform, "Mune_L"),
                [Bone.MuneSubL] = CMT.SearchObjName(transform, "Mune_L_sub"),
                [Bone.MuneR] = CMT.SearchObjName(transform, "Mune_R"),
                [Bone.MuneSubR] = CMT.SearchObjName(transform, "Mune_R_sub"),
                [Bone.Pelvis] = CMT.SearchObjName(transform, "Bip01 Pelvis"),
                [Bone.Hip] = CMT.SearchObjName(transform, "Bip01"),
                [Bone.Spine] = CMT.SearchObjName(transform, "Bip01 Spine"),
                [Bone.Spine0a] = CMT.SearchObjName(transform, "Bip01 Spine0a"),
                [Bone.Spine1] = CMT.SearchObjName(transform, "Bip01 Spine1"),
                [Bone.Spine1a] = CMT.SearchObjName(transform, "Bip01 Spine1a"),
                [Bone.ClavicleL] = CMT.SearchObjName(transform, "Bip01 L Clavicle"),
                [Bone.ClavicleR] = CMT.SearchObjName(transform, "Bip01 R Clavicle"),
                [Bone.UpperArmL] = CMT.SearchObjName(transform, "Bip01 L UpperArm"),
                [Bone.ForearmL] = CMT.SearchObjName(transform, "Bip01 L Forearm"),
                [Bone.HandL] = CMT.SearchObjName(transform, "Bip01 L Hand"),
                [Bone.UpperArmR] = CMT.SearchObjName(transform, "Bip01 R UpperArm"),
                [Bone.ForearmR] = CMT.SearchObjName(transform, "Bip01 R Forearm"),
                [Bone.HandR] = CMT.SearchObjName(transform, "Bip01 R Hand"),
                [Bone.ThighL] = CMT.SearchObjName(transform, "Bip01 L Thigh"),
                [Bone.CalfL] = CMT.SearchObjName(transform, "Bip01 L Calf"),
                [Bone.FootL] = CMT.SearchObjName(transform, "Bip01 L Foot"),
                [Bone.ThighR] = CMT.SearchObjName(transform, "Bip01 R Thigh"),
                [Bone.CalfR] = CMT.SearchObjName(transform, "Bip01 R Calf"),
                [Bone.FootR] = CMT.SearchObjName(transform, "Bip01 R Foot"),
                // fingers
                [Bone.Finger0L] = CMT.SearchObjName(transform, "Bip01 L Finger0"),
                [Bone.Finger01L] = CMT.SearchObjName(transform, "Bip01 L Finger01"),
                [Bone.Finger02L] = CMT.SearchObjName(transform, "Bip01 L Finger02"),
                [Bone.Finger0NubL] = CMT.SearchObjName(transform, "Bip01 L Finger0Nub"),
                [Bone.Finger1L] = CMT.SearchObjName(transform, "Bip01 L Finger1"),
                [Bone.Finger11L] = CMT.SearchObjName(transform, "Bip01 L Finger11"),
                [Bone.Finger12L] = CMT.SearchObjName(transform, "Bip01 L Finger12"),
                [Bone.Finger1NubL] = CMT.SearchObjName(transform, "Bip01 L Finger1Nub"),
                [Bone.Finger2L] = CMT.SearchObjName(transform, "Bip01 L Finger2"),
                [Bone.Finger21L] = CMT.SearchObjName(transform, "Bip01 L Finger21"),
                [Bone.Finger22L] = CMT.SearchObjName(transform, "Bip01 L Finger22"),
                [Bone.Finger2NubL] = CMT.SearchObjName(transform, "Bip01 L Finger2Nub"),
                [Bone.Finger3L] = CMT.SearchObjName(transform, "Bip01 L Finger3"),
                [Bone.Finger31L] = CMT.SearchObjName(transform, "Bip01 L Finger31"),
                [Bone.Finger32L] = CMT.SearchObjName(transform, "Bip01 L Finger32"),
                [Bone.Finger3NubL] = CMT.SearchObjName(transform, "Bip01 L Finger3Nub"),
                [Bone.Finger4L] = CMT.SearchObjName(transform, "Bip01 L Finger4"),
                [Bone.Finger41L] = CMT.SearchObjName(transform, "Bip01 L Finger41"),
                [Bone.Finger42L] = CMT.SearchObjName(transform, "Bip01 L Finger42"),
                [Bone.Finger4NubL] = CMT.SearchObjName(transform, "Bip01 L Finger4Nub"),
                [Bone.Finger0R] = CMT.SearchObjName(transform, "Bip01 R Finger0"),
                [Bone.Finger01R] = CMT.SearchObjName(transform, "Bip01 R Finger01"),
                [Bone.Finger02R] = CMT.SearchObjName(transform, "Bip01 R Finger02"),
                [Bone.Finger0NubR] = CMT.SearchObjName(transform, "Bip01 R Finger0Nub"),
                [Bone.Finger1R] = CMT.SearchObjName(transform, "Bip01 R Finger1"),
                [Bone.Finger11R] = CMT.SearchObjName(transform, "Bip01 R Finger11"),
                [Bone.Finger12R] = CMT.SearchObjName(transform, "Bip01 R Finger12"),
                [Bone.Finger1NubR] = CMT.SearchObjName(transform, "Bip01 R Finger1Nub"),
                [Bone.Finger2R] = CMT.SearchObjName(transform, "Bip01 R Finger2"),
                [Bone.Finger21R] = CMT.SearchObjName(transform, "Bip01 R Finger21"),
                [Bone.Finger22R] = CMT.SearchObjName(transform, "Bip01 R Finger22"),
                [Bone.Finger2NubR] = CMT.SearchObjName(transform, "Bip01 R Finger2Nub"),
                [Bone.Finger3R] = CMT.SearchObjName(transform, "Bip01 R Finger3"),
                [Bone.Finger31R] = CMT.SearchObjName(transform, "Bip01 R Finger31"),
                [Bone.Finger32R] = CMT.SearchObjName(transform, "Bip01 R Finger32"),
                [Bone.Finger3NubR] = CMT.SearchObjName(transform, "Bip01 R Finger3Nub"),
                [Bone.Finger4R] = CMT.SearchObjName(transform, "Bip01 R Finger4"),
                [Bone.Finger41R] = CMT.SearchObjName(transform, "Bip01 R Finger41"),
                [Bone.Finger42R] = CMT.SearchObjName(transform, "Bip01 R Finger42"),
                [Bone.Finger4NubR] = CMT.SearchObjName(transform, "Bip01 R Finger4Nub"),
                // Toes
                [Bone.Toe0L] = CMT.SearchObjName(transform, "Bip01 L Toe0"),
                [Bone.Toe01L] = CMT.SearchObjName(transform, "Bip01 L Toe01"),
                [Bone.Toe0NubL] = CMT.SearchObjName(transform, "Bip01 L Toe0Nub"),
                [Bone.Toe1L] = CMT.SearchObjName(transform, "Bip01 L Toe1"),
                [Bone.Toe11L] = CMT.SearchObjName(transform, "Bip01 L Toe11"),
                [Bone.Toe1NubL] = CMT.SearchObjName(transform, "Bip01 L Toe1Nub"),
                [Bone.Toe2L] = CMT.SearchObjName(transform, "Bip01 L Toe2"),
                [Bone.Toe21L] = CMT.SearchObjName(transform, "Bip01 L Toe21"),
                [Bone.Toe2NubL] = CMT.SearchObjName(transform, "Bip01 L Toe2Nub"),
                [Bone.Toe0R] = CMT.SearchObjName(transform, "Bip01 R Toe0"),
                [Bone.Toe01R] = CMT.SearchObjName(transform, "Bip01 R Toe01"),
                [Bone.Toe0NubR] = CMT.SearchObjName(transform, "Bip01 R Toe0Nub"),
                [Bone.Toe1R] = CMT.SearchObjName(transform, "Bip01 R Toe1"),
                [Bone.Toe11R] = CMT.SearchObjName(transform, "Bip01 R Toe11"),
                [Bone.Toe1NubR] = CMT.SearchObjName(transform, "Bip01 R Toe1Nub"),
                [Bone.Toe2R] = CMT.SearchObjName(transform, "Bip01 R Toe2"),
                [Bone.Toe21R] = CMT.SearchObjName(transform, "Bip01 R Toe21"),
                [Bone.Toe2NubR] = CMT.SearchObjName(transform, "Bip01 R Toe2Nub")
            };
        }
    }

    internal struct AttachPointInfo
    {
        public AttachPoint AttachPoint { get; }
        public string MaidGuid { get; }
        public int MaidIndex { get; }
        public static AttachPointInfo Empty
        {
            get => new AttachPointInfo(AttachPoint.None, String.Empty, -1);
        }

        public AttachPointInfo(AttachPoint attachPoint, string maidGuid, int maidIndex)
        {
            this.AttachPoint = attachPoint;
            this.MaidGuid = maidGuid;
            this.MaidIndex = maidIndex;
        }
    }
}
