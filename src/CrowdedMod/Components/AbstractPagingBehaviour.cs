using System;
using UnityEngine;

namespace CrowdedMod.Components;

// Interface until unhollower implements generic il2cpp (if it's possible)
/// <summary>
/// This class is not actually abstract because unhollower does not support it.<br/>
/// You need to implement <see cref="OnPageChanged"/> and <see cref="MaxPageIndex"/>.
/// </summary>
public class AbstractPagingBehaviour : MonoBehaviour
{
    public AbstractPagingBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public const string PAGE_INDEX_GAME_OBJECT_NAME = "CrowdedMod_PageIndex";

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

    public virtual void Start() => OnPageChanged();

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.mouseScrollDelta.y > 0f)
            Cycle(false);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.mouseScrollDelta.y < 0f)
            Cycle(true);
    }

    /// <summary>
    /// Loops around if you go over the limits.<br/>
    /// Attempting to go up a page while on the first page will take you to the last page and vice versa.
    /// </summary>
    public virtual void Cycle(bool increment)
    {
        var change = increment ? 1 : -1;
        PageIndex = Mathf.Clamp(PageIndex + change, 0, MaxPageIndex);
    }
}