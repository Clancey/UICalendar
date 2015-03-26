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
namespace UICalendar
{
	public static class Images
	{
		private static UIImage _leftArrow;
		public static UIImage leftArrow {
			get {
				if (_leftArrow == null)
					_leftArrow = UIImage.FromFile ("Images/Calendar/leftarrow.png");
				return _leftArrow;
			}
		}
		private static UIImage _rightArrow;
		public static UIImage rightArrow {
			get {
				if (_rightArrow == null)
					_rightArrow = UIImage.FromFile ("Images/Calendar/rightarrow.png");
				return _rightArrow;
			}
		}
		private static UIImage _dateCell;
		public static UIImage dateCell {
			get {
				if (_dateCell == null)
					_dateCell = UIImage.FromFile ("Images/Calendar/datecell.png");
				return _dateCell;
			}
		}
		private static UIImage _todayselected;
		public static UIImage todayselected {
			get {
				if (_todayselected == null)
					_todayselected = UIImage.FromFile ("Images/Calendar/todayselected.png");
				return _todayselected;
			}
		}
		private static UIImage _today;
		public static UIImage today {
			get {
				if (_today == null)
					_today = UIImage.FromFile ("Images/Calendar/today.png");
				return _today;
			}
		}
		private static UIImage _datecellselected;
		public static UIImage datecellselected {
			get {
				if (_datecellselected == null)
					_datecellselected = UIImage.FromFile ("Images/Calendar/datecellselected.png");
				return _datecellselected;
			}
		}
		private static UIImage _calendarTopBar;
		public static UIImage calendarTopBar {
			get {
				if (_calendarTopBar == null)
					_calendarTopBar = UIImage.FromFile ("Images/Calendar/topbar.png");
				return _calendarTopBar;
			}
		}
		private static UIImage _cellBackground;
		public static UIImage cellBackground {
			get {
				if (_cellBackground == null)
					_cellBackground = UIImage.FromFile ("Images/texture.png");
				return _cellBackground;
			}
		}
	

		//buttons
	}
}

