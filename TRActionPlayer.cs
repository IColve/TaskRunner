using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 任务队列执行器
/// </summary>
public class TRActionPlayer
{
    /// <summary>
    /// 任务队列数组
    /// </summary>
    private List<TRActionData> _actionDataList;
    
    /// <summary>
    /// 当前执行的任务下标 
    /// </summary>
    private int _runIndex;

    /// <summary>
    /// 是否已经打断
    /// </summary>
    private bool _isStop;
    
    private List<TRActionTaskData> _taskDataList;

    /// <summary>
    /// 添加任务
    /// </summary>
    public void AddAction(int layer, Action<TRActionTaskData> action)
    {
        if (action == null)
        {
            Debug.LogError("添加了空任务");
            return;
        }
        
        if (_actionDataList == null)
        {
            _actionDataList = new List<TRActionData>();
        }
        
        _actionDataList.Add(new TRActionData(layer, action));
    }
    
    /// <summary>
    /// 移除任务
    /// </summary>
    /// <param name="layer"></param>
    public void SubAction(int layer)
    {
        if (_actionDataList == null)
        {
            return;
        }

        _actionDataList.RemoveAll(x => x.Layer == layer);
    }

    /// <summary>
    /// 修改顺序
    /// </summary>
    /// <param name="action"></param>
    /// <param name="layer"></param>
    public void ChangeLayer(Action<Action> action, int layer)
    {
        if (_actionDataList == null)
        {
            return;
        }        
        
        _actionDataList.ForEach(x =>
        {
            if (x.TRAction == action)
            {
                x.Layer = layer;
            }
        });
    }

    /// <summary>
    /// 获取到排序后的任务列表
    /// </summary>
    /// <returns></returns>
    private List<List<Action<TRActionTaskData>>> GetSortActionList()
    {
        _actionDataList.Sort((a, b) => a.Layer.CompareTo(b.Layer));

        List<List<Action<TRActionTaskData>>> actionListList = new List<List<Action<TRActionTaskData>>>();

        List<Action<TRActionTaskData>> actionList = null;
        int lastLayer = -1;
        
        for (int i = 0; i < _actionDataList.Count; i++)
        {
            TRActionData actionData = _actionDataList[i];

            if (actionList == null || lastLayer != actionData.Layer)
            {
                if (actionList != null)
                {
                    actionListList.Add(actionList);
                }
                
                actionList = new List<Action<TRActionTaskData>>();
            }
            
            actionList.Add(actionData.TRAction);
            lastLayer = actionData.Layer;
        }

        return actionListList;
    }

    /// <summary>
    /// 执行队列
    /// </summary>
    public void Start()
    {
        if (_actionDataList == null || _actionDataList.Count == 0)
        {
            return;
        }
    
        RunAction(GetSortActionList());
    }
    
    /// <summary>
    /// 执行队列
    /// </summary>
    /// <param name="actionListList"></param>
    private void RunAction(List<List<Action<TRActionTaskData>>> actionListList)
    {
        if (_runIndex >= actionListList.Count)
        {
            return;
        }

        if (_isStop)
        {
            return;
        }

        if (_taskDataList == null)
        {
            _taskDataList = new List<TRActionTaskData>();
        }
        
        var actionList = actionListList[_runIndex];

        int finishCount = 0;

        Action taskFinishAction = () =>
        {
            finishCount++;
            if (finishCount >= actionList.Count)
            {
                _runIndex++;
                RunAction(actionListList);
                _taskDataList.Clear();
            }
        };

        for (int i = 0; i < actionList.Count; i++)
        {
            TRActionTaskData taskData = new TRActionTaskData(taskFinishAction);
            _taskDataList.Add(taskData);
            actionList[i]?.Invoke(taskData);
        }
    }

    /// <summary>
    /// 打断队列
    /// </summary>
    public void Stop()
    {
        _isStop = true;

        if (_taskDataList != null)
        {
            _taskDataList.ForEach(x => x.IsTaskStop = true);
        }
    }
    
    private class TRActionData
    {
        /// <summary>
        /// 播放顺序
        /// </summary>
        public int Layer;

        /// <summary>
        /// 执行的方法
        /// </summary>
        public readonly Action<TRActionTaskData> TRAction;

        public TRActionData(int layer, Action<TRActionTaskData> action)
        {
            this.Layer = layer;
            this.TRAction = action;
        }
    }
}

public class TRActionTaskData
{
    /// <summary>
    /// 任务结束时执行的逻辑
    /// </summary>
    private readonly Action _endAction;

    /// <summary>
    /// 是否已经结束
    /// </summary>
    private bool _isEnd;

    /// <summary>
    /// 是否队列中断
    /// </summary>
    public bool IsTaskStop;
        
    public TRActionTaskData(Action endAction)
    {
        _endAction = endAction;
    }

    /// <summary>
    /// 任务结束
    /// </summary>
    public void OnTaskEnd()
    {
        if (_isEnd)
        {
            return;
        }

        _isEnd = true;
            
        _endAction?.Invoke();
    }
}
