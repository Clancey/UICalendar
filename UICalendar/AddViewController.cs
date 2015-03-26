using System;
using MonoTouch.UIKit;
using MonoTouch.EventKitUI;
using MonoTouch.EventKit;
using MonoTouch.Dialog;
namespace UICalendar
{
	public class AddViewController : DialogViewController
	{
		public EKEventEditViewController addController;
		ImageStringElement taskElement;
		ImageStringElement calendarElement;
		ImageStringElement checkListElement;
		ImageStringElement phoneCallElement;
		ImageStringElement emailElement;
		ImageStringElement websiteElement;
		
		public AddViewController(UIViewController theView,DateTime duedate) : this(theView,null,0,0,duedate)
		{
			
		}		
		public AddViewController(UIViewController theView,int groupId) : this(theView,null,0,groupId,Util.DateTimeMin)
		{
			
		}
		public AddViewController (UIViewController theView, BaseItem ParentTask,long assignedID) : this(theView,ParentTask,assignedID,0,Util.DateTimeMin)
		{
			
		}
		
		public override void ViewWillAppear (bool animated)
		{			
			this.TableView.BackgroundColor = UIColor.FromPatternImage(Images.UIStockImageUnderPageBackground);
			base.ViewWillAppear (animated);
		}
		
		public AddViewController (UIViewController theView, BaseItem ParentTask,long assignedID,int groupID, DateTime duedate) : base (null,true)
		{
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered, delegate{
				this.DeactivateController(true);
			});
			
			taskElement = new ImageStringElement("Item",Images.boxImg,delegate {
				this.NavigationController.PopViewControllerAnimated(false);
				var task = new BaseItem(assignedID){
					Kind = TaskKind.Item,
					ExternalParentID = ParentTask == null ? 0: ParentTask.ExternalID ,
					ParentID = ParentTask == null ? 0: ParentTask.ID ,
					GroupID = ParentTask == null ? groupID: ParentTask.GroupID,
					OwnerID = ParentTask == null ? Settings.Sync.userID : ParentTask.OwnerID,
					DueDate = duedate
				};
				theView.NavigationController.PushViewController(new DetailViewController(task,true),true);
			});
			
			calendarElement = new ImageStringElement("Calendar Event",Images.AppleCalendar,delegate {				
				this.NavigationController.PopViewControllerAnimated(false);
				addController = new EKEventEditViewController();	
				// set the addController's event store to the current event store.
				addController.EventStore = Util.MyEventStore;
				addController.Event = EKEvent.FromStore(Util.MyEventStore);
				if(duedate.Year < 2000)
					duedate = DateTime.Today;
				addController.Event.StartDate = duedate.AddHours(DateTime.Now.Hour);
				addController.Event.EndDate = duedate.AddHours(DateTime.Now.Hour + 1);
				
				addController.Completed += delegate(object theSender, EKEventEditEventArgs eva) {
					switch (eva.Action)
					{
						case EKEventEditViewAction.Canceled :
							theView.NavigationController.DismissModalViewControllerAnimated(true);
							break;
						case EKEventEditViewAction.Deleted :
							theView.NavigationController.DismissModalViewControllerAnimated(true);
							break;
						case EKEventEditViewAction.Saved:
							theView.NavigationController.DismissModalViewControllerAnimated(true);
							break;
					}
				};
				
				theView.NavigationController.PresentModalViewController(addController,true);
			});
			
			checkListElement = new ImageStringElement("List",Images.checkListImage, delegate{	
				this.NavigationController.PopViewControllerAnimated(false);		
				var task = new CheckList(assignedID){
				Kind = TaskKind.List,
					ExternalParentID = ParentTask == null ? 0: ParentTask.ExternalID ,
					ParentID = ParentTask == null ? 0: ParentTask.ID ,
					GroupID = ParentTask == null ? 0: ParentTask.GroupID,
					OwnerID = ParentTask == null ? Settings.Sync.userID : ParentTask.OwnerID,
					DueDate = duedate
				};
				
				var taskVC = new DetailViewController(task,true);
				taskVC.TaskListSaved += savedTask => {					
						theView.NavigationController.PushViewController(new TaskViewController(savedTask.Description,true,savedTask),true);
				};
				
				theView.NavigationController.PushViewController(taskVC,true);
			});
			
			phoneCallElement = new ImageStringElement("Phone Call",Images.phoneImg, delegate{
				this.NavigationController.PopViewControllerAnimated(false);
				var task = new BaseItem(assignedID){
					Kind = TaskKind.PhoneCall,
					ExternalParentID = ParentTask == null ? 0: ParentTask.ExternalID ,
					ParentID = ParentTask == null ? 0: ParentTask.ID ,
					GroupID = ParentTask == null ? 0: ParentTask.GroupID,
					OwnerID = ParentTask == null ? Settings.Sync.userID : ParentTask.OwnerID,
					DueDate = duedate
					
				};
				theView.NavigationController.PushViewController(new DetailViewController(task,true),true);
				
			});
			
			emailElement = new ImageStringElement("Send an Email",Images.emailImg,delegate{
				this.NavigationController.PopViewControllerAnimated(false);
				var task = new BaseItem(assignedID){
					Kind = TaskKind.Email,
					ExternalParentID = ParentTask == null ? 0: ParentTask.ExternalID ,
					ParentID = ParentTask == null ? 0: ParentTask.ID ,
					GroupID = ParentTask == null ? 0: ParentTask.GroupID,
					OwnerID = ParentTask == null ? Settings.Sync.userID : ParentTask.OwnerID,
					DueDate = duedate
					
				};
				theView.NavigationController.PushViewController(new DetailViewController(task,true),true);
				
			});
			
			websiteElement = new ImageStringElement("Visit a Website",Images.globeImg,delegate{
				this.NavigationController.PopViewControllerAnimated(false);
				var task = new BaseItem(assignedID){
					Kind = TaskKind.VisitAWebsite,
					ExternalParentID = ParentTask == null ? 0: ParentTask.ExternalID ,
					ParentID = ParentTask == null ? 0: ParentTask.ID ,
					GroupID = ParentTask == null ? 0: ParentTask.GroupID,
					OwnerID = ParentTask == null ? Settings.Sync.userID : ParentTask.OwnerID,
					DueDate = duedate
					
				};
				theView.NavigationController.PushViewController(new DetailViewController(task,true),true);
				
			});
			
			
			
			Root = new RootElement("Add new item")
			{
				new Section()
				{
					taskElement,
					calendarElement,
					checkListElement,
				},
				new Section()
				{
					phoneCallElement,
					emailElement,
					websiteElement,
				}
			};
			
		///
		}
	}
}

