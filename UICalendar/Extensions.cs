using System;
using System.Drawing;
namespace UICalendar
{
	public static class Extensions
	{
		#region Points
		public static PointF Subtract (this PointF orgPoint, PointF point)
		{
			var x = orgPoint.X - point.X;
			var y = orgPoint.Y - point.Y;
			return new PointF (x, y);
		}
		public static PointF Add (this PointF orgPoint, PointF point)
		{
			var x = orgPoint.X + point.X;
			var y = orgPoint.Y + point.Y;
			return new PointF (x, y);
		}

		public static PointF Add (this PointF orgPoint, Size size)
		{
			var x = orgPoint.X + size.Width;
			var y = orgPoint.Y + size.Height;
			return new PointF (x, y);
		}

		public static PointF Add (this PointF orgPoint, SizeF size)
		{
			var x = orgPoint.X + size.Width;
			var y = orgPoint.Y + size.Height;
			return new PointF (x, y);
		}

		public static Point Add (this Point orgPoint, Point point)
		{
			var x = orgPoint.X + point.X;
			var y = orgPoint.Y + point.Y;
			return new Point (x, y);
		}
		public static Point Subtract (this Point orgPoint, Point point)
		{
			var x = orgPoint.X - point.X;
			var y = orgPoint.Y - point.Y;
			return new Point (x, y);
		}

		public static Point Subtract (this Point orgPoint, PointF point)
		{
			var pointF = Point.Round (point);
			var x = orgPoint.X - pointF.X;
			var y = orgPoint.Y - pointF.Y;
			return new Point (x, y);
		}
		#endregion

		#region Sizes
		public static SizeF Subtract (this SizeF orgSize, PointF point)
		{
			var x = orgSize.Width - point.X;
			var y = orgSize.Height - point.Y;
			return new SizeF (x, y);
		}
		public static SizeF Add (this SizeF orgSize, PointF point)
		{
			var x = orgSize.Width + point.X;
			var y = orgSize.Height + point.Y;
			return new SizeF (x, y);
		}

		public static SizeF Add (this SizeF orgSize, Size size)
		{
			var x = orgSize.Width + size.Width;
			var y = orgSize.Height + size.Height;
			return new SizeF (x, y);
		}

		public static SizeF Add (this SizeF orgSize, SizeF size)
		{
			var x = orgSize.Width + size.Width;
			var y = orgSize.Height + size.Height;
			return new SizeF (x, y);
		}

		public static Size Add (this Size orgSize, Point point)
		{
			var x = orgSize.Width + point.X;
			var y = orgSize.Height + point.Y;
			return new Size (x, y);
		}
		public static Size Subtract (this Size orgSize, Point point)
		{
			var x = orgSize.Width - point.X;
			var y = orgSize.Height - point.Y;
			return new Size (x, y);
		}

		public static SizeF Subtract (this SizeF orgSize, SizeF size)
		{
			var x = orgSize.Width - size.Width;
			var y = orgSize.Height - size.Height;
			return new SizeF (x, y);
		}

		public static Size Subtract (this Size orgSize, SizeF size)
		{
			var sizeF = Size.Round (size);
			var x = orgSize.Width - sizeF.Width;
			var y = orgSize.Height - sizeF.Height;
			return new Size (x, y);
		}
		#endregion

		#region Rectangles
		public static float AbsoluteWidth(this RectangleF orgRect)
		{
			return orgRect.X + orgRect.Width;	
		}
		public static float AbsoluteHeight(this RectangleF orgRect)
		{
			return orgRect.Y + orgRect.Height;	
		}
		
		public static RectangleF SetLocation (this RectangleF orgRect, PointF point)
		{
			orgRect.Location = point;
			return orgRect;
		}
		
		public static RectangleF SetLocation (this RectangleF orgRect, float x,float y)
		{
			orgRect.Location = new PointF(x,y);
			return orgRect;
		}
		public static RectangleF SetLocation (this RectangleF orgRect, SizeF point)
		{
			orgRect.Location = new PointF(point.Width,point.Height);
			return orgRect;
		}

		public static RectangleF SetSize (this RectangleF orgRect, SizeF size)
		{
			orgRect.Size = size;
			return orgRect;
		}
		public static RectangleF SetSize (this RectangleF orgRect, float width,float height)
		{
			orgRect.Size = new SizeF(width,height);;
			return orgRect;
		}
		
		public static RectangleF SetHeight (this RectangleF orgRect, int height)
		{
			orgRect.Height = height;
			return orgRect;
		}
		
		public static RectangleF SetHeight (this RectangleF orgRect, float height)
		{
			orgRect.Height = height;
			return orgRect;
		}
		
		public static RectangleF SetWidth (this RectangleF orgRect, int width)
		{
			orgRect.Width = width;
			return orgRect;
		}
		
		public static RectangleF SetWidth (this RectangleF orgRect, float width)
		{
			orgRect.Width = width;
			return orgRect;
		}

		public static RectangleF Add (this RectangleF orgRect, SizeF size)
		{
			orgRect.Size = orgRect.Size.Add (size);
			return orgRect;
		}
		public static RectangleF Add (this RectangleF orgRect, PointF point)
		{
			orgRect.Location = orgRect.Location.Add (point);
			return orgRect;
		}

		public static RectangleF Add (this RectangleF orgRect, PointF point, SizeF size)
		{
			orgRect.Location = orgRect.Location.Add (point);
			orgRect.Size = orgRect.Size.Add (size);
			return orgRect;
		}
		
		public static RectangleF Add ( this RectangleF orgRect, float x, float y, float width, float height)
		{
			orgRect.X += x;
			orgRect.Y += y;
			orgRect.Width += width;
			orgRect.Height += height;
			return orgRect;
		}

		public static RectangleF AddSize (this RectangleF orgRect, float width, float height)
		{
			orgRect.Width += width;
			orgRect.Height += height;
			return orgRect;
		}
		public static RectangleF AddSize (this RectangleF orgRect, SizeF size)
		{
			orgRect.Width += size.Width;
			orgRect.Height += size.Height;
			return orgRect;
		}

		public static RectangleF AddLocation (this RectangleF orgRect, float x, float y)
		{
			orgRect.X += x;
			orgRect.Y += y;
			return orgRect;
		}

		public static RectangleF Subtract (this RectangleF orgRect, SizeF size)
		{
			orgRect.Size = orgRect.Size.Subtract (size);
			return orgRect;
		}

		public static RectangleF Subtract (this RectangleF orgRect, PointF point)
		{
			orgRect.Location = orgRect.Location.Subtract (point);
			return orgRect;
		}
		
		public static RectangleF Subtract ( this RectangleF orgRect, float x, float y, float width, float height)
		{
			orgRect.X -= x;
			orgRect.Y -= y;
			orgRect.Width -= width;
			orgRect.Height -= height;
			return orgRect;
		}

		public static RectangleF SubtractSize (this RectangleF orgRect, float width, float height)
		{
			orgRect.Width -= width;
			orgRect.Height -= height;
			return orgRect;
		}

		public static RectangleF SubtractLocation (this RectangleF orgRect, float x, float y)
		{
			orgRect.X -= x;
			orgRect.Y -= y;
			return orgRect;
		}

		public static RectangleF Subtract (this RectangleF orgRect, PointF point, SizeF size)
		{
			orgRect.Location = orgRect.Location.Subtract (point);
			orgRect.Size = orgRect.Size.Subtract (size);
			return orgRect;
		}
		#endregion
	}
}

