//
//  CalendarMonthView.cs
//
//  Converted to MonoTouch on 1/22/09 - Eduardo Scoz || http://UICatalog.com
//  Originally reated by Devin Ross on 7/28/09  - tapku.com || http://github.com/devinross/tapkulibrary
//
/*
 
 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:
 
 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 */

using System;
using Foundation;
using UIKit;
using System.Collections;
using System.Collections.Generic;
using CoreGraphics;
using CoreAnimation;
using System.Linq;

namespace UICalendar
{

	public delegate void DateSelected (DateTime date);
	public delegate void MonthChanged (DateTime monthSelected);
	public delegate bool IsDayMarked (DateTime date);

	public class CalendarMonthView : UIView
	{
		public DateSelected OnDateSelected;
		public DateSelected OnFinishedDateSelection;
		public DateSelected MonthChanged;
		public IsDayMarked IsDayMarkedDelegate;
		public ICollection<DateTime> MarkedDay;
		public UIColor ToolbarColor {
			get{return toolbar.TintColor;}
			set{toolbar.TintColor = value;}
		}

		public DateTime CurrentMonthYear;
		public DateTime CurrentDate { get; internal set; }
		public nfloat CurrentWidth { get; set;}
		public nfloat CurrentDayBlockWidth {
			get{ 
				//return (nfloat)Math.Round (CurrentWidth / 7, 0);
				return CurrentWidth / 7;
			}
		}
		public bool ShowToolBar;

		private UIScrollView _scrollView;
		private UIImageView _shadow;
		private bool calendarIsLoaded;
		private UIToolbar toolbar;
		private UIBarButtonItem todayButton;
		private UIBarButtonItem tomorrowBtn;
		private UIBarButtonItem nextWeekBtn;
		private UIBarButtonItem noneBtn;

		private MonthGridView _monthGridView;
		private UIButton _leftButton, _rightButton;

		public Action SizeChanged;

		public CGSize Size {
			get { return new CGSize (_scrollView.Frame.Size.Width, _scrollView.Frame.Size.Height + _scrollView.Frame.Y + (ShowToolBar ? toolbar.Frame.Height : 0)); }
		}

		public CalendarMonthView () : this(DateTime.Today)
		{
			
		}
		public CalendarMonthView (DateTime currentDate, bool showToolBar) : base(new CGRect (0, 0, 320, 260))
		{
			ShowToolBar = showToolBar;
			if (currentDate.Year < 2010)
				CurrentDate = DateTime.Today;
			else
				CurrentDate = currentDate;
			CurrentMonthYear = new DateTime (CurrentDate.Year, CurrentDate.Month, 1);
			LayoutSubviews ();
		}
		public CalendarMonthView (DateTime currentDate) : base(new CGRect (0, 0, 320, 260))
		{
			if (currentDate.Year < 2010)
				CurrentDate = DateTime.Today;
			else
				CurrentDate = currentDate;
			CurrentMonthYear = new DateTime (CurrentDate.Year, CurrentDate.Month, 1);
			LayoutSubviews ();
		}
		public CalendarMonthView (DateTime currentDate, DateTime[] markedDays, nfloat currentWidth) : base(new CGRect (0, 0, currentWidth, 260))
		{
			Console.WriteLine ("Date Received");
			CurrentWidth = currentWidth;
			MarkedDay = markedDays;
			if (currentDate.Year < 2010)
				CurrentDate = DateTime.Today;
			else
				CurrentDate = currentDate;
			CurrentMonthYear = new DateTime (CurrentDate.Year, CurrentDate.Month, 1);
			LayoutSubviews ();
		}

		public override void LayoutSubviews ()
		{
			if (calendarIsLoaded)
				return;
			
			_scrollView = new UIScrollView (new CGRect (0, 44, CurrentWidth, 460 - 44)) { ContentSize = new CGSize (CurrentWidth, 260), ScrollEnabled = false, Frame = new CGRect (0, 44, CurrentWidth, 460 - 44), BackgroundColor = UIColor.FromRGBA (222 / 255f, 222 / 255f, 225 / 255f, 1f) };
			
			_shadow = new UIImageView (UIImage.FromFile("Images/shadow.png"));

			if (ShowToolBar) {
				toolbar = new UIToolbar (new CGRect (0, 0, CurrentWidth, 44));
				todayButton = new UIBarButtonItem ("Today", UIBarButtonItemStyle.Bordered, delegate {
					if (OnDateSelected != null)
						OnDateSelected (DateTime.Today);
					else
						MoveCalendarMonths (DateTime.Today, true);
				});
				tomorrowBtn = new UIBarButtonItem ("Tomorrow", UIBarButtonItemStyle.Bordered, delegate {
					if (OnDateSelected != null)
						OnDateSelected (DateTime.Today.AddDays (1));
					else
						MoveCalendarMonths (DateTime.Today.AddDays (1), true);
				});
				nextWeekBtn = new UIBarButtonItem ("Next Week", UIBarButtonItemStyle.Bordered, delegate {
					if (OnDateSelected != null)
						OnDateSelected (DateTime.Today.AddDays (7));
					else
						MoveCalendarMonths (DateTime.Today.AddDays (7), true);
				});
				noneBtn = new UIBarButtonItem ("None", UIBarButtonItemStyle.Bordered, delegate {
					if (OnDateSelected != null)
						OnDateSelected (DateTime.MinValue);
				});
				toolbar.SetItems (new UIBarButtonItem[3] { todayButton, tomorrowBtn, nextWeekBtn }, true);
			}
			
			LoadButtons ();
			
			LoadInitialGrids ();
			
			BackgroundColor = UIColor.Clear;
			AddSubview (_scrollView);
			//AddSubview (_shadow);
			if (ShowToolBar)
				AddSubview (toolbar);
			_scrollView.AddSubview (_monthGridView);
			
			calendarIsLoaded = true;
		}

		private void LoadButtons ()
		{
			_leftButton = UIButton.FromType (UIButtonType.Custom);
			_leftButton.TouchUpInside += HandlePreviousMonthTouch;
			_leftButton.SetImage (Images.leftArrow, UIControlState.Normal);
			AddSubview (_leftButton);
			_leftButton.Frame = new CGRect (10, 0, 44, 42);
			
			_rightButton = UIButton.FromType (UIButtonType.Custom);
			_rightButton.TouchUpInside += HandleNextMonthTouch;
			_rightButton.SetImage (Images.rightArrow, UIControlState.Normal);
			AddSubview (_rightButton);
			_rightButton.Frame = new CGRect (CurrentWidth - 56, 0, 44, 42);
		}

		private void HandlePreviousMonthTouch (object sender, EventArgs e)
		{
			MoveCalendarMonths (false, true);
		}
		private void HandleNextMonthTouch (object sender, EventArgs e)
		{
			MoveCalendarMonths (true, true);
		}

		public void MoveCalendarMonths (bool upwards, bool animated)
		{
			CurrentMonthYear = CurrentMonthYear.AddMonths (upwards ? 1 : -1);
			if (MonthChanged != null)
				MonthChanged (CurrentMonthYear);
			UserInteractionEnabled = false;
			
			var gridToMove = CreateNewGrid (CurrentMonthYear);
			var pointsToMove = (upwards ? 0 + _monthGridView.Lines : 0 - _monthGridView.Lines) * 44;
			
			if (upwards && gridToMove.weekdayOfFirst == 0)
				pointsToMove += 44;
			if (!upwards && _monthGridView.weekdayOfFirst == 0)
				pointsToMove -= 44;
			
			gridToMove.Frame = new CGRect (new CGPoint (0, pointsToMove), gridToMove.Frame.Size);
			
			_scrollView.AddSubview (gridToMove);
			
			if (animated) {
				UIView.BeginAnimations ("changeMonth");
				UIView.SetAnimationDuration (0.4);
				UIView.SetAnimationDelay (0.1);
				UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
			}
			
			_monthGridView.Center = new CGPoint (_monthGridView.Center.X, _monthGridView.Center.Y - pointsToMove);
			gridToMove.Center = new CGPoint (gridToMove.Center.X, gridToMove.Center.Y - pointsToMove);
			
			_monthGridView.Alpha = 0;
			
			_shadow.Frame = new CGRect (new CGPoint (0, gridToMove.Lines * 44 - 88), _shadow.Frame.Size);
			
			var oldFrame = _scrollView.Frame;
			_scrollView.Frame = new CGRect (_scrollView.Frame.Location, new CGSize (_scrollView.Frame.Width, (gridToMove.Lines + 1) * 44));
			_scrollView.ContentSize = _scrollView.Frame.Size;
			if (ShowToolBar) {
				var toolRect = toolbar.Frame;
				toolRect.Y = _scrollView.Frame.Y + _scrollView.Frame.Height;
				toolbar.Frame = toolRect;
			}
			
			SetNeedsDisplay ();
			
			if (animated)
				UIView.CommitAnimations ();
			
			_monthGridView = gridToMove;
			
			UserInteractionEnabled = true;
			if (oldFrame != _scrollView.Frame && SizeChanged != null)
			{
				this.Frame = new CGRect(this.Frame.Location,Size);	
				SizeChanged ();
			}
		}


		public void MoveCalendarMonths (DateTime date, bool animated)
		{
			bool upwards = false;
			if (date.Month == CurrentMonthYear.Month && date.Year == CurrentMonthYear.Year)
				animated = false; else if (date > CurrentMonthYear)
				upwards = true;
			CurrentMonthYear = new DateTime (date.Year, date.Month, 1);
			if (MonthChanged != null)
				MonthChanged (CurrentMonthYear);
			UserInteractionEnabled = false;
			
			var gridToMove = CreateNewGrid (CurrentMonthYear);
			var pointsToMove = (upwards ? 0 + _monthGridView.Lines : 0 - _monthGridView.Lines) * 44;
			
			if (upwards && gridToMove.weekdayOfFirst == 0)
				pointsToMove += 44;
			if (!upwards && _monthGridView.weekdayOfFirst == 0)
				pointsToMove -= 44;
			
			gridToMove.Frame = new CGRect (new CGPoint (0, pointsToMove), gridToMove.Frame.Size);
			
			_scrollView.AddSubview (gridToMove);
			
			if (animated) {
				UIView.BeginAnimations ("changeMonth");
				UIView.SetAnimationDuration (0.4);
				UIView.SetAnimationDelay (0.1);
				UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
			}
			
			_monthGridView.Center = new CGPoint (_monthGridView.Center.X, _monthGridView.Center.Y - pointsToMove);
			gridToMove.Center = new CGPoint (gridToMove.Center.X, gridToMove.Center.Y - pointsToMove);			
			_monthGridView.Alpha = 0;			
			_shadow.Frame = new CGRect (new CGPoint (0, gridToMove.Lines * 44 - 88), _shadow.Frame.Size);
			
			var oldFrame = _scrollView.Frame;
			_scrollView.Frame = new CGRect (_scrollView.Frame.Location, new CGSize (_scrollView.Frame.Width, (gridToMove.Lines + 1) * 44));
			_scrollView.ContentSize = _scrollView.Frame.Size;
			if (ShowToolBar) {
				var toolRect = toolbar.Frame;
				toolRect.Y = _scrollView.Frame.Y + _scrollView.Frame.Height;
				toolbar.Frame = toolRect;
			}
			
			SetNeedsDisplay ();
			
			if (animated)
				UIView.CommitAnimations ();
			
			_monthGridView = gridToMove;
			
			UserInteractionEnabled = true;
			if (oldFrame != _scrollView.Frame && SizeChanged != null)
			{
				this.Frame = new CGRect(this.Frame.Location,Size);	
				SizeChanged ();
			}
		}

		private MonthGridView CreateNewGrid (DateTime date)
		{
			var grid = new MonthGridView (this, date, CurrentDate, CurrentDayBlockWidth);
			grid.BuildGrid ();
			grid.Frame = new CGRect (0, 0, CurrentWidth, 400);
			return grid;
		}

		private void LoadInitialGrids ()
		{
			_monthGridView = CreateNewGrid (CurrentMonthYear);
			
			var rect = _scrollView.Frame;
			rect.Size = new CGSize { Height = (_monthGridView.Lines + 1) * 44, Width = rect.Size.Width };
			_scrollView.Frame = rect;
			Frame = new CGRect (Frame.X, Frame.Y, _scrollView.Frame.Size.Width, _scrollView.Frame.Size.Height + 44);
			
			var imgRect = _shadow.Frame;
			imgRect.Y = rect.Size.Height - 132;
			_shadow.Frame = imgRect;
			if (ShowToolBar) {
				var toolRect = toolbar.Frame;
				toolRect.Y = rect.Size.Height + rect.Y;
				toolbar.Frame = toolRect;
			}
		}

		public override void Draw (CGRect rect)
		{
			//Images.calendarTopBar.Draw (new CGPoint (0, 0));
			Images.calendarTopBar.Draw (rect);
			DrawDayLabels (rect);
			DrawMonthLabel (rect);
		}

		private void DrawMonthLabel (CGRect rect)
		{
			var r = new CGRect (new CGPoint (0, 5), new CGSize { Width = CurrentWidth, Height = 42 });
			UIColor.DarkGray.SetColor ();
			var s = new NSString (CurrentMonthYear.ToString ("MMMM yyyy"));
			s.DrawString (r, UIFont.BoldSystemFontOfSize (20), UILineBreakMode.WordWrap, UITextAlignment.Center);
		}

		private void DrawDayLabels (CGRect rect)
		{
			var font = UIFont.BoldSystemFontOfSize (10);
			UIColor.DarkGray.SetColor ();
			var context = UIGraphics.GetCurrentContext ();
			context.SaveState ();
			context.SetShadow (new CGSize (0, -1), 0.5f, UIColor.White.CGColor);
			var i = 0;
			foreach (var d in Enum.GetNames (typeof(DayOfWeek))) {
				var s = new NSString (d.Substring (0, 3));
				s.DrawString (new CGRect (i * CurrentDayBlockWidth, 44 - 12, CurrentDayBlockWidth-1, 10), font, UILineBreakMode.WordWrap, UITextAlignment.Center);
				i++;
			}
			context.RestoreState ();
		}

		internal bool isDayMarker (DateTime date)
		{
			if (IsDayMarkedDelegate != null) {
				return IsDayMarkedDelegate (date);
			}
			if (MarkedDay != null) {
				var isMarked = MarkedDay.Contains (date.Date);
				return isMarked;
			}
			return false;
		}
	}

	internal class MonthGridView : UIView
	{
		private CalendarMonthView _calendarMonthView;

		private readonly DateTime _currentDay;
		public DateTime SelectedDate;
		private DateTime _currentMonth;
		protected readonly IList<CalendarDayView> _dayTiles = new List<CalendarDayView> ();
		public int Lines { get; set; }
		protected CalendarDayView SelectedDayView { get; set; }
		public int weekdayOfFirst;
		public IList<DateTime> Marks { get; set; }
		private nfloat currentDayBlockWidth;

		public MonthGridView (CalendarMonthView calendarMonthView, DateTime month, DateTime day, nfloat currentDayBlockWidth)
		{
			SelectedDate = day;
			_calendarMonthView = calendarMonthView;
			_currentDay = DateTime.Today;
			_currentMonth = month.Date;
			this.currentDayBlockWidth = currentDayBlockWidth;
		}

		public void BuildGrid ()
		{
			DateTime previousMonth;
			try {
				previousMonth = _currentMonth.AddMonths (-1);
			} catch {
				previousMonth = _currentMonth;
			}
			var daysInPreviousMonth = DateTime.DaysInMonth (previousMonth.Year, previousMonth.Month);
			var daysInMonth = DateTime.DaysInMonth (_currentMonth.Year, _currentMonth.Month);
			weekdayOfFirst = (int)_currentMonth.DayOfWeek;
			var lead = daysInPreviousMonth - (weekdayOfFirst - 1);
			
			// build last month's days
			for (int i = 1; i <= weekdayOfFirst; i++) {
				var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
				var dayView = new CalendarDayView { Frame = new CGRect ((i - 1) * currentDayBlockWidth - 1, 0, currentDayBlockWidth+1, 45), Text = lead.ToString (), Marked = _calendarMonthView.isDayMarker (viewDay) };
				AddSubview (dayView);
				_dayTiles.Add (dayView);
				lead++;
			}
			
			var position = weekdayOfFirst + 1;
			var line = 0;
			
			// current month
			for (int i = 1; i <= daysInMonth; i++) {
				var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
				var dayView = new CalendarDayView { Frame = new CGRect ((position - 1) * currentDayBlockWidth - 1, line * 44, currentDayBlockWidth + 1, 45), Today = (_currentDay.Date == viewDay.Date), Text = i.ToString (), Active = true, Tag = i, Marked = _calendarMonthView.isDayMarker (viewDay), Selected = (SelectedDate.Day == i) };
				
				if (dayView.Selected)
					SelectedDayView = dayView;
				
				AddSubview (dayView);
				_dayTiles.Add (dayView);
				
				position++;
				if (position > 7) {
					position = 1;
					line++;
				}
			}
			
			//next month
			if (position != 1) {
				int dayCounter = 1;
				for (int i = position; i < 8; i++) {
					var viewDay = new DateTime (_currentMonth.Year, _currentMonth.Month, i);
					var dayView = new CalendarDayView { Frame = new CGRect ((i - 1) * currentDayBlockWidth - 1, line * 44, currentDayBlockWidth+1, 45), Text = dayCounter.ToString (), Marked = _calendarMonthView.isDayMarker (viewDay) };
					AddSubview (dayView);
					_dayTiles.Add (dayView);
					dayCounter++;
				}
			}
			
			Frame = new CGRect (Frame.Location, new CGSize (Frame.Width, (line + 1) * 44));
			
			Lines = (position == 1 ? line - 1 : line);
			
			if (SelectedDayView != null)
				this.BringSubviewToFront (SelectedDayView);
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			if (SelectDayView ((UITouch)touches.AnyObject)) {
				SelectedDate = new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag);
				if (_calendarMonthView.OnDateSelected != null)
					_calendarMonthView.OnDateSelected (new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag));
			}
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			if (SelectDayView ((UITouch)touches.AnyObject)) {
				SelectedDate = new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag);
				if (_calendarMonthView.OnDateSelected != null)
					_calendarMonthView.OnDateSelected (new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag));
			}
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			if (_calendarMonthView.OnFinishedDateSelection == null)
				return;
			SelectedDate = new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag);
			_calendarMonthView.OnFinishedDateSelection (new DateTime (_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag));
		}

		private bool SelectDayView (UITouch touch)
		{
			var p = touch.LocationInView (this);
			
			int index = ((int)p.Y / 44) * 7 + ((int)p.X / (int)currentDayBlockWidth);
			if (index < 0 || index >= _dayTiles.Count)
				return false;
			
			var newSelectedDayView = _dayTiles[index];
			if (newSelectedDayView == SelectedDayView)
				return false;
			
			if (!newSelectedDayView.Active && touch.Phase != UITouchPhase.Moved) {
				var day = int.Parse (newSelectedDayView.Text);
				if (day > 15)
					_calendarMonthView.MoveCalendarMonths (false, true);
				else
					_calendarMonthView.MoveCalendarMonths (true, true);
				return false;
			} else if (!newSelectedDayView.Active) {
				return false;
			}
			
			SelectedDayView.Selected = false;
			this.BringSubviewToFront (newSelectedDayView);
			newSelectedDayView.Selected = true;
			
			SelectedDayView = newSelectedDayView;
			SetNeedsDisplay ();
			return true;
		}
	}

	internal class CalendarDayView : UIView
	{
		string _text;
		bool _active, _today, _selected, _marked;
		public string Text {
			get { return _text; }
			set {
				_text = value;
				SetNeedsDisplay ();
			}
		}
		public bool Active {
			get { return _active; }
			set {
				_active = value;
				SetNeedsDisplay ();
			}
		}
		public bool Today {
			get { return _today; }
			set {
				_today = value;
				SetNeedsDisplay ();
			}
		}
		public bool Selected {
			get { return _selected; }
			set {
				_selected = value;
				SetNeedsDisplay ();
			}
		}
		public bool Marked {
			get { return _marked; }
			set {
				_marked = value;
				SetNeedsDisplay ();
			}
		}

		public override void Draw (CGRect rect)
		{
			UIImage img;
			UIColor color;
			
			if (!Active) {
				color = UIColor.FromRGBA (0.576f, 0.608f, 0.647f, 1f);
				img = Images.dateCell;
			} else if (Today && Selected) {
				color = UIColor.White;
				img = Images.todayselected;
			} else if (Today) {
				color = UIColor.White;
				img = Images.today;
			} else if (Selected) {
				color = UIColor.White;
				img = Images.datecellselected;
			} else {
				//color = UIColor.DarkTextColor;
				color = UIColor.FromRGBA (0.275f, 0.341f, 0.412f, 1f);
				img = Images.dateCell;
			}
			//img.Draw (new CGPoint (0, 0));
			img.Draw (rect);
			color.SetColor ();
			var s1 = new NSString(Text);
			s1.DrawString (CGRect.Inflate (Bounds, 4, -8), UIFont.BoldSystemFontOfSize (22), UILineBreakMode.WordWrap, UITextAlignment.Center);
			
			if (Marked) {
				var context = UIGraphics.GetCurrentContext ();
				if (Selected || Today)
					context.SetFillColor (1, 1, 1, 1); else if (!Active)
					UIColor.LightGray.SetColor ();
				else
					context.SetFillColor (75 / 255f, 92 / 255f, 111 / 255f, 1);
				context.SetLineWidth (0);
				context.AddEllipseInRect (new CGRect (Frame.Size.Width / 2 - 2, 45 - 10, 4, 4));
				context.FillPath ();
				
			}
		}
	}
}
