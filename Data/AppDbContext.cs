using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Data.Models.Token;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Opinion> Opinions => Set<Opinion>();
        public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
        public DbSet<WorkTaskGallery> WorkTaskGalleries => Set<WorkTaskGallery>();
        public DbSet<TaskApplication> TaskApplications => Set<TaskApplication>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
               .HasOne(a => a.UserProfile)
               .WithOne(a => a.AppUser)
               .HasForeignKey<UserProfile>(s => s.AppUserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
               .HasOne(a => a.CompanyProfile)
               .WithOne(a => a.AppUser)
               .HasForeignKey<CompanyProfile>(s => s.AppUserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyProfile>()
                .HasOne(c => c.Address)
                .WithOne(a => a.CompanyProfile)
                .HasForeignKey<Address>(a => a.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.CompanyUser)
                .WithMany(u => u.Opinions)
                .HasForeignKey(o => o.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.AuthorUser)
                .WithMany()
                .HasForeignKey(o => o.AuthorUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Opinion>()
                .HasOne(o => o.WorkTask)
                .WithOne(t => t.Opinion)
                .HasForeignKey<Opinion>(o => o.WorkTaskId);

            modelBuilder.Entity<WorkTask>()
                .HasMany(a => a.Tags)
                .WithMany(a => a.WorkTasks)
                .UsingEntity(j => j.ToTable("WotkTaskTags"));

            modelBuilder.Entity<WorkTask>()
                .HasOne(w => w.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(w => w.CreatedByUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskApplication>()
                .HasOne(a => a.WorkTask)
                .WithMany(t => t.Applications)
                .HasForeignKey(a => a.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskApplication>()
                .HasOne(a => a.CompanyUser)
                .WithMany(u => u.CompanyApplications)
                .HasForeignKey(a => a.CompanyUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompanyProfile>()
                .HasMany(c => c.Realizations)
                .WithOne(r => r.CompanyProfile)
                .HasForeignKey(r => r.CompanyProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTaskGallery>()
                .HasOne(g => g.WorkTask)
                .WithMany(t => t.Images)
                .HasForeignKey(g => g.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTask>()
                .HasOne(w => w.Category)
                .WithMany(c => c.WorkTasks)
                .HasForeignKey(w => w.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.UserId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.CreatedAtUtc);

            modelBuilder.Entity<TaskApplication>()
                .HasIndex(a => new { a.WorkTaskId, a.CompanyUserId })
                .IsUnique();

            modelBuilder.Entity<CompanyProfile>()
                .HasIndex(o => new { o.Email })
                .IsUnique();

            modelBuilder.Entity<UserProfile>()
                .HasIndex(o => new { o.Email })
                .IsUnique();

            var categories = Enum.GetValues(typeof(CategoryType))
                .Cast<CategoryType>()
                .Select((type, i) => new Category { Id = i + 1, CategoryType = type })
                .ToArray();

            modelBuilder.Entity<Category>().HasData(categories);

            int tagId = 1;
            var tags = new List<Tag>
            {
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Murarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Tynkarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Malarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Tapeciarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Płytkarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Posadzkarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Cieśla },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Dekarz },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Elewacje },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Docieplenia },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.KierownikBudowy },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.Architekt },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.RemontMieszkania },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.RemontŁazienki },
                new() { Id = tagId++, CategoryType = CategoryType.BudownictwoIRemonty, TagType = TagType.RemontKuchni },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Elektryk },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Hydraulik },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Gazownik },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Klimatyzacja },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Wentylacja },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.OgrzewaniePodłogowe },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Fotowoltaika },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Alarmy },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.SmartHome },
                new() { Id = tagId++, CategoryType = CategoryType.InstalacjeTechniczne, TagType = TagType.Monitoring },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.MebleNaWymiar },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.MontażMebli },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.Stolarz },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.Drzwi },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.Okna },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.Parapety },
                new() { Id = tagId++, CategoryType = CategoryType.MebleIMontaż, TagType = TagType.RoletyIZasłony },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.Ogrodnik },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.ProjektowanieOgrodu },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.PielęgnacjaRoślin },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.SystemyNawadniania },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.KoszenieTrawy },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.Tarasy },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.Altany },
                new() { Id = tagId++, CategoryType = CategoryType.OgródINatura, TagType = TagType.Baseny },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.TransportKrajowy },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.TransportMiędzynarodowy },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.Przeprowadzki },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.Kurier },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.Taxi },
                new() { Id = tagId++, CategoryType = CategoryType.TransportILogistyka, TagType = TagType.Dostawa },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.Programowanie },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.TworzenieStron },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.SEO },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.Copywriting },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.Grafika },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.SocialMedia },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.MontażWideo },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.AdministracjaSystemami },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.PomocIT },
                new() { Id = tagId++, CategoryType = CategoryType.ITIUsługiZdalne, TagType = TagType.WirtualnyAsystent },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.ArchitektWnętrz },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.Grafik },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.Fotograf },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.Wideo },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.Branding },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.UXUI },
                new() { Id = tagId++, CategoryType = CategoryType.ProjektowanieIKreatywność, TagType = TagType.Animacja2D3D },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.KorepetycjeMatematyka },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.KorepetycjeAngielski },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.KursProgramowania },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.NaukaMuzyki },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.NaukaTańca },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.SzkoleniaZawodowe },
                new() { Id = tagId++, CategoryType = CategoryType.EdukacjaISzkolenia, TagType = TagType.Tłumaczenia },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.Marketing },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.Księgowość },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.KadryIPłace },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.BHP },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.SprzedażTelefoniczna },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.CallCenter },
                new() { Id = tagId++, CategoryType = CategoryType.UsługiDlaFirm, TagType = TagType.DoradztwoBiznesowe },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.DoradcaFinansowy },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.Kredyty },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.Leasing },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.Księgowy },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.Adwokat },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.Notariusz },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.RadcaPrawny },
                new() { Id = tagId++, CategoryType = CategoryType.FinanseIPrawo, TagType = TagType.DoradcaPodatkowy },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Fryzjer },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Kosmetyczka },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Masaż },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Dietetyk },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.TrenerPersonalny },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.ManicurePedicure },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Makijaż },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Spa },
                new() { Id = tagId++, CategoryType = CategoryType.ZdrowieIUroda, TagType = TagType.Psycholog },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Mechanik },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Lakiernik },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Myjnia },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Detailing },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.SerwisOpon },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Blacharz },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Diagnosta },
                new() { Id = tagId++, CategoryType = CategoryType.Motoryzacja, TagType = TagType.Elektryk },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.SprzątanieDomów },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.SprzątanieBiur },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.MycieOkien },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.CzyszczenieTapicerki },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.Ozonowanie },
                new() { Id = tagId++, CategoryType = CategoryType.Sprzątanie, TagType = TagType.PranieDywanów },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.DJ },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.ZespółMuzyczny },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.FotografNaWesele },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.OrganizacjaEventu },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.Catering },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.Dekoracje },
                new() { Id = tagId++, CategoryType = CategoryType.ImprezyIRozrywka, TagType = TagType.AnimatorDlaDzieci },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.NaprawyDomowe },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.MontażAGD },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.NaprawaAGD },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.NaprawaDrzwi },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.NaprawaOkien },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.Spawanie },
                new() { Id = tagId++, CategoryType = CategoryType.ZłotaRączka, TagType = TagType.Ślusarz },
                new() { Id = tagId++, CategoryType = CategoryType.Other, TagType = TagType.Other }
            };
            modelBuilder.Entity<Tag>().HasData(tags);

            base.OnModelCreating(modelBuilder);
        }
    }
}
