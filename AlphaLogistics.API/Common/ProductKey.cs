namespace WALMS.API.Common
{
	public class ProductKey
	{
		public int ProductId { get; set; }
		public int? ProductVariantId { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is ProductKey otherKey)
			{
				return this.ProductId == otherKey.ProductId && this.ProductVariantId == otherKey.ProductVariantId;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 23 + ProductId.GetHashCode();
			hash = hash * 23 + (ProductVariantId?.GetHashCode() ?? 0);
			return hash;
		}
	}

}
