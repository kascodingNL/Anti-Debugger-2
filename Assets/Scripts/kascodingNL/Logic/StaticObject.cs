using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.kascodingNL.Logic
{
    class StaticObject : MonoBehaviour
    {
        public Vector3 spawnPos;
        public Quaternion spawnRotation;

        public Transform start;

        void Awake()
        {
            spawnPos = start.position;
            spawnRotation = start.rotation;
        }

        void Update()
        {
            transform.position = spawnPos;
            transform.rotation = spawnRotation;
        }

        public void ChangePosition(Vector3 newPos)
        {
            spawnPos = newPos;
        }

        public void ChangeRotation(Quaternion newQuaternion)
        {
            transform.rotation = newQuaternion;
            spawnRotation = newQuaternion;
        }
    }
}
