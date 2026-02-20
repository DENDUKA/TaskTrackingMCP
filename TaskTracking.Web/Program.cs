using TaskTracking.Web.Components;
using TaskTracking.Web.Infrastructure.Data;
using TaskTracking.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register SQLite connection factory
var connectionFactory = new SqliteConnectionFactory("Data Source=tasktracking.db");
connectionFactory.InitializeDatabase();
builder.Services.AddSingleton(connectionFactory);

// Register repositories
builder.Services.AddScoped<IBoardRepository, SqliteBoardRepository>();
builder.Services.AddScoped<ITaskRepository, SqliteTaskRepository>();
builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
builder.Services.AddScoped<ICommentRepository, SqliteCommentRepository>();

// Register services
builder.Services.AddScoped<TaskTracking.Web.Services.ITaskService, TaskTracking.Web.Services.TaskService>();
builder.Services.AddScoped<TaskTracking.Web.Services.IBoardService, TaskTracking.Web.Services.BoardService>();
builder.Services.AddScoped<TaskTracking.Web.Services.IAccountService, TaskTracking.Web.Services.AccountService>();
builder.Services.AddScoped<TaskTracking.Web.Services.ICurrentUserService, TaskTracking.Web.Services.CurrentUserService>();

var app = builder.Build();

// Базовый путь для работы в подпапке IIS
app.UsePathBase("/TaskTrackingMCP");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
