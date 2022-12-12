using Android.Views;
using Java.Lang;

namespace SkyDrop.Droid.Helper
{
    public abstract class BaseOnDragListener : Object, View.IOnDragListener
    {
        public abstract bool OnDrag(View v, DragEvent e);
    }
}