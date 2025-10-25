namespace fuszerkomat_api.VM
{
    public class OpinionFiltersVM
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public Rated Rated { get; set; } = Rated.Both;
    }

    public enum Rated
    {
        True,
        False,
        Both
    }
}
