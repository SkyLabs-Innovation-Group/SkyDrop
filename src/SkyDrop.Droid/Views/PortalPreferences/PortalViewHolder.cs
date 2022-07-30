using System;
using System.Diagnostics;
using Android.Views;
using Android.Widget;
using Java.Interop;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.PortalPreferences
{
    internal class PortalViewHolder : MvxRecyclerViewHolder
    {
        private ImageView reorderIcon;

        public PortalViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
        {
        }

        public void Bind()
        {
            reorderIcon = ItemView.FindViewById<ImageView>(Resource.Id.chevron);
            reorderIcon.Touch += ReorderIcon_Touch;
        }

        // Let's translate Googlish to English
        private enum TouchEventType { Start, Drag, End }
        private TouchEventType GetTouchEventType(MotionEventActions e) => e switch
        {
            MotionEventActions.Down => TouchEventType.Start,
            MotionEventActions.Move => TouchEventType.Drag,
            _ => TouchEventType.End
        };

        private (double, double) startPosition = (0, 0);

        private void ReorderIcon_Touch(object sender, View.TouchEventArgs e)
        {
            (double, double) getPosition(View.TouchEventArgs e) => (e.Event.GetX(), e.Event.GetY());

            var touchEvent = GetTouchEventType(e.Event.Action);
            switch (touchEvent)
            {
                case TouchEventType.Start:
                    // Save initial position

                    startPosition = getPosition(e);

                    Debug.WriteLine($"[Drag] START {e.Event.Action}: ({startPosition})");

                    break;

                case TouchEventType.Drag:
                    // Do nothing?
                    break;

                case TouchEventType.End:

                    (double, double) endPosition = getPosition(e);

                    Debug.WriteLine($"[Drag] END {e.Event.Action}: ({endPosition})");

                    double vector = endPosition.Item2 - startPosition.Item2;

                    // TODO Calculate if re-ordered based on vertical change 
                    Debug.WriteLine($"[Drag] END vertical change: ({vector})");

                    break;


                default:
                    Debug.WriteLine($"[Drag] invalid? {e.Event.Action}: ({e.Event.GetX()},{e.Event.GetY()})");
                    break;
            }
        }


        private void ReorderIcon_Click(object sender, EventArgs e)
        {
            reorderIcon.StartDragAndDrop(null, null, null, 0);
        }
    }
}

