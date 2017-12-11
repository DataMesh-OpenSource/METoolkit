using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.SpectatorView
{
    public class LiveUIManager : MonoBehaviour
    {

        private static LiveUIManager instance;
        private Dictionary<string, BaseUIForm> dicAllUIForms;//所有的UIForm缓存
        private Dictionary<string, BaseUIForm> dicCurrentShowUIForm;//当前显示的ui窗体
        private Stack<BaseUIForm> stackCurrentUIForms;

        public LiveUIForms liveUIForms;

        public static LiveUIManager Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
            dicAllUIForms = new Dictionary<string, BaseUIForm>();
            dicCurrentShowUIForm = new Dictionary<string, BaseUIForm>();
            stackCurrentUIForms = new Stack<BaseUIForm>();
            InitLoadAllUIForms();
        }

        private void InitLoadAllUIForms()
        {
            dicAllUIForms.Add(SysDefine.UI_UIFormAdvanced, liveUIForms.advancedUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormAnchorControl, liveUIForms.anchorControlUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormFunction, liveUIForms.functionUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormHolographic, liveUIForms.holographicUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormHololensAgent, liveUIForms.hololensAgentUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormMediaOperation, liveUIForms.mediaOperationUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormSetting, liveUIForms.settingUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormSocial, liveUIForms.socialUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormLivePreview, liveUIForms.livePreviewUIForm);
            dicAllUIForms.Add(SysDefine.UI_UIFormLiveInfomation, liveUIForms.liveInfomationsUIForm);

            dicCurrentShowUIForm.Add(SysDefine.UI_UIFormFunction, liveUIForms.functionUIForm);
            dicCurrentShowUIForm.Add(SysDefine.UI_UIFormMediaOperation, liveUIForms.mediaOperationUIForm);
            dicCurrentShowUIForm.Add(SysDefine.UI_UIFormLivePreview, liveUIForms.livePreviewUIForm);


        }

        public void ShowUIForms(string uiFormname)
        {
            BaseUIForm baseUIForm = null;
            if (string.IsNullOrEmpty(uiFormname)) return;
            baseUIForm = LoadFormsToAllUIFormsCatch(uiFormname);
            if (uiFormname == null)
                return;
            if (baseUIForm.CurrentUIType.isClearStack)
                ClearStackArray();

            switch (baseUIForm.CurrentUIType.UIFormShowModel)
            {
                case UIFormShowModel.Normal:
                    LoadUIToCurrentCache(uiFormname);
                    break;
                case UIFormShowModel.HideOther:
                    EnterUIForm(uiFormname);
                    break;
                case UIFormShowModel.ReverseChange:
                    PushUIFormToStack(uiFormname);
                    break;
            }

        }

        public void CloseUIForm(string uiFormname)
        {
            BaseUIForm baseUIForm;
            if (string.IsNullOrEmpty(uiFormname))
                return;
            dicCurrentShowUIForm.TryGetValue(uiFormname, out baseUIForm);
            if (baseUIForm == null) return;
            switch (baseUIForm.CurrentUIType.UIFormShowModel)
            {
                case UIFormShowModel.Normal:
                    ExitUIForm(uiFormname);
                    break;

                case UIFormShowModel.HideOther:
                    ExitUIFormAndDisplayOthers(uiFormname);
                    break;

                case UIFormShowModel.ReverseChange:
                    PopUIForm();
                    break;
            }
        }



        private BaseUIForm LoadFormsToAllUIFormsCatch(string uiFormName)
        {
            BaseUIForm baseUIForm = null;
            dicAllUIForms.TryGetValue(uiFormName, out baseUIForm);
            if (baseUIForm == null)
                Debug.LogWarning("cant catch the UIForm");
            return baseUIForm;
        }

        private bool ClearStackArray()
        {
            if (stackCurrentUIForms != null && stackCurrentUIForms.Count >= 1)
            {
                stackCurrentUIForms.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 加载并显示某个窗口
        /// </summary>
        /// <param name="uiFormname"></param>
        private void LoadUIToCurrentCache(string uiFormname)
        {
            BaseUIForm baseUIForm;
            BaseUIForm baseUIFormFromAllCatch;
            dicCurrentShowUIForm.TryGetValue(uiFormname, out baseUIForm);
            if (baseUIForm != null)
                return;
            dicAllUIForms.TryGetValue(uiFormname, out baseUIFormFromAllCatch);
            if (baseUIFormFromAllCatch != null)
            {
                dicCurrentShowUIForm.Add(uiFormname, baseUIFormFromAllCatch);
                baseUIFormFromAllCatch.Display();
            }

        }
        //打开指定窗口，关闭其他窗口
        private void EnterUIForm(string uiFormname)
        {
            BaseUIForm baseUIForm;
            dicCurrentShowUIForm.TryGetValue(uiFormname, out baseUIForm);
            if (baseUIForm != null)
                return;
            foreach (BaseUIForm baseUI in dicCurrentShowUIForm.Values)
            {
                baseUI.Hiding();
            }
            foreach (BaseUIForm baseUI in stackCurrentUIForms)
            {
                baseUI.Hiding();
            }
            BaseUIForm baseUIFormFromAllCatch;
            dicAllUIForms.TryGetValue(uiFormname, out baseUIFormFromAllCatch);
            if (baseUIFormFromAllCatch != null)
            {
                dicCurrentShowUIForm.Add(uiFormname, baseUIFormFromAllCatch);
                baseUIFormFromAllCatch.Display();
            }
        }

        //PushUI入栈
        private void PushUIFormToStack(string uiFormname)
        {
            BaseUIForm baseUIForm;
            if (stackCurrentUIForms != null && stackCurrentUIForms.Count > 0)
            {
                BaseUIForm topUIForm = stackCurrentUIForms.Peek();
                topUIForm.Freeze();
            }
            dicAllUIForms.TryGetValue(uiFormname, out baseUIForm);
            if (baseUIForm != null)
            {
                stackCurrentUIForms.Push(baseUIForm);
                baseUIForm.Display();
            }
            else
            {
                Debug.LogWarning("Please Check UIFormname , UIForm is null");
            }
        }

        private void ExitUIForm(string strUIFormname)
        {
            BaseUIForm baseUIForm;
            dicCurrentShowUIForm.TryGetValue(strUIFormname, out baseUIForm);
            if (baseUIForm == null)
                return;
            baseUIForm.Hiding();
            dicCurrentShowUIForm.Remove(strUIFormname);
        }

        private void ExitUIFormAndDisplayOthers(string strUIFormname)
        {
            BaseUIForm baseUIForm;
            if (string.IsNullOrEmpty(strUIFormname)) return;
            dicCurrentShowUIForm.TryGetValue(strUIFormname, out baseUIForm);
            if (baseUIForm == null)
                return;
            baseUIForm.Hiding();
            dicCurrentShowUIForm.Remove(strUIFormname);
            foreach (BaseUIForm baseUI in dicCurrentShowUIForm.Values)
            {
                baseUI.Display();
            }
            foreach (BaseUIForm baseUI in stackCurrentUIForms)
            {
                baseUI.Display();
            }

        }

        private void PopUIForm()
        {
            if (stackCurrentUIForms.Count >= 2)
            {
                BaseUIForm topUIForm = stackCurrentUIForms.Pop();
                topUIForm.Hiding();
                BaseUIForm nextTopUIForm = stackCurrentUIForms.Peek();
                nextTopUIForm.Display();
            }
            else if (stackCurrentUIForms.Count == 1)
            {
                BaseUIForm topUIForm = stackCurrentUIForms.Pop();
                topUIForm.Hiding();
            }
        }

    }
}


