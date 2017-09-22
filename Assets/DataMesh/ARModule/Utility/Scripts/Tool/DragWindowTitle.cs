using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragWindowTitle : MonoBehaviour , IDragHandler , IPointerDownHandler{

    private Canvas canvas;
    private Vector2 distanceClickPointAndTargetRect;//初始点击点和目标Rect之间的间距
    private Vector2 clickPointVec2; //鼠标点击点的Vector2坐标
    private Vector2 dragPointVec2;
    private float screenWidth;
    private float screenHeight;

    public RectTransform targetRect;//想要移动的Rect

   

    private void Awake()
    {
        canvas = targetRect.transform.parent.GetComponent<Canvas>();
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        if (canvas == null)
        {
            Debug.LogError("get targetParentComponent‘Canvas’failed , please check the targetRect parent");
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera, out clickPointVec2);
        distanceClickPointAndTargetRect = targetRect.anchoredPosition - clickPointVec2;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        if (mouseX > 0 && mouseX < screenWidth && mouseY > 0 && mouseY < screenHeight)
        {
            canvas = canvas == null ? GetComponent<Canvas>() : canvas;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                Input.mousePosition, canvas.worldCamera, out dragPointVec2))
            {
                targetRect.anchoredPosition = dragPointVec2 + distanceClickPointAndTargetRect;
            }
        }
    }

    

}
