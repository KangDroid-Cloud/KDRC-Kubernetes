using k8s;
using KDRC_Kubernetes.Services.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Kubernetes Service
var kubeConfig = KubernetesClientConfiguration.InClusterConfig();
builder.Services.AddSingleton<IKubernetes>(new Kubernetes(kubeConfig));

// Add MassTransit
builder.Services.AddMassTransit(a =>
{
    a.AddConsumer<AccountCreatedConsumer>();

    a.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], builder.Configuration["RabbitMq:VirtualHost"], h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("account.created:KubernetesConsumer", endpointConfig =>
        {
            endpointConfig.Bind("account.created");
            endpointConfig.ConfigureConsumer<AccountCreatedConsumer>(ctx);
        });
    });
});

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

app.Run();