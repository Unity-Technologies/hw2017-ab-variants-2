using UnityEngine;
using UnityEngine.VR;

public class CameraPositionTrigger : MonoBehaviour 
{
	[SerializeField] Transform cameraMount;
	[SerializeField] Transform vrCameraRig;

	Animator cameraAnim;


	void Start()
	{
		if(vrCameraRig == null)
			vrCameraRig = GameObject.Find("CameraRig").transform;
		
		cameraAnim = vrCameraRig.GetComponent<Animator> ();
	}

	void OnTriggerEnter(Collider other)
	{
		cameraAnim.SetTrigger ("Blink");
		Invoke ("MoveCamera", .05f);
	}

	void MoveCamera()
	{
		vrCameraRig.position = cameraMount.position;
		vrCameraRig.rotation = cameraMount.rotation;
	}
}
