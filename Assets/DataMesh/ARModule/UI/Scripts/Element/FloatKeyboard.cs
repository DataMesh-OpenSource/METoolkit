using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DataMesh.AR.Interactive;

namespace DataMesh.AR.UI
{

    public class FloatKeyboard : MonoBehaviour
    {
        public GameObject keyPrefab;
        public Transform keyArea;
        public Transform keyShiftArea;
        public RectTransform bg;
        public Text outputText;

        private BoxCollider bgCollider;

        public int width = 60;
        public int height = 60;
        public int widthInterval = 5;
        public int heightInterval = 5;

        public int keyCountX = 12;
        public int keyCountY = 5;

        public CommonButton buttonExit;
        public TransitObject showTransit;

        public System.Action<string> callbackValueChange;
        public System.Action<string> callbackInputFinish;
        public System.Action callbackExit;

        private int startX = 0;
        private int startY = 0;

        private MultiInputManager inputManager;

        private bool isShift = false;

        private bool hasInit = false;
        private bool isBusy = false;

        private class Key
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public string text;
            public string key;

            public Key(int _x, int _y, int _w, int _h, string _t, string _k)
            {
                x = _x;
                y = _y;
                width = _w;
                height = _h;
                text = _t;
                key = _k;
            }
        }

        private Key[] keys;
        private Key[] keysShift;

        private string _outputString = "";
        private string outputString
        {
            get { return _outputString; }
            set
            {
                _outputString = value;
                outputText.text = _outputString;
                callbackValueChange?.Invoke(outputString);
            }
        }

        void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SetTransfrom(Transform parent, Vector3 pos, Vector3 rotate, Vector3 scale)
        {
            transform.SetParent(parent);
            transform.localPosition = pos;
            transform.localEulerAngles = rotate;
            transform.localScale = scale;
        }

        // Use this for initialization
        private void Init()
        {
            buttonExit.callbackClick = OnExit;

            bgCollider = bg.GetComponent<BoxCollider>();

            int totalWidth = keyCountX * width + (keyCountX - 1) * widthInterval;
            int totalHeight = keyCountY * width + (keyCountY - 1) * heightInterval;
            startX = 0 - totalWidth / 2;
            startY = 0;//totalHeight / 2;

            bg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth + 20);
            bg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight + 10 + 80);
            bg.localPosition = new Vector3(0, 130, 0);
            bgCollider.size = new Vector3(totalWidth, totalHeight, 1);

            RectTransform textTrans = outputText.transform as RectTransform;
            textTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);

            keys = new Key[] {
                new Key(0,  0,  1, 1, "1", "1"),
                new Key(1,  0,  1, 1, "2", "2"),
                new Key(2,  0,  1, 1, "3", "3"),
                new Key(3,  0,  1, 1, "4", "4"),
                new Key(4,  0,  1, 1, "5", "5"),
                new Key(5,  0,  1, 1, "6", "6"),
                new Key(6,  0,  1, 1, "7", "7"),
                new Key(7,  0,  1, 1, "8", "8"),
                new Key(8,  0,  1, 1, "9", "9"),
                new Key(9,  0,  1, 1, "0", "0"),
                new Key(10, 0,  1, 1, "-", "-"),
                new Key(11, 0,  2, 2, "Back", "Back"),
                new Key(0,  1,  1, 1, "q", "q"),
                new Key(1,  1,  1, 1, "w", "w"),
                new Key(2,  1,  1, 1, "e", "e"),
                new Key(3,  1,  1, 1, "r", "r"),
                new Key(4,  1,  1, 1, "t", "t"),
                new Key(5,  1,  1, 1, "y", "y"),
                new Key(6,  1,  1, 1, "u", "u"),
                new Key(7,  1,  1, 1, "i", "i"),
                new Key(8,  1,  1, 1, "o", "o"),
                new Key(9,  1,  1, 1, "p", "p"),
                new Key(10, 1,  1, 1, "=", "="),
                new Key(0,  2,  1, 1, "a", "a"),
                new Key(1,  2,  1, 1, "s", "s"),
                new Key(2,  2,  1, 1, "d", "d"),
                new Key(3,  2,  1, 1, "f", "f"),
                new Key(4,  2,  1, 1, "g", "g"),
                new Key(5,  2,  1, 1, "h", "h"),
                new Key(6,  2,  1, 1, "j", "j"),
                new Key(7,  2,  1, 1, "k", "k"),
                new Key(8,  2,  1, 1, "l", "l"),
                new Key(9,  2,  1, 1, ";", ";"),
                new Key(10, 2,  1, 1, "'", "'"),
                new Key(11, 2,  2, 3, "Return", "Return"),
                new Key(0,  3,  1, 1, "z", "z"),
                new Key(1,  3,  1, 1, "x", "x"),
                new Key(2,  3,  1, 1, "c", "c"),
                new Key(3,  3,  1, 1, "v", "v"),
                new Key(4,  3,  1, 1, "b", "b"),
                new Key(5,  3,  1, 1, "n", "n"),
                new Key(6,  3,  1, 1, "m", "m"),
                new Key(7,  3,  1, 1, ",", ","),
                new Key(8,  3,  1, 1, ".", "."),
                new Key(9,  3,  1, 1, "/", "/"),
                new Key(10, 3,  1, 1, "\\", "\\"),
                new Key(0,  4,  2, 1, "Shift", "Shift"),
                new Key(2,  4,  6, 1, "Space", " "),
                new Key(8,  4,  1, 1, "[", "["),
                new Key(9,  4,  1, 1, "]", "]"),
                new Key(10,  4,  1, 1, "`", "`"),
            };

            keysShift = new Key[] {
                new Key(0,  0,  1, 1, "!", "!"),
                new Key(1,  0,  1, 1, "@", "@"),
                new Key(2,  0,  1, 1, "#", "#"),
                new Key(3,  0,  1, 1, "$", "$"),
                new Key(4,  0,  1, 1, "%", "%"),
                new Key(5,  0,  1, 1, "^", "^"),
                new Key(6,  0,  1, 1, "&", "&"),
                new Key(7,  0,  1, 1, "*", "*"),
                new Key(8,  0,  1, 1, "(", "("),
                new Key(9,  0,  1, 1, ")", "0)"),
                new Key(10, 0,  1, 1, "_", "_"),
                new Key(11, 0,  2, 2, "Back", "Back"),
                new Key(0,  1,  1, 1, "Q", "Q"),
                new Key(1,  1,  1, 1, "W", "W"),
                new Key(2,  1,  1, 1, "E", "E"),
                new Key(3,  1,  1, 1, "R", "R"),
                new Key(4,  1,  1, 1, "T", "T"),
                new Key(5,  1,  1, 1, "Y", "Y"),
                new Key(6,  1,  1, 1, "U", "U"),
                new Key(7,  1,  1, 1, "I", "I"),
                new Key(8,  1,  1, 1, "O", "O"),
                new Key(9,  1,  1, 1, "P", "P"),
                new Key(10, 1,  1, 1, "+", "+"),
                new Key(0,  2,  1, 1, "A", "A"),
                new Key(1,  2,  1, 1, "S", "S"),
                new Key(2,  2,  1, 1, "D", "D"),
                new Key(3,  2,  1, 1, "F", "F"),
                new Key(4,  2,  1, 1, "G", "G"),
                new Key(5,  2,  1, 1, "H", "H"),
                new Key(6,  2,  1, 1, "J", "J"),
                new Key(7,  2,  1, 1, "K", "K"),
                new Key(8,  2,  1, 1, "L", "K"),
                new Key(9,  2,  1, 1, ":", ":"),
                new Key(10, 2,  1, 1, "\"", "\""),
                new Key(11, 2,  2, 3, "Return", "Return"),
                new Key(0,  3,  1, 1, "Z", "Z"),
                new Key(1,  3,  1, 1, "X", "X"),
                new Key(2,  3,  1, 1, "C", "C"),
                new Key(3,  3,  1, 1, "V", "V"),
                new Key(4,  3,  1, 1, "B", "B"),
                new Key(5,  3,  1, 1, "N", "N"),
                new Key(6,  3,  1, 1, "M", "M"),
                new Key(7,  3,  1, 1, "<", "<"),
                new Key(8,  3,  1, 1, ">", ">"),
                new Key(9,  3,  1, 1, "?", "?"),
                new Key(10, 3,  1, 1, "|", "|"),
                new Key(0,  4,  2, 1, "Shift", "Shift"),
                new Key(2,  4,  6, 1, "Space", " "),
                new Key(8,  4,  1, 1, "{", "{"),
                new Key(9,  4,  1, 1, "}", "}"),
                new Key(10,  4,  1, 1, "~", "~"),
            };


            for (int i = 0;i < keys.Length;i ++)
            {
                AddKeyButton(keys[i], keyArea);
            }

            for (int i = 0; i < keysShift.Length; i++)
            {
                AddKeyButton(keysShift[i], keyShiftArea);
            }

            outputText.text = "";

            hasInit = true;
        }

        private void AddKeyButton(Key key, Transform parentArea)
        {
            int w = key.width * width + (key.width - 1) * widthInterval;
            int h = key.height * height + (key.height - 1) * heightInterval;
            int x = startX + key.x * (width + widthInterval) + w / 2;
            int y = startY - key.y * (height + heightInterval) - h / 2;

            GameObject keyObj = Instantiate(keyPrefab) as GameObject;
            keyObj.name = key.text;
            RectTransform keyTrans = keyObj.transform as RectTransform;
            keyTrans.SetParent(parentArea);
            keyTrans.localPosition = new Vector3(x, y, 0);
            keyTrans.localRotation = Quaternion.identity;
            keyTrans.localScale = Vector3.one;

            CommonButton button = keyObj.GetComponent<CommonButton>();
            button.ButtonName = key.text;
            button.SetSize(w, h);

            button.param = key.key;
            button.callbackClick = OnClick;
        }

        private int oldLayerMask;
        public void TurnOn()
        {
            if (!hasInit)
                Init();

            outputString = "";

            inputManager = MultiInputManager.Instance;
            oldLayerMask = inputManager.layerMask;
            inputManager.layerMask = LayerMask.GetMask("FloatKeyboard");

            isShift = false;
            RefreshKeyboard();

            if (showTransit != null)
            {
                isBusy = true;
                gameObject.SetActive(true);
                showTransit.transit(true, () => { isBusy = false; });
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void TurnOff()
        {
            inputManager.layerMask = oldLayerMask;

            if (showTransit != null)
            {
                isBusy = true;
                showTransit.transit(false, () => { isBusy = false; gameObject.SetActive(false); });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void RefreshKeyboard()
        {
            if (isShift)
            {
                keyShiftArea.gameObject.SetActive(true);
                keyArea.gameObject.SetActive(false);
            }
            else
            {
                keyShiftArea.gameObject.SetActive(false);
                keyArea.gameObject.SetActive(true);
            }
        }

        private void OnClick(CommonButton btn)
        {
            if (isBusy)
                return;

            string key = btn.param as string;
            if (key == "Shift")
            {
                isShift = !isShift;
                RefreshKeyboard();
            }
            else if (key == "Back")
            {
                if (outputString.Length > 0)
                {
                    outputString = outputString.Substring(0, outputString.Length - 1);
                }
            }
            else if (key == "Return")
            {
                callbackInputFinish?.Invoke(outputString);
            }
            else
            {
                outputString += key;
            }

        }

        private void OnExit(CommonButton btn)
        {
            callbackExit?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}