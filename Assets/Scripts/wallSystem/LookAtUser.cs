using UnityEngine;

//Script that makes the images stare at u
public class LookAtUser : MonoBehaviour
{
    private GameObject _player;

    // Use this for initialization
    private void Start()
    {
        _player = GameObject.Find("Participant");
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 origin = transform.position - _player.transform.position;
        origin = origin.normalized;
        transform.rotation = Quaternion.Euler(0, Mathf.Rad2Deg * Mathf.Atan2(origin.x, origin.z), 0);
    }
}
