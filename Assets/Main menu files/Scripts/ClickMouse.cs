using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClickMouse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public MeshRenderer mr;
    public Material emissiveMaterial;
    public GameObject light;
    public TMP_Text text;
    public GameObject TimelineObj;
    public CinemachineVirtualCamera CameraVirtual;
    public GameObject eventManager;

    private EventHandler eventHandlerScript;
    private CinemachinePOV cameraVirtualPOV;
    private TimelineControler timelineControllerScript;

    public enum Buttons
    {
        Start,
        Settings,
        PowerSelector,
        Exit,
        Marketplace,
        About
    }

    public Buttons CurrentButton;

    void Start () {
        if (TimelineObj != null) timelineControllerScript = TimelineObj.GetComponent<TimelineControler>();
        if (CameraVirtual != null) cameraVirtualPOV = CameraVirtual.GetComponent<CinemachinePOV>();
        eventHandlerScript = eventManager.GetComponent<EventHandler>();
        Cursor.lockState = CursorLockMode.Confined;
    }

    // ------------ Cursor Checking ------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log($" Clicked {gameObject.name}");
        // freeLook.enabled = false;
        ButtonClicked();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (mr != null) mr.material.color = Color.cyan;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (light != null) light.SetActive(true);
        if (emissiveMaterial != null) emissiveMaterial.EnableKeyword("_EMISSION");
        if (mr != null) mr.material.color = Color.green;
        // Debug.Log($"You are pointing at {gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (light != null) light.SetActive(false);
        if (emissiveMaterial != null) emissiveMaterial.DisableKeyword("_EMISSION");
        if (mr != null) mr .material.color = Color.white;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mr != null) mr .material.color = Color.white;
    }

    // ------------------ Menu Actions ------------------------------
    public void ButtonClicked()
    {
        switch (CurrentButton)
        {
            case Buttons.Start:
                // Debug.Log("I got started");
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                // TODO: Disable Mouse input.
                if (timelineControllerScript != null) timelineControllerScript.Play();
                StartCoroutine(eventHandlerScript.StartChangeScene(timelineControllerScript.PlayDuration()));
                break;
            case Buttons.PowerSelector:
                eventHandlerScript.StartStageSelectorScene();
                break;
            case Buttons.About:
                StartCoroutine(eventHandlerScript.StartAboutScene());
                break;
            case Buttons.Marketplace:
                break;
            case Buttons.Exit:
                break;
        }
    }
}
