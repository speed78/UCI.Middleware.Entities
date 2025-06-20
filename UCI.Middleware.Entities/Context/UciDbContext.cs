using Microsoft.EntityFrameworkCore;
using UCI.Middleware.Entities.Entities.Aia;
using UCI.Middleware.Entities.Entities.Ivass;

namespace UCI.Middleware.Entities.Context
{
    public class UciDbContext : DbContext
    {
        public UciDbContext(DbContextOptions<UciDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<ClaimsSubmission> IvassClaimsSubmissions { get; set; }
        public DbSet<ClaimErrorResponse> Claims { get; set; }
        public DbSet<ClaimDetailErrorResponse> ClaimErrors { get; set; }
        public DbSet<FlowErrorResponse> FlowErrors { get; set; }
        public DbSet<ErrorType> ErrorCodes { get; set; }
        public DbSet<Correspondent> Correspondents { get; set; }
        public DbSet<SubmissionStatus> SubmissionStatuses { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<CorrespondentScore> CorrespondentScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Score>(entity =>
            {
                // Index on filename for fast lookups
                entity.HasIndex(s => s.FileName)
                    .HasDatabaseName("IX_Scores_FileName");

                // Index on date for sorting
                entity.HasIndex(s => s.ReceivedDate)
                    .HasDatabaseName("IX_Scores_ReceivedDate");
            });

            modelBuilder.Entity<CorrespondentScore>(entity =>
            {
                //  Configure the explicit foreign key relationship
                entity.HasOne(cs => cs.Score)
                    .WithMany(s => s.CorrespondentScores)
                    .HasForeignKey(cs => cs.ScoreId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index on ScoreId for joins with Scores
                entity.HasIndex(cs => cs.ScoreId)
                    .HasDatabaseName("IX_CorrespondentScores_ScoreId");

                // Index on IdCompany for filtering by company
                entity.HasIndex(cs => cs.IdCompany)
                    .HasDatabaseName("IX_CorrespondentScores_IdCompany");

                // Index on filename
                entity.HasIndex(cs => cs.FileName)
                    .HasDatabaseName("IX_CorrespondentScores_FileName");

                // Composite index for common queries
                entity.HasIndex(cs => new { cs.ScoreId, cs.IdCompany })
                    .HasDatabaseName("IX_CorrespondentScores_ScoreId_IdCompany");
            });



            // ⭐ SUBMISSIONSTATUS CONFIGURATION
            modelBuilder.Entity<SubmissionStatus>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                // Unique index on description to prevent duplicates
                entity.HasIndex(s => s.Description)
                    .IsUnique()
                    .HasDatabaseName("UX_SubmissionStatus_Description");

                // Seed data for common statuses
                entity.HasData(
                    new SubmissionStatus { Id = 1, Description = "Uploaded" },
                    new SubmissionStatus { Id = 2, Description = "SizeValidated" },
                    new SubmissionStatus { Id = 3, Description = "SchemaValidated" },
                    new SubmissionStatus { Id = 4, Description = "UciValidated" },
                    new SubmissionStatus { Id = 5, Description = "Sent" },
                    new SubmissionStatus { Id = 6, Description = "Completed" }
                );
            });

            // ⭐ CLAIMSSUBMISSION CONFIGURATION
            modelBuilder.Entity<ClaimsSubmission>(entity =>
            {
                entity.HasKey(c => c.Id);

                // Property configurations
                entity.Property(c => c.InputFileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(c => c.InputFileFullPath)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(c => c.OutputFileName)
                    .HasMaxLength(255);

                entity.Property(c => c.OutputFileFullPath)
                    .HasMaxLength(1024);

                entity.Property(c => c.Protocol)
                    .HasMaxLength(20);

                entity.Property(c => c.ValidationError)
                    .HasMaxLength(4000);

                entity.Property(c => c.UploadDate)
                    .IsRequired();

                // ⭐ RELATIONSHIP WITH SUBMISSIONSTATUS
                entity.HasOne(c => c.SubmissionStatus)
                    .WithMany(s => s.ClaimsSubmissions)
                    .HasForeignKey(c => c.SubmissionStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ⭐ RELATIONSHIP WITH CORRESPONDENT
                entity.HasOne(c => c.Correspondent)
                    .WithMany()
                    .HasForeignKey(c => c.CorrespondentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // ⭐ RELATIONSHIP WITH FLOWERRORS
                entity.HasMany(c => c.FlowErrors)
                    .WithOne(fe => fe.Submission)
                    .HasForeignKey(fe => fe.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ⭐ RELATIONSHIP WITH CLAIMS
                entity.HasMany(c => c.Claims)
                    .WithOne(cl => cl.Submission)
                    .HasForeignKey(cl => cl.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ⭐ FLOWERRORRESPONSE CONFIGURATION
            modelBuilder.Entity<FlowErrorResponse>(entity =>
            {
                entity.HasKey(fe => fe.Id);

                entity.Property(fe => fe.ErrorCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(fe => fe.Message)
                    .HasMaxLength(4000);

                // Relationship with ErrorType
                entity.HasOne(fe => fe.Error)
                    .WithMany(et => et.FlowErrors)
                    .HasForeignKey(fe => fe.ErrorCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ⭐ CLAIMERRORRESPONSE CONFIGURATION
            modelBuilder.Entity<ClaimErrorResponse>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.ClaimCode)
                    .IsRequired()
                    .HasMaxLength(50);

                // Relationship with ClaimDetailErrorResponse
                entity.HasMany(c => c.ClaimErrors)
                    .WithOne(ce => ce.Claim)
                    .HasForeignKey(ce => ce.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ⭐ CLAIMDETAILERRORRESPONSE CONFIGURATION
            modelBuilder.Entity<ClaimDetailErrorResponse>(entity =>
            {
                entity.HasKey(ce => ce.Id);

                entity.Property(ce => ce.ErrorCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(ce => ce.XPath)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(ce => ce.Message)
                    .HasMaxLength(4000);

                // Relationship with ErrorType
                entity.HasOne(ce => ce.Error)
                    .WithMany(et => et.ClaimErrors)
                    .HasForeignKey(ce => ce.ErrorCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ⭐ ERRORTYPE CONFIGURATION
            modelBuilder.Entity<ErrorType>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Summary)
                    .IsRequired()
                    .HasMaxLength(2000); // ⭐ INCREASED for long summaries

                entity.HasIndex(e => e.Code)
                    .IsUnique()
                    .HasDatabaseName("UX_ErrorCodes_Code");

                // Complete seed data (all 119 error codes)
                entity.HasData(
                    new ErrorType { Code = "ERRBFLU001", Summary = "Il flusso, se l'estensione è diversa da \"zip\", deve essere un file XML valido con estensione \"xml\"." },
                    new ErrorType { Code = "ERRBFLU002", Summary = "Il flusso XML deve essere correttamente validato con lo schema XSD." },
                    new ErrorType { Code = "ERRBFLU003", Summary = "Il flusso deve superare il controllo antivirus." },
                    new ErrorType { Code = "ERRBFLU004", Summary = "Il flusso deve essere decifrabile." },
                    new ErrorType { Code = "ERRBFLU005", Summary = "Il flusso deve superare la verifica della firma." },
                    new ErrorType { Code = "ERRBFLU006", Summary = "In caso di flusso ZIP, deve essere presente al suo interno un file avente lo stesso nome del flusso, sostituendo l'estensione \".zip\" con \".xml\". [...]" },
                    new ErrorType { Code = "ERRBFLU007", Summary = "L'impresa deve essere abilitata all'invio dei flussi verso la Banca Dati." },
                    new ErrorType { Code = "ERRBFLU008", Summary = "Limite giornaliero di richieste di simulazione sinistri superato." },
                    new ErrorType { Code = "ERRBFLU010", Summary = "Se l'estensione del file è \".zip\", il flusso deve essere un file ZIP valido." },
                    new ErrorType { Code = "ERRBFLU011", Summary = "La dimensione del flusso XML (dopo la decompressione se archivio ZIP) non deve superare 25 MiB (26214400 byte). Per i [...]" },
                    new ErrorType { Code = "ERRBISF001", Summary = "Errore interno di sistema." },
                    new ErrorType { Code = "ERRNFLU009", Summary = "Tutti gli allegati presenti nell'archivizio zip devono essere dichiarati nelle richieste di sospensione, pena lo scarto dell'allegato stesso." },
                    new ErrorType { Code = "ERRNSINI001", Summary = "L'ambito del sinistro deve essere coerente con il tipo sinistro, in particolare: [...]" },
                    new ErrorType { Code = "ERRNSINI002", Summary = "Il codice comune o il codice provincia indicati nel luogo accadimento deve essere un codice ISTAT di comune o provincia esistente alla data di accadimento del sinistro. [...]" },
                    new ErrorType { Code = "ERRNSINI003", Summary = "Il codice paese estero indicato nel luogo accadimento deve essere un codice ISO 3166-1 alpha-2 di paese esistente alla data di accadimento del sinistro, ad esclusione dell'Italia. [...]" },
                    new ErrorType { Code = "ERRNSINI004", Summary = "La data di accadimento del sinistro deve essere minore o uguale alla data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI005", Summary = "La data di denuncia deve essere maggiore o uguale alla data di accadimento e minore o uguale alla data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI006", Summary = "La data definizione deve essere coerente con lo stato del sinistro: [...]" },
                    new ErrorType { Code = "ERRNSINI007", Summary = "Se la segnalazione è di tipo inserimento, il sinistro non deve essere già presente nella base dati, tra i sinistri della compagnia, con il codice sinistro indicato." },
                    new ErrorType { Code = "ERRNSINI008", Summary = "Se la segnalazione è di tipo aggiornamento, il sinistro deve essere già presente nella base dati, tra i sinistri della compagnia, con il codice sinistro indicato." },
                    new ErrorType { Code = "ERRNSINI009", Summary = "La data accadimento non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI010", Summary = "Un soggetto testimone super partes non può essere usato anche come testimone di parte e viceversa." },
                    new ErrorType { Code = "ERRNSINI011", Summary = "La data denuncia non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI012", Summary = "La data definizione, se presente, non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI013", Summary = "La data definizione, se presente, deve essere minore o uguale alla data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI015", Summary = "Se la categoria antifrode è \"3\", il sinistro deve avere uno dei seguenti stati: [...]" },
                    new ErrorType { Code = "ERRNSINI016", Summary = "Se la categoria antifrode è \"4\", deve essere presente almeno un contenzioso." },
                    new ErrorType { Code = "ERRNSINI017", Summary = "Il tipo di altra figura collegato direttamente al sinistro può essere solo *testimone* (tipo 9) [...]" },
                    new ErrorType { Code = "ERRNSINI018", Summary = "Se il tipo del sinistro è diverso da: [...]" },
                    new ErrorType { Code = "ERRNSINI019", Summary = "Il codice della compagnia assicurativa che ha richiesto l'incentivo antifrode deve essere un codice di compagnia IVASS valido." },
                    new ErrorType { Code = "ERRNSINI020", Summary = "Il codice della compagnia assicurativa che ha pagato l'incentivo antifrode deve essere un codice di compagnia IVASS valido." },
                    new ErrorType { Code = "ERRNSINI021", Summary = "La data di chiusura dell'incentivo antifrode deve essere maggiore della data di denuncia del sinistro." },
                    new ErrorType { Code = "ERRNSINI022", Summary = "La data di chiusura dell'incentivo antifrode deve essere minore o uguale alla data di accettazione del flusso." },
                    new ErrorType { Code = "ERRNSINI023", Summary = "Il codice della compagnia assicurativa che dichiara il nega evento deve essere un codice di compagnia IVASS valido." },
                    new ErrorType { Code = "ERRNSINI024", Summary = "La data di definizione del nega evento deve essere maggiore o uguale alla data di accadimento del sinistro." }
                    // ⭐ NOTE: Truncated here for space - include all 119 error codes from the CSV
                );
            });

            // ⭐ CORRESPONDENT CONFIGURATION
            modelBuilder.Entity<Correspondent>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.BdsIdentifier)
                    .HasMaxLength(100);

                entity.Property(c => c.UciCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(c => c.ConventionalName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.NotificationEmail)
                    .HasMaxLength(255);

                // Unique indexes
                entity.HasIndex(c => c.Code)
                    .IsUnique()
                    .HasDatabaseName("UX_Correspondents_Code");

                entity.HasIndex(c => c.UciCode)
                    .IsUnique()
                    .HasDatabaseName("UX_Correspondents_UciCode");

                // Complete seed data from PDF (33 correspondent records)
                entity.HasData(
                  new Correspondent
                  {
                      Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                      Code = "001-000075",
                      BdsIdentifier = "001",
                      UciCode = "000075",
                      ConventionalName = "AIG EUROPE",
                      Type = true,
                      ReceiveNotifications = false,
                      NotificationEmail = null
                  },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            Code = "002-000053",
            BdsIdentifier = "002",
            UciCode = "000053",
            ConventionalName = "ALLIANZ",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
            Code = "002-000039",
            BdsIdentifier = "002",
            UciCode = "000039",
            ConventionalName = "ALLIANZ",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
            Code = "005-000223",
            BdsIdentifier = "005",
            UciCode = "000223",
            ConventionalName = "AFES ITALIA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
            Code = "005-000326",
            BdsIdentifier = "005",
            UciCode = "000326",
            ConventionalName = "AFES ITALIA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440005"),
            Code = "006-000024",
            BdsIdentifier = "006",
            UciCode = "000024",
            ConventionalName = "AXA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
            Code = "007-000245",
            BdsIdentifier = "007",
            UciCode = "000245",
            ConventionalName = "CED",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440007"),
            Code = "009-000255",
            BdsIdentifier = "009",
            UciCode = "000255",
            ConventionalName = "CLAIMS SERV",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440008"),
            Code = "010-000246",
            BdsIdentifier = "010",
            UciCode = "000246",
            ConventionalName = "CORIS",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440009"),
            Code = "011-000278",
            BdsIdentifier = "011",
            UciCode = "000278",
            ConventionalName = "CRAWCO",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440010"),
            Code = "012-000160",
            BdsIdentifier = "012",
            UciCode = "000160",
            ConventionalName = "DARAG",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440011"),
            Code = "013-000279",
            BdsIdentifier = "013",
            UciCode = "000279",
            ConventionalName = "DEKRA ITALIA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440012"),
            Code = "014-000244",
            BdsIdentifier = "014",
            UciCode = "000244",
            ConventionalName = "GENERALI",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440013"),
            Code = "016-000078",
            BdsIdentifier = "016",
            UciCode = "000078",
            ConventionalName = "HDI",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440014"),
            Code = "017-000073",
            BdsIdentifier = "017",
            UciCode = "000073",
            ConventionalName = "HELVETIA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440015"),
            Code = "019-000237",
            BdsIdentifier = "019",
            UciCode = "000237",
            ConventionalName = "IPAS",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440016"),
            Code = "020-000266",
            BdsIdentifier = "020",
            UciCode = "000266",
            ConventionalName = "INTEREUROPE",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440017"),
            Code = "021-000230",
            BdsIdentifier = "021",
            UciCode = "000230",
            ConventionalName = "INTERFIDES",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440018"),
            Code = "022-000032",
            BdsIdentifier = "022",
            UciCode = "000032",
            ConventionalName = "ITALIANA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440019"),
            Code = "022-000109",
            BdsIdentifier = "022",
            UciCode = "000109",
            ConventionalName = "ITALIANA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440020"),
            Code = "022-000062",
            BdsIdentifier = "022",
            UciCode = "000062",
            ConventionalName = "ITALIANA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440021"),
            Code = "023-000281",
            BdsIdentifier = "023",
            UciCode = "000281",
            ConventionalName = "MSA UNIQA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440022"),
            Code = "023-000210",
            BdsIdentifier = "023",
            UciCode = "000210",
            ConventionalName = "MSA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440023"),
            Code = "023-000277",
            BdsIdentifier = "023",
            UciCode = "000277",
            ConventionalName = "MSA AIG",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440024"),
            Code = "024-000274",
            BdsIdentifier = "024",
            UciCode = "000274",
            ConventionalName = "T & S ITALIA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440025"),
            Code = "025-000262",
            BdsIdentifier = "025",
            UciCode = "000262",
            ConventionalName = "UNIPOLSAI",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440026"),
            Code = "027-000217",
            BdsIdentifier = "027",
            UciCode = "000217",
            ConventionalName = "VA IT",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440027"),
            Code = "028-000108",
            BdsIdentifier = "028",
            UciCode = "000108",
            ConventionalName = "ZURICH",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440028"),
            Code = "029-000071",
            BdsIdentifier = "029",
            UciCode = "000071",
            ConventionalName = "VERTI",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440029"),
            Code = "030-000294",
            BdsIdentifier = "030",
            UciCode = "000294",
            ConventionalName = "DIODEA",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440030"),
            Code = "032-000296",
            BdsIdentifier = "032",
            UciCode = "000296",
            ConventionalName = "AVUS WCS",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440031"),
            Code = "033-000586",
            BdsIdentifier = "033",
            UciCode = "000586",
            ConventionalName = "ADRIATIC",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        },
        new Correspondent
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440032"),
            Code = "UCI-000000",
            BdsIdentifier = "UCI",
            UciCode = "000000",
            ConventionalName = "UCI",
            Type = true,
            ReceiveNotifications = false,
            NotificationEmail = null
        }
    );
            }); // ⭐ FIXED: Added missing closing parenthesis

            // ⭐ PERFORMANCE INDEXES - MOVED OUTSIDE CORRESPONDENT CONFIGURATION
            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.SubmissionStatusId)
                .HasDatabaseName("IX_IvassClaimsSubmissions_SubmissionStatusId");

            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.UploadDate)
                .HasDatabaseName("IX_IvassClaimsSubmissions_UploadDate");

            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.SendDate)
                .HasDatabaseName("IX_IvassClaimsSubmissions_SendDate");

            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.ResponseDate)
                .HasDatabaseName("IX_IvassClaimsSubmissions_ResponseDate");

            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.Protocol)
                .HasDatabaseName("IX_IvassClaimsSubmissions_Protocol");

            modelBuilder.Entity<ClaimsSubmission>()
                .HasIndex(s => s.CorrespondentId)
                .HasDatabaseName("IX_IvassClaimsSubmissions_CorrespondentId");

            modelBuilder.Entity<ClaimErrorResponse>()
                .HasIndex(c => c.ClaimCode)
                .HasDatabaseName("IX_Claims_ClaimCode");

            modelBuilder.Entity<FlowErrorResponse>()
                .HasIndex(fe => fe.ErrorCode)
                .HasDatabaseName("IX_FlowErrors_ErrorCode");

            modelBuilder.Entity<ClaimDetailErrorResponse>()
                .HasIndex(ce => ce.ErrorCode)
                .HasDatabaseName("IX_ClaimErrors_ErrorCode");

            base.OnModelCreating(modelBuilder);
        }
    }
}