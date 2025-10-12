var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database
var postgres = builder.AddContainer("postgres", "postgres:16")
    .WithEnvironment("POSTGRES_DB", "verdaVidaLawnCare")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEndpoint(5432, 5432, name: "postgres", scheme: "tcp");

// RabbitMQ Message Broker
var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq:3-management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithEndpoint(5672, 5672, name: "amqp", scheme: "tcp")
    .WithHttpEndpoint(15672, 15672, name: "management");

// MailHog SMTP Server for email testing
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(8025, 8025, name: "mailhog-ui")
    .WithEndpoint(1025, 1025, name: "smtp", scheme: "tcp")
    .WithEnvironment("MH_STORAGE", "maildir")
    .WithEnvironment("MH_MAILDIR_PATH", "/maildir");

// TODO: Add service projects when they are created
// builder.AddProject<Projects.VerdaVidaLawnCare_CoreAPI>("coreapi")
//     .WithReference(postgres)
//     .WithReference(rabbitmq);

// builder.AddProject<Projects.VerdaVidaLawnCare_Communications>("communications")
//     .WithReference(postgres)
//     .WithReference(rabbitmq)
//     .WithReference(mailhog);

// builder.AddProject<Projects.VerdaVidaLawnCare_Equipment>("equipment")
//     .WithReference(postgres)
//     .WithReference(rabbitmq);

builder.Build().Run();
