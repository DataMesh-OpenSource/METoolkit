using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.SpectatorView
{

    public class CalibrationData
    {
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector2 DSLR_fov { get; set; }
        public Vector4 DSLR_distortion { get; set; }
        public Vector4 DSLR_matrix { get; set; }
    }

}