using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.UI
{
    public class UIManager : DataMesh.AR.MEHoloModuleSingleton<UIManager>
    {
        public BlockMenuManager menuManager;
        public CursorController cursorController;
        public BlockListManager listManager;


        protected override void _Init()
        {
            menuManager.Init();
            cursorController.Init();
        }

        protected override void _TurnOn()
        {
            menuManager.TurnOn();
            cursorController.TurnOn();
        }

        protected override void _TurnOff()
        {
            menuManager.TurnOff();
            cursorController.TurnOff();
        }

        public void EnableCursor(bool enable)
        {
            if (enable)
                cursorController.TurnOn();
            else
                cursorController.TurnOff();
        }


    }
}