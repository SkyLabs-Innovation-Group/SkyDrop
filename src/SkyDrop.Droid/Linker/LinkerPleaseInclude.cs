using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Commands;
using MvvmCross.Core;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.IoC;
using MvvmCross.Navigation;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using MvvmCross.WeakSubscription;

namespace SkyDrop.Droid.Linker
{
    // This class is never actually executed, but when Xamarin linking is enabled it does how to ensure types and properties
    // are preserved in the deployed app
    [Preserve(AllMembers = true)]
    public class LinkerPleaseInclude
    {
        public void Include(Button button)
        {
            button.Click += (s, e) => button.Text = $"{button.Text}";
        }

        public void Include(View view)
        {
            view.Click += (s, e) => view.ContentDescription = $"{view.ContentDescription}";
        }

        public void Include(TextView text)
        {
            text.AfterTextChanged += (sender, args) => text.Text = $"{text.Text}";
            text.Hint = $"{text.Hint}";
        }

        public void Include(CompoundButton cb)
        {
            cb.CheckedChange += (sender, args) => cb.Checked = !cb.Checked;
        }

        public void Include(SeekBar sb)
        {
            sb.ProgressChanged += (sender, args) => sb.Progress = sb.Progress + 1;
        }

        public void Include(RadioGroup radioGroup)
        {
            radioGroup.CheckedChange += (sender, args) => radioGroup.Check(args.CheckedId);
        }

        public void Include(RatingBar ratingBar)
        {
            ratingBar.RatingBarChange += (sender, args) => ratingBar.Rating = 0 + ratingBar.Rating;
        }

        public void Include(CardView cardView)
        {
            EventHandler f = null;
            cardView.Click += f;
        }

        public void Include(Activity act)
        {
            act.Title = $"{act.Title}";
        }

        public void Include(ICommand command)
        {
            command.Execute(null);
        }

        public void Include<T>(MvxAsyncCommand<T> command)
        {
            command.Execute(null);
        }

        public void Include(MvxAsyncCommand command)
        {
            command.Execute(null);
        }

        public void Include(MvxCommand command)
        {
            command.Execute(null);
        }

        public void Include(IMvxAsyncCommand command)
        {
            command.Execute(null);
        }

        public void Include(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += (s, e) =>
            {
                _ = $"{e.Action}{e.NewItems}{e.NewStartingIndex}{e.OldItems}{e.OldStartingIndex}";
            };
        }

        public void Include(INotifyPropertyChanged changed)
        {
            changed.PropertyChanged += (sender, e) => { _ = e.PropertyName; };
        }

        public void Include(MvxPropertyInjector injector)
        {
            _ = new MvxPropertyInjector();
        }

        public void Include(MvxTaskBasedBindingContext context)
        {
            context.Dispose();
            var context2 = new MvxTaskBasedBindingContext();
            context2.Dispose();
        }

        public void Include(MvxViewModelViewTypeFinder viewModelViewTypeFinder)
        {
            _ = new MvxViewModelViewTypeFinder(null, null);
        }

        public void Include(MvxNavigationService service, IMvxViewModelLoader loader)
        {
            _ = new MvxNavigationService(null, loader);
            _ = new MvxAppStart<MvxNullViewModel>(null, null);
        }

        public void Include(RecyclerView.ViewHolder vh, MvxRecyclerView list)
        {
            vh.ItemView.Click += (sender, args) => { };
            vh.ItemView.LongClick += (sender, args) => { };
            list.ItemsSource = null;
            list.Click += (sender, args) => { };
        }

        public void Include(ConsoleColor color)
        {
            Console.Write("");
            Debug.WriteLine("");
            _ = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public void Include(MvxSettings settings)
        {
            _ = new MvxSettings();
        }

        public void Include(MvxStringToTypeParser parser)
        {
            _ = new MvxStringToTypeParser();
        }

        public void Include(MvxViewModelLoader loader)
        {
            _ = new MvxViewModelLoader(null);
        }

        public void Include(MvxViewModelViewLookupBuilder builder)
        {
            _ = new MvxViewModelViewLookupBuilder();
        }

        public void Include(MvxCommandCollectionBuilder builder)
        {
            _ = new MvxCommandCollectionBuilder();
        }

        public void Include(MvxStringDictionaryNavigationSerializer serializer)
        {
            _ = new MvxStringDictionaryNavigationSerializer();
        }

        public void Include(MvxChildViewModelCache cache)
        {
            _ = new MvxChildViewModelCache();
        }

        public void Include(MvxPluginManager p)
        {
            var _ = p;
        }

        public void Include(MvxPluginAttribute p)
        {
            var _ = p;
        }

        public void Include<TS, TR>(MvxWeakEventSubscription<TS, TR> mvxWeakEventSubscription)
            where TS : class
        {
            mvxWeakEventSubscription?.Dispose();
        }

        public void Include(IMvxPlugin p)
        {
            var _ = p;
        }
    }
}