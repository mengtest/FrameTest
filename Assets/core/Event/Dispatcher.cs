using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dispatcher : Singletion<Dispatcher>
{
    Dictionary<string, Dictionary<int, UnityAction<object[]>>> eventMap = new Dictionary<string, Dictionary<int, UnityAction<object[]>>>();
    int curIndex = 0;
    public int Subscibe(string eventType, UnityAction<object[]> unityEvent)
    {
        curIndex++;
        Dictionary<int, UnityAction<object[]>> events;
        if (!eventMap.TryGetValue(eventType, out events))
        {
            events = new Dictionary<int, UnityAction<object[]>>();
            eventMap.Add(eventType, events);
        }
        events.Add(curIndex, unityEvent);
        return curIndex;
    }

    public void Unsubscribe(string eventType, int id)
    {
        if (nextFrameTempDic.ContainsKey(eventType))
        {
            nextFrameTempDic.Remove(eventType);
        }

        if (nextFrameDic.ContainsKey(eventType))
        {
            nextFrameDic.Remove(eventType);
        }

        Dictionary<int, UnityAction<object[]>> events;
        if (eventMap.TryGetValue(eventType, out events))
        {
            if (events.ContainsKey(id))
            {
                events.Remove(id);
            }
        }
    }

    public void Dispatch(string eventType, params object[] args)
    {
        Dictionary<int, UnityAction<object[]>> events;
        if (eventMap.TryGetValue(eventType, out events))
        {
            foreach (UnityAction<object[]> ev in events.Values)
            {
                ev.Invoke(args);
            }
        }
    }

    Dictionary<string, List<object[]>> nextFrameDic = new Dictionary<string, List<object[]>>();
    Dictionary<string, List<object[]>> nextFrameTempDic = new Dictionary<string, List<object[]>>();
    public void DispatchNextFrame(string eventType, params object[] args)
    {     
        if (eventMap.ContainsKey(eventType))
        {
            List<object[]> argsList;
            if (!nextFrameTempDic.TryGetValue(eventType, out argsList))
            {
                argsList = new List<object[]>();
                nextFrameTempDic.Add(eventType, argsList);
            }
            argsList.Add(args);
        }
    }

    public void OnUpdate()
    {
        if (nextFrameDic != null && nextFrameDic.Count > 0)
        {
            foreach (var executeArgsKp in nextFrameDic)
            {
                foreach (var executeArgs in executeArgsKp.Value)
                {
                    Dispatch(executeArgsKp.Key, executeArgs);
                }
            }
            nextFrameDic.Clear();
        }

        if (nextFrameTempDic != null && nextFrameTempDic.Count > 0)
        {
            foreach (var executeArgs in nextFrameTempDic)
            {
                nextFrameDic.Add(executeArgs.Key, executeArgs.Value);
            }
            nextFrameTempDic.Clear();
        }
    }
}
