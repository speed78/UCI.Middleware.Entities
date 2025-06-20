using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UCI.Middleware.Entities.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Correspondents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BdsIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UciCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConventionalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorsType",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorsType", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileFullPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorrespondentScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScoreId = table.Column<int>(type: "int", nullable: false),
                    IdCompany = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileFullPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrespondentScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrespondentScores_Scores_ScoreId",
                        column: x => x.ScoreId,
                        principalTable: "Scores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimsSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InputFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    InputFileFullPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    OutputFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OutputFileFullPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ValidationError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastResponseAttemptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmissionStatusId = table.Column<int>(type: "int", nullable: false),
                    CorrespondentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimsSubmissions_Correspondents_CorrespondentId",
                        column: x => x.CorrespondentId,
                        principalTable: "Correspondents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClaimsSubmissions_SubmissionStatuses_SubmissionStatusId",
                        column: x => x.SubmissionStatusId,
                        principalTable: "SubmissionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimsErrorResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimsErrorResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimsErrorResponse_ClaimsSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ClaimsSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowErrorsResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowErrorsResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowErrorsResponse_ClaimsSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "ClaimsSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowErrorsResponse_ErrorsType_ErrorCode",
                        column: x => x.ErrorCode,
                        principalTable: "ErrorsType",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimDetailErrorsResponse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    XPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimDetailErrorsResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimDetailErrorsResponse_ClaimsErrorResponse_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "ClaimsErrorResponse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimDetailErrorsResponse_ErrorsType_ErrorCode",
                        column: x => x.ErrorCode,
                        principalTable: "ErrorsType",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Correspondents",
                columns: new[] { "Id", "BdsIdentifier", "Code", "ConventionalName", "NotificationEmail", "ReceiveNotifications", "Type", "UciCode" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440000"), "001", "001-000075", "AIG EUROPE", null, false, true, "000075" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), "002", "002-000053", "ALLIANZ", null, false, true, "000053" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), "002", "002-000039", "ALLIANZ", null, false, true, "000039" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440003"), "005", "005-000223", "AFES ITALIA", null, false, true, "000223" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440004"), "005", "005-000326", "AFES ITALIA", null, false, true, "000326" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440005"), "006", "006-000024", "AXA", null, false, true, "000024" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440006"), "007", "007-000245", "CED", null, false, true, "000245" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440007"), "009", "009-000255", "CLAIMS SERV", null, false, true, "000255" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440008"), "010", "010-000246", "CORIS", null, false, true, "000246" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440009"), "011", "011-000278", "CRAWCO", null, false, true, "000278" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440010"), "012", "012-000160", "DARAG", null, false, true, "000160" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440011"), "013", "013-000279", "DEKRA ITALIA", null, false, true, "000279" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440012"), "014", "014-000244", "GENERALI", null, false, true, "000244" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440013"), "016", "016-000078", "HDI", null, false, true, "000078" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440014"), "017", "017-000073", "HELVETIA", null, false, true, "000073" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440015"), "019", "019-000237", "IPAS", null, false, true, "000237" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440016"), "020", "020-000266", "INTEREUROPE", null, false, true, "000266" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440017"), "021", "021-000230", "INTERFIDES", null, false, true, "000230" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440018"), "022", "022-000032", "ITALIANA", null, false, true, "000032" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440019"), "022", "022-000109", "ITALIANA", null, false, true, "000109" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440020"), "022", "022-000062", "ITALIANA", null, false, true, "000062" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440021"), "023", "023-000281", "MSA UNIQA", null, false, true, "000281" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440022"), "023", "023-000210", "MSA", null, false, true, "000210" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440023"), "023", "023-000277", "MSA AIG", null, false, true, "000277" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440024"), "024", "024-000274", "T & S ITALIA", null, false, true, "000274" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440025"), "025", "025-000262", "UNIPOLSAI", null, false, true, "000262" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440026"), "027", "027-000217", "VA IT", null, false, true, "000217" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440027"), "028", "028-000108", "ZURICH", null, false, true, "000108" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440028"), "029", "029-000071", "VERTI", null, false, true, "000071" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440029"), "030", "030-000294", "DIODEA", null, false, true, "000294" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440030"), "032", "032-000296", "AVUS WCS", null, false, true, "000296" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440031"), "033", "033-000586", "ADRIATIC", null, false, true, "000586" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440032"), "UCI", "UCI-000000", "UCI", null, false, true, "000000" }
                });

            migrationBuilder.InsertData(
                table: "ErrorsType",
                columns: new[] { "Code", "Summary" },
                values: new object[,]
                {
                    { "ERRBFLU001", "Il flusso, se l'estensione è diversa da \"zip\", deve essere un file XML valido con estensione \"xml\"." },
                    { "ERRBFLU002", "Il flusso XML deve essere correttamente validato con lo schema XSD." },
                    { "ERRBFLU003", "Il flusso deve superare il controllo antivirus." },
                    { "ERRBFLU004", "Il flusso deve essere decifrabile." },
                    { "ERRBFLU005", "Il flusso deve superare la verifica della firma." },
                    { "ERRBFLU006", "In caso di flusso ZIP, deve essere presente al suo interno un file avente lo stesso nome del flusso, sostituendo l'estensione \".zip\" con \".xml\". [...]" },
                    { "ERRBFLU007", "L'impresa deve essere abilitata all'invio dei flussi verso la Banca Dati." },
                    { "ERRBFLU008", "Limite giornaliero di richieste di simulazione sinistri superato." },
                    { "ERRBFLU010", "Se l'estensione del file è \".zip\", il flusso deve essere un file ZIP valido." },
                    { "ERRBFLU011", "La dimensione del flusso XML (dopo la decompressione se archivio ZIP) non deve superare 25 MiB (26214400 byte). Per i [...]" },
                    { "ERRBISF001", "Errore interno di sistema." },
                    { "ERRNFLU009", "Tutti gli allegati presenti nell'archivizio zip devono essere dichiarati nelle richieste di sospensione, pena lo scarto dell'allegato stesso." },
                    { "ERRNSINI001", "L'ambito del sinistro deve essere coerente con il tipo sinistro, in particolare: [...]" },
                    { "ERRNSINI002", "Il codice comune o il codice provincia indicati nel luogo accadimento deve essere un codice ISTAT di comune o provincia esistente alla data di accadimento del sinistro. [...]" },
                    { "ERRNSINI003", "Il codice paese estero indicato nel luogo accadimento deve essere un codice ISO 3166-1 alpha-2 di paese esistente alla data di accadimento del sinistro, ad esclusione dell'Italia. [...]" },
                    { "ERRNSINI004", "La data di accadimento del sinistro deve essere minore o uguale alla data di accettazione del flusso." },
                    { "ERRNSINI005", "La data di denuncia deve essere maggiore o uguale alla data di accadimento e minore o uguale alla data di accettazione del flusso." },
                    { "ERRNSINI006", "La data definizione deve essere coerente con lo stato del sinistro: [...]" },
                    { "ERRNSINI007", "Se la segnalazione è di tipo inserimento, il sinistro non deve essere già presente nella base dati, tra i sinistri della compagnia, con il codice sinistro indicato." },
                    { "ERRNSINI008", "Se la segnalazione è di tipo aggiornamento, il sinistro deve essere già presente nella base dati, tra i sinistri della compagnia, con il codice sinistro indicato." },
                    { "ERRNSINI009", "La data accadimento non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    { "ERRNSINI010", "Un soggetto testimone super partes non può essere usato anche come testimone di parte e viceversa." },
                    { "ERRNSINI011", "La data denuncia non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    { "ERRNSINI012", "La data definizione, se presente, non deve essere anteriore a 40 anni prima della data di accettazione del flusso." },
                    { "ERRNSINI013", "La data definizione, se presente, deve essere minore o uguale alla data di accettazione del flusso." },
                    { "ERRNSINI015", "Se la categoria antifrode è \"3\", il sinistro deve avere uno dei seguenti stati: [...]" },
                    { "ERRNSINI016", "Se la categoria antifrode è \"4\", deve essere presente almeno un contenzioso." },
                    { "ERRNSINI017", "Il tipo di altra figura collegato direttamente al sinistro può essere solo *testimone* (tipo 9) [...]" },
                    { "ERRNSINI018", "Se il tipo del sinistro è diverso da: [...]" },
                    { "ERRNSINI019", "Il codice della compagnia assicurativa che ha richiesto l'incentivo antifrode deve essere un codice di compagnia IVASS valido." },
                    { "ERRNSINI020", "Il codice della compagnia assicurativa che ha pagato l'incentivo antifrode deve essere un codice di compagnia IVASS valido." },
                    { "ERRNSINI021", "La data di chiusura dell'incentivo antifrode deve essere maggiore della data di denuncia del sinistro." },
                    { "ERRNSINI022", "La data di chiusura dell'incentivo antifrode deve essere minore o uguale alla data di accettazione del flusso." },
                    { "ERRNSINI023", "Il codice della compagnia assicurativa che dichiara il nega evento deve essere un codice di compagnia IVASS valido." },
                    { "ERRNSINI024", "La data di definizione del nega evento deve essere maggiore o uguale alla data di accadimento del sinistro." }
                });

            migrationBuilder.InsertData(
                table: "SubmissionStatuses",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 1, "Uploaded" },
                    { 2, "SizeValidated" },
                    { 3, "SchemaValidated" },
                    { 4, "UciValidated" },
                    { 5, "Sent" },
                    { 6, "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDetailErrorsResponse_ClaimId",
                table: "ClaimDetailErrorsResponse",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimErrors_ErrorCode",
                table: "ClaimDetailErrorsResponse",
                column: "ErrorCode");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ClaimCode",
                table: "ClaimsErrorResponse",
                column: "ClaimCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimsErrorResponse_SubmissionId",
                table: "ClaimsErrorResponse",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_CorrespondentId",
                table: "ClaimsSubmissions",
                column: "CorrespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_Protocol",
                table: "ClaimsSubmissions",
                column: "Protocol");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_ResponseDate",
                table: "ClaimsSubmissions",
                column: "ResponseDate");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_SendDate",
                table: "ClaimsSubmissions",
                column: "SendDate");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_SubmissionStatusId",
                table: "ClaimsSubmissions",
                column: "SubmissionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IvassClaimsSubmissions_UploadDate",
                table: "ClaimsSubmissions",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "UX_Correspondents_Code",
                table: "Correspondents",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Correspondents_UciCode",
                table: "Correspondents",
                column: "UciCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondentScores_FileName",
                table: "CorrespondentScores",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondentScores_IdCompany",
                table: "CorrespondentScores",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondentScores_ScoreId",
                table: "CorrespondentScores",
                column: "ScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrespondentScores_ScoreId_IdCompany",
                table: "CorrespondentScores",
                columns: new[] { "ScoreId", "IdCompany" });

            migrationBuilder.CreateIndex(
                name: "UX_ErrorCodes_Code",
                table: "ErrorsType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowErrors_ErrorCode",
                table: "FlowErrorsResponse",
                column: "ErrorCode");

            migrationBuilder.CreateIndex(
                name: "IX_FlowErrorsResponse_SubmissionId",
                table: "FlowErrorsResponse",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_FileName",
                table: "Scores",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_ReceivedDate",
                table: "Scores",
                column: "ReceivedDate");

            migrationBuilder.CreateIndex(
                name: "UX_SubmissionStatus_Description",
                table: "SubmissionStatuses",
                column: "Description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimDetailErrorsResponse");

            migrationBuilder.DropTable(
                name: "CorrespondentScores");

            migrationBuilder.DropTable(
                name: "FlowErrorsResponse");

            migrationBuilder.DropTable(
                name: "ClaimsErrorResponse");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "ErrorsType");

            migrationBuilder.DropTable(
                name: "ClaimsSubmissions");

            migrationBuilder.DropTable(
                name: "Correspondents");

            migrationBuilder.DropTable(
                name: "SubmissionStatuses");
        }
    }
}
