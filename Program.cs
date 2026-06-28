var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddScoped<PayrollDashboard.Repositories.IPayrollRepository, PayrollDashboard.Repositories.SqlitePayrollRepository>();
builder.Services.AddScoped<PayrollDashboard.Services.PayrollIngestionService>();
builder.Services.AddScoped<PayrollDashboard.Services.PayrollPdfParser>();
builder.Services.AddScoped<PayrollDashboard.Services.IFileStorageService, PayrollDashboard.Services.LocalFileStorageService>();
builder.Services.AddScoped<PayrollDashboard.Repositories.IAnnualIncomeRepository, PayrollDashboard.Repositories.SqliteAnnualIncomeRepository>();
builder.Services.AddScoped<PayrollDashboard.Services.AnnualIncomePdfParser>();
builder.Services.AddScoped<PayrollDashboard.Services.AnnualIncomeIngestionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
