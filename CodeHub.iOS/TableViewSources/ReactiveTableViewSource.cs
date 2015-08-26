using System;
using UIKit;
using System.Reactive.Subjects;
using System.Reactive;
using CodeHub.Core.ViewModels;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using CoreGraphics;

namespace CodeHub.iOS.TableViewSources
{
    public abstract class ReactiveTableViewSource<TViewModel> : ReactiveUI.ReactiveTableViewSource<TViewModel>, IInformsEnd
    {
        private readonly ISubject<Unit> _requestMoreSubject = new Subject<Unit>();
        private readonly ISubject<CGPoint> _scrollSubject = new Subject<CGPoint>();

        public IObservable<CGPoint> DidScroll
        {
            get { return _scrollSubject.AsObservable(); }
        }

        public IObservable<Unit> RequestMore
        {
            get { return _requestMoreSubject; }
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _scrollSubject.OnNext(scrollView.ContentOffset);
        }

        protected ReactiveTableViewSource(UITableView tableView, nfloat height, nfloat? heightHint = null)
            : base(tableView)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
        }

        protected ReactiveTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<TViewModel> collection, 
            Foundation.NSString cellKey, nfloat height, nfloat? heightHint = null, Action<UITableViewCell> initializeCellAction = null) 
            : base(tableView, collection, cellKey, (float)height, initializeCellAction)
        {
            tableView.RowHeight = height;
            tableView.EstimatedRowHeight = heightHint ?? tableView.EstimatedRowHeight;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, Foundation.NSIndexPath indexPath)
        {
            if (indexPath.Section == (NumberOfSections(tableView) - 1) &&
                indexPath.Row == (RowsInSection(tableView, indexPath.Section) - 1))
            {
                // We need to skip an event loop to stay out of trouble
                BeginInvokeOnMainThread(() => _requestMoreSubject.OnNext(Unit.Default));
            }
        }

        public override void RowSelected(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var item = ItemAt(indexPath) as ICanGoToViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();

            base.RowSelected(tableView, indexPath);
        }
    }

    public interface IInformsEnd
    {
        IObservable<Unit> RequestMore { get; }
    }
}

