using Nexora.Finance.CLI.Data;
using Nexora.Finance.CLI.Services;

var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nexora");
Directory.CreateDirectory(baseDir);
Directory.CreateDirectory(Path.Combine(baseDir, "exports"));
Directory.CreateDirectory(Path.Combine(baseDir, "imports"));

var dbPath = Path.Combine(baseDir, "nexora.db");
var connectionString = $"Data Source={dbPath}";

var factory = new SqliteConnectionFactory(connectionString);

DbInitializer.EnsureCreated(factory);

var repo = new TransactionRepository(factory);
var txService = new TransactionService(repo);
var fileService = new FileService();

var app = new Nexora.Finance.CLI.App(txService, fileService);
app.Run();