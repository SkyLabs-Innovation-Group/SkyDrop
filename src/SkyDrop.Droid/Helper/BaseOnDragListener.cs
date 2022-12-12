using System;
using Android.Views;
using Java.Interop;

namespace SkyDrop.Droid.Helper
{
    public abstract class BaseOnDragListener : Java.Lang.Object, View.IOnDragListener
    {
        public abstract bool OnDrag(View v, DragEvent e);
    }
}

