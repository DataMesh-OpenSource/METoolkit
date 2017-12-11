using DataMesh.AR.SpectatorView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormHolographic : BaseUIForm
    {

        public InputField frameOffsetInput;
        public InputField antiShakeBefore;
        public InputField antiShakeAfter;
        public Slider alphaSlider;
        public Slider filterSlider;
        public Slider soundSlider;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            frameOffsetInput.text = LiveParam.SyncDelayTime.ToString();
            antiShakeBefore.text = LiveParam.AntiShakeBeforeTime.ToString();
            antiShakeAfter.text = LiveParam.AntiShakeAfterTime.ToString();

            alphaSlider.value = LiveParam.Alpha;
            soundSlider.value = LiveParam.SoundVolume;
            filterSlider.value = LiveParam.Filter;

            alphaSlider.onValueChanged.AddListener(OnAlphaSliderChange);
            frameOffsetInput.onValueChanged.AddListener(OnFrameOffsetInput);

            soundSlider.onValueChanged.AddListener(OnSoundSliderChange);
            filterSlider.onValueChanged.AddListener(OnFilterSliderChange);

            antiShakeBefore.onValueChanged.AddListener(OnAntiShakeBeforeChange);
            antiShakeAfter.onValueChanged.AddListener(OnAntiShakeAfterChange);

        }

        private void CloseSettingPannel(GameObject obj)
        {
            Debug.Log("Close Setting Pannel");
        }

        private void OnAlphaSliderChange(float value)
        {
            LiveParam.Alpha = value;
        }
        private void OnSoundSliderChange(float value)
        {
            LiveParam.SoundVolume = value;
        }

        private void OnFilterSliderChange(float value)
        {
            LiveParam.Filter = value;
        }
        private void OnFrameOffsetInput(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.SyncDelayTime = frame;
            }
        }

        private void OnAntiShakeBeforeChange(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.AntiShakeBeforeTime = frame;
            }
        }

        private void OnAntiShakeAfterChange(string value)
        {
            float frame = -9999;
            float.TryParse(value, out frame);

            if (frame != -9999)
            {
                LiveParam.AntiShakeAfterTime = frame;
            }
        }
#endif
    }

}

