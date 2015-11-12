﻿//------------------------------------------------------------------------------
//
//      CosmosEngine - The Lightweight Unity3D Game Develop Framework
//
//                     Version 0.9.1 (20151010)
//                     Copyright © 2011-2015
//                   MrKelly <23110388@qq.com>
//              https://github.com/mr-kelly/CosmosEngine
//
//------------------------------------------------------------------------------
//#define COBJECT_DEBUGGER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KEngine;

/// <summary>
/// CosmosEngine标准Object,，带有自动Debug~
/// </summary>
public class KObject : IDisposable
{
    public KObject()
    {
        this.StartWatch();
    }

    public virtual void Dispose()
    {
        this.StopWatch();
    }
}

/// <summary>
/// 手动打开或关闭，用于任何object
/// </summary>
public static class CObjectDebuggerExtensions
{
    public static void StartWatch(this object obj)
    {
        KObjectDebugger.StartWatch(obj);
    }

    public static void StopWatch(this object obj)
    {
        KObjectDebugger.StopWatch(obj);
    }
}

/// <summary>
/// 对C#非MonoBehaviour对象以GameObject形式表现，方便调试
/// </summary>
public class KObjectDebugger : KBehaviour
{
    public static Dictionary<object, KObjectDebugger> Cache = new Dictionary<object, KObjectDebugger>();
    public static IEnumerator GlobalDebugCoroutine;  // 不用Update，用这个~

    public const string ContainerName = "KObjectDebugger";
    public object WatchObject;
    public List<string> DebugStrs = new List<string>();
    private GameObject _cacheGameObject;

    public static void StopWatch(object obj)
    {
#if COBJECT_DEBUGGER
        if (!Logger.IsEditor || !Application.isPlaying || IsApplicationQuited)
            return;

        KAsync.AddMainThreadCall(() =>
        {

            try
            {

                KObjectDebugger debuger;
                if (KObjectDebugger.Cache.TryGetValue(obj, out debuger))
                {
                    GameObject.Destroy(debuger.gameObject);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
 
        });
#endif
    }

    public static void StartWatch(object obj)
    {
#if COBJECT_DEBUGGER
        if (!Logger.IsEditor || !Application.isPlaying || IsApplicationQuited)
            return;

        KAsync.AddMainThreadCall(() =>
        {
            try
            {

                var newDebugger = new GameObject(string.Format("{0}-{1}", obj.ToString(), obj.GetType())).AddComponent<KObjectDebugger>();
                newDebugger.WatchObject = obj;

                CDebuggerObjectTool.SetParent(ContainerName, obj.GetType().Name, newDebugger.gameObject);

                Cache[obj] = newDebugger;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }

        });
#endif
    }

    void Awake()
    {
        if (!Logger.IsEditor)
        {
            Logger.LogError("Error Open KObjectDebugger on not Unity Editor");
            return;
        }
        _cacheGameObject = gameObject;
        if (GlobalDebugCoroutine == null)
        {
            GlobalDebugCoroutine = CoGlobalDebugCoroutine();
            KEngine.KEngine.EngineInstance.StartCoroutine(GlobalDebugCoroutine);
        }
    }

    /// <summary>
    /// 主要为了清理和改名
    /// </summary>
    /// <returns></returns>
    static IEnumerator CoGlobalDebugCoroutine()
    {
        while (true)
        {
            if (Cache.Count <= 0)
            {
                yield return null;
                continue;
            }

            var copyCache = new Dictionary<object, KObjectDebugger>();
            foreach (var kv in Cache)  // copy
            {
                copyCache[kv.Key] = kv.Value;
            }

            foreach (var kv in copyCache)
            {
                var debugger = kv.Value;
                if (debugger.WatchObject == null)
                {
                    GameObject.Destroy(debugger._cacheGameObject);
                }
                else
                {

                    if (!debugger.IsDestroyed && debugger._cacheGameObject.name != debugger.WatchObject.ToString())
                    {
                        debugger._cacheGameObject.name = debugger.WatchObject.ToString();
                    }
                }
                yield return null;
            }
        }

    }
}