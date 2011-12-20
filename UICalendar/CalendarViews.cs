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
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreAnimation;
using System.Linq;
using MonoTouch.EventKit;
using MonoTouch.EventKitUI;
using MonoTouch.ObjCRuntime;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;

namespace UICalendar
{
	internal class Block
	{
		internal ArrayList Columns;
		public ArrayList events = new ArrayList ();


		internal Block ()
		{
		}
		internal DateTime BoxStart {
			get { return (from CalendarDayEventView e in events.ToArray ()select e.BoxStart).Min (); }
		}

		internal DateTime BoxEnd {
			get { return (from CalendarDayEventView e in events.ToArray ()select e.BoxEnd).Max (); }
		}

		internal void Add (CalendarDayEventView ev)
		{
			events.Add (ev);
			ArrangeColumns ();
		}

		private BlockColumn createColumn ()
		{
			var col = new BlockColumn ();
			Columns.Add (col);
			col.Block = this;
			
			return col;
		}

		public void ArrangeColumns ()
		{
			// cleanup
			Columns = new ArrayList ();
			
			foreach (CalendarDayEventView e in events)
				e.Column = null;
			
			// there always will be at least one column because arrangeColumns is called only from Add()
			createColumn ();
			
			foreach (CalendarDayEventView e in events) {
				foreach (BlockColumn col in Columns) {
					if (col.CanAdd (e)) {
						col.Add (e);
						break;
					}
				}
				// it wasn't placed 
				if (e.Column == null) {
					BlockColumn col = createColumn ();
					col.Add (e);
				}
			}
		}


		internal bool OverlapsWith (CalendarDayEventView e)
		{
			if (events.Count == 0)
				return false;
			
			return (BoxStart < e.BoxEnd && BoxEnd > e.startDate);
		}
	}

	internal class BlockColumn
	{
		internal Block Block;
		private ArrayList events = new ArrayList ();

		internal BlockColumn ()
		{
		}

		/// <summary>
		/// Gets the order number of the column.
		/// </summary>
		public int Number {
			get {
				if (Block == null)
					throw new ApplicationException ("This Column doesn't belong to any Block.");
				
				return Block.Columns.IndexOf (this);
			}
		}

		internal bool CanAdd (CalendarDayEventView e)
		{
			foreach (CalendarDayEventView ev in events) {
				if (ev.OverlapsWith (e))
					return false;
			}
			return true;
		}

		internal void Add (CalendarDayEventView e)
		{
			if (e.Column != null)
				throw new ApplicationException ("This Event was already placed into a Column.");
			
			events.Add (e);
			e.Column = this;
		}
	}


	public class RotatingCalendarView : RotatingViewController
	{
		private UIBarButtonItem _leftButton, _rightButton, _orgLefButton;
		public EventClicked OnEventClicked, OnEventDoubleClicked;
		// public EventClicked OnEventDoubleClicked;
		public DateTime CurrentDate { get; internal set; }
		public DateTime FirstDayOfWeek { get; set; }
		public CalendarDayTimelineView SingleDayView { get; set; }
		public TrueWeekView WeekView { get; set; }
		// public TrueWeekViewController WeekController { get; set; }
		public NSAction AddNewEvent { get; set; }
		private UIToolbar bottomBar;
		public EKEventEditViewController addController;
		bool hasLoaded;
		
		public RotatingCalendarView (RectangleF rect) : this(rect,0)
		{
			
		}
		
		public RotatingCalendarView (RectangleF rect, float tabBarHeight)
		{
			
			notificationObserver = NSNotificationCenter.DefaultCenter.AddObserver ("EKEventStoreChangedNotification", EventsChanged);
			this.Title = "Calendar";
			CurrentDate = DateTime.Today;
			SingleDayView = new CalendarDayTimelineView (rect, tabBarHeight);
			WeekView = new TrueWeekView (CurrentDate);
			WeekView.UseCalendar = true;
			LandscapeLeftView = WeekView;
			LandscapeRightView = WeekView;
			PortraitView = SingleDayView;
			SingleDayView.OnEventClicked += theEvent =>
			{
				if (theEvent != null) {
					if (OnEventClicked != null) {
						OnEventClicked (theEvent);
					}
				}
			};
			
			WeekView.OnEventClicked += theEvent =>
			{
				if (theEvent != null) {
					if (OnEventClicked != null) {
						OnEventClicked (theEvent);
					}
				}
			};
			SingleDayView.dateChanged += theDate => { CurrentDate = theDate; };
			this.OnEventClicked += theEvent =>
			{
				
				//Util.MyEventStore.RemoveEvents(Util.getEvent(theEvent),EKSpan.ThisEvent,theError.Handle);
				addController = new EKEventEditViewController ();
				
				// set the addController's event store to the current event store.
				addController.EventStore = Util.MyEventStore;
				addController.Event = Util.getEvent (theEvent);
				
				addController.Completed += delegate(object sender, EKEventEditEventArgs e) { this.DismissModalViewControllerAnimated (true); };
				
				try {
					if (this.ModalViewController == null)
						this.NavigationController.PresentModalViewController (addController, true);
				} catch (Exception ex) {
					//rotatingView.NavigationController.PopViewControllerAnimated(false);
				}
			};
		}

		public override void ViewWillAppear (bool animated)
		{
			if (!hasLoaded) {
				_orgLefButton = NavigationItem.LeftBarButtonItem;
				hasLoaded = true;
			}
			WeekView.isVisible = true;
			SingleDayView.isVisible = true;
			base.ViewWillAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			WeekView.isVisible = false;
			SingleDayView.isVisible = false;
			base.ViewWillDisappear (animated);
		}

		private void landScapeNavBar ()
		{
			_leftButton = new UIBarButtonItem (Graphics.AdjustImage(Images.leftArrow,CGBlendMode.SourceAtop,UIColor.White), UIBarButtonItemStyle.Bordered, HandlePreviousWeekTouch);
			NavigationItem.LeftBarButtonItem = _leftButton;
			NavigationItem.Title = WeekView.FirstDayOfWeek.ToString("MMM dd yyyy") + " - " + WeekView.FirstDayOfWeek.AddDays (6).ToString("MMM dd yyyy");
			_rightButton = new UIBarButtonItem (Graphics.AdjustImage(Images.rightArrow,CGBlendMode.SourceAtop,UIColor.White), UIBarButtonItemStyle.Bordered, HandleNextWeekTouch);
			NavigationItem.RightBarButtonItem = _rightButton;
		}

		private void portriatNavBar ()
		{
			//  _leftButton = new UIBarButtonItem("Calendars", UIBarButtonItemStyle.Bordered, HandlePreviousDayTouch);
			NavigationItem.LeftBarButtonItem = _orgLefButton;
			NavigationItem.Title = "Calendar";
			_rightButton = new UIBarButtonItem (UIBarButtonSystemItem.Add, delegate {
				addController = new EKEventEditViewController();	
				// set the addController's event store to the current event store.
				addController.EventStore = Util.MyEventStore;
				addController.Event = EKEvent.FromStore(Util.MyEventStore);
				addController.Event.StartDate = DateTime.Now;
				addController.Event.EndDate = DateTime.Now.AddHours(1);
				
				addController.Completed += delegate(object theSender, EKEventEditEventArgs eva) {
					switch (eva.Action)
					{
					case EKEventEditViewAction.Canceled :
						case EKEventEditViewAction.Deleted :
						case EKEventEditViewAction.Saved:
						this.NavigationController.DismissModalViewControllerAnimated(true);
						
						break;
					}
				};
				this.NavigationController.PresentModalViewController (addController, true);
			});
			
			NavigationItem.RightBarButtonItem = _rightButton;
		}

		private void EventsChanged (NSNotification notification)
		{
			WeekView.EventsNeedRefresh = true;
			SingleDayView.EventsNeedRefresh = true;
			switch (UIDevice.CurrentDevice.Orientation) {
			case UIDeviceOrientation.Portrait:
				SingleDayView.reloadDay ();
				break;
			case UIDeviceOrientation.LandscapeLeft:
				WeekView.reloadDay ();
				break;
			case UIDeviceOrientation.LandscapeRight:
				WeekView.reloadDay ();
				break;
			}
		}

		public override void SetupNavBar ()
		{
			switch (InterfaceOrientation) {
			case UIInterfaceOrientation.Portrait:
				portriatNavBar ();
				break;
			
			case UIInterfaceOrientation.LandscapeLeft:
				landScapeNavBar ();
				break;
			case UIInterfaceOrientation.LandscapeRight:
				landScapeNavBar ();
				break;
			}
		}

		private void setDate (DateTime date)
		{
			CurrentDate = date;
			WeekView.SetDayOfWeek (CurrentDate);
			SingleDayView.SetDate (CurrentDate);
			if (UIDevice.CurrentDevice.Orientation != UIDeviceOrientation.Portrait) {
				NavigationItem.Title = WeekView.FirstDayOfWeek.ToString("MMM dd yyyy") + " - " + WeekView.FirstDayOfWeek.AddDays (6).ToString("MMM dd yyyy");
				WeekView.ReDraw ();
			}
		}

		private void HandleNextWeekTouch (object sender, EventArgs e)
		{
			setDate (CurrentDate.AddDays (7));
		}

		private void HandlePreviousWeekTouch (object sender, EventArgs e)
		{
			setDate (CurrentDate.AddDays (-7));
		}

		private void HandlePreviousDayTouch (object sender, EventArgs e)
		{
		}

		private void AddNewEventClicked (object sender, EventArgs e)
		{
			if (AddNewEvent != null)
				AddNewEvent ();
		}
	}

	public delegate void EventClicked (CalendarDayEventView theEvent);

	public delegate void clicked ();

	public delegate void DateChanged (DateTime newDate);

	public class CalendarDayEventView : UIView
	{
		private bool multipleTouches = false;
		public EventClicked OnEventClicked;
		public EventClicked OnEventDoubleClicked;
		public object ParentView;
		private bool twoFingerTapIsPossible = true;
		public int id { get; set; }
		public DateTime startDate { get; set; }
		public DateTime endDate { get; set; }
		public bool AllDay { get; set; }
		public NSDate nsStartDate { get; set; }
		public NSDate nsEndDate { get; set; }
		public string Title { get; set; }
		public string location { get; set; }
		public UIColor color { get; set; }
		internal BlockColumn Column { get; set; }
		public string eventIdentifier { get; set; }
		public EKCalendar theCal { get; set; }
		
		public CalendarDayEventView (EKEvent theEvent)
		{
			if (theEvent != null) {
				eventIdentifier = theEvent.EventIdentifier;
				theCal = theEvent.Calendar;
				nsStartDate = theEvent.StartDate;
				nsEndDate = theEvent.EndDate;
				startDate = Util.NSDateToDateTime (theEvent.StartDate);
				endDate = Util.NSDateToDateTime (theEvent.EndDate);
				TimeSpan dateDif = endDate - startDate;
				AllDay = theEvent.AllDay;
				if (dateDif.Minutes < 30 && dateDif.Minutes > 1) {
					endDate = endDate.AddMinutes (30 - dateDif.Minutes);
				}
				Title = theEvent.Title;
				location = theEvent.Location;
				if (theEvent.Calendar != null) {
					color = new UIColor (theEvent.Calendar.CGColor);
				}
			}
			Frame = new RectangleF (0, 0, 0, 0);
			setupCustomInitialisation ();
		}


		public CalendarDayEventView ()
		{
			Frame = new RectangleF (0, 0, 0, 0);
			setupCustomInitialisation ();
		}

		public CalendarDayEventView (RectangleF frame)
		{
			Frame = frame;
			setupCustomInitialisation ();
		}

		public DateTime BoxStart {
			get { return startDate; }
		}

		public DateTime BoxEnd {
			get {
				if (endDate.Minute > 30) {
					DateTime hourPlus = endDate.AddHours (1);
					return new DateTime (hourPlus.Year, hourPlus.Month, hourPlus.Day, hourPlus.Hour, 0, 0);
				} else if (endDate.Minute > 0) {
					return new DateTime (endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 30, 0);
				} else {
					return new DateTime (endDate.Year, endDate.Month, endDate.Day, endDate.Hour, 0, 0);
				}
			}
		}


		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			if (evt.TouchesForView (this).Count > 1)
				multipleTouches = true;
			if (evt.TouchesForView (this).Count > 2)
				twoFingerTapIsPossible = true;
			base.TouchesBegan (touches, evt);
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			bool allTouchesEnded = (touches.Count == evt.TouchesForView (this).Count);
			
			// first check for plain single/double tap, which is only possible if we haven't seen multiple touches
			if (!multipleTouches) {
				var touch = (UITouch)touches.AnyObject;
				// tapLocation = touch.LocationInView(this);
				var myParentView = ParentView as CalendarDayTimelineView;
				if (touch.TapCount == 1) {
					if (myParentView != null) {
						if (myParentView.OnEventClicked != null)
							myParentView.OnEventClicked (this);
					} else if (OnEventClicked != null)
						OnEventClicked (this);
				} else if (touch.TapCount == 2) {
					if (myParentView != null) {
						if (myParentView.OnEventDoubleClicked != null)
							myParentView.OnEventDoubleClicked (this);
					} else if (OnEventDoubleClicked != null)
						OnEventDoubleClicked (this);
				}
			}
			
			base.TouchesEnded (touches, evt);
		}

		public bool OverlapsWith (CalendarDayEventView e)
		{
			return (BoxStart < e.BoxEnd && BoxEnd > e.startDate);
		}


		public void setupCustomInitialisation ()
		{
			BackgroundColor = color;
			Alpha = 0.8f;
			CALayer layer = Layer;
			layer.MasksToBounds = true;
			layer.CornerRadius = 5.0f;
			// You can even add a border
			layer.BorderWidth = 0.5f;
			layer.BorderColor = UIColor.LightGray.CGColor;
		}

		public override void Draw (RectangleF rect)
		{
			CGContext context = UIGraphics.GetCurrentContext ();
			
			context.SaveState ();
			
			
			// Set shadow
			context.SetShadowWithColor (new SizeF (0.0f, 1.0f), 0.7f, UIColor.Black.CGColor);
			
			// Set text color
			UIColor.White.SetColor ();
			
			var titleRect = new RectangleF (Bounds.X + consts.HORIZONTAL_OFFSET, Bounds.Y + consts.VERTICAL_OFFSET, Bounds.Width - 2 * consts.HORIZONTAL_OFFSET, consts.EVENT_FONT_SIZE + 4.0f);
			
			var locationRect = new RectangleF (Bounds.X + consts.HORIZONTAL_OFFSET, Bounds.Y + consts.VERTICAL_OFFSET + consts.EVENT_FONT_SIZE + 4.0f, Bounds.Width - 2 * consts.HORIZONTAL_OFFSET, consts.EVENT_FONT_SIZE + 4.0f);
			
			// Drawing code
			if (Bounds.Height > consts.VERTICAL_DIFF) {
				// Draw both title and location
				if (!string.IsNullOrEmpty (Title)) {
					DrawString (Title, titleRect, UIFont.BoldSystemFontOfSize (consts.EVENT_FONT_SIZE), UILineBreakMode.TailTruncation, UITextAlignment.Left);
				}
				if (!string.IsNullOrEmpty (location)) {
					DrawString (location, locationRect, UIFont.SystemFontOfSize (consts.EVENT_FONT_SIZE), UILineBreakMode.TailTruncation, UITextAlignment.Left);
				}
			} else {
				// Draw only title
				if (!string.IsNullOrEmpty (Title)) {
					DrawString (Title, titleRect, UIFont.BoldSystemFontOfSize (consts.EVENT_FONT_SIZE), UILineBreakMode.TailTruncation, UITextAlignment.Left);
				}
			}
			
			// Restore the context state
			context.RestoreState ();
		}
	}

	public class CalendarDayTimelineView : UIView
	{
		private UIView dayView;
		private UIView monthView;
		private UIView weekView;
		private UIButton _leftButton;
		private UIButton _rightButton;
		public DateTime currentDate;
		public DateChanged dateChanged;
		public List<CalendarDayEventView> monthEvents;
		public DateTime currentMonth;
		private float NavBarHeight;
		public EventClicked OnEventClicked;
		public EventClicked OnEventDoubleClicked;
		private RectangleF orgRect;
		private UIView parentScrollView;
		private UIScrollView scrollView;
		private UIView allDayView;
		private TimeLineView timelineView;

		private float curScrollH;
		private float curScrollW;

		public bool UseCalendar { get; set; }
		public bool EventsNeedRefresh { get; set; }
		public bool isVisible { get; set; }
		public bool ShouldRefreshUI { get; set; }

		// bottom tab bar
		private const float bottomBarH = 40;
		private UIToolbar bottomBar;
		private UISegmentedControl calViewSwitcher;
		private UIBarButtonItem todayBtn;

		public CalendarDayTimelineView (RectangleF rect, float tabBarHeight)
		{
			orgRect = rect;
			NavBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
			Frame = rect;
			setup ();
			setupCustomInitialisation ();
		}

		public CalendarDayTimelineView ()
		{
			var screenFrame = UIScreen.MainScreen.Bounds;
			NavBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
			Frame = new RectangleF (0, 0, screenFrame.Width, screenFrame.Height);
			orgRect = Frame;
			setup ();
			setupCustomInitialisation ();
		}

		private void setup ()
		{
			UseCalendar = true;
			EventsNeedRefresh = true;
			
			
			ClearsContextBeforeDrawing = true;
			var recognizerLeft = new UISwipeGestureRecognizer (this, new Selector ("swipeLeft"));
			recognizerLeft.Direction = UISwipeGestureRecognizerDirection.Left;
			AddGestureRecognizer (recognizerLeft);
			
			var recognizerRight = new UISwipeGestureRecognizer (this, new Selector ("swipeRight"));
			recognizerRight.Direction = UISwipeGestureRecognizerDirection.Right;
			AddGestureRecognizer (recognizerRight);
			
			curScrollH = GetStartPosition (DateTime.Now);
			curScrollW = 0;
			
		}

		private float CurrentWidth {
			get {
				switch (UIDevice.CurrentDevice.Orientation) {
				case UIDeviceOrientation.Portrait:
					return orgRect.Size.Width;
				case UIDeviceOrientation.LandscapeLeft:
					return orgRect.Size.Height;
				case UIDeviceOrientation.LandscapeRight:
					return orgRect.Size.Height;
				}
				return orgRect.Size.Width;
			}
		}

		private float CurrentHeight {
			get {
				switch (UIDevice.CurrentDevice.Orientation) {
				case UIDeviceOrientation.Portrait:
					return orgRect.Size.Height - NavBarHeight;
				case UIDeviceOrientation.LandscapeLeft:
					return orgRect.Size.Width - NavBarHeight;
				case UIDeviceOrientation.LandscapeRight:
					return orgRect.Size.Width - NavBarHeight;
				}
				return orgRect.Size.Height - NavBarHeight;
			}
		}

		[Export("swipeLeft")]
		public void swipeLeft (UISwipeGestureRecognizer sender)
		{
			SetDate (currentDate.AddDays (-1));
		}

		[Export("swipeRight")]
		public void swipeRight (UISwipeGestureRecognizer sender)
		{
			SetDate (currentDate.AddDays (1));
		}

		public void SetDate (DateTime date)
		{
			currentDate = date;
			var newMonth = new DateTime (date.Year, date.Month, 1);
			if (currentMonth != newMonth)
				EventsNeedRefresh = true;
			currentMonth = newMonth;
			reloadDay ();
			dateChanged (currentDate);
		}


		public void setupCustomInitialisation ()
		{
			
			foreach (UIView v in Subviews) {
				v.RemoveFromSuperview ();
			}
			if (monthEvents == null)
				monthEvents = new List<CalendarDayEventView> ();
			// Initialization code
			// events = new List<CalendarDayEventView>();
			// Add main scroll view
			var viewFrame = new RectangleF (Bounds.X, Bounds.Y, CurrentWidth, CurrentHeight - bottomBarH);
			dayView = new UIView (viewFrame);
			monthView = new UIView (viewFrame);
			weekView = new UIView (viewFrame);
			dayView.AddSubview (getScrollView ());
			ShouldRefreshUI = true;
			switch (Settings.lastCal) {
			case 0:
				AddSubview (dayView);
				break;
			case 1:
				AddSubview (monthView);
				break;
			case 2:
				var frame = weekView.Bounds;
				weekView.AddSubview (new UILabel (frame) { Lines = 2, Text = "Please rotate for the week view\r\n (replace with graphic)", TextAlignment = UITextAlignment.Center });
				AddSubview (weekView);
				break;
			}
			AddSubview (bottomBar);
			LoadButtons ();
			
			scrollView.AddSubview (getTimeLineView ());
			
		}

		public override void Draw (RectangleF rect)
		{
			// DrawDayLabels(rect);
			if (Settings.lastCal == 0) {
				Images.calendarTopBar.Draw (new PointF (0, 0));
				DrawDayLabel (rect);
			}
			//	base.Draw (rect);
		}

		private void DrawDayLabel (RectangleF rect)
		{
			var r = new RectangleF (48, 12, CurrentWidth - 93, 35 );
			if(currentDate.Date == DateTime.Today)
				UIColor.Blue.SetColor();
			else
				UIColor.DarkGray.SetColor ();
			string dayOfWeek = currentDate.DayOfWeek.ToString();
			string dateString =  currentDate.ToString("MMM dd yyyy");
			DrawString (dayOfWeek, r, UIFont.BoldSystemFontOfSize (19), UILineBreakMode.WordWrap, UITextAlignment.Left);
			DrawString (dateString, r, UIFont.BoldSystemFontOfSize (19), UILineBreakMode.WordWrap, UITextAlignment.Right);
		}

		private void LoadButtons ()
		{
			_leftButton = UIButton.FromType (UIButtonType.Custom);
			_leftButton.TouchUpInside += delegate { SetDate (currentDate.AddDays (-1)); };
			_leftButton.SetImage (Images.leftArrow, UIControlState.Normal);
			dayView.AddSubview (_leftButton);
			_leftButton.Frame = new RectangleF (10, 5, 35, 35);
			
			_rightButton = UIButton.FromType (UIButtonType.Custom);
			_rightButton.TouchUpInside += delegate { SetDate (currentDate.AddDays (1)); };
			_rightButton.SetImage (Images.rightArrow, UIControlState.Normal);
			dayView.AddSubview (_rightButton);
			_rightButton.Frame = new RectangleF (CurrentWidth - 45, 5, 35, 35);
		}

		private UIView getScrollView ()
		{
			parentScrollView = new UIView (new RectangleF (Bounds.X, Bounds.Y + 44, CurrentWidth, CurrentHeight - 44 - bottomBarH));
			scrollView = new UIScrollView (parentScrollView.Bounds);
			scrollView.ContentSize = new SizeF (CurrentWidth, consts.TIMELINE_HEIGHT);
			scrollView.ScrollEnabled = true;
			scrollView.MaximumZoomScale = 100f;
			scrollView.MinimumZoomScale = .01f;
			scrollView.BackgroundColor = UIColor.White;
			scrollView.AlwaysBounceVertical = true;
			scrollView.Scrolled += delegate { scrolled (); };
			parentScrollView.Add (scrollView);
			setBottomToolBar ();
			return parentScrollView;
		}

		private void scrolled ()
		{
			curScrollH = scrollView.ContentOffset.Y;
			curScrollW = scrollView.ContentOffset.X;
		}

		private void setBottomToolBar ()
		{
			var scrollFrame = parentScrollView.Frame;
			bottomBar = new UIToolbar (new RectangleF (scrollFrame.X, scrollFrame.Height, scrollFrame.Width, bottomBarH)){TintColor = UIColor.Black};
			calViewSwitcher = new UISegmentedControl (new RectangleF (scrollFrame.Width / 2 - 90, 5, 180, 28));
			calViewSwitcher.InsertSegment ("Day", 0, false);
			calViewSwitcher.InsertSegment ("Month", 1, false);
			calViewSwitcher.InsertSegment ("Week", 2, false);
			calViewSwitcher.ControlStyle = UISegmentedControlStyle.Bar;
			calViewSwitcher.SelectedSegment = Settings.lastCal;
			calViewSwitcher.ValueChanged += delegate {Settings.lastCal = calViewSwitcher.SelectedSegment; ViewSwitched (); };
			calViewSwitcher.TintColor = UIColor.Black;
			//calViewSwitcher.Selected
			todayBtn = new UIBarButtonItem ("Today", UIBarButtonItemStyle.Bordered, delegate {
				curScrollH = GetStartPosition (DateTime.Now);
				curScrollW = 0;
				SetDate (DateTime.Today);
			});
			//UIToolbar toolbar = new UIToolbar(new RectangleF(5,0,75,35));
			//toolbar.TintColor = UIColor.Clear;
			//toolbar.BackgroundColor = UIColor.Clear;
			//toolbar.SetItems(new UIBarButtonItem[]{todayBtn},false);
			bottomBar.SetItems (new UIBarButtonItem[] { todayBtn }, false);
			//bottomBar.AddSubview(toolbar);	
			
			bottomBar.AddSubview (calViewSwitcher);
		}

		private void ViewSwitched ()
		{
			ShouldRefreshUI = true;
			reloadDay ();
		}

		private TimeLineView getTimeLineView ()
		{
			timelineView = new TimeLineView (new RectangleF (Bounds.X, Bounds.Y, CurrentWidth, consts.TIMELINE_HEIGHT));
			timelineView.BackgroundColor = UIColor.White;
			
			
			return timelineView;
		}

		public override void MovedToWindow ()
		{
			if (Window != null) {
				//setupCustomInitialisation();
				reloadDay ();
			}
		}


		public void ScrollToTime (DateTime time)
		{
			scrollView.ScrollRectToVisible (new RectangleF (0, GetStartPosition (time), 300, CurrentHeight), false);
		}

		private float GetStartPosition (DateTime time)
		{
			Int32 hourStart = time.Hour;
			var hourStartPosition = (float)Math.Round ((hourStart * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
			// Get the minute start position
			// Round minute to each 5
			Int32 minuteStart = time.Minute;
			// minuteStart = Convert.ToInt32(Math.Round(minuteStart/5.0f)*5);
			Double minDif = (Convert.ToDouble (minuteStart) / 60);
			var minuteStartPosition = (float)(minDif * consts.VERTICAL_DIFF);
			
			return (hourStartPosition + minuteStartPosition + consts.EVENT_VERTICAL_DIFF);
		}

		public void reloadDay ()
		{
			//if (!isVisible)
			//	return;
			if (ShouldRefreshUI || Settings.lastCal != 1)
				setupCustomInitialisation ();
			// If no current day was given
			// Make it today
			if (currentDate <= Util.DateTimeMin) {
				// Dont' want to inform the observer
				currentDate = DateTime.Today;
				currentMonth = new DateTime (currentDate.Year, currentDate.Month, 1);
			}
			SetNeedsDisplay ();
			if (UseCalendar) {
				if (EventsNeedRefresh) {
					monthEvents.Clear ();
					
					// endDate is 1 day = 60*60*24 seconds = 86400 seconds from startDate	
					foreach (EKEvent theEvent in Util.FetchEvents (currentMonth.AddMonths (-1), currentMonth.AddMonths (1))) {
						monthEvents.Add (new CalendarDayEventView (theEvent));
					}
					EventsNeedRefresh = false;
				}
			}
			if (ShouldRefreshUI) {
				switch (Settings.lastCal) {
				case 0:
					buildDayView ();
					break;
				case 1:
					buildMonthView ();
					break;
				}
				ShouldRefreshUI = false;
			}
			
		}

		public void buildDayView ()
		{
			buildAllDayView();
			scrollView.Frame = parentScrollView.Bounds;
			scrollView.ContentSize = new SizeF (CurrentWidth, consts.TIMELINE_HEIGHT);
			// Remove all previous view event
			foreach (UIView view in scrollView.Subviews) {
				if (view is TimeLineView) {
					timelineView.Frame = new RectangleF (Bounds.X, Bounds.Y, CurrentWidth, consts.TIMELINE_HEIGHT);
					timelineView.CurrentWidth = CurrentWidth;
				} else {
					view.RemoveFromSuperview ();
				}
			}
			
			// Ask the delgate about the events that correspond
			// the the currently displayed day view
			var dailyEvents = monthEvents.Where (x => (x.startDate.Date == currentDate || x.endDate.Date == currentDate) && !x.AllDay).ToList ();
			if (dailyEvents.Count > 0) {
				// _events = events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate).ThenByDescending(x => x.endDate).ToList();
				
				var blocks = new List<Block> ();
				var lastBlock = new Block ();
				// Removed the thenByDecending. Caused a crash on the device. This is needed for 2 events with the same start time though....
				// foreach (CalendarDayEventView e in events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
				//.ThenByDescending(x => x.endDate).ToList())
				foreach (CalendarDayEventView e in dailyEvents.OrderBy (x => x.startDate)) {
					// if there is no block, create the first one
					if (blocks.Count == 0) {
						lastBlock = new Block ();
						blocks.Add (lastBlock);
					// or if the event doesn't overlap with the last block, create a new block
					} else if (!lastBlock.OverlapsWith (e)) {
						lastBlock = new Block ();
						blocks.Add (lastBlock);
					}
					
					// any case, add it to some block
					lastBlock.Add (e);
				}
				foreach (Block theBlock in blocks) {
					//theBlock.ArrangeColumns();
					foreach (CalendarDayEventView theEvent in theBlock.events) {
						theEvent.ParentView = this;
						
						// Get the hour start position
						Int32 hourStart = theEvent.startDate.Hour;
						//Util.WriteLine ("Start time: " + hourStart);
						var hourStartPosition = (float)Math.Round ((hourStart * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
						//Util.WriteLine ("Hour float: " + hourStartPosition);
						// Get the minute start position
						Int32 minuteStart = theEvent.startDate.Minute;
						var minuteStartPosition = (float)((Convert.ToDouble (minuteStart) / 60) * consts.VERTICAL_DIFF);
						
						
						// Get the hour end position
						Int32 hourEnd = theEvent.endDate.Hour;
						if (theEvent.startDate.Date != theEvent.endDate.Date) {
							hourEnd = 23;
						}
						var hourEndPosition = (float)Math.Round ((hourEnd * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
						// Get the minute end position
						// Round minute to each 5
						Int32 minuteEnd = theEvent.endDate.Minute;
						if (theEvent.startDate.Date != theEvent.endDate.Date) {
							minuteEnd = 55;
						}
						// minuteEnd = Convert.ToInt32(Math.Round(minuteEnd/5.0)*5);
						// float minuteEndPosition = (float) Math.Round((minuteEnd < 30) ? 0 : VERTICAL_DIFF/2.0f);
						var minuteEndPosition = (float)((Convert.ToDouble (minuteEnd) / 60) * consts.VERTICAL_DIFF);
						
						float eventHeight = 0.0f;
						
						// Calculate the event Height.
						eventHeight = (hourEndPosition + minuteEndPosition) - hourStartPosition - minuteStartPosition;
						// Set the min Height to 30 min
												/*
							if (eventHeight <  (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF))
							{
								eventHeight = (VERTICAL_DIFF/2) - (2*EVENT_VERTICAL_DIFF);	
							}
							*/

						float availableWidth = CurrentWidth - (consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF) - consts.HORIZONTAL_LINE_DIFF - consts.EVENT_HORIZONTAL_DIFF;
						float currentWidth = availableWidth / theBlock.Columns.Count;
						int currentInt = theEvent.Column.Number;
						float x = consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF + consts.EVENT_HORIZONTAL_DIFF + (currentWidth * currentInt);
						float y = hourStartPosition + minuteStartPosition + consts.EVENT_VERTICAL_DIFF;
						var eventFrame = new RectangleF (x, y, currentWidth, eventHeight);
						
						theEvent.Frame = eventFrame;
						//event.delegate = self;
						theEvent.SetNeedsDisplay ();
						timelineView.AddSubview (theEvent);
						
						
						// Log the extracted date values
						//Util.Log ("hourStart: {0} minuteStart: {1}", hourStart, minuteStart);
						
					}
				}
			}
			scrollView.ScrollRectToVisible (new RectangleF (curScrollW, curScrollH, scrollView.Frame.Width, scrollView.Frame.Height), false);
			//disable scroll to time, keep current scroll;
		}
		
		public void buildAllDayView()
		{
			if(allDayView != null)
				allDayView.RemoveFromSuperview();
		
			var dailyEvents = monthEvents.Where (day => (day.startDate.Date == currentDate || day.endDate.Date == currentDate) && day.AllDay).ToList ();
			
			var baseFrame = new RectangleF (Bounds.X, Bounds.Y + 44, CurrentWidth, CurrentHeight - 44 - bottomBarH);
			if(dailyEvents.Count == 0)
			{
				parentScrollView.Frame = baseFrame;
				return;	
			}
			var height = 15 + dailyEvents.Count() * (consts.AllDayEventHeight );
			allDayView = new UIView(new RectangleF(0,baseFrame.Y,this.CurrentWidth, height));
			allDayView.BackgroundColor = UIColor.White;
			baseFrame.Y += height;
			baseFrame.Height -= height;
			var x = consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF + consts.EVENT_HORIZONTAL_DIFF;
			var current = 0;
			var currentH = 5f;
			foreach(var theEvent in dailyEvents)
			{
				theEvent.Frame = new RectangleF(x,currentH ,CurrentWidth - x - (consts.HORIZONTAL_OFFSET),consts.AllDayEventHeight);
				allDayView.AddSubview(theEvent);
				currentH += 5f + consts.AllDayEventHeight;
				current ++;
			}
			UIFont timeFont = UIFont.BoldSystemFontOfSize (consts.FONT_SIZE);
			var label = new UILabel(new RectangleF(consts.HORIZONTAL_OFFSET,(height - consts.FONT_SIZE - 9f)/2,x-(consts.HORIZONTAL_OFFSET*2),consts.FONT_SIZE + 4f));
			label.Font = timeFont;
			label.TextColor = UIColor.Black;
			label.Text = "all-day";
			//label.TextAlignment = UITextAlignment.Right;
			allDayView.AddSubview(label);
			allDayView.AddSubview(new HorizontalDividerView(new RectangleF(0,height - 5,CurrentWidth,5)));
			parentScrollView.Frame = baseFrame;
			dayView.AddSubview(allDayView);
			
		}
			/*
			DateTime dateToScrollTo;
			if (currentDate == DateTime.Today)
			    dateToScrollTo = DateTime.Now;
			else
			    dateToScrollTo = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 8, 0, 0, 0);
			ScrollToTime(dateToScrollTo);
			*/			
		
		CalendarMonthView calMonthView;
		DialogViewController eventDvc;
		UIView eventDvcTableView;
		public void buildMonthView ()
		{
			calMonthView = new CalendarMonthView (currentDate, monthEvents.Select (x => x.startDate.Date).ToArray ());
			eventDvc = buildMonthSingleDayEventList (calMonthView.Frame);
			calMonthView.IsDayMarkedDelegate += date => { return monthEvents.Where (x => x.startDate.Date == date.Date || x.endDate == date.Date).Count () > 0; };
			calMonthView.OnDateSelected += date =>
			{
				//SelectedView = 0;
				SetDate (date);
				eventDvc.TableView.RemoveFromSuperview ();
				eventDvc = buildMonthSingleDayEventList (new RectangleF(calMonthView.Frame.Location, calMonthView.Size));
				eventDvc.Root = getMonthDayEvents ();
				monthView.AddSubview (eventDvc.TableView);
				monthView.BringSubviewToFront (calMonthView);
				
			};
			
			eventDvc.Root = getMonthDayEvents ();
			calMonthView.SizeChanged += delegate {
				eventDvc.TableView.RemoveFromSuperview ();
				eventDvc = buildMonthSingleDayEventList (new RectangleF(calMonthView.Frame.Location, calMonthView.Size));
				eventDvc.Root = getMonthDayEvents ();
				monthView.AddSubview (eventDvc.TableView);
				monthView.BringSubviewToFront (calMonthView);
			};
			calMonthView.MonthChanged += delegate {
				SetDate (calMonthView.CurrentMonthYear);
				eventDvc.TableView.RemoveFromSuperview ();
				eventDvc = buildMonthSingleDayEventList (new RectangleF(calMonthView.Frame.Location, calMonthView.Size));
				eventDvc.Root = getMonthDayEvents ();
				monthView.AddSubview (eventDvc.TableView);
				monthView.BringSubviewToFront (calMonthView);
			};
			monthView.BackgroundColor = UIColor.White;
			monthView.AddSubview (eventDvc.TableView);
			monthView.AddSubview (calMonthView);
			buildMonthSingleDayEventList (calMonthView.Frame);
		}

		public DialogViewController buildMonthSingleDayEventList (RectangleF rect)
		{
			var dvc = new DialogViewController (UITableViewStyle.Plain, null);
			dvc.Style = UITableViewStyle.Plain;
			dvc.View.Frame = new RectangleF (0, rect.Height + rect.Y, rect.Width, monthView.Frame.Height - rect.Height - bottomBarH);
			return dvc;
		}
		public RootElement getMonthDayEvents ()
		{
			var section = new Section ();
			
			foreach (var theEvent in monthEvents.Where (x => x.startDate.Date == currentDate || x.endDate.Date == currentDate).OrderBy(x=> x.startDate).ToList ()) {
				var theelement = new MonthEventElement (theEvent);
				theelement.OnEventClicked += theClickedEvent =>
				{
					if (OnEventClicked != null)
						OnEventClicked (theClickedEvent);
				};
				section.Add (theelement);
				//dailyEvents.Add(new CalendarDayEventView(theEvent));
			}
			return new RootElement ("") { section };
		}

		#region Nested type: TimeLineView

		internal class TimeLineView : UIView
		{
			public TimeLineView (RectangleF rect)
			{
				Frame = rect;
				setupCustomInitialisation ();
			}


			public float CurrentWidth { get; set; }

			public void setupCustomInitialisation ()
			{
				// Initialization code
			}

// Setup array consisting of string
// representing time aka 12 (12 am), 1 (1 am) ... 25 x


			public override void Draw (RectangleF rect)
			{
				// Drawing code
				// Here Draw timeline from 12 am to noon to 12 am next day
				// Times appearance
				
				UIFont timeFont = UIFont.BoldSystemFontOfSize (consts.FONT_SIZE);
				UIColor timeColor = UIColor.Black;
				
				// Periods appearance
				UIFont periodFont = UIFont.SystemFontOfSize (consts.FONT_SIZE);
				UIColor periodColor = UIColor.Gray;
				
				// Draw each times string
				for (Int32 i = 0; i < consts.times.Length; i++) {
					// Draw time
					timeColor.SetColor ();
					string time = consts.times[i];
					
					var timeRect = new RectangleF (consts.HORIZONTAL_OFFSET, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.TIME_WIDTH, consts.FONT_SIZE + 4.0f);
					
					// Find noon
					if (i == 24 / 2) {
						timeRect = new RectangleF (consts.HORIZONTAL_OFFSET, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.TIME_WIDTH + consts.PERIOD_WIDTH, consts.FONT_SIZE + 4.0f);
					}
					
					DrawString (time, timeRect, timeFont, UILineBreakMode.WordWrap, UITextAlignment.Right);
					
					
					// Draw period
					// Only if it is not noon
					if (i != 24 / 2) {
						periodColor.SetColor ();
						
						string period = consts.periods[i];
						DrawString (period, new RectangleF (consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.PERIOD_WIDTH, consts.FONT_SIZE + 4.0f), periodFont, UILineBreakMode.WordWrap, UITextAlignment.Right);
					}
					
					
					CGContext context = UIGraphics.GetCurrentContext ();
					
					// Save the context state 
					context.SaveState ();
					context.SetStrokeColorWithColor (UIColor.LightGray.CGColor);
					
					// Draw line with a black stroke color
					// Draw line with a 1.0 stroke width
					context.SetLineWidth (0.5f);
					// Translate context for clear line
					context.TranslateCTM (-0.5f, -0.5f);
					context.BeginPath ();
					context.MoveTo (consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0) / 2.0)));
					context.AddLineToPoint (CurrentWidth, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round ((consts.FONT_SIZE + 4.0) / 2.0));
					context.StrokePath ();
					
					if (i != consts.times.Length - 1) {
						context.BeginPath ();
						context.MoveTo (consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0f) / 2.0f)) + (float)Math.Round ((consts.VERTICAL_DIFF / 2.0f)));
						context.AddLineToPoint (CurrentWidth, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0f) / 2.0f)) + (float)Math.Round ((consts.VERTICAL_DIFF / 2.0f)));
						float[] dash1 = { 4.0f, 3.0f };
						context.SetLineDash (0.0f, dash1, 2);
						context.StrokePath ();
					}
					
					// Restore the context state
					context.RestoreState ();
				}
			}
			
			///////
		}
		
		#endregion
	}

	public class TrueWeekViewController : UIViewController
	{
		private TrueWeekView weekView;

		public TrueWeekViewController (DateTime date)
		{
			weekView = new TrueWeekView (date) { Frame = new RectangleF (0, 0, 480, 220) };
			SetCurrentDate (date);
			View.AddSubview (weekView);
		}

		public DateTime CurrentDate { get; internal set; }
		public DateTime FirstDayOfWeek { get; set; }

		public void SetCurrentDate (DateTime date)
		{
			CurrentDate = date;
			FirstDayOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
			weekView.SetDayOfWeek (date);
		}

		public void ReDraw ()
		{
			weekView.SetupWindow ();
		}
	}

	public class TrueWeekView : UIView
	{
		public static float DayWidth = ((480) / 3);
		public static float TotalWidth = DayWidth * 7;
		private List<Block> blocks;
		private List<CalendarDayEventView> events;
		private GridLineView gridView;
		private UIView HeaderView;
		private WeekTopView header;
		private UIView allDayView;

		public EventClicked OnEventClicked;
		private TimeView rowHeader;

		public TrueWeekView (DateTime date)
		{
			SetDayOfWeek (date);
			BackgroundColor = UIColor.White;
			SetupWindow ();
			curRect = new RectangleF (0, GetStartPosition (DateTime.Now), myScrollView.ContentSize.Width, myScrollView.ContentSize.Height);
			curZoom = myScrollView.ZoomScale;
		}

		public bool isVisible { get; set; }

		public DateTime CurrentDate { get; internal set; }
		public DateTime FirstDayOfWeek { get; internal set; }
		private ScrollViewWithHeader myScrollView { get; set; }
		public bool UseCalendar { get; set; }
		public bool EventsNeedRefresh { get; set; }
		private RectangleF curRect;
		private float curZoom;


		public void SetDayOfWeek (DateTime date)
		{
			if (myScrollView != null) {
				curRect = myScrollView.VisbleContentRect;
				curZoom = myScrollView.ZoomScale;
			}
			EventsNeedRefresh = true;
			CurrentDate = date;
			FirstDayOfWeek = date.AddDays (-1 * (int)date.DayOfWeek);
		}

		public override void MovedToWindow ()
		{
			if (isVisible) {
				reloadDay ();
			}
		}

		public void SetupWindow ()
		{
			foreach (UIView view in Subviews) {
				view.RemoveFromSuperview ();
			}
			header = new WeekTopView (this);
			HeaderView = new UIView(header.Frame);
			HeaderView.AddSubview(header);
			UILabel alldayLabel = null;
			if(allDayView != null)
			{
				allDayView.Frame = allDayView.Frame.SetLocation(new PointF(0,HeaderView.Frame.Height));
				HeaderView.Frame = HeaderView.Frame.AddSize(0,allDayView.Frame.Height);
				header.Frame = HeaderView.Frame;
				HeaderView.AddSubview(allDayView);
				
				UIFont timeFont = UIFont.BoldSystemFontOfSize (consts.FONT_SIZE);
				//alldayLabel = new UILabel(new RectangleF(consts.HORIZONTAL_OFFSET,(allDayView.Frame.Height - consts.FONT_SIZE - 9f)/2 + allDayView.Frame.Y,50,consts.FONT_SIZE + 4f));
				//alldayLabel.Font = timeFont;
				//alldayLabel.TextColor = UIColor.Black;
				//alldayLabel.Text = "all-day";
				//alldayLabel.TextAlignment = UITextAlignment.Right;
			}
				
			rowHeader = new TimeView ();
			gridView = new GridLineView ();
			myScrollView = new ScrollViewWithHeader (Frame, HeaderView, rowHeader, gridView, true);
			
			AddSubview (myScrollView);
			if(alldayLabel != null)
				AddSubview(alldayLabel);
		}

		private float GetStartPosition (DateTime time)
		{
			Int32 hourStart = time.Hour;
			var hourStartPosition = (float)Math.Round ((hourStart * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
			// Get the minute start position
			// Round minute to each 5
			Int32 minuteStart = time.Minute;
			// minuteStart = Convert.ToInt32(Math.Round(minuteStart/5.0f)*5);
			Double minDif = (Convert.ToDouble (minuteStart) / 60);
			var minuteStartPosition = (float)(minDif * consts.VERTICAL_DIFF);
			
			return (hourStartPosition + minuteStartPosition + consts.EVENT_VERTICAL_DIFF);
		}

		public void reloadDay ()
		{
			if (isVisible) {
				// If no current day was given
				// Make it today
				if (CurrentDate <= Util.DateTimeMin) {
					// Dont' want to inform the observer
					CurrentDate = DateTime.Today;
				}
				
				if (UseCalendar) {
					if (EventsNeedRefresh) {
						events = new List<CalendarDayEventView> ();
						
						DateTime endDate = FirstDayOfWeek.AddDays (6).AddSeconds (86400);
						foreach (EKEvent theEvent in Util.FetchEvents (FirstDayOfWeek, endDate)) {
							events.Add (new CalendarDayEventView (theEvent));
						}
						
						if (events != null) {
							// _events = events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate).ThenByDescending(x => x.endDate).ToList();
							
							blocks = new List<Block> ();
							var lastBlock = new Block ();
							// Removed the thenByDecending. Caused a crash on the device. This is needed for 2 events with the same start time though....
							// foreach (CalendarDayEventView e in events.Where(x => x.startDate.Date == currentDate.Date || x.endDate.Date == currentDate.Date).OrderBy(x => x.startDate))//.ThenByDescending(x => x.endDate).ToList())
							//.ThenByDescending(x => x.endDate).ToList())
							foreach (CalendarDayEventView e in events.Where(x=> !x.AllDay).OrderBy (x => x.startDate)) {
								// if there is no block, create the first one
								if (blocks.Count == 0) {
									lastBlock = new Block ();
									blocks.Add (lastBlock);
								// or if the event doesn't overlap with the last block, create a new block
								} else if (!lastBlock.OverlapsWith (e)) {
									lastBlock = new Block ();
									blocks.Add (lastBlock);
								}
								
								// any case, add it to some block
								lastBlock.Add (e);
							}
						}
						
						EventsNeedRefresh = false;
					}
					// Ask the delgate about the events that correspond
					// the the currently displayed day view
					
					reloadAllDay();
					
					SetupWindow ();
					foreach (Block theBlock in blocks) {
						int dayColumn = (theBlock.BoxStart - FirstDayOfWeek).Days;
						//theBlock.ArrangeColumns();
						foreach (CalendarDayEventView theEvent in theBlock.events) {
							theEvent.ParentView = this;
							DateTime startDate = theEvent.startDate;
							// Making sure delgate sending date that match current day
							// Get the hour start position
							Int32 hourStart = theEvent.startDate.Hour;
							var hourStartPosition = (float)Math.Round ((hourStart * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
							// Get the minute start position
							Int32 minuteStart = theEvent.startDate.Minute;
							var minuteStartPosition = (float)((Convert.ToDouble (minuteStart) / 60) * consts.VERTICAL_DIFF);

							// Get the hour end position
							Int32 hourEnd = theEvent.endDate.Hour;
							if (theEvent.startDate.Date != theEvent.endDate.Date) {
								hourEnd = 23;
							}
							var hourEndPosition = (float)Math.Round ((hourEnd * consts.VERTICAL_DIFF) + consts.VERTICAL_OFFSET + ((consts.FONT_SIZE + 4.0f) / 2.0f));
							// Get the minute end position
							// Round minute to each 5
							Int32 minuteEnd = theEvent.endDate.Minute;
							if (theEvent.startDate.Date != theEvent.endDate.Date) {
								minuteEnd = 55;
							}
							// minuteEnd = Convert.ToInt32(Math.Round(minuteEnd/5.0)*5);
							// float minuteEndPosition = (float) Math.Round((minuteEnd < 30) ? 0 : VERTICAL_DIFF/2.0f);
							var minuteEndPosition = (float)((Convert.ToDouble (minuteEnd) / 60) * consts.VERTICAL_DIFF);
							
							float eventHeight = 0.0f;
							
							// Calculate the event Height.
							eventHeight = (hourEndPosition + minuteEndPosition) - hourStartPosition - minuteStartPosition;
							
							
							float availableWidth = DayWidth - 2;
							float currentWidth = availableWidth / theBlock.Columns.Count;
							int currentInt = theEvent.Column.Number;
							float x = ((currentWidth) * currentInt) + 2 + ((availableWidth + 2) * dayColumn);
							float y = hourStartPosition + minuteStartPosition;
							var eventFrame = new RectangleF (x, y, currentWidth, eventHeight);
							
							theEvent.Frame = eventFrame;
							theEvent.OnEventClicked += theDate => { eventWasClicked (theDate); };
							//event.delegate = self;
							//theEvent.SetNeedsDisplay();
							gridView.AddSubview (theEvent);
							
							
							// Log the extracted date values
							//Util.Log ("hourStart: {0} minuteStart: {1}", hourStart, minuteStart);
						}
					}
				}
				if (curRect.Height < 0)
					curRect.Height = myScrollView.ContentSize.Height;
				if (curRect.Width < 0)
					curRect.Width = myScrollView.ContentSize.Width;
				myScrollView.ZoomScale = curZoom;
				myScrollView.VisbleContentRect = curRect;
			}
		}
		
		private void reloadAllDay()
		{
			if(allDayView != null)
				allDayView.RemoveFromSuperview();
		
			var weekEvents = events.Where (day => day.AllDay).ToList ();
			if(weekEvents.Count == 0)
			{
				allDayView = null;
				return;	
			}
			
			var maxCount = weekEvents.GroupBy(dayEvent => (dayEvent.startDate - FirstDayOfWeek).Days).Select( dayEvent => dayEvent.Count()).Max();
			
			var height = 15 + maxCount * (consts.AllDayEventHeight );
			allDayView = new UIView(new RectangleF(0,0,TotalWidth, height));
			//allDayView.BackgroundColor = UIColor.White;
			var xValue = 2;//consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH + consts.PERIOD_WIDTH + consts.HORIZONTAL_LINE_DIFF + consts.EVENT_HORIZONTAL_DIFF;
			
			foreach(var dayEvents in weekEvents.OrderBy(x=> x.startDate).GroupBy(x=> x.startDate.Date).ToList())
			{
				var current = 0;
				var currentH = 5f;
				var col = (dayEvents.ToList()[0].startDate - FirstDayOfWeek).Days;
				foreach(var theEvent in dayEvents)
				{
					
					theEvent.Frame = new RectangleF(xValue + col * DayWidth,currentH ,DayWidth - 2,consts.AllDayEventHeight);
					allDayView.AddSubview(theEvent);
					currentH += 5f + consts.AllDayEventHeight;
					current ++;
				}
			}
			//allDayView.AddSubview(label);
			allDayView.AddSubview(new HorizontalDividerView(new RectangleF(0,height - 5,TotalWidth,5)));
			//parentScrollView.Frame = baseFrame;
			//dayView.AddSubview(allDayView);
			
		}
		private void eventWasClicked (CalendarDayEventView theEvent)
		{
			if (!myScrollView.isMoving ()) {
				if (OnEventClicked != null)
					OnEventClicked (theEvent);
			}
		}

		public void ReDraw ()
		{
			reloadDay ();
		}

		#region Nested type: GridLineView

		private class GridLineView : UIView
		{
			float totalH = (24 * consts.VERTICAL_OFFSET) + (23 * consts.VERTICAL_DIFF);
			public GridLineView ()
			{
				BackgroundColor = UIColor.White;
				Frame = new RectangleF (0, 0, TotalWidth, totalH);
				setupCustomInitialisation ();
			}


			public float CurrentWidth { get; set; }

			public void setupCustomInitialisation ()
			{
				// Initialization code
			}

			public override void Draw (RectangleF rect)
			{
				// Drawing code
				// Here Draw timeline from 12 am to noon to 12 am next day
				// Times appearance
				
				UIFont timeFont = UIFont.BoldSystemFontOfSize (consts.FONT_SIZE);
				UIColor timeColor = UIColor.Black;
				
				// Periods appearance
				UIFont periodFont = UIFont.SystemFontOfSize (consts.FONT_SIZE);
				UIColor periodColor = UIColor.Gray;
				//draw vertical lines
				CGContext context = UIGraphics.GetCurrentContext ();
				
				context.SetLineWidth (0.5f);
				for (int i = 0; i <= 8; i++) {
					float lineWidth = (i * DayWidth);
					context.BeginPath ();
					context.MoveTo (lineWidth + 1, 0);
					context.AddLineToPoint (lineWidth + 1, totalH);
					context.StrokePath ();
				}
				
				// Draw each times string
				for (Int32 i = 0; i < consts.times.Length; i++) {
					// Draw time
					timeColor.SetColor ();
					
					
					// Save the context state 
					context.SaveState ();
					context.SetStrokeColorWithColor (UIColor.LightGray.CGColor);
					
					// Draw line with a black stroke color
					// Draw line with a 1.0 stroke width
					context.SetLineWidth (0.5f);
					// Translate context for clear line
					context.TranslateCTM (-0.5f, -0.5f);
					context.BeginPath ();
					context.MoveTo (0, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0) / 2.0)));
					context.AddLineToPoint (TotalWidth, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round ((consts.FONT_SIZE + 4.0) / 2.0));
					context.StrokePath ();
					
					if (i != consts.times.Length - 1) {
						context.BeginPath ();
						context.MoveTo (0, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0f) / 2.0f)) + (float)Math.Round ((consts.VERTICAL_DIFF / 2.0f)));
						context.AddLineToPoint (TotalWidth, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF + (float)Math.Round (((consts.FONT_SIZE + 4.0f) / 2.0f)) + (float)Math.Round ((consts.VERTICAL_DIFF / 2.0f)));
						float[] dash1 = { 4.0f, 3.0f };
						context.SetLineDash (0.0f, dash1, 2);
						context.StrokePath ();
					}
					
					// Restore the context state
					context.RestoreState ();
				}
			}
			
			///////
		}

		#endregion

		#region Nested type: TimeView

		private class TimeView : UIView
		{
			public TimeView ()
			{
				BackgroundColor = UIColor.White;
				Frame = new RectangleF (0, 0, consts.TIME_WIDTH + consts.VERTICAL_OFFSET + consts.PERIOD_WIDTH, (24 * consts.VERTICAL_OFFSET) + (23 * consts.VERTICAL_DIFF));
			}


			public float CurrentWidth { get; set; }

			public void ReDraw ()
			{
				Draw (Frame);
			}
			// Setup array consisting of string
			// representing time periods aka AM or PM
			// Matching the array of times 25 x
			
			public override void Draw (RectangleF rect)
			{
				// Drawing code
				// Here Draw timeline from 12 am to noon to 12 am next day
				// Times appearance
				
				UIFont timeFont = UIFont.BoldSystemFontOfSize (consts.FONT_SIZE);
				UIColor timeColor = UIColor.Black;
				
				// Periods appearance
				UIFont periodFont = UIFont.SystemFontOfSize (consts.FONT_SIZE);
				UIColor periodColor = UIColor.Gray;
				
				// Draw each times string
				for (Int32 i = 0; i < consts.times.Length; i++) {
					// Draw time
					timeColor.SetColor ();
					string time = consts.times[i];
					
					var timeRect = new RectangleF (consts.HORIZONTAL_OFFSET, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.TIME_WIDTH, consts.FONT_SIZE + 4.0f);
					
					// Find noon
					if (i == 24 / 2) {
						timeRect = new RectangleF (consts.HORIZONTAL_OFFSET, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.TIME_WIDTH + consts.PERIOD_WIDTH, consts.FONT_SIZE + 4.0f);
					}
					
					DrawString (time, timeRect, timeFont, UILineBreakMode.WordWrap, UITextAlignment.Right);
					
					
					// Draw period
					// Only if it is not noon
					if (i != 24 / 2) {
						periodColor.SetColor ();
						
						string period = consts.periods[i];
						DrawString (period, new RectangleF (consts.HORIZONTAL_OFFSET + consts.TIME_WIDTH, consts.VERTICAL_OFFSET + i * consts.VERTICAL_DIFF, consts.PERIOD_WIDTH, consts.FONT_SIZE + 4.0f), periodFont, UILineBreakMode.WordWrap, UITextAlignment.Center);
					}
				}
			}
			
			///////
		}

		#endregion

		#region Nested type: WeekTopView

		private class WeekTopView : UIView
		{
			/*
			[Export ("layerClass")]
			public static Class LayerClass()
			{
				return new  Class( typeof(weekTopLevelLayer));	
			}
			*/

			public WeekTopView (TrueWeekView theParent)
			{
				Opaque = true;
				
				parent = theParent;
				Frame = new RectangleF (0, 0, TotalWidth, 35);
				BackgroundColor = UIColor.White;
			}
				/*
				
				weekTopLevelLayer tempTiledLayer = (weekTopLevelLayer)this.Layer;
		        tempTiledLayer.LevelsOfDetail = 5;
				tempTiledLayer.FirstDayOfWeek = parent.FirstDayOfWeek;
				tempTiledLayer.TotalWidth = TotalWidth;
				tempTiledLayer.DayWidth = DayWidth;
		        tempTiledLayer.LevelsOfDetailBias = 2;
		        */				
			
						public TrueWeekView parent { get; set; }

			public void ReDraw ()
			{
				//this.Draw(this.Frame);
			}

			[Export("drawRect")]
			private void DrawRect (RectangleF rect)
			{
			}

			[Export("drawInContext")]
			private void DrawInContext (CALayer layer)
			{
			}

			public override void Draw (RectangleF rect)
			{
				Images.calendarTopBar.Draw (new RectangleF (-25, 0, (TotalWidth + 50), 35));
				CGContext context = UIGraphics.GetCurrentContext ();
				
				context.SetLineWidth (0.5f);
				for (int i = 0; i <= 8; i++) {
					float lineWidth = (i * DayWidth);
					context.BeginPath ();
					context.MoveTo (lineWidth + 1, 0);
					context.AddLineToPoint (lineWidth + 1, rect.Height);
					context.StrokePath ();
					if (i <= 7) {
						DateTime theDay = parent.FirstDayOfWeek.AddDays (i);
						DrawDayLabel (new RectangleF (lineWidth, 0, DayWidth, 35), theDay);
					}
				}
			}


			private void DrawDayLabel (RectangleF rect, DateTime date)
			{
				// var r = new RectangleF(new PointF(0, 5), new SizeF {Width = CurrentWidth, Height = 35});
				if (date == DateTime.Today)
					UIColor.Blue.SetColor ();
				else
					UIColor.DarkGray.SetColor ();
				RectangleF dayRect = rect.SetSize(new SizeF(rect.Width,35));
				dayRect.Height = dayRect.Height / 2;
				
				DrawString (date.DayOfWeek.ToString (), dayRect, UIFont.BoldSystemFontOfSize (12), UILineBreakMode.WordWrap, UITextAlignment.Center);
				
				RectangleF dateRect = dayRect;
				dateRect.Y += dayRect.Height;
				DrawString (date.ToString ("M/d"), dateRect, UIFont.BoldSystemFontOfSize (12), UILineBreakMode.WordWrap, UITextAlignment.Center);
				UIColor.Black.SetColor ();
			}
		}
		
		#endregion
	}
}
