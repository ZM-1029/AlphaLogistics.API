namespace AlphaLogistics.API.Model
{
    public class PradeshMaster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsFixed { get; set; } = true;
        public decimal Charge { get; set; }
    }
}
