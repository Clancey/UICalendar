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
using System.Collections.Generic;
using MonoTouch.Dialog;
using UIKit;
using Foundation;
using System.Linq;
using CoreGraphics;
namespace UICalendar
{
	public static class Settings
	{
		
		private static NSUserDefaults prefs =  NSUserDefaults.StandardUserDefaults ;
			
		public static nint lastCal
		{
			get {return prefs.IntForKey("lastCal");}
			set {prefs.SetInt(value,"lastCal");prefs.Synchronize();}
		}
	
		
	}
	
}

