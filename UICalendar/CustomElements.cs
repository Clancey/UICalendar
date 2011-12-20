using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.EventKit;
using MonoTouch.EventKitUI;
using System.Linq;
using MonoTouch.ObjCRuntime;
using MonoTouch.Dialog.Utilities;

namespace UICalendar
{
	
	public class MonthEventElement : Element
	{
		public CalendarDayEventView TheEvent;

		public static float margin = 5f;
		public static float circleWidth = 25;
		public static float timeWidth = 62f;
		public static float subTimeWidth = 25f;

		static NSString key = new NSString ("MonthEventElement");
		static UIFont font = UIFont.BoldSystemFontOfSize (17f);
		static UIFont timeFont = UIFont.BoldSystemFontOfSize (14f);
		static UIFont timeSubFont = UIFont.SystemFontOfSize (14f);
		static UIFont subFont = UIFont.SystemFontOfSize (12);
		public EventClicked OnEventClicked;

		public virtual UIColor backColor {
			get { return UIColor.Clear; }
		}

		public MonthEventElement (CalendarDayEventView theEvent) : base("")
		{
			TheEvent = theEvent;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key) as MonthEventCellView;
			if (cell == null)
				cell = new MonthEventCellView (this);
			else
				cell.UpdateFrom (this);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			return cell;
			
		}
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (OnEventClicked != null)
				OnEventClicked (TheEvent);
		}

		public class MonthEventCellView : UITableViewCell
		{
			MonthEventElement parent;
			UIView circleView;
			UILabel timeLabel;
			UILabel timeSubLabel;
			UILabel label;
			UILabel lblSub;
			bool timeHasSubLabel;

			public MonthEventCellView (MonthEventElement parent) : base(UITableViewCellStyle.Value1, key)
			{
				this.parent = parent;
				
				circleView = new Circle { Color = parent.TheEvent.color, BackgroundColor = UIColor.Clear };
				setupTimeLabels ();
				
				BackgroundColor = UIColor.Clear;
				label = new UILabel { TextAlignment = UITextAlignment.Left, Text = parent.TheEvent.Title, Font = font, TextColor = UIColor.Black, Lines = 1, LineBreakMode = UILineBreakMode.TailTruncation, BackgroundColor = UIColor.Clear };
				lblSub = new UILabel { TextAlignment = UITextAlignment.Left, Text = parent.TheEvent.location, Font = subFont, TextColor = UIColor.Gray, BackgroundColor = UIColor.Clear };
				
				ContentView.Add (circleView);
				ContentView.Add (timeSubLabel);
				ContentView.Add (timeLabel);
				ContentView.Add (label);
				ContentView.Add (lblSub);
			}

			public override void Draw (RectangleF rect)
			{
				base.Draw (rect);
			}

			public void setupTimeLabels ()
			{
				timeLabel = new UILabel { Font = MonthEventElement.timeFont, TextAlignment = UITextAlignment.Right, BackgroundColor = UIColor.Clear };
				timeSubLabel = new UILabel { Font = MonthEventElement.timeSubFont, TextAlignment = UITextAlignment.Center, TextColor = UIColor.Gray, BackgroundColor = UIColor.Clear };
				
				if (parent.TheEvent.AllDay) {
					timeLabel.Text = "all-day";
					return;
				}
				var time = parent.TheEvent.startDate.ToShortTimeString ();
				if (time == "12:00 PM") {
					timeLabel.Text = "NOON";
					return;
				}
				
				
				timeHasSubLabel = true;
				var timeParts = time.Split (char.Parse (" "));
				var timeOnly = timeParts[0];
				var timeOnlyParts = timeOnly.Split (char.Parse (":"));
				
				timeLabel.Text = timeOnlyParts[1] == "00" ? timeOnlyParts[0] : timeOnly;
				timeSubLabel.Text = timeParts[1];
				
			}

			public void Refresh ()
			{
				label.Text = parent.TheEvent.Title;
				lblSub.Text = parent.TheEvent.location;
				circleView.BackgroundColor = parent.TheEvent.color;
				setupTimeLabels ();
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var full = ContentView.Bounds;
				var frame = full;
				frame.Height -= margin * 2;
				frame.X = margin;
				frame.Width = circleWidth;
				circleView.Frame = frame;
				
				frame.X += circleWidth;
				frame.Height = 24f;
				frame.Y = (full.Height - frame.Height) / 2;
				if (!timeHasSubLabel) {
					frame.Width = timeWidth;
					timeLabel.Frame = frame;
					frame.X += timeWidth;
				} else {
					frame.Width = timeWidth - subTimeWidth;
					var timeFrame = frame;
					timeLabel.Frame = timeFrame;
					
					frame.X += frame.Width;
					frame.Width = subTimeWidth;
					var subTimeFrame = frame;
					timeSubLabel.Frame = subTimeFrame;
					frame.X += subTimeWidth;
				}
				
				frame.X += margin * 2;
				frame.Height = 27f;
				frame.Y = (full.Height - frame.Height) / 2;
				frame.Width = full.Width - frame.X;
				if (string.IsNullOrEmpty (lblSub.Text)) {
					frame.Width = full.Width - frame.X;
					label.Frame = frame;
				} else {
					var mid = full.Height / 2;
					var labelTop = mid - frame.Height + 8;
					frame.Y = labelTop;
					label.Frame = frame;
					
					frame.Y = mid - 4;
					lblSub.Frame = frame;
				}
			}

			public void UpdateFrom (MonthEventElement newParent)
			{
				parent = newParent;
				Refresh ();
			}
		}

		private class Circle : UIView
		{
			const float diameter = 15;
			public UIColor Color { get; set; }
			public override void Draw (RectangleF rect)
			{
				var context = UIGraphics.GetCurrentContext ();
				
				context.SaveState ();
				var frame = rect;
				frame.X = (frame.Width - diameter) / 2;
				frame.Y = frame.Height / 2;
				frame.Height = diameter;
				frame.Width = diameter;
				
				//context.SetFillColorWithColor(UIColor.Black.ColorWithAlpha(.7f).CGColor);
				//context.FillEllipseInRect(frame);
				
				frame.Y -= 3;
				context.SetFillColorWithColor (UIColor.White.CGColor);
				context.FillRect (rect);
				context.SetFillColorWithColor (Color.CGColor);
				context.FillEllipseInRect (frame);
				
				context.RestoreState ();
				
				//
				var shineFrame = frame;
				shineFrame.Height = (shineFrame.Height / 2);
				
				// the colors
				var topColor = UIColor.White.ColorWithAlpha (0.5f).CGColor;
				var bottomColor = UIColor.White.ColorWithAlpha (0.10f).CGColor;
				List<float> colors = new List<float> ();
				colors.AddRange (topColor.Components);
				colors.AddRange (bottomColor.Components);
				float[] locations = new float[] { 0, 1 };
				
				CGGradient gradient = new CGGradient (topColor.ColorSpace, colors.ToArray (), locations);
				
				context.SaveState ();
				context.SetShouldAntialias (true);
				context.AddEllipseInRect (shineFrame);
				context.Clip ();
				
				var startPoint = new PointF (shineFrame.GetMidX (), shineFrame.GetMidY ());
				var endPoint = new PointF (shineFrame.GetMidX (), shineFrame.GetMaxY ());
				
				context.DrawLinearGradient (gradient, startPoint, endPoint, CGGradientDrawingOptions.DrawsBeforeStartLocation);
				gradient.Dispose ();
				context.RestoreState ();
				
			}
		}
	}

}

