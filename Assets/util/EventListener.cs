using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class EventListener : UIBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
                            IDropHandler, IScrollHandler, ISelectHandler, IDeselectHandler
{
    public delegate void VoidDelegate(GameObject go);

    public delegate void EventDelegate(PointerEventData dt);

    public delegate void BaseEventDelegate(BaseEventData dt);

    public VoidDelegate onClick;

    public VoidDelegate onEnter;

    public VoidDelegate onExit;

    public EventDelegate onDown;

    public EventDelegate onUp;

    public EventDelegate onDrag;

    public EventDelegate onBeginDrag;

    public EventDelegate onEndDrag;

    public UnityAction<bool> onRepeat;

    public EventDelegate onDrop;

    public EventDelegate onScroll;

    public BaseEventDelegate onSelect;

    public BaseEventDelegate onDeselect;

    public float durationThreshold = 0.05f;

    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;

    static public EventListener Get(GameObject go)
    {
        EventListener listener = go.GetComponent<EventListener>();
        if (listener == null) listener = go.AddComponent<EventListener>();
        return listener;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (onClick != null)
        {
            //Debug.Log(gameObject.name);
            onClick(gameObject);
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
            onDrag(eventData);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null)
            onBeginDrag(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null)
            onEndDrag(eventData);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null)
            onDown(eventData);
        timePressStarted = Time.realtimeSinceStartup;
        isPointerDown = true;
        longPressTriggered = false;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null)
            onUp(eventData);
        if (onRepeat != null && isPointerDown)
            onRepeat(false);
        isPointerDown = false;

    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null)
            onEnter(gameObject);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null)
            onExit(gameObject);
        if (onRepeat != null && isPointerDown)
            onRepeat(false);
        isPointerDown = false;

    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        if (onDrop != null)
            onDrop(eventData);
    }

    void IScrollHandler.OnScroll(PointerEventData eventData)
    {
        if (onScroll != null)
            onScroll(eventData);
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        if (onSelect != null)
            onSelect(eventData);
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        if (onDeselect != null)
            onDeselect(eventData);
    }


    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.realtimeSinceStartup - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                if (onRepeat != null)
                {
                    onRepeat(true);
                }
            }
        }
    }
}
