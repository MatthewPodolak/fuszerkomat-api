namespace fuszerkomat_api.VM
{
    public class OpinionFiltersVM
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public List<OpinionType>? Types { get; set; }
    }

    public enum OpinionType
    {
        Rated,
        NotRated
    }
}
