using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using WALMS.API.Handler;
using WebApplication1.Models;
using Windows.Devices.Radios;

namespace WALMS.API.Common
{
	public static class AppConstants
	{
        public static class Shift
        {
            public const int General = 36;
        }
        public static class RegularizationStatus
        {
            public const int Approved =	1;
            public const int Rejected =	3;
           
        }
        public static class LeavePolicyMaster
        {
            public const int PaidLeave = 30;

        }
        public static class AppName
        {
            public const string WareHouseAPP = "Warehouse App";
            public const string DriverAPP = "Driver App";
            public const string FaceRecognationApp = "Kiosk Device";
            public const string WebAPP = "Web App";
        }
        public enum UnassignmentReason
		{ 
		  VehicleCapacity,
		  InvalidAddress,
		  RecurringRouteNotDefine,
		  MissingVahicleAssignment,
		  MissingDriverAssignment,
		  RRNotDefinedonPostal,
		  ScheduleDateNotFound,
		  UnknownIssue,
		  ManuallyUnassigned,
          RecurringNotDefineOnSpecial,
		  SpecialOrderDateNotFound,
		  InBoundScanningNotCompleted
        }
        public static class EmployeeStatus
        {
            public const int Active = 1;
            public const int Resigned = 2;
        }
        public static class Menu
        {
            public const int EmployeeHome = 104;
            public const int CustomerHome = 66;           
        }
        public static class HolidaySrc
        {
            public const int Location = 1;
            public const int Department = 2;
            public const int Designation = 3;
            public const int ShiftId = 4;
        }
        //public static class DocType
        //{
        //    public const int Document1 = 5;
        //    public const int Document2 = 6;
        //    public const int Document3 = 8;
        //    public const int Document4 = 9;
        //    public const int POI = 4;
        //    public const int POA = 7;
        //}
        public static class DocType
        {
            public const int Document1 = 5;
            public const int Document2 = 6;
            public const int Document3 = 8;
            public const int Document4 = 9;
            public const int Document5 = 17;
            public const int POI1 = 4;
            public const int POI2 = 12;
            public const int POI3 = 13;
            public const int POI4 = 15;
            public const int POA1 = 7;
            public const int POA2 = 10;
            public const int POA3 = 11;
            public const int POA4 = 14;
            public const int POA5 = 16;
        }
        public static class RateCardType
		{
			public const int ProductCategory = 1;
			public const int Surcharge = 2;
			public const int AddOn = 3;
			public const int Other = 4;
			public const int SKU = 7;
			public const int Product = 6;
			public const int Category = 5;
		}
        public static class UnitTypes
        {
            public const int KG = 1;
            public const int CBM = 2;
            public const int Item = 3;         
            public const int PerDelivery = 5;         
        }
        public static class AdditonalType
        {
            public const int Fuel = 1;
            public const int VAT = 2;           
        }
        public static class RuleTypes
        {
            public const int Fixed = 1;
            public const int Percentage = 2;
            public const int PerItem = 3;
            public const int PerKG = 4;
            public const int PerMinute = 5;
            public const int PerCBM = 6;
            public const int Range = 7;
            public const int Conditional = 8;
            public const int PerPallet = 9;
        }
        public static class DefaultNotification
		{
            public const int EmailOrderReceivedSuccessfully = 31;
            public const int EmailTemplateForgotPassword = 15;
            public const int EmailTemplatePasswordReset = 16;
           public const int EmailDeliveryFailure = 26;
            public const int EmailTemplateOrderArrival = 19;
            public const int EmailTemplateOrderCompleted = 28;
            public const int EmailTemplateOrderCancellation = 40;
            public const int EmailTemplateDeliveryDateNotification = 29;
        }

        public static class OrderType
		{
			public const int Delivery = 1;
			public const int Exchange = 2;

		}
        public static class Service
        {
            public const int Exchange = 1;
            public const int SelectDayDelivery = 27;
            public const int NextDayDelivery = 26;
            public const int SelectDayDeliveryOption = 43;   //39 for prod  //43 for test
            public const int ROC = 5;   //39 for prod  //43 for test
            public const int Unpack = 30;   //39 for prod  //43 for test
            public const int Assemble = 32;   //39 for prod  //43 for test
            public const int Collection = 49;   //39 for prod  //43 for test

        }
        public static class ServiceOptions
        {
            public const int Exchange = 1;
            public const int SelectDayDelivery = 27;
            public const int NextDayDelivery = 26;
            public const int SelectDayDeliveryOption = 43;   //39 for prod  //43 for test
            public const int ROCOption = 29;   //39 for prod  //43 for test
            public const int UnpackOption = 23;   //39 for prod  //43 for test
            public const int AssembleOption = 25;   //39 for prod  //43 for test
            public const int CollectionOption = 34;

        }
        public static class EmailTemplate
		{

			public const int LoginTemplate = 14;
			public const int OrderStatusChanged = 3;
			public const int OrderUpdated = 4;
			public const int OrderSuccessfullyPlaced = 8;
			public const int OrderAllocationAlert = 11;
			public const int OrderrecievedInWarehouse = 13;
			public const int OrderCanceled = 12;
			public const int RegisteredSuccessfully = 1;
			public const int ForgotPasswordTemplate = 15;
			public const int OrderCreatedSuccessfully = 18;
			public const int OrderArrivalTemplate = 19;
			public const int OrderRadiusTemplate = 20;
            public const int BankDetailsChange = 45;
            public const int DeliveryScheduledTemplate = 29;
            public const int DeliveryDriverTemplate = 42;
            public const int DriverArrivalTextTemplate = 32;
        }

        public static class SMSTemplate
		{
			public const int LoginTemplate = 16;

        }
		public static class Tables
		{
			public const string RecurringRouteTable = "RecurringRoute";

		}
		public static class BulkOrder
		{
			public const int WareHouseId = 11; // mohali(18) , UK(11)
			public const int UnitId = 5;
			public const int CategoryId = 3;
			public const int OrderDelivery = 1;//Delivery

		}
		public static class Status
		{
			public const int OrderCreated = 31;		
			public const int OrderRecievedIntoWarehouse = 32;
			public const int OrderCanceled = 46;
			public const int OrderConfirmed = 33;
			public const int OrderInventoryBalanced = 44;
			public const int Delivered = 40;
			public const int OrderReleased = 38;
			public const int OrderInTransit = 47;
			public const int OutforDelivery = 39;
			public const int PickupSuccess = 48;
			public const int PartialPickup = 49;
            public const int ScheduleDatenotconfirmed = 15;
            public const int ScheduleDateconfirmed = 16;
            public const int ScheduleTimeconfirmed = 17;

        }
		public static class UserRole
		{
			public const int Driver = 13;
			public const int Mate =12;		
			public const int Customer =9;
			public const int SuperAdmin =1;
			public const int Admin =15;
			public const int User =2;
			public const int AccountManager =16;
			public const int WareHouseWorker = 17;
			public const int Employee = 19;
			
		}

        public static class LoginUserInfo
        {
            public static int CreatedBy
            {
                get => UserContext.UserId != null ? Convert.ToInt32(UserContext.UserId) : 100;
            }

            public static int UpdatedBy
            {
                get => UserContext.UserId != null ? Convert.ToInt32(UserContext.UserId) : 100;
            }
        }

       /* public static class LoginUserInfo
		{
			public static int CreatedBy = UserContext.UserId != null ? Convert.ToInt32(UserContext.UserId):100;

			public static int UpdatedBy = UserContext.UserId != null ? Convert.ToInt32(UserContext.UserId) : 100;
		}*/
		public static class OrderOption
		{
			public const int FullStockControl = 8;
			public const int SoldOut = 10;
			public const int CollectionFromOtherLocation = 13;
			public const int CollectionFromSupplier = 12;
			public const int CollectionFromCustomer = 11;
			public const int CustomerDeliveringIntoDepot = 14;
			public const int SupplierDeliveringIntoDepot = 15;
			public const int Return = 12;
			public const int WareHouseLoc = 34;

		}
		public static class Alert
		{
			public const int Internal = 4;
			public const int External = 5;
			public const int Both = 6;

		}
		public static class TransactionType
		{
			public const int CheckIn = 1;
			public const int CheckOut = 2;
		
		}
		public static class ProcessType
		{
			public const int Manual = 1;
			public const int Barcode = 2;

		}

		public static class DocumentType
        {
			public const int RighttoWork = 3;
            public const int ProofofIdentity = 1;
			public const int ProofofAddress = 2;
            public const int WorkPermit = 7;
            public const int UKNaturalisationCertificate = 6;
            public const int BritishPassport = 5;
            public const int DrivingLicense = 4;

		}

		public static class LeaveStatus
		{
			public const int Approved = 1;
            public const int InProgress =2;
            public const int Rejected = 3;
            public const int Submitted = 4;
			public const int SelfCancelled = 5;
        }

	}
}
