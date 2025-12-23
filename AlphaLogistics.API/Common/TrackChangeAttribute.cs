namespace WALMS.API.Common
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class TrackChangeAttribute : Attribute
	{
		// You can extend this attribute with additional properties if needed,
		// but as a marker attribute, this is sufficient.
	}

}
