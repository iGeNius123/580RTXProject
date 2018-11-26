using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSoureRotate : MonoBehaviour {
    public GameObject light;
	
	// Update is called once per frame
	void Update () {
        float x = light.transform.eulerAngles.x;
        float y = light.transform.eulerAngles.y;
        float z = light.transform.eulerAngles.z;

        light.transform.eulerAngles = new Vector3(x,y+0.5f,z);
	}
}
