using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Network
{
    /// <summary>
    /// Room机制还没有实现，因此先做一个简单的临时类来管理当前房间名称 
    /// </summary>
    public class RoomManager
    {
        private static RoomManager _instance = null;
        public static RoomManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RoomManager();
                return _instance;
            }
        }

        private string currentRoomName = "Room22";

        private RoomManager() { }

        public string GetCurrentRoom()
        {
            return currentRoomName;
        }

        public void SetCurrentRoom(string name)
        {
            currentRoomName = name;
        }
    }
}