namespace Logistics.Common
{
    public enum FormDefination : int
    {
        //Account
        Role = 101,
        RolePermissions = 102,
        RoleControlPermission = 103,
        User = 104,

        //Client
        ClientMaster = 201,
        ProductMaster = 202,
        Order = 203,

        //Masters
        CityMaster = 301,
        CountryMaster = 302,
        CurrencyMaster = 303,
        DeliveryStatusMaster = 304,
        DepartmentMaster = 306,
        DesignationMaster = 307,
        InvoiceTermMaster = 308,
        LanguageMaster = 309,
        LocationMaster = 310,
        MeasurementMaster = 311,
        PostalCodeMaster = 312,
        ProductChargeCategoryMaster = 313,
        StateMaster = 314,
        VehicleSizeMaster = 315,
        VehicleMaster = 316,
        RouteMaster = 317,
        RetailerMaster = 318,
        SupplierMaster = 319,
        DepotMaster = 320,
        EmployeeRoleMaster = 321,
        EmployeeMaster = 322,
        TimedServiceMaster = 323
    }
}
