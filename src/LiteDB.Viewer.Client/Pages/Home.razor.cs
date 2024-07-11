using BlazorFileSaver;
using LiteDB.Engine;
using LiteDB.Viewer.Client.Models;
using LiteDB.Viewer.Client.Pages.Dialogs;
using LiteDB.Viewer.Client.Pages.Shared;
using LiteDB.Viewer.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Colors;

namespace LiteDB.Viewer.Client.Pages
{
    public partial class HomeViewModel : ViewModelBase
    {
        [Inject] protected IAppStateService AppState { get; set; } = null!;
        [Inject] protected IBlazorFileSaver FileSaver { get; set; } = null!;

        public DBCollection[] AllCollections { get; set; } = [];
        public List<DBQuery> AllQueries { get; set; } = [];
        public DBQuery? SelectedQuery { get; set; }
        public DBCollection SelectedCollection { get; set; } = null!;
        public int ActiveQueryIndex { get; set; }

        private Func<Task>? _createDatabaseAsyncCommand;
        public Func<Task> CreateDatabaseAsyncCommand => _createDatabaseAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleCreateDatabaseAsync, "Unable to create database");

        private Func<Task>? _connectDatabaseAsyncCommand;
        public Func<Task> ConnectDatabaseAsyncCommand => _connectDatabaseAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleConnectDatabaseAsync, "Unable to connect to database");

        private Func<Task>? _disconnectDatabaseAsyncCommand;
        public Func<Task> DisconnectDatabaseAsyncCommand => _disconnectDatabaseAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleDisconnectDatabaseAsync, "Unable to disconnect from database");

        private Func<DBCollection, Task>? _databaseInfoAsyncCommand;
        public Func<DBCollection, Task> DatabaseInfoAsyncCommand => _databaseInfoAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleDatabaseInfoAsync, "Unable to get database info");

        private Func<DBCollection, Task>? _importDatabaseAsyncCommand;
        public Func<DBCollection, Task> ImportDatabaseAsyncCommand => _importDatabaseAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleImportDatabaseAsync, "Unable to import database");

        private Func<DBCollection, Task>? _rebuildDatabaseAsyncCommand;
        public Func<DBCollection, Task> RebuildDatabaseAsyncCommand => _rebuildDatabaseAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleRebuildDatabaseAsync, "Unable to rebuild database");

        private Func<DBCollection, Task>? _newQueryAsyncCommand;
        public Func<DBCollection, Task> NewQueryAsyncCommand => _newQueryAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleNewQueryAsync, "Unable to create new query");

        private Func<DBCollection, Task>? _countCollectionAsyncCommand;
        public Func<DBCollection, Task> CountCollectionAsyncCommand => _countCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleCountCollectionAsync, "Unable to count collection");

        private Func<DBCollection, Task>? _explainCollectionAsyncCommand;
        public Func<DBCollection, Task> ExplainCollectionAsyncCommand => _explainCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleExplainCollectionAsync, "Unable to explain collection");

        private Func<DBCollection, Task>? _indexesCollectionAsyncCommand;
        public Func<DBCollection, Task> IndexesCollectionAsyncCommand => _indexesCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleIndexesCollectionAsync, "Unable to get indexes for collection");

        private Func<DBCollection, Task>? _exportCollectionAsyncCommand;
        public Func<DBCollection, Task> ExportCollectionAsyncCommand => _exportCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleExportCollectionAsync, "Unable to export collection");

        private Func<DBCollection, Task>? _analyzeCollectionAsyncCommand;
        public Func<DBCollection, Task> AnalyzeCollectionAsyncCommand => _analyzeCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleAnalyzeCollectionAsync, "Unable to analyze collection");

        private Func<DBCollection, Task>? _renameCollectionAsyncCommand;
        public Func<DBCollection, Task> RenameCollectionAsyncCommand => _renameCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleRenameCollectionAsync, "Unable to rename collection");

        private Func<DBCollection, Task>? _dropCollectionAsyncCommand;
        public Func<DBCollection, Task> DropCollectionAsyncCommand => _dropCollectionAsyncCommand ??= CreateEventCallbackAsyncCommand<DBCollection>(HandleDropCollectionAsync, "Unable to drop collection");

        private Func<Task>? _refreshCollectionsAsyncCommand;
        public Func<Task> RefreshCollectionsAsyncCommand => _refreshCollectionsAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleRefreshCollectionsAsync, "Unable to refresh collections");

        private Func<Task>? _runQueryAsyncCommand;
        public Func<Task> RunQueryAsyncCommand => _runQueryAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleRunQueryAsync, "Unable to run query");

        private Func<Task>? _loadQueryAsyncCommand;
        public Func<Task> LoadQueryAsyncCommand => _loadQueryAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleLoadQueryAsync, "Unable to load query");

        private Func<Task>? _saveQueryAsyncCommand;
        public Func<Task> SaveQueryAsyncCommand => _saveQueryAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleSaveQueryAsync, "Unable to save query");

        private Func<Task>? _beginTransactionAsyncCommand;
        public Func<Task> BeginTransactionAsyncCommand => _beginTransactionAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleBeginTransactionAsync, "Unable to begin transaction");

        private Func<Task>? _commitTransactionAsyncCommand;
        public Func<Task> CommitTransactionAsyncCommand => _commitTransactionAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleCommitTransactionAsync, "Unable to commit transaction");

        private Func<Task>? _rollbackTransactionAsyncCommand;
        public Func<Task> RollbackTransactionAsyncCommand => _rollbackTransactionAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleRollbackTransactionAsync, "Unable to rollback transaction");

        private Func<Task>? _checkpointTransactionAsyncCommand;
        public Func<Task> CheckpointTransactionAsyncCommand => _checkpointTransactionAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleCheckpointTransactionAsync, "Unable to checkpoint transaction");

        private Func<Task>? _addQueryAsyncCommand;
        public Func<Task> AddQueryAsyncCommand => _addQueryAsyncCommand ??= CreateEventCallbackAsyncCommand(HandleAddQueryAsync, "Unable to add query");

        private Func<MudTabPanel, Task>? _closeQueryAsyncCommand;
        public Func<MudTabPanel, Task> CloseQueryAsyncCommand => _closeQueryAsyncCommand ??= CreateEventCallbackAsyncCommand<MudTabPanel>(HandleCloseQueryAsync, "Unable to close query");

        private async Task HandleCreateDatabaseAsync()
        {
            var (success, results) = await ShowDialogAsync<CreateDatabaseDialog, DatabaseCreateResults>("Create database", new MudBlazor.DialogParameters());

            if (success && results is not null)
            {
                var settings = await BuildEngineSettingsAsync(results);
                await OpenDatabaseAsync(settings, results.Name);
                await CreateNewQueryAsync("", false);
            }
        }

        private async Task HandleConnectDatabaseAsync()
        {
            var (success, results) = await ShowDialogAsync<ConnectDatabaseDialog, DatabaseConnectResults>("Database connection", new MudBlazor.DialogParameters());

            if (success && results is not null)
            {
                var settings = await BuildEngineSettingsAsync(results);
                await OpenDatabaseAsync(settings, results.Name);
                await CreateNewQueryAsync("", false);
            }
        }

        private async Task<EngineSettings> BuildEngineSettingsAsync(DatabaseConnectResults results)
        {
            var memStream = new MemoryStream();
            await results.FileStream!.CopyToAsync(memStream);
            memStream.Position = 0;

            return new EngineSettings
            {
                DataStream = memStream,
                ReadOnly = results.IsReadOnly,
                Upgrade = results.IsUpgrade,
            };
        }

        private async Task<EngineSettings> BuildEngineSettingsAsync(DatabaseCreateResults results)
        {
            await Task.Yield();
            var memStream = new MemoryStream();

            return new EngineSettings
            {
                DataStream = memStream,
                InitialSize = results.InitialSize,
                Collation = BuildCollation(results),                
            };
        }

        private Collation BuildCollation(DatabaseCreateResults results)
        {
            if (string.IsNullOrWhiteSpace(results.Culture))
            {
                return Collation.Default;
            }

            if (string.IsNullOrWhiteSpace(results.Sort))
            {
                return new Collation(results.Culture);
            }

            var lcid = CultureInfo.GetCultureInfo(results.Culture).LCID;
            var sort = Enum.Parse<CompareOptions>(results.Sort);
            return new Collation(lcid, sort);
        }

        private async Task OpenDatabaseAsync(EngineSettings settings, string name)
        {
            await Task.Yield();
            var engine = new LiteEngine(settings);
            var db = new LiteDatabase(engine);

            AppState.IsDBOpen = true;
            AppState.Database = db;
            AppState.DatabaseStream = settings.DataStream;
            AppState.DatabaseName = name;

            await InitializeDisplayAsync();
        }

        private async Task InitializeDisplayAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            await RefreshCollectionsAsync();
            AllQueries = [];
        }

        private async Task RefreshCollectionsAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            var root = new DBCollection
            {
                Name = AppState.DatabaseName ?? "",
                Icon = Icons.Material.TwoTone.Dataset,
                IsRoot = true,
            };

            root.Children = AppState.Database!.GetCollectionNames()
                .Select(x => new DBCollection
                {
                    Name = x,
                    Icon = Icons.Material.TwoTone.DataObject,
                    IsRoot = false
                })
                .ToArray();

            AllCollections = [root];
        }

        private async Task HandleDisconnectDatabaseAsync()
        {
            await Task.Yield();
            AppState.IsDBOpen = false;
            AppState.Database!.Dispose();
            AppState.Database = null;

            AppState.DatabaseStream!.Dispose();
            AppState.DatabaseStream = null;

            AllCollections = [];
            AllQueries = [];
            SelectedQuery = null;
        }

        private async Task HandleDatabaseInfoAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = "SELECT $ FROM $database;";
            await CreateNewQueryAsync(query, true);
        }

        private async Task HandleImportDatabaseAsync(DBCollection collection)
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            var (success, results) = await ShowDialogAsync<ImportCollectionDialog, CollectionImportResults>("Import Collection");

            if (!success && results is not null)
            {
                return;
            }

            try
            {
                var items = await System.Text.Json.JsonSerializer.DeserializeAsync<BsonDocument[]>(results!.FileStream!);

                var newCollection = AppState.Database!.GetCollection(results.Name);
                newCollection.InsertBulk(items);
            }
            finally
            {
                results!.FileStream!.Dispose();
            }

            await ShowSnackbarMessageAsync($"Collection '{results.Name}' imported");
        }

        private async Task HandleRebuildDatabaseAsync(DBCollection collection)
        {
            var (success, results) = await ShowDialogAsync<RebuildDatabaseDialog, DatabaseRebuildResults>("Rebuild");

            if (!success && results is not null)
            {
                return;
            }

            var query = $"REBUILD {{ collation: '{results!.Culture}/{results.Sort}' }}";
            await CreateNewQueryAsync(query, true);

            AppState.DatabaseName = results.Name;
        }

        private async Task HandleNewQueryAsync(DBCollection collection)
        {
            await Task.Yield();
            await CreateNewQueryAsync("", false);
        }

        private async Task HandleCountCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $"SELECT COUNT(*) FROM {collection.Name};";
            await CreateNewQueryAsync(query, true);
        }

        private async Task HandleExplainCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $"EXPLAIN SELECT $ FROM {collection.Name};";
            await CreateNewQueryAsync(query, true);
        }

        private async Task HandleIndexesCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $@"SELECT $ FROM $indexes WHERE collection = ""{collection.Name}"";";
            await CreateNewQueryAsync(query, true);
        }

        private async Task HandleExportCollectionAsync(DBCollection collection)
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }
            
            var allRecords = AppState.Database!.GetCollection("").FindAll();
            var json = System.Text.Json.JsonSerializer.Serialize(allRecords);

            var name = $"{collection.Name}.json";
            await FileSaver.SaveAs(json, name);
        }

        private async Task HandleAnalyzeCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $"ANALYZE {collection.Name};";
            await CreateNewQueryAsync(query, true);
        }

        private async Task HandleRenameCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $"RENAME COLLECTION {collection.Name} TO new_name;";
            await CreateNewQueryAsync(query, false);
        }

        private async Task HandleDropCollectionAsync(DBCollection collection)
        {
            await Task.Yield();
            var query = $"DROP COLLECTION {collection.Name};";
            await CreateNewQueryAsync(query, false);
        }

        private async Task HandleRefreshCollectionsAsync()
        {
            await Task.Yield();
            await RefreshCollectionsAsync();
        }

        private async Task HandleRunQueryAsync()
        {
            if (SelectedQuery is null)
            {
                await ShowAlertAsync("No query selected");
                return;
            }

            await ExecuteQueryAsync(SelectedQuery);
        }

        private async Task HandleLoadQueryAsync()
        {
            var (success, results) = await ShowDialogAsync<ImportQueryDialog, QueryImportResults>("Import Query");

            if (!success && results is not null)
            {
                return;
            }

            try
            {
                using var reader = new StreamReader(results!.FileStream!);
                var query = await reader.ReadToEndAsync();

                await CreateNewQueryAsync(query, false);
            }
            finally
            {
                results!.FileStream!.Dispose();
            }
        }

        private async Task HandleSaveQueryAsync()
        {
            if (SelectedQuery is null)
            {
                await ShowAlertAsync("No query selected");
                return;
            }

            var name = "query.sql";
            await FileSaver.SaveAs(name, SelectedQuery.Query);
        }

        private async Task HandleBeginTransactionAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            var success = AppState.Database.BeginTrans();

            if (success)
            {
                await ShowSnackbarMessageAsync($"Transaction started");
            }            
            else
            {
                await ShowSnackbarMessageAsync($"Unable to start transaction");
            }
        }

        private async Task HandleCommitTransactionAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            var success = AppState.Database.Commit();

            if (success)
            {
                await ShowSnackbarMessageAsync($"Transaction committed");
            }
            else
            {
                await ShowSnackbarMessageAsync($"Unable to commit transaction");
            }
        }

        private async Task HandleRollbackTransactionAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            var success = AppState.Database.Rollback();

            if (success)
            {
                await ShowSnackbarMessageAsync($"Transaction rolled back");
            }
            else
            {
                await ShowSnackbarMessageAsync($"Unable to rollback transaction");
            }
        }

        private async Task HandleCheckpointTransactionAsync()
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            AppState.Database.Checkpoint();
            await ShowSnackbarMessageAsync($"Checkpoint created");            
        }

        private async Task HandleAddQueryAsync()
        {
            await Task.Yield();
            await CreateNewQueryAsync("", false);
        }

        private async Task HandleCloseQueryAsync(MudTabPanel panel)
        {
            var isConfirmed = await ConfirmActionAsync("Close query", "Close query ''? All information will be lost!");

            if (isConfirmed != true)
            {
                return;
            }

            AllQueries.RemoveAll(x => x.QueryId == (Guid)panel.ID);
        }

        private async Task CreateNewQueryAsync(string query, bool run)
        {
            await Task.Yield();
            var newQuery = new DBQuery
            {
                Query = query,
                Header = $"Query {AllQueries.Count + 1}"
            };

            AllQueries.Add(newQuery);            

            if (run)
            {
                await RefreshAsync();
                await ExecuteQueryAsync(newQuery);
            }
        }

        private async Task ExecuteQueryAsync(DBQuery query)
        {
            if (AppState.Database is null)
            {
                await ShowAlertAsync("Database is not connected");
                return;
            }

            try
            {
                using var reader = AppState.Database.Execute(query.Query);

                if (!reader.HasValues)
                {
                    await ShowSnackbarMessageAsync("No data returned");
                    return;
                }

                BuildQueryJsonResult(reader, query);
            }
            catch (LiteException ex)
            {
                query.JsonResult = ex.Message;
                //ClearGridData();
            }
            catch (Exception ex)
            {
                BuildQueryErrorResult(ex, query);
            }
        }

        private void BuildQueryJsonResult(IBsonDataReader reader, DBQuery query)
        {
            var json = new StringBuilder();
            using var writer = new StringWriter(json);

            var jsonWriter = new JsonWriter(writer)
            {
                Pretty = true,
                Indent = 2
            };

            while (reader.Read())
            {
                jsonWriter.Serialize(reader.Current);
                json.AppendLine();
            }

            query.JsonResult = json.ToString();
            //BuildGridData();
        }

        private void BuildQueryErrorResult(Exception ex, DBQuery query)
        {
            var builder = new StringBuilder();

            builder.AppendLine(ex.Message);
            builder.AppendLine();
            builder.AppendLine("===================================================");
            builder.AppendLine(ex.StackTrace);

            query.JsonResult = builder.ToString();
            //ClearGridData();
        }
    }
}
