using System.Collections.Generic;
using UnityEngine;

namespace audio
{
    public class SSAudioPD : MonoBehaviour
    {
        static private float linearLink=-200/Mathf.PI;
        static private float b = 500.0f;

        LibPdInstance puredataInstance;
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
            Debug.Log(puredataInstance.patchName);
            user = Camera.main;
            target = GameObject.FindWithTag("Pickup");
            targetCol = target.GetComponent<Collider>();
            distance = Vector3.Distance(
               targetCol.ClosestPoint(user.transform.position),
               user.transform.position
           );
            verticalAngle = 0;
            horizontalAngle = 0;
            
        }

        // Update is called once per frame
        void Update()
        {
            horizontalAngle = Mathf.Acos(Vector2.Dot(new Vector2(user.transform.forward.x, user.transform.forward.z), 
                                                    new Vector2(target.transform.forward.x, target.transform.forward.z)));
            puredataInstance.SendFloat("freq", horizontalAngle*linearLink+b);
        }
    }
}