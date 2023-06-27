using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthetication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

//Adicionando swagger
builder.Services.AddEndpointsApiExplorer();
//Gera o codigo da interface do swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();
LoadConfiguration(app);
// HTTPS - Secret, forcando o redirecionamento de http para https
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();
app.UseStaticFiles();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development Environment.");

    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();

void LoadConfiguration(WebApplication app)
{
    Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
    Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");
    Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");

    var smtp = new Configuration.SmtpConfiguration();
    app.Configuration.GetSection("SmtpConfiguration").Bind(smtp);
    Configuration.Smtp = smtp;
}

void ConfigureAuthetication(WebApplicationBuilder builder)
{
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    //Adicionando suporte de cache em memoria
    builder.Services.AddMemoryCache();
    //Compressao de dados para enviar ao front end
    builder.Services.AddResponseCompression(options =>
    {
        //options.Providers.Add<BrotliCompressionProvider>();
        //options.Providers.Add<CustomCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
    //Level de compressao Optimal
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });


    builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(x =>
                {
                    //Manipula as referencias - fazer com que ignore ciclos sub sequentes do objeto
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    //Quanto tiver algum valor null no objeto ele nao renderiza o objeto na tela
                    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DeafultConnection");
    //Sempre utilizar o AddDbContext para inicializar uma injecao com o DbContext
    builder.Services.AddDbContext<BlogDataContext>(options => options.UseSqlServer(connectionString));
    //Sempre vai criar uma nova instancia 
    builder.Services.AddTransient<TokenService>();
    builder.Services.AddTransient<EmailService>();
    //Dura por requisicao
    //builder.Services.AddScoped();
    //Carrega para memoria da aplicacao, sempre estara na memoria da aplicacao
    //builder.Services.AddSingleton();
}