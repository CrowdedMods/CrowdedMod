using System;
using UnityEngine;

namespace CrowdedMod.Components;

// Interface until unhollower implements generic il2cpp (if it's possible)
/// <summary>
/// This class is not actually abstract because unhollower does not support it <br/>
/// You need to implement <see cref="OnPageChanged"/> and <see cref="MaxPageIndex"/>
/// </summary>
public class AbstractPagingBehaviour : MonoBehaviour
{
    public AbstractPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    private int _page;

    public virtual int MaxPerPage => 15;
    // public virtual IEnumerable<T> Targets { get; }

    public virtual int PageIndex
    {
        get => _page;
        set
        {
            _page = value;
            OnPageChanged();
        }
    }

    public virtual int MaxPageIndex => throw new NotImplementedException();
    // public virtual int MaxPages => Targets.Count() / MaxPerPage;

    public virtual void OnPageChanged() => throw new NotImplementedException();

    public void Start()
    {
        OnPageChanged();
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0f)
        {
            PageIndex = Mathf.Clamp(PageIndex - 1, 0, MaxPageIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0f)
        {
            PageIndex = Mathf.Clamp(PageIndex + 1, 0, MaxPageIndex);
        }
    }
}