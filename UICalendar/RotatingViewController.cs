using System;
using UIKit;
using Foundation;
using MonoTouch;
// 
//  Copyright 2011  Clancey
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
namespace UICalendar
{

	[Register("RotatingViewController")]
	public abstract partial class RotatingViewController : UIViewController
	{
		public NSObject notificationObserver;

		public RotatingViewController (IntPtr handle) : base(handle)
		{
			initialize ();
		}

		[Export("initWithCoder:")]
		public RotatingViewController (NSCoder coder) : base(coder)
		{
			initialize ();
		}

		public RotatingViewController (string nibName, NSBundle bundle) : base(nibName, bundle)
		{
			initialize ();
		}

		public RotatingViewController () : base()
		{
			initialize ();
		}
		private void initialize ()
		{
		}

		public UIViewController LandscapeLeftViewController { get; set; }
		public UIViewController LandscapeRightViewController { get; set; }
		public UIViewController PortraitViewController { get; set; }
		public UIView PortraitView { get; set; }
		public UIView LandscapeLeftView { get; set; }
		public UIView LandscapeRightView { get; set; }
		public bool viewControllerVisible { get; set; }

		public override void ViewDidLoad ()
		{
			//  SetView();
		}

		public override void ViewWillAppear (bool animated)
		{
			viewControllerVisible = true;
			SetView ();
		}

		private void _showView (UIView view)
		{
			_removeAllViews ();
			view.Frame = View.Frame;
			View.AddSubview (view);
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft)
				return true; else if (toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
				return true; else if (toInterfaceOrientation == UIInterfaceOrientation.Portrait)
				return true;
			return false;
		}

		public abstract void SetupNavBar ();

		private void SetView ()
		{
			//Console.WriteLine(InterfaceOrientation);
			switch (InterfaceOrientation) {
			case UIInterfaceOrientation.Portrait:
				_showView (PortraitView);
				break;
			
			case UIInterfaceOrientation.LandscapeLeft:
				_showView (LandscapeLeftView);
				break;
			case UIInterfaceOrientation.LandscapeRight:
				_showView (LandscapeRightView);
				break;
			}
			SetupNavBar ();
		}



		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			SetView ();
		}



		private void _removeAllViews ()
		{
			PortraitView.RemoveFromSuperview ();
			LandscapeLeftView.RemoveFromSuperview ();
			LandscapeRightView.RemoveFromSuperview ();
		}

		public override void ViewDidDisappear (bool animated)
		{
			viewControllerVisible = false;
			base.ViewWillDisappear (animated);
		}
	}
}

