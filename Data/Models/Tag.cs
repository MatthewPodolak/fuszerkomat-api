using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public TagType TagType { get; set; } = TagType.Other;

        public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
    }
    public enum TagType
    {
        BudowaDomu,
        Elektryk,
        Hydraulik,
        Malarz,
        MebleIZabudowa,
        Motoryzacja,
        Ogrod,
        OrganizacjaImprez,
        Projektowanie,
        Remont,
        Sprzatanie,
        SzkoleniaIJęzykiObce,
        Transport,
        UsługiDlaBiznesu,
        MontazINaprawa,
        UsługiFinansowe,
        UsługiPrawneIAdministracyjne,
        UsługiZdalne,
        ZdrowieIUroda,
        ZlotaRaczka,
        Other
    }
}
