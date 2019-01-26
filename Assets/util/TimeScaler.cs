using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using sw.model;

namespace sw.service
{
    public class TimeScaler : MonoBehaviour
    {
        //public TimeScaleData[] arrayTimeScaleData;

        //private List<TimeClientData> m_listTimeScaleData = new List<TimeClientData>();

        void Start()
        {

        }

        void Update()
        {
            //StartScale();
        }

        //void OnEnable()
        //{
        //    m_listTimeScaleData.Clear();

        //    foreach (TimeScaleData item in arrayTimeScaleData)
        //    {
        //        TimeClientData data = new TimeClientData();
        //        data.beginTime = Time.time + item.beginFrame / 30f;
        //        data.duration = (item.endFrame - item.beginFrame) / 30f + data.beginTime;
        //        data.scale = item.scale;
        //        m_listTimeScaleData.Add(data);
        //    }
        //}

        //void StartScale()
        //{
        //    for (int i = 0; i < m_listTimeScaleData.Count; )
        //    {
        //        if (m_listTimeScaleData[i].beginTime <= Time.time)
        //        {
        //            if (m_listTimeScaleData[i].duration > Time.time)
        //            {
        //                Time.timeScale = m_listTimeScaleData[i].scale;
        //                ++i;
        //            }
        //            else
        //            {
        //                Time.timeScale = 1;

        //                m_listTimeScaleData.RemoveAt(i);
        //            }
        //        }
        //        else
        //        {
        //            ++i;
        //        }
        //    }
        //}
    }
}

