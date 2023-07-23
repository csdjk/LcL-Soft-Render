using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace LcLSoftRenderer
{


    public static class Global
    {
        public static float4 ambientColor { get; set; }
        public static LcLLight light { get; set; }
        public static float3 cameraPosition { get; set; }
        public static float3 cameraDirection { get; set; }



    }
}

