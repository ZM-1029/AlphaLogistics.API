
namespace WALMS.API.Common
{
	public static class AppConstants
	{
		public static class OrderStatus
		{
			public const int Pending = 1;
			public const int Confirmed = 2;
			public const int Processing = 3;
			public const int Packed = 4;
			public const int Shipped = 5;
			public const int InTransit = 6;
			public const int OutForDelivery = 7;
			public const int Delivered = 8;
			public const int Cancelled = 9;
			public const int Returned = 10;
			public const int Refunded = 11;
		}

        public static class OrderType
		{
			public const int Delivery = 1;
			public const int Exchange = 2;

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

     
     
	
	}
}
