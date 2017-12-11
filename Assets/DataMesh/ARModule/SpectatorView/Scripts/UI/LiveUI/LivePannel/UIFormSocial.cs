using DataMesh.AR.Event;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace DataMesh.AR.SpectatorView
{
    public class UIFormSocial : BaseUIForm , IPointerClickHandler
    {

        public Dropdown albumProfileDropdown;//相册下拉列表
        public Dropdown RecordTimeDropdown;//录制时间下拉列表

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private SocialControl socialControl;

        private void Awake()
        {
            
        }

        private void Start()
        {
            socialControl = SocialControl.Instance;
            LoadRecordTimeData();
            LoadAlbumListData();
            albumProfileDropdown.onValueChanged.AddListener(AlbumNameChanged);
            RecordTimeDropdown.onValueChanged.AddListener(RecordTimeChanged);        
        }

        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// 刷新TimeDropDown数据
        /// </summary>
        private void LoadRecordTimeData()
        {
            List<string> timeList = socialControl.GetTimeList();
            if (timeList != null)
            {
                RecordTimeDropdown.ClearOptions();
                for (int i = 0; i < timeList.Count; i++)
                {
                    RecordTimeDropdown.options.Add(new Dropdown.OptionData(timeList[i]));
                }
                RecordTimeDropdown.value = 0;
                RecordTimeDropdown.RefreshShownValue();
            }
            else
            {
                Debug.Log("Time list is null");
            }
        }

        private void Update()
        {

        }

        /// <summary>
        /// 刷新相册列表
        /// </summary>
        /// <param name="profileName"></param>
        private void LoadAlbumListData()
        {
            List<string> albumList = socialControl.GetAlbumList();
            if (albumList != null)
            {
                albumProfileDropdown.options.Clear();
                for (int i = 0; i < albumList.Count; i++)
                {
                    albumProfileDropdown.options.Add(new Dropdown.OptionData(albumList[i]));
                }
                albumProfileDropdown.value = 0;
                albumProfileDropdown.RefreshShownValue();
            }
        }

        public void AlbumNameChanged(int value)
        {
            string strValue = albumProfileDropdown.options[value].text;
            socialControl.CurrentAlbumName = strValue;
        }

        public void RecordTimeChanged(int value)
        {
            string strValue = RecordTimeDropdown.options[value].text;
            if (strValue != "无限制")
            {
                int time = int.Parse(strValue);
                socialControl.RecordTime = time;
            }
            else
            {
                socialControl.RecordTime = -1;
            }
        }

        private void albumClick(GameObject obj)
        {
            Debug.Log("albumClick");
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (albumProfileDropdown.isActiveAndEnabled)
            {
                albumProfileDropdown.Hide();
            }
            if (RecordTimeDropdown.isActiveAndEnabled)
            {
                RecordTimeDropdown.Hide();
            }
        }

#else
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {

        }
#endif



    }
}


