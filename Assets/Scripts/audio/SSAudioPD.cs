using System.Collections.Generic;
using UnityEngine;

namespace audio
{
    public class SSAudioPD : MonoBehaviour
    {
        LibPdInstance puredataInstance;
        private float freq = 100;

        Camera user;
        GameObject target;
        Collider targetCol;

        private float distance;
        private float verticalAngle;
        private float horizontalAngle;

        
        // Use this for initialization
        void Awake()
        {
            puredataInstance = GetComponent<LibPdInstance>();
            user = Camera.main;
            target = GameObject.FindWithTag("Pickup");
            targetCol = target.GetComponent<Collider>();
            distance = Vector3.Distance(
               targetCol.ClosestPoint(user.transform.position),
               user.transform.position
           );
            

        }

        // Update is called once per frame
        void Update()
        {
            distance = Vector3.Distance(
               targetCol.ClosestPoint(user.transform.position),
               user.transform.position
           );
            puredataInstance.SendFloat("freq", freq);
        }
    }
}