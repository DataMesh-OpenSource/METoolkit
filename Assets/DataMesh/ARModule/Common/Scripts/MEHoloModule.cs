using UnityEngine;

namespace DataMesh.AR
{
    public abstract class MEHoloModule : MonoBehaviour
    {
        public bool AutoTurnOn = false;

        protected bool hasInit = false;
        protected bool hasTurnOn = false;

        /// <summary>
        /// 初始化模块
        /// </summary>
        public void Init()
        {
            if (hasInit)
                return;

            _Init();
            hasInit = true;

            if (AutoTurnOn)
            {
                TurnOn();
            }
        }

        /// <summary>
        /// 实际的初始化操作，应由子类实现 
        /// </summary>
        protected abstract void _Init();

        /// <summary>
        /// 检查模块是否已经初始化
        /// </summary>
        /// <returns></returns>
        public bool HasInit()
        {
            return hasInit;
        }

        /// <summary>
        /// 启动模块
        /// </summary>
        public void TurnOn()
        {
            if (!HasInit())
                return;

            if (hasTurnOn)
                return;

            hasTurnOn = true;

            _TurnOn();
        }

        /// <summary>
        /// 检查模块是否已经被开启
        /// </summary>
        /// <returns></returns>
        public bool HasTurnOn()
        {
            return hasTurnOn;
        }

        /// <summary>
        /// 实际的启动操作，应由子类实现
        /// </summary>
        protected abstract void _TurnOn();

        /// <summary>
        /// 关闭模块 
        /// </summary>
        public void TurnOff()
        {
            if (!HasInit())
                return;

            if (!hasTurnOn)
                return;

            _TurnOff();

            hasTurnOn = false;
        }

        protected abstract void _TurnOff();
    }


    public abstract class MEHoloModuleSingleton<T> : MEHoloModule where T : MEHoloModuleSingleton<T>
    {
        private static T _Instance;


        public static T Instance
        {
            get
            {
                return _Instance;
            }
        }

        protected virtual void Awake()
        {
            _Instance = (T)this;
        }


        protected virtual void OnDestroy()
        {
        }

    }
}