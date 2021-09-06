using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Windslayer.Client
{
    public class MainCamera : MonoBehaviour
    {
        [Tooltip("Camera speed")]
        public float Speed;

        public Transform Target { get; set; }

        Vector2 m_TargetPosition;

        void Update()
        {
            if (!Target) {
                return;
            }

            m_TargetPosition = Target.position;
            
            transform.position = new Vector3(
                Mathf.Lerp(transform.position.x, m_TargetPosition.x, Speed * Time.fixedDeltaTime),
                Mathf.Lerp(transform.position.y, m_TargetPosition.y, Speed * Time.fixedDeltaTime),
                -10f
            );
        }
    }
}
