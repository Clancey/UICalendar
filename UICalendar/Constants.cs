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
namespace UICalendar
{
	public static class consts
	{
		private static string[] _times;
		public static string[] times
		{
		    get
		    {
		        if (_times == null)
		        {
		            _times = new string[]
		                         {
		                             "12",
		                             "1",
		                             "2",
		                             "3",
		                             "4",
		                             "5",
		                             "6",
		                             "7",
		                             "8",
		                             "9",
		                             "10",
		                             "11",
		                             "Noon",
		                             "1",
		                             "2",
		                             "3",
		                             "4",
		                             "5",
		                             "6",
		                             "7",
		                             "8",
		                             "9",
		                             "10",
		                             "11",
		                             "12",
		                             ""
		                         };
		        }
		        return _times;
		    }
		}
		
		// Setup array consisting of string
		// representing time periods aka AM or PM
		// Matching the array of times 25 x
		private static string[] _periods;
		public static string[] periods
            {
                get
                {
                    if (_periods == null)
                    {
                        _periods = new string[]
                                       {
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "AM",
                                           "",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "PM",
                                           "AM",
                                           ""
                                       };
                    }
                    return _periods;
                }
            }
		public static float HORIZONTAL_OFFSET = 3.0f;
        public static float VERTICAL_OFFSET = 5.0f;
        public static float TIME_WIDTH = 20.0f;
        public static float PERIOD_WIDTH = 26.0f;
        public static float VERTICAL_DIFF = 45.0f;
        public static float FONT_SIZE = 14.0f;
        public static float EVENT_FONT_SIZE = 12.0f;
		
		public static float AllDayEventHeight = 25;

        public static float HORIZONTAL_LINE_DIFF = 10.0f;

        public static float TIMELINE_HEIGHT = (24*VERTICAL_OFFSET) + (23*VERTICAL_DIFF);

        public static float EVENT_VERTICAL_DIFF = 0.0f;
        public static float EVENT_HORIZONTAL_DIFF = 2.0f;
	}
}

