using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FitMode = UnityEngine.UI.ContentSizeFitter.FitMode;
using UnityEngine.EventSystems;
using System;
 
 
[RequireComponent(typeof(ContentSizeFitter))]
public class ContentImmediate :MonoBehaviour
{
    private RectTransform m_RectTransform;
    private RectTransform rectTransform
    {
        get
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();
            return m_RectTransform;
        }
    }
 
    private ContentSizeFitter m_ContentSizeFitter;
    private ContentSizeFitter contentSizeFitter
    {
        get
        {
            if (m_ContentSizeFitter == null)
                m_ContentSizeFitter = GetComponent<ContentSizeFitter>();
            return m_ContentSizeFitter;
        }
    }
    //立即获取ContentSizeFitter的区域
    public Vector2 GetPreferredSize () 
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        return  new Vector2(HandleSelfFittingAlongAxis(0),HandleSelfFittingAlongAxis(1));
    }
 
    private float HandleSelfFittingAlongAxis(int axis)
    {
        FitMode fitting = (axis == 0 ? contentSizeFitter.horizontalFit : contentSizeFitter.verticalFit);
        if (fitting == FitMode.MinSize){
            return LayoutUtility.GetMinSize(rectTransform, axis);
        }else{
            return LayoutUtility.GetPreferredSize(rectTransform, axis);
        }
    }
 
}
