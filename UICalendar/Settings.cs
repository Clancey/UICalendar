using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Linq;
using System.Drawing;
namespace UICalendar
{
	public static class Settings
	{
		
		private static NSUserDefaults prefs =  NSUserDefaults.StandardUserDefaults ;
			
		public static int lastCal
		{
			get {return prefs.IntForKey("lastCal");}
			set {prefs.SetInt(value,"lastCal");prefs.Synchronize();}
		}
	
		
	}
	
}

