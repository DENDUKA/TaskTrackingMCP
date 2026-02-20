using TaskTracking.Web.Components;
using TaskTracking.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories
builder.Services.AddSingleton<IBoardRepository, InMemoryBoardRepository>();
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ICommentRepository, InMemoryCommentRepository>();

// Register services
builder.Services.AddScoped<TaskTracking.Web.Services.ITaskService, TaskTracking.Web.Services.TaskService>();
builder.Services.AddScoped<TaskTracking.Web.Services.IBoardService, TaskTracking.Web.Services.BoardService>();
builder.Services.AddScoped<TaskTracking.Web.Services.IAccountService, TaskTracking.Web.Services.AccountService>();
builder.Services.AddScoped<TaskTracking.Web.Services.ICurrentUserService, TaskTracking.Web.Services.CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
