#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEHoloClient.Sync;

using WebSocketSharp;
using WebSocketSharp.Server;

public class LiveServerHandler : BevMessageHandler
{
    public static System.Action<LiveServerHandler> cbOpen;
    public static System.Action<LiveServerHandler> cbClose;

    protected override void OnOpen()
    {
        //base.OnOpen();

        Debug.Log("Bev connection opened.");
        if (cbOpen != null)
            cbOpen(this);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        //base.OnClose(e);

        Debug.Log("Bev connection closed.");
        if (cbClose != null)
            cbClose(this);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        // Enqueue to the SyncQueue.
        SyncServer.SyncQueue.Enqueue(e.RawData);
    }

}

#endif
