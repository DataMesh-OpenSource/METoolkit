#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEHoloClient.Sync;

using WebSocketSharp;
using WebSocketSharp.Server;

public class LiveServerHandler : BevMessageHandler
{
    public System.Action cbOpen;
    public System.Action cbClose;

    protected override void OnOpen()
    {
        Debug.Log("Bev connection opened.");
        if (cbOpen != null)
            cbOpen();
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log("Bev connection closed.");
        if (cbClose != null)
            cbClose();
    }

}

#endif
