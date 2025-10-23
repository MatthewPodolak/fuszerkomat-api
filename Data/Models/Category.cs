using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public CategoryType CategoryType { get; set; } = CategoryType.Other;

        public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
    }

    public enum CategoryType
    {
        BudownictwoIRemonty,
        InstalacjeTechniczne,
        MebleIMontaż,
        OgródINatura,
        TransportILogistyka,
        ITIUsługiZdalne,
        ProjektowanieIKreatywność,
        EdukacjaISzkolenia,
        UsługiDlaFirm,
        FinanseIPrawo,
        ZdrowieIUroda,
        Motoryzacja,
        Sprzątanie,
        ImprezyIRozrywka,
        ZłotaRączka,
        Other
    }
}
