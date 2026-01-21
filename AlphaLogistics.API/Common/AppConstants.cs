
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
			public const int Customer =6;
			public const int SuperAdmin =1;
			public const int Admin =3;
			public const int CustomerService =5;
			public const int Vendor =4;		
		}

     
    
	
	}
}
