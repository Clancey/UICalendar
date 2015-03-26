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
using CoreGraphics;
using Foundation;


namespace UICalendar
{
	public static class Extensions
	{
		#region Points
		public static CGPoint Subtract (this CGPoint orgPoint, CGPoint point)
		{
			var x = orgPoint.X - point.X;
			var y = orgPoint.Y - point.Y;
			return new CGPoint (x, y);
		}
		public static CGPoint Add (this CGPoint orgPoint, CGPoint point)
		{
			var x = orgPoint.X + point.X;
			var y = orgPoint.Y + point.Y;
			return new CGPoint (x, y);
		}

		public static CGPoint Add (this CGPoint orgPoint, CGSize size)
		{
			var x = orgPoint.X + size.Width;
			var y = orgPoint.Y + size.Height;
			return new CGPoint (x, y);
		}

//		public static CGPoint Add (this CGPoint orgPoint, CGSize size)
//		{
//			var x = orgPoint.X + size.Width;
//			var y = orgPoint.Y + size.Height;
//			return new CGPoint (x, y);
//		}
//
//		public static CGPoint Add (this CGPoint orgPoint, CGPoint point)
//		{
//			var x = orgPoint.X + point.X;
//			var y = orgPoint.Y + point.Y;
//			return new CGPoint (x, y);
//		}
//		public static CGPoint Subtract (this CGPoint orgPoint, CGPoint point)
//		{
//			var x = orgPoint.X - point.X;
//			var y = orgPoint.Y - point.Y;
//			return new CGPoint (x, y);
//		}
//
//		public static CGPoint Subtract (this CGPoint orgPoint, CGPoint point)
//		{
//			var pointF = CGPoint.Round (point);
//			var x = orgPoint.X - pointF.X;
//			var y = orgPoint.Y - pointF.Y;
//			return new CGPoint (x, y);
//		}
		#endregion

		#region Sizes
		public static CGSize Subtract (this CGSize orgSize, CGPoint point)
		{
			var x = orgSize.Width - point.X;
			var y = orgSize.Height - point.Y;
			return new CGSize (x, y);
		}
		public static CGSize Add (this CGSize orgSize, CGPoint point)
		{
			var x = orgSize.Width + point.X;
			var y = orgSize.Height + point.Y;
			return new CGSize (x, y);
		}

		public static CGSize Add (this CGSize orgSize, CGSize size)
		{
			var x = orgSize.Width + size.Width;
			var y = orgSize.Height + size.Height;
			return new CGSize (x, y);
		}

//		public static CGSize Add (this CGSize orgSize, CGSize size)
//		{
//			var x = orgSize.Width + size.Width;
//			var y = orgSize.Height + size.Height;
//			return new CGSize (x, y);
//		}
//
//		public static CGSize Add (this CGSize orgSize, CGPoint point)
//		{
//			var x = orgSize.Width + point.X;
//			var y = orgSize.Height + point.Y;
//			return new CGSize (x, y);
//		}
//		public static CGSize Subtract (this CGSize orgSize, CGPoint point)
//		{
//			var x = orgSize.Width - point.X;
//			var y = orgSize.Height - point.Y;
//			return new CGSize (x, y);
//		}
//
//		public static CGSize Subtract (this CGSize orgSize, CGSize size)
//		{
//			var x = orgSize.Width - size.Width;
//			var y = orgSize.Height - size.Height;
//			return new CGSize (x, y);
//		}
//
//		public static CGSize Subtract (this CGSize orgSize, CGSize size)
//		{
//			var sizeF = CGSize.Round (size);
//			var x = orgSize.Width - sizeF.Width;
//			var y = orgSize.Height - sizeF.Height;
//			return new CGSize (x, y);
//		}
		#endregion

		#region Rectangles
		public static nfloat AbsoluteWidth(this CGRect orgRect)
		{
			return orgRect.X + orgRect.Width;	
		}
		public static nfloat AbsoluteHeight(this CGRect orgRect)
		{
			return orgRect.Y + orgRect.Height;	
		}
		
		public static CGRect SetLocation (this CGRect orgRect, CGPoint point)
		{
			orgRect.Location = point;
			return orgRect;
		}
		
		public static CGRect SetLocation (this CGRect orgRect, float x,float y)
		{
			orgRect.Location = new CGPoint(x,y);
			return orgRect;
		}
		public static CGRect SetLocation (this CGRect orgRect, CGSize point)
		{
			orgRect.Location = new CGPoint(point.Width,point.Height);
			return orgRect;
		}

		public static CGRect SetSize (this CGRect orgRect, CGSize size)
		{
			orgRect.Size = size;
			return orgRect;
		}
		public static CGRect SetSize (this CGRect orgRect, float width,float height)
		{
			orgRect.Size = new CGSize(width,height);;
			return orgRect;
		}
		
		public static CGRect SetHeight (this CGRect orgRect, int height)
		{
			orgRect.Height = height;
			return orgRect;
		}
		
		public static CGRect SetHeight (this CGRect orgRect, float height)
		{
			orgRect.Height = height;
			return orgRect;
		}
		
		public static CGRect SetWidth (this CGRect orgRect, int width)
		{
			orgRect.Width = width;
			return orgRect;
		}
		
		public static CGRect SetWidth (this CGRect orgRect, float width)
		{
			orgRect.Width = width;
			return orgRect;
		}

		public static CGRect Add (this CGRect orgRect, CGSize size)
		{
			orgRect.Size = orgRect.Size.Add (size);
			return orgRect;
		}
		public static CGRect Add (this CGRect orgRect, CGPoint point)
		{
			orgRect.Location = orgRect.Location.Add (point);
			return orgRect;
		}

		public static CGRect Add (this CGRect orgRect, CGPoint point, CGSize size)
		{
			orgRect.Location = orgRect.Location.Add (point);
			orgRect.Size = orgRect.Size.Add (size);
			return orgRect;
		}
		
		public static CGRect Add ( this CGRect orgRect, float x, float y, float width, float height)
		{
			orgRect.X += x;
			orgRect.Y += y;
			orgRect.Width += width;
			orgRect.Height += height;
			return orgRect;
		}

		public static CGRect AddSize (this CGRect orgRect, nfloat width, nfloat height)
		{
			orgRect.Width += width;
			orgRect.Height += height;
			return orgRect;
		}
		public static CGRect AddSize (this CGRect orgRect, CGSize size)
		{
			orgRect.Width += size.Width;
			orgRect.Height += size.Height;
			return orgRect;
		}

		public static CGRect AddLocation (this CGRect orgRect, float x, float y)
		{
			orgRect.X += x;
			orgRect.Y += y;
			return orgRect;
		}

		public static CGRect Subtract (this CGRect orgRect, CGSize size)
		{
			orgRect.Size = orgRect.Size.Subtract (size.ToCGPoint());
			return orgRect;
		}

		public static CGRect Subtract (this CGRect orgRect, CGPoint point)
		{
			orgRect.Location = orgRect.Location.Subtract (point);
			return orgRect;
		}
		
		public static CGRect Subtract ( this CGRect orgRect, float x, float y, float width, float height)
		{
			orgRect.X -= x;
			orgRect.Y -= y;
			orgRect.Width -= width;
			orgRect.Height -= height;
			return orgRect;
		}

		public static CGRect SubtractSize (this CGRect orgRect, float width, float height)
		{
			orgRect.Width -= width;
			orgRect.Height -= height;
			return orgRect;
		}

		public static CGRect SubtractLocation (this CGRect orgRect, float x, float y)
		{
			orgRect.X -= x;
			orgRect.Y -= y;
			return orgRect;
		}

		public static CGRect Subtract (this CGRect orgRect, CGPoint point, CGSize size)
		{
			orgRect.Location = orgRect.Location.Subtract (point);
			orgRect.Size = orgRect.Size.Subtract (size.ToCGPoint());
			return orgRect;
		}
		#endregion

		public static DateTime NSDateToDateTime(this NSDate date)
		{
			DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
				new DateTime(2001, 1, 1, 0, 0, 0) );
			return reference.AddSeconds(date.SecondsSinceReferenceDate);
		}

		public static NSDate DateTimeToNSDate(this DateTime date)
		{
			if (date.Kind == DateTimeKind.Unspecified)
				date = DateTime.SpecifyKind (date, DateTimeKind.Local /*or DateTimeKind.Utc, this depends on each app */);
				return (NSDate) date;
			}
	}
}

