using UnityEngine;

#if UNITY_METRO && !UNITY_EDITOR
#else
// 这些平台上，不应该存在这些接口，但为了保证操作一致性，还是需要这些接口的定义
// 这里需要定义一些假接口，以保跨平台时可编译过

namespace DataMesh.AR.FakeUWP
{
    public class WorldAnchor : MonoBehaviour
    {
        public bool isLocated = true;

        public System.Action<WorldAnchor, bool> OnTrackingChanged;
    }

    public class WorldAnchorStore
    {
        public static void GetAsync(System.Action<WorldAnchorStore> cb)
        {
            WorldAnchorStore store = new WorldAnchorStore();
            cb(store);
        }

        public WorldAnchor Load(string name, GameObject rootObj)
        {
            Debug.Log("[Fake] Anchor Loaded!");
            return null;
        }

        public bool Save(string name, WorldAnchor anchor)
        {
            Debug.Log("[Fake] Anchor Saved!");
            return true;
        }

        public void Delete(string name)
        {
            Debug.Log("[Fake] Anchor Deleted!");
        }
    }

    public class HolographicSettings
    {
        public static void SetFocusPointForFrame(Vector3 planePosition, Vector3 gazeNormal, Vector3 velocity)
        {
            //Debug.Log("[Fake] Set Focus Point!");
        }
    }

    public sealed class GestureRecognizer
    {
        public event GestureErrorDelegate GestureErrorEvent;
        public event HoldCanceledEventDelegate HoldCanceledEvent;
        public event HoldCompletedEventDelegate HoldCompletedEvent;
        public event HoldStartedEventDelegate HoldStartedEvent;
        public event ManipulationCanceledEventDelegate ManipulationCanceledEvent;
        public event ManipulationCompletedEventDelegate ManipulationCompletedEvent;
        public event ManipulationStartedEventDelegate ManipulationStartedEvent;
        public event ManipulationUpdatedEventDelegate ManipulationUpdatedEvent;
        public event NavigationCanceledEventDelegate NavigationCanceledEvent;
        public event NavigationCompletedEventDelegate NavigationCompletedEvent;
        public event NavigationStartedEventDelegate NavigationStartedEvent;
        public event NavigationUpdatedEventDelegate NavigationUpdatedEvent;
        public event RecognitionEndedEventDelegate RecognitionEndedEvent;
        public event RecognitionStartedEventDelegate RecognitionStartedEvent;
        public event TappedEventDelegate TappedEvent;

        public void CancelGestures() { }
        public void Dispose() { }
        public bool IsCapturingGestures() { return false; }
        public GestureSettings SetRecognizableGestures(GestureSettings newMaskValue) { return new GestureSettings(); }
        public void StartCapturingGestures() { }
        public void StopCapturingGestures() { }

        public delegate void GestureErrorDelegate(string error, int hresult);
        public delegate void HoldCanceledEventDelegate(InteractionSourceKind source, Ray headRay);
        public delegate void HoldCompletedEventDelegate(InteractionSourceKind source, Ray headRay);
        public delegate void HoldStartedEventDelegate(InteractionSourceKind source, Ray headRay);
        public delegate void ManipulationCanceledEventDelegate(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay);
        public delegate void ManipulationCompletedEventDelegate(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay);
        public delegate void ManipulationStartedEventDelegate(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay);
        public delegate void ManipulationUpdatedEventDelegate(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay);
        public delegate void NavigationCanceledEventDelegate(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay);
        public delegate void NavigationCompletedEventDelegate(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay);
        public delegate void NavigationStartedEventDelegate(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay);
        public delegate void NavigationUpdatedEventDelegate(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay);
        public delegate void RecognitionEndedEventDelegate(InteractionSourceKind source, Ray headRay);
        public delegate void RecognitionStartedEventDelegate(InteractionSourceKind source, Ray headRay);
        public delegate void TappedEventDelegate(InteractionSourceKind source, int tapCount, Ray headRay);
    }

    public enum GestureSettings
    {
        None = 0,
        Tap = 1,
        DoubleTap = 2,
        Hold = 4,
        ManipulationTranslate = 8,
        NavigationX = 16,
        NavigationY = 32,
        NavigationZ = 64,
        NavigationRailsX = 128,
        NavigationRailsY = 256,
        NavigationRailsZ = 512
    }

    public enum InteractionSourceKind
    {
        Other = 0,
        Hand = 1,
        Voice = 2,
        Controller = 3
    }
}
#endif
