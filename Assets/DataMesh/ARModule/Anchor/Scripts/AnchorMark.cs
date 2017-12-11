using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.Anchor
{
    public enum AnchorAdjestType
    {
        Free,
        Move,
        Rotate,
        None
    }

    public class AnchorMark : MonoBehaviour
    {
        [HideInInspector]
        public string anchorName;

        public GameObject markCubePrefab;

        public AnchorAdjestButton buttonMove;
        public AnchorAdjestButton buttonRotate;
        public AnchorAdjestButton buttonFree;

        public Text anchorNameText;

        private SceneAnchorController controller;

        public GameObject moveAxis;
        public GameObject moveAxisX;
        public GameObject moveAxisY;
        public GameObject moveAxisZ;


        public GameObject cursorRotateScene;

        public Vector3 offset = Vector3.zero;

        public GameObject tips;

        private bool needShowTips = false;

        [HideInInspector]
        public GameObject border;

        private AnchorAdjestType adjustType = AnchorAdjestType.None;

        [HideInInspector]
        public Transform rootObjectTransform;
        //public bool followRoot = true;

        private Transform trans;

        private AnchorObjectInfo info;

        private float distance = 30f;
        private float viewAngle = 30;

        void Awake()
        {
            trans = transform;

            HideTips();

            HideMoveAxis();

            HideRotateScene();
        }

        void LateUpdate()
        {
            trans.position = rootObjectTransform.position + offset;
            trans.rotation = rootObjectTransform.rotation;
        }

        public void Init(string name, AnchorObjectInfo info)
        {
            anchorName = name;
            this.info = info;

            anchorNameText.text = anchorName;

            controller = SceneAnchorController.Instance;
            buttonMove.Init(this);
            buttonRotate.Init(this);
            buttonFree.Init(this);

            buttonMove.SetActive(false);
            buttonRotate.SetActive(false);
            buttonFree.SetActive(false);

            // 创建外框 
            GameObject obj = GameObject.Instantiate<GameObject>(this.markCubePrefab);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(
                info.definition.xMin + (info.definition.xMax - info.definition.xMin) / 2,
                info.definition.yMin + (info.definition.yMax - info.definition.yMin) / 2,
                info.definition.zMin + (info.definition.zMax - info.definition.zMin) / 2
                );
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(
                info.definition.xMax - info.definition.xMin,
                info.definition.yMax - info.definition.yMin,
                info.definition.zMax - info.definition.zMin
                );

            obj.layer = controller.AnchorMarkLayer;
            border = obj;

            render = border.GetComponent<Renderer>();

            SetMarkSelected(false);
        }

        private Color normalColor = new Color(0.25f, 0.6896552f, 1f);
        private Color selectedColor = new Color(1f, 0.7310345f, 0.25f);
        Renderer render;

        public void SetMarkSelected(bool active)
        {
            Material mat = render.material;
            if (active)
            {
                mat.SetColor("_EmissionColor", selectedColor);
                border.GetComponent<AlphaBlink>().StartBlink();
            }
            else
            {
                mat.SetColor("_EmissionColor", normalColor);
                border.GetComponent<AlphaBlink>().StopBlink();
            }

            Collider col = border.GetComponent<Collider>();
            col.enabled = !active;
        }

        public void StartAdjust()
        {
            SetAdjustType(AnchorAdjestType.None);
        }

        public void SetAdjustType(AnchorAdjestType t)
        {
            adjustType = t;
            switch (adjustType)
            {
                case AnchorAdjestType.None:
                    buttonMove.SetActive(false);
                    buttonRotate.SetActive(false);
                    buttonFree.SetActive(false);
                    HideMoveAxis();
                    HideRotateScene();
                    needShowTips = false;
                    break;
                case AnchorAdjestType.Free:
                    buttonMove.SetActive(false);
                    buttonRotate.SetActive(false);
                    buttonFree.SetActive(true);
                    HideMoveAxis();
                    HideRotateScene();
                    needShowTips = false;
                    break;
                case AnchorAdjestType.Move:
                    buttonMove.SetActive(true);
                    buttonRotate.SetActive(false);
                    buttonFree.SetActive(false);
                    ShowMoveAxis(Vector3.one);
                    HideRotateScene();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    needShowTips = true;
#else
                    needShowTips = false;
#endif
                    break;
                case AnchorAdjestType.Rotate:
                    buttonMove.SetActive(false);
                    buttonRotate.SetActive(true);
                    buttonFree.SetActive(false);
                    HideMoveAxis();
                    ShowRotateScene();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    needShowTips = true;
#else
                    needShowTips = false;
#endif
                    break;
            }
            tips.SetActive(needShowTips);
        }

        public void ShowTips()
        {
            buttonMove.gameObject.SetActive(true);
            buttonRotate.gameObject.SetActive(true);
            buttonFree.gameObject.SetActive(true);
            tips.SetActive(false);
            tips.SetActive(needShowTips);
        }

        public void HideTips()
        {
            buttonMove.gameObject.SetActive(false);
            buttonRotate.gameObject.SetActive(false);
            buttonFree.gameObject.SetActive(false);
            tips.SetActive(false);
        }


        /*
        public void OnTapOnObject()
        {
            if (controller.currentAnchorInfo == null)
            {
                controller.SelectMark(this);
            }

        }
        */

        public void ShowMoveAxis(Vector3 dir)
        {
            moveAxis.SetActive(true);

            moveAxisX.SetActive(false);
            moveAxisY.SetActive(false);
            moveAxisZ.SetActive(false);
            if (dir.x != 0)
            {
                moveAxisX.SetActive(true);
            }
            if (dir.y != 0)
            {
                moveAxisY.SetActive(true);
            }
            if (dir.z != 0)
            {
                moveAxisZ.SetActive(true);
            }
        }

        public void HideMoveAxis()
        {
            moveAxis.SetActive(false);
        }

        public void ShowRotateScene()
        {
            cursorRotateScene.SetActive(true);
        }

        public void HideRotateScene()
        {
            cursorRotateScene.SetActive(false);
        }


    }

}