using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public TagType TagType { get; set; } = TagType.Other;
        public CategoryType CategoryType { get; set; } = CategoryType.Other;
        public ICollection<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();
    }
    public enum TagType
    {
        // --- Budownictwo i remonty ---
        Murarz,
        Tynkarz,
        Malarz,
        Tapeciarz,
        Płytkarz,
        Posadzkarz,
        Cieśla,
        Dekarz,
        Elewacje,
        Docieplenia,
        KierownikBudowy,
        Architekt,
        RemontMieszkania,
        RemontŁazienki,
        RemontKuchni,

        // --- Instalacje techniczne ---
        Elektryk,
        Hydraulik,
        Gazownik,
        Klimatyzacja,
        Wentylacja,
        OgrzewaniePodłogowe,
        Fotowoltaika,
        Alarmy,
        SmartHome,
        Monitoring,

        // --- Meble i montaż ---
        MebleNaWymiar,
        MontażMebli,
        Stolarz,
        Drzwi,
        Okna,
        Parapety,
        RoletyIZasłony,

        // --- Ogród i natura ---
        Ogrodnik,
        ProjektowanieOgrodu,
        PielęgnacjaRoślin,
        SystemyNawadniania,
        KoszenieTrawy,
        Tarasy,
        Altany,
        Baseny,

        // --- Transport i logistyka ---
        TransportKrajowy,
        TransportMiędzynarodowy,
        Przeprowadzki,
        Kurier,
        Taxi,
        Dostawa,

        // --- IT i usługi zdalne ---
        Programowanie,
        TworzenieStron,
        SEO,
        Copywriting,
        Grafika,
        SocialMedia,
        MontażWideo,
        AdministracjaSystemami,
        PomocIT,
        WirtualnyAsystent,

        // --- Projektowanie i kreatywność ---
        ArchitektWnętrz,
        Grafik,
        Fotograf,
        Wideo,
        Branding,
        UXUI,
        Animacja2D3D,

        // --- Edukacja i szkolenia ---
        KorepetycjeMatematyka,
        KorepetycjeAngielski,
        KursProgramowania,
        NaukaMuzyki,
        NaukaTańca,
        SzkoleniaZawodowe,
        Tłumaczenia,

        // --- Usługi dla firm ---
        Marketing,
        Księgowość,
        KadryIPłace,
        BHP,
        SprzedażTelefoniczna,
        CallCenter,
        DoradztwoBiznesowe,

        // --- Finanse i prawo ---
        DoradcaFinansowy,
        Kredyty,
        Leasing,
        Księgowy,
        Adwokat,
        Notariusz,
        RadcaPrawny,
        DoradcaPodatkowy,

        // --- Zdrowie i uroda ---
        Fryzjer,
        Kosmetyczka,
        Masaż,
        Dietetyk,
        TrenerPersonalny,
        ManicurePedicure,
        Makijaż,
        Spa,
        Psycholog,

        // --- Motoryzacja ---
        Mechanik,
        Lakiernik,
        Myjnia,
        Detailing,
        SerwisOpon,
        Blacharz,
        Diagnosta,
        Elektryka,

        // --- Sprzątanie ---
        SprzątanieDomów,
        SprzątanieBiur,
        MycieOkien,
        CzyszczenieTapicerki,
        Ozonowanie,
        PranieDywanów,

        // --- Imprezy i rozrywka ---
        DJ,
        ZespółMuzyczny,
        FotografNaWesele,
        OrganizacjaEventu,
        Catering,
        Dekoracje,
        AnimatorDlaDzieci,

        // --- Złota rączka / naprawy ---
        NaprawyDomowe,
        MontażAGD,
        NaprawaAGD,
        NaprawaDrzwi,
        NaprawaOkien,
        Spawanie,
        Ślusarz,
        Other
    }
}
