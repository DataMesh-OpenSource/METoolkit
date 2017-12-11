using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

using System.Text;
using UnityEngine.UI;

namespace DataMesh.AR.Common {
    public class ConnToCloud : MonoBehaviour
    {
        Encoding encoding = Encoding.UTF8;
        public string context;
        // Use this for initialization
        public string main_url = "http://192.168.1.102:8123";
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }
        IEnumerator StartLoop(string getUrl)
        {
            int i = 0;
            while (true)
            {
                i++;
                Debug.Log("StartLoop:" + i.ToString());
                yield return new WaitForSeconds(1f);
                StartCoroutine(Upload());
                //StartCoroutine(GetTextWWW(getUrl));
                //StartCoroutine(GetText(getUrl));
                //StartCoroutine(Location());

            }

        }
        IEnumerator Upload()
        {
            WWWForm form = new WWWForm();
            form.AddField("gazedata", "1234567890");

            using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.102:8080/hololens/collection", form))
            {
                yield return www.Send();
                if (www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                }
            }
        }
        public IEnumerator GetText(string getUrl)
        {
            UnityWebRequest www = UnityWebRequest.Get(getUrl);
            yield return www.Send();
            if (www.isNetworkError)
            {
                Debug.Log("error:" + www.error);
                context = "error:" + www.error;
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);
                context = www.downloadHandler.text;

                // Or retrieve results as binary data
                //byte[] results = www.downloadHandler.data;
            }
        }
        IEnumerator GetTextWWW(string getUrl)
        {
            using (WWW www = new WWW(getUrl))
            {
                while (!www.isDone)
                    yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log("error:" + www.error);
                    context = "error:" + www.error;
                }

                else
                {
                    Debug.Log(www.text);
                    context = www.text;
                }
            }

        }
        IEnumerator Location()
        {
            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Unable to use device");
                yield break;
            }


            // Start service before querying location
            Input.location.Start();

            // Wait until service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                Debug.Log("Timed out");
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Unable to determine device location");
                yield break;
            }
            else
            {
                // Access granted and location value could be retrieved
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
                context = "Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp;
            }

            // Stop service if there is no need to query location updates continuously
            Input.location.Stop();
        }


    }

}
