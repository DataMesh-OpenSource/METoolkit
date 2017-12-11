using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// LiveUI窗体的类型(是否还用得到?)
/// </summary>
public enum UIFormType
{
    Normal,
    Fixed,
    PopUp,
}

public enum UIFormShowModel
{
    Normal,
    ReverseChange,//反向切换
    HideOther,
}


public class SysDefine {

    public const string UI_UIFormAdvanced = "UIFormAdvanced";
    public const string UI_UIFormAnchorControl = "UIFormAnchorControl";
    public const string UI_UIFormFunction = "UIFormFunction";
    public const string UI_UIFormHolographic = "UIFormHolographic";
    public const string UI_UIFormHololensAgent = "UIFormHololensAgent";
    public const string UI_UIFormMediaOperation = "UIFormMediaOperation";
    public const string UI_UIFormSetting = "UIFormSetting";
    public const string UI_UIFormSocial = "UIFormSocial";
    public const string UI_UIFormLivePreview = "UIFormLivePreview";
    public const string UI_UIFormLiveInfomation = "UIFormLiveInfomation";

    public const string MESSAGE_CheckAndUploadToServer = "CheckAndUploadToServer";

    public const string MESSAGE_Infomation = "InfomationMessage";
    public const string MESSAGE_InfomationTypeNormal = "InfomationTypeNormal";
    public const string MESSAGE_InfomationTypeError = "InfomationTypeError";

}


