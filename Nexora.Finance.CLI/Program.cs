using Nexora.Finance.CLI.Data;
using Nexora.Finance.CLI.Services;

Directory.CreateDirectory("DataStore");
Directory.CreateDirectory("DataStore/exports");
Directory.CreateDirectory("DataStore/imports");

var connectionString = "Data Source=DataStore/nexora.db";
var factory = new SqliteConnectionFactory(connectionString);

DbInitializer.EnsureCreated(factory);

var repo = new TransactionRepository(factory);
var txService = new TransactionService(repo);
var fileService = new FileService();

var app = new Nexora.Finance.CLI.App(txService, fileService);
app.Run();