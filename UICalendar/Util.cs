// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Globalization;
using MonoTouch.CoreAnimation;
using MonoTouch.EventKit;
using System.Linq;
using System.Collections.Generic;
namespace UICalendar
{
	public static partial class Util
	{
		public static void fadeView (UIView theView, double duration)
		{
			theView.Alpha = 1;
			// Begin the Animation Block.
			CABasicAnimation fadeInAnimation = CABasicAnimation.FromKeyPath (@"opacity");
			
			// Set the Duration of the Animation (Pre-defined above.)
			fadeInAnimation.RepeatDuration = duration;
			
			// Set the Beginning Alpha of the Animation (Pre-defined above.)
			fadeInAnimation.From = NSNumber.FromFloat (0.0f);
			
			// Set the Ending Alpha of the Animation (Pre-defined above.)
			fadeInAnimation.To = NSNumber.FromFloat (1.0f);
			//[fadeInAnimation setDelegate: self];
			
			// Apply the Animation to the UIImageView.
			theView.Layer.AddAnimation (fadeInAnimation, "animateOpacity");
			
		}

		public static EKEventStore MyEventStore = new EKEventStore ();

		public static EKEvent[] FetchEvents (DateTime startDate, DateTime endDate)
		{
			// Create the predicate. Pass it the default calendar.
			//Util.WriteLine ("Getting Calendars");
			EKEventStore store = new EKEventStore ();
			var calendarArray = store.Calendars;
			//Util.WriteLine ("Predicate");
			//Convert to NSDate
			NSDate nstartDate = Util.DateTimeToNSDate (startDate);
			NSDate nendDate = Util.DateTimeToNSDate (endDate);
			NSPredicate predicate = store.PredicateForEvents (nstartDate, nendDate, calendarArray);
			
			//Util.WriteLine ("Fetching Events");
			// Fetch all events that match the predicate.
			var eventsArray = store.EventsMatching (predicate);
			//Util.WriteLine ("Returning results");
			if (eventsArray == null) {
				eventsArray = new List<EKEvent> ().ToArray ();
			}
			return eventsArray;
		}

		public static DateTime NSDateToDateTime (MonoTouch.Foundation.NSDate date)
		{
			if(date == null)
				return DateTime.MinValue;
			var theDate = ((DateTime)date).ToLocalTime ();
			return theDate;
		}		
		public static DateTime UtcToLocal (DateTime date)
		{
			var newDate = new DateTime (date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
			return newDate;
		}
		public static NSDate DateTimeToNSDate (DateTime date)
		{
			var newDate = (NSDate)date;
			return newDate;
		}
		
		public static DateTime DateTimeMin {
			get { return DateTime.FromFileTimeUtc (0); }
		}
		
		public static EKEvent getEvent (CalendarDayEventView theEventView)
		{
			EKEventStore store = MyEventStore;
			var calendarArray = store.Calendars;
			//Util.WriteLine ("Predicate");
			//var newNSDate = (NSDate)theEventView.endDate;
			//Console.WriteLine ("Date is: {0} {1} {2}", NSDate.Now.ToString (), ((NSDate) DateTime.Now).ToString (), DateTime.Now);
			NSPredicate predicate = store.PredicateForEvents (theEventView.nsStartDate, theEventView.nsEndDate, calendarArray);
			//Util.WriteLine ("Fetching Events");
			// Fetch all events that match the predicate.
			return store.EventsMatching (predicate).Where (x => x.EventIdentifier == theEventView.eventIdentifier).FirstOrDefault ();
		}
		/// <summary>
		///   A shortcut to the main application
		/// </summary>
		public static UIApplication MainApp = UIApplication.SharedApplication;
		
	}
}
