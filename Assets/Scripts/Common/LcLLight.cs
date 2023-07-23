using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
namespace LcLSoftRenderer
{
    [ExecuteAlways]
    public class LcLLight : MonoBehaviour
    {
        public float3 pos => transform.position;
        public float3 direction => -transform.forward;
        public float3 color => m_Light.color.ToFloat3();
        public float intensity => m_Light.intensity;
        public float range => m_Light.range;

        Light m_Light;
        private void OnEnable()
        {
            m_Light = GetComponent<Light>();
            Global.light = this;
        }
    }
}
