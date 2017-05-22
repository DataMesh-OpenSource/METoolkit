using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_METRO && !UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

namespace DataMesh.AR.Interactive
{

    public class SpeechManager : DataMesh.AR.MEHoloModuleSingleton<SpeechManager>
    {
#if UNITY_METRO && !UNITY_EDITOR
        private KeywordRecognizer keywordRecognizer = null;
#endif
        private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();


        protected override void Awake()
        {
            base.Awake();
        }

        protected override void _Init()
        {
            
        }

        protected override void _TurnOn()
        {
#if UNITY_METRO && !UNITY_EDITOR
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
#endif
            StartRecognize();
        }

        protected override void _TurnOff()
        {
#if UNITY_METRO && !UNITY_EDITOR
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Dispose();
                keywordRecognizer = null;
            }
#endif
        }

        public void AddKeywords(string key, System.Action cbDeal)
        {
            _TurnOff();

            if (keywords.ContainsKey(key))
                return;

            keywords.Add(key, cbDeal);

            if (AutoTurnOn)
                _TurnOn();
        }

        /// <summary>
        /// 初始化。注意，一定要在所有keyword添加之后才能调用！
        /// </summary>
        public void StartRecognize()
        {
#if UNITY_METRO && !UNITY_EDITOR

            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

            
            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
#endif
        }

#if UNITY_METRO && !UNITY_EDITOR
        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            System.Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }
#endif
    }
}