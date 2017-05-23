//This script handles raycasting from the main camera into the scene. It is also responsible for knowing what the user is
//looking at and telling the interactable objects what the player is doing (looking at them, looking away, clicking them, and 
//releasing them)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaycastInput : MonoBehaviour 
{
	[SerializeField] string primaryInputAxis = "Fire1";
	[SerializeField] LayerMask whatIsInteractable;		//The layers that this raycast affects
	[SerializeField] float rayDistance = 20f;			//How far should the ray be cast?
	[SerializeField] bool drawDebugLine;				//Should we draw a debug ray

	Ray ray;
	RaycastHit rayHit;									//The results of a raycast
	PointerEventData eventData;							//The data for our simulated events


	void Reset()
	{
		//Set the Layer Mask to interact with everything but the Ignore Raycast layer using bitwise operations
		whatIsInteractable = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
	}

	void Start()
	{
		eventData = new PointerEventData (EventSystem.current);
		eventData.pointerId = 0;

		eventData.position = new Vector2 (Screen.width / 2f, Screen.height / 2f);

		eventData.pressPosition = eventData.position;
	}

	void Update () 
	{
		//Every frame we look for interactables and check for hardware inputs
		LookForInteractables ();
		CheckInput ();
	}


	void LookForInteractables()
	{
		//Generate a new ray at our input object facing forward
		ray = new Ray (transform.position, transform.forward);

		if(drawDebugLine)
			Debug.DrawLine (transform.position, transform.position + transform.forward * rayDistance, Color.red);

		Physics.Raycast (ray, out rayHit, rayDistance, whatIsInteractable);

		//We didn't hit anything
		if (rayHit.transform == null)
		{
			LookAway ();
			return;
		}

		eventData.pointerCurrentRaycast = ConvertRaycastHitToRaycastResult (rayHit);

		if (eventData.pointerEnter == rayHit.transform.gameObject)
			return;
		
		LookAway ();

		eventData.pointerEnter = rayHit.transform.gameObject;
		ExecuteEvents.ExecuteHierarchy (eventData.pointerEnter, eventData, ExecuteEvents.pointerEnterHandler);
	}

	void CheckInput()
	{
		//If we press the Fire1 input axis...
		if (Input.GetButtonDown (primaryInputAxis) && eventData.pointerEnter != null) 
		{
			eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
			eventData.pointerPress = ExecuteEvents.ExecuteHierarchy (eventData.pointerEnter, eventData, ExecuteEvents.pointerDownHandler);
		} 
		//Otherwise, if we just released the Fire1 input axis...
		else if(Input.GetButtonUp(primaryInputAxis))
		{
			if(eventData.pointerPress != null)
				ExecuteEvents.ExecuteHierarchy (eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

			if(eventData.pointerPress == eventData.pointerEnter)
				ExecuteEvents.ExecuteHierarchy (eventData.pointerEnter, eventData, ExecuteEvents.pointerClickHandler);

			eventData.pointerPress = null;
		}
	}

	void LookAway()
	{
		if (eventData.pointerEnter != null) 
		{
			ExecuteEvents.ExecuteHierarchy (eventData.pointerEnter, eventData, ExecuteEvents.pointerExitHandler);
			eventData.pointerEnter = null;
		}
	}

	RaycastResult ConvertRaycastHitToRaycastResult(RaycastHit hit)
	{
		RaycastResult rayResult = new RaycastResult ();
		rayResult.gameObject = hit.transform.gameObject;
		rayResult.distance = rayHit.distance;
		rayResult.worldPosition = rayHit.point;
		rayResult.worldNormal = rayHit.normal;

		return rayResult;
	}
}
