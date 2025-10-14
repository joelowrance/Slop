var builder = DistributedApplication.CreateBuilder(args);

var rabbitUser = builder.AddParameter("mquser", "guest");
var rabbitPass = builder.AddParameter("mqpassword", "guest");
var postgresUser = builder.AddParameter("postgresUser", "postgres");
var postgresPass = builder.AddParameter("postgresPass", "postgres");

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres", postgresUser, postgresPass, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataBindMount(@"c:\temp\VerdaViva\Postgres");


// RabbitMQ Message Broker
var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass, 5672)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataBindMount(@"c:\temp\VerdaViva\RabbitMQ")
    .WithManagementPlugin(port: 15762);

// MailHog SMTP Server for email testing
// var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
//     .WithHttpEndpoint(8025, 8025, name: "mailhog-ui")
//     .WithEndpoint(1025, 1025, name: "smtp", scheme: "tcp")
//     .WithEnvironment("MH_STORAGE", "maildir")
//     .WithEnvironment("MH_MAILDIR_PATH", "/maildir");

var coreApiDatabase = postgres.AddDatabase("verdevida-connection", "verdevida");

// Add service projects
builder.AddProject<Projects.VerdaVidaLawnCare_CoreAPI>("coreapi")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WaitFor(coreApiDatabase)
    .WithReference(coreApiDatabase);

builder.Build().Run();
