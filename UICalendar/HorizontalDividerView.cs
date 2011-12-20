using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
namespace UICalendar
{
	public class HorizontalDividerView : UIView
	{
		public HorizontalDividerView (RectangleF rect) : base (rect)
		{	
			var backgroundLayer = new CAGradientLayer();
			backgroundLayer.Frame = this.Bounds;
			backgroundLayer.Colors = new MonoTouch.CoreGraphics.CGColor[] { UIColor.White.CGColor,UIColor.LightGray.CGColor };
			Layer.AddSublayer(backgroundLayer);

		}
		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
			
			CGContext context = UIGraphics.GetCurrentContext ();
			context.SaveState ();
			context.SetFillColorWithColor(UIColor.Black.CGColor);
			context.SetLineWidth(1);
			context.SetFillColorWithColor(UIColor.Clear.CGColor);
			context.AddRect(this.Frame.Subtract(1,1,1,1));
			context.RestoreState();			
		}
	}
}

