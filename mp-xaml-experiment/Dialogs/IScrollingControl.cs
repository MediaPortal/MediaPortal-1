using System;
namespace Infinity.Wpf.Controls
{
    interface IScrollingControl
    {
        ScrollDirection ScrollDirection { get; set; }
        ScrollSpeed ScrollSpeed { get; set; }
        void ScrollReset();
        void ScrollStart();
    }
}
