using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace DataMesh.AR.Common {

    public class FollowGaze : MonoBehaviour
    {
        public int menuFollowflag;
        public float distance;
        public GameObject cam;
        public GameObject hud;
        public float speed = 0.5f;
        public int scale;
        private float mFloat = 0;
        bool isYes = false;
        // Use this for initialization
        void Start()
        {
#if UNITY_EDITOR
            // When in the Unity editor, try loading saved meshes from a model.
            distance = 1f;
#endif

            isYes = false;
            menuFollowflag = 1;
            //StartCoroutine(Destroy());
        }

        // Update is called once per frame
        void Update()
        {
            if (menuFollowflag == 1)
            {
                Vector3 Position1 = hud.transform.position - cam.transform.position;
                Vector3 toPosition = cam.transform.forward;
                var angle = Vector3.Angle(Position1, toPosition);
                float tmpdistance = Distance(hud, cam);
                Ray ray = new Ray(cam.transform.position, toPosition - cam.transform.position);
                toPosition = ray.GetPoint(2);
                //print(angle.ToString() + "-----" + System.Math.Abs(angle % 45).ToString() + "-----" + (angle / 45).ToString() + "-----"+ distance .ToString()+ "-----");
                if (System.Math.Abs(angle % 45) > 16 || angle / 45 >= 1 || System.Math.Abs(tmpdistance - distance) > 0.1f)
                {
                    Quaternion toQuat = cam.transform.localRotation;
                    toQuat.z = 0;
                    //toQuat.x = 0;
                    hud.transform.rotation = toQuat;
                    //hud.transform.position = Vector3.Lerp(hud.transform.position, ProposeTransformPosition(), speed * 0.2f);
                    isYes = true;
                    if (System.Math.Abs(tmpdistance - distance) > 0.3f)
                    {
                        scale = 5;
                    }
                    else
                    {
                        scale = 1;
                    }
                }
                else
                {
                    isYes = false;
                }




                //print(isYes);
                if (isYes)
                {
                    mFloat = Time.deltaTime * speed * 0.2f * scale;
                    // 弧线的中心

                    Vector3 center = cam.transform.position;

                    // 向下移动中心，垂直于弧线

                    // 相对于中心在弧线上插值

                    Vector3 riseRelCenter = hud.transform.position - center;

                    Vector3 setRelCenter = ProposeTransformPosition() - center;

                    hud.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, mFloat);

                    hud.transform.position += center;


                }

            }



            else if (menuFollowflag == 2)
            {
                Quaternion toQuat = hud.transform.rotation;
                toQuat.z = 0;
                toQuat.x = 0;
                hud.transform.rotation = toQuat;
            }
            else if (menuFollowflag == 3)
            {
                Quaternion toQuat = cam.transform.rotation;
                toQuat.z = 0;
                toQuat.x = 0;
                hud.transform.rotation = toQuat;
            }





        }

        public void OnSetMenuFollowFlag()
        {
            //menuFollowEnabled =! menuFollowEnabled;
            print(menuFollowflag);
            menuFollowflag = menuFollowflag + 1;
            if (menuFollowflag > 3)
            {
                menuFollowflag = 1;
            }
        }
        Vector3 ProposeTransformPosition()
        {
            // Put the model 2m in front of the user.
            Vector3 retval = cam.transform.position + cam.transform.forward * distance;

            return retval;
        }
        public float Distance(GameObject fromObject, GameObject toObject)
        {
            Vector3 f;
            Vector3 t;
            //m,n定义两个私有 Vector3类型   
            f = fromObject.transform.position;
            t = toObject.transform.position;
            //赋m,n予a,b的位置 
            return Vector3.Distance(f, t);
        }

        IEnumerator Destroy()
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(this.gameObject);
        }
    }
}
