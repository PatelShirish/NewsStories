using HackerNews;
using HackerNews.ConfigSettings;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(nameof(HackerService), h =>
{
    h.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
});

builder.Services.AddScoped<IHackerService, HackerService>();

builder.Services.AddMemoryCache(s => s.SizeLimit = 1);

builder.Services.AddAutoMapper(typeof(Program));

// Allow only 50 user requests in 1 minute
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Request.Headers.Host.ToString(), partition =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 50,
                AutoReplenishment = true,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

builder.Services.Configure<StoryUrlsAppSettings>(builder.Configuration.GetSection("StoryUrls"));

builder.Services.Configure<CacheAppSettings>(builder.Configuration.GetSection("Cache"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiter();

app.Run();
