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
using UIKit;
using CoreGraphics;
using CoreGraphics;
using CoreAnimation;
namespace UICalendar
{
	public class HorizontalDividerView : UIView
	{
		public HorizontalDividerView (CGRect rect) : base (rect)
		{	
			var backgroundLayer = new CAGradientLayer();
			backgroundLayer.Frame = this.Bounds;
			backgroundLayer.Colors = new CoreGraphics.CGColor[] { UIColor.White.CGColor,UIColor.LightGray.CGColor };
			Layer.AddSublayer(backgroundLayer);

		}
		public override void Draw (CGRect rect)
		{
			base.Draw (rect);
			
			CGContext context = UIGraphics.GetCurrentContext ();
			context.SaveState ();
			context.SetFillColor(UIColor.Black.CGColor);
			context.SetLineWidth(1);
			context.SetFillColor(UIColor.Clear.CGColor);
			context.AddRect(this.Frame.Subtract(1,1,1,1));
			context.RestoreState();			
		}
	}
}

