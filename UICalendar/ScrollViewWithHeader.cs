using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
namespace UICalendar
{
	public class ScrollViewWithHeader : UIView
    {
        private UIView _content;
        private UIScrollView _header;
        private UIView _headerContent;

        private MyScrollViewDelegate _headerDelegate;
        private RectangleF _headerFrame;
        private UIScrollView _mainContent;
        private MyScrollViewDelegate _mainContentDelegate;
        private MyScrollViewDelegate _rHeaderDelegate;
        private RectangleF _rHeaderFrame;
        private UIScrollView _rowHeader;
        private UIView _rowHeaderContent;
		public PointF ContentOffset = new PointF(0,0);
		public EventHandler Scrolled{get;set;}
		public SizeF ContentSize{get;private set;}
		public float ZoomScale { 
			get {
				return _mainContent.ZoomScale;
			}			
			set {
				_mainContent.SetZoomScale(value,false);	
				ZoomHeader();
			}
		}
		public RectangleF VisbleContentRect
		{
			get {
				var location = _mainContent.ContentOffset;
				var size = _mainContent.Bounds.Size;
				return new RectangleF(location,size);
			}
			set {
				_mainContent.ScrollRectToVisible(value,false);
				scrollHeader();				
			}
		}
		public NSAction Zoomed {get;set;}
		

        private bool isZooming;
		public ScrollViewWithHeader(IntPtr handle):base(handle)
		{
            SetupController(new RectangleF(0,0,0,0), null, null, null, false);
		}


        public ScrollViewWithHeader(RectangleF rect, UIView header, UIView content, bool enableZoom) : base(rect)
        {
            SetupController(rect, header, null, content, enableZoom);
        }

        public ScrollViewWithHeader(RectangleF rect, UIView header, UIView rowHeader, UIView content, bool enableZoom)
            : base(rect)
        {
            SetupController(rect, header, rowHeader, content, enableZoom);
        }

        public bool isMoving()
        {
            if (_mainContent.Zooming)
                return true;
            return false;
        }

        public void ScrollContents(RectangleF visibleRect, bool animate)
        {
            _mainContent.ScrollRectToVisible(visibleRect, animate);
        }
		
		public void SetZoomScale(float zoomScale, bool animate)
		{
			_mainContent.SetZoomScale(zoomScale, animate);
		}

        public void SetupController(RectangleF rect, UIView header, UIView rowHeader, UIView content, bool enableZoom)
        {
            _content = content;
            _headerContent = header;
            _rowHeaderContent = rowHeader;

            _headerDelegate = new MyScrollViewDelegate();
            _headerDelegate.theView = _headerContent;
            _rHeaderDelegate = new MyScrollViewDelegate();
            if (rowHeader != null)
                _rHeaderDelegate.theView = _rowHeaderContent;
            _mainContentDelegate = new MyScrollViewDelegate();
            _mainContentDelegate.theView = _content;

            float minZoom = .4f;
            float maxZoom = 1.3f;

            SizeF hSize = header.Frame.Size;
            SizeF cSize = content.Frame.Size;
            SizeF rSize;
            if (rowHeader != null)
                rSize = rowHeader.Frame.Size;
            else
                rSize = new SizeF(0, 0);
            //Set the content width to match the top header width
            if (hSize.Width > cSize.Width)
                cSize.Width = hSize.Width;
            else
                hSize.Width = cSize.Width;
            // Set the content height to match the
            if (rSize.Height > cSize.Height)
                cSize.Height = rSize.Height;
            else
                rSize.Height = cSize.Height;
            // Create the viewable size based off of the current frame;
            var hRect = new RectangleF(rSize.Width, 0, rect.Width - rSize.Width, hSize.Height);
            var cRect = new RectangleF(rSize.Width, hSize.Height, rect.Width - rSize.Width, rect.Height - hSize.Height);
            var rRect = new RectangleF(0, hSize.Height, rSize.Width, rect.Height - hSize.Height);
            _headerFrame = hRect;
            _rHeaderFrame = rRect;

            _header = new UIScrollView(hRect);
            _header.ContentSize = hSize;
            _header.Bounces = false;
            // Hide scroll bars on the headers
            _header.ShowsVerticalScrollIndicator = false;
            _header.ShowsHorizontalScrollIndicator = false;
            if (enableZoom)
            {
                // Sets the zoom level
                _header.MaximumZoomScale = maxZoom;
                _header.MinimumZoomScale = minZoom;
                // create a delegate to return the zoom image.
                //_header.ViewForZoomingInScrollView += delegate {return _headerContent;};
            }

            _headerDelegate.Scrolling += delegate
                                             {
                                                 if (!_mainContent.Zooming && !isZooming)
                                                 {
                                                     scrollContent();
                                                     scrollHeader();
                                                 }
                                             };
            _header.Delegate = _headerDelegate;
            _header.AddSubview(header);


            _mainContent = new UIScrollView(cRect);
            _mainContent.ContentSize = cSize;
            _mainContent.AddSubview(content);
            _mainContent.Bounces = false;
			ContentSize = cRect.Size;
            if (enableZoom)
            {
                _mainContent.MaximumZoomScale = maxZoom;
                _mainContent.MinimumZoomScale = minZoom;
                _mainContent.BouncesZoom = false;
                // create a delegate to return the zoom image.
                //_mainContent.ViewForZoomingInScrollView += delegate {return _content;};

                _mainContentDelegate.ZoomStarted += delegate
                                                        {
                                                            //Tell the class you are zooming
                                                            isZooming = true;
                                                            ZoomHeader();
                                                        };
                _mainContentDelegate.ZoomEnded += delegate
                                                      {
                                                          ZoomHeader();
                                                          isZooming = false;
                                                          // Rescroll the content to make sure it lines up with the header
															if(Zoomed != null)
																Zoomed();
                                                          //scrollContent();
                                                      };
            }

            _mainContentDelegate.Scrolling += delegate
                                                  {
                                                      scrollHeader();
                                                      ZoomHeader();
                                                      //Rescroll the content to make sure it lines up with the header
                                                      if (!_mainContent.Zooming && !isZooming)
                                                          scrollContent();
														this.ContentOffset =  _mainContent.ContentOffset;
														if(this.Scrolled != null)
															Scrolled(this,null);
                                                  };
            _mainContent.Delegate = _mainContentDelegate;


            _rowHeader = new UIScrollView(rRect);
            _rowHeader.ContentSize = rSize;
            _rowHeader.Bounces = false;
            if (enableZoom)
            {
                _rowHeader.MaximumZoomScale = maxZoom;
                _rowHeader.MinimumZoomScale = minZoom;
                //if (rowHeader != null)
                //_rowHeader.ViewForZoomingInScrollView += delegate {return _rowHeaderContent;};
            }
            // Hide scroll bars on the headers
            _rowHeader.ShowsVerticalScrollIndicator = false;
            _rowHeader.ShowsHorizontalScrollIndicator = false;
            if (rowHeader != null)
                _rowHeader.AddSubview(rowHeader);

            _rHeaderDelegate.Scrolling += delegate
                                              {
                                                  if (!_mainContent.Zooming && !isZooming)
                                                      scrollContent();
                                              };
            _rowHeader.Delegate = _rHeaderDelegate;

            AddSubview(_header);
            AddSubview(_rowHeader);
            AddSubview(_mainContent);
        }

        // Sets the content scroll to match the headers
        private void scrollContent()
        {
            PointF hOffSet = _header.ContentOffset;
            PointF cOffSet = _mainContent.ContentOffset;
            var rOffSet = new PointF(0, cOffSet.Y);
            if (_rowHeader != null)
                rOffSet = _rowHeader.ContentOffset;

            if (cOffSet.X != hOffSet.X || rOffSet.Y != cOffSet.Y)
            {
                RectangleF cFrame = _mainContent.Frame;
                cFrame.X = hOffSet.X;
                cFrame.Y = rOffSet.Y;
                _mainContent.ScrollRectToVisible(cFrame, false);
            }
        }

        // Lines the headers up with the content
        private void scrollHeader()
        {
            PointF hOffSet = _header.ContentOffset;
            PointF cOffSet = _mainContent.ContentOffset;
            var rOffSet = new PointF(0, cOffSet.Y);
            if (_rowHeader != null)
                rOffSet = _rowHeader.ContentOffset;

            if (cOffSet.X != hOffSet.X)
            {
                RectangleF hFrame = _header.Frame;
                hFrame.X = cOffSet.X;
                hFrame.Y = hOffSet.Y;
                _header.ScrollRectToVisible(hFrame, false);
            }
            if (rOffSet.Y != cOffSet.Y)
            {
                RectangleF rFrame = _rowHeader.Frame;
                rFrame.X = rFrame.X;
                rFrame.Y = cOffSet.Y;
                _rowHeader.ScrollRectToVisible(rFrame, false);
            }
        }

        // Sets the zoom level of the headers so they match the content
        private void ZoomHeader()
        {
            float scale = _mainContent.ZoomScale;
            if (scale != _header.ZoomScale)
            {
                RectangleF headerFrame = _header.Frame;
                headerFrame.Height = _headerFrame.Height*scale;

                RectangleF rHeaderFrame = _rowHeader.Frame;
                rHeaderFrame.Width = _rHeaderFrame.Width*scale;

                // Resize the frame to match the correct height
                headerFrame.X = rHeaderFrame.Width;
                headerFrame.Width = Frame.Width - rHeaderFrame.Width;
                _header.Frame = headerFrame;
                _header.SetZoomScale(scale, false);

                // resize the frame to match the corect width
                rHeaderFrame.Y = headerFrame.Height;
                rHeaderFrame.Height = Frame.Height - headerFrame.Height;
                _rowHeader.Frame = rHeaderFrame;
                _rowHeader.SetZoomScale(scale, false);

                // resize the content to take the left over area
                RectangleF mainFrame = _mainContent.Frame;
                mainFrame.Height = rHeaderFrame.Height;
                mainFrame.Width = headerFrame.Width;
                mainFrame.X = rHeaderFrame.Width;
                mainFrame.Y = headerFrame.Height;
				ContentSize = mainFrame.Size;
                _mainContent.Frame = mainFrame;
                scrollHeader();
            }
            else
            {
               // Console.WriteLine("skipped zooming");
            }
        }

        #region Nested type: MyScrollViewDelegate

        private partial class MyScrollViewDelegate : UIScrollViewDelegate
        {
            public UIView theView { get; set; }
            public NSAction Scrolling { get; set; }
            public NSAction ZoomStarted { get; set; }
            public NSAction ZoomEnded { get; set; }
			
			public MyScrollViewDelegate() :base()
			{
				
			}
			public MyScrollViewDelegate(IntPtr handle):base(handle)
			{
				
			}

            public override void Scrolled(UIScrollView scrollView)
            {
                if (Scrolling != null)
                {
                    Scrolling();
                }
            }

            public override void ZoomingStarted(UIScrollView scrollView, UIView view)
            {
                if (ZoomStarted != null)
                    ZoomStarted();
            }

            public override void ZoomingEnded(UIScrollView scrollView, UIView withView, float atScale)
            {
                if (ZoomEnded != null)
                    ZoomEnded();
            }

            public override UIView ViewForZoomingInScrollView(UIScrollView scrollView)
            {
                return theView;
            }
        }

        #endregion
    }
}
