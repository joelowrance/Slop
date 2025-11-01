using VerdaVidaLawnCare.AppHost.AspireIntegrations;
using VerdaVidaLawnCare.AppHost.AspireIntegrations.OpenTelemetryCollector;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitUser = builder.AddParameter("mquser", "guest");
var rabbitPass = builder.AddParameter("mqpassword", "guest");
var postgresUser = builder.AddParameter("postgresUser", "postgres");
var postgresPass = builder.AddParameter("postgresPass", "postgres");
var webPort = builder.AddParameter("webPort", "80");

// PostgreSQL Database
var postgres = builder.AddPostgres("postgres", postgresUser, postgresPass, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataBindMount(@"c:\temp\VerdaViva\Postgres");


// RabbitMQ Message Broker
var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass, 5672)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataBindMount(@"c:\temp\VerdaViva\RabbitMQ")
    .WithManagementPlugin(port: 15762);


var prometheus = builder.AddPrometheus("prometheus", 9090);

var grafana = builder.AddGrafana("grafana", 3000)
    .WithReference(prometheus.Resource)
    .WaitFor(prometheus);

var jaeger = builder.AddJaeger("jaeger", 16686);

var test2 =
    ReferenceExpression.Create($"{prometheus.GetEndpoint("http").Property(EndpointProperty.Url)}/api/v1/otlp");

//var test = prometheus.GetEndpoint("http");

builder.AddOpenTelemetryCollector("otelcollector", "../otelcollector/config.yaml")
    .WithEnvironment("PROMETHEUS_ENDPOINT",
        ReferenceExpression.Create($"{prometheus.GetEndpoint("http").Property(EndpointProperty.Url)}/api/v1/otlp"))
    .WithEnvironment("JAEGER_ENDPOINT",
        ReferenceExpression.Create($"{jaeger.GetEndpoint("otlp-http").Property(EndpointProperty.Url)}"))
    .WithEnvironment("NEWRELIC_ENDPOINT", "https://otlp.nr-data.net")
    .WithEnvironment("NEWRELIC_API_KEY", "939d15c405f33e29f1cba797e6e74819FFFFNRAL");


// MailHog SMTP Server for email testing
// var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
//     .WithHttpEndpoint(8025, 8025, name: "mailhog-ui")
//     .WithEndpoint(1025, 1025, name: "smtp", scheme: "tcp")
//     .WithEnvironment("MH_STORAGE", "maildir")
//     .WithEnvironment("MH_MAILDIR_PATH", "/maildir");

var coreApiDatabase = postgres.AddDatabase("verdevida-connection", "verdevida");

// add a python project
#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var pythonApi = builder.AddDockerfile("pytest", "../PythonTest", "Dockerfile")
    .WithHttpEndpoint(port: 8080, targetPort: 80)
    .WithExternalHttpEndpoints();
#pragma warning restore ASPIREHOSTINGPYTHON001

// Add service projects
var coreapi = builder.AddProject<Projects.VerdaVidaLawnCare_CoreAPI>("coreapi")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WaitFor(coreApiDatabase)
    .WithReference(coreApiDatabase)
    .WaitFor(grafana)
    .WaitFor(jaeger)
    .WithEnvironment("PROMETHEUS_ENDPOINT", test2)
    .WithReference(pythonApi.GetEndpoint("http"));

builder.AddProject<Projects.VerdaVidaLawnCare_Communications>("communications")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WaitFor(grafana)
    .WaitFor(jaeger)
    .WithEnvironment("PROMETHEUS_ENDPOINT", test2)
    .PublishAsDockerFile();

// builder.AddPythonApp("python", "../PythonTest", "app.py");
// var pythonapp = builder.AddPythonApp("instrumented-python-app", "../InstrumentedPythonProject", "app.py")
//     .WithHttpEndpoint(env: "PORT")
//     .WithEnvironment("DEBUG", "True")
//     .WithEnvironment("VIRTUAL_ENV", "AspireTestApp")
//     .WithExternalHttpEndpoints();

// Add React frontend
builder.AddNpmApp("web", "../VerdaVidaLawnCare.Web")
    .WithReference(coreapi)
    .WithEnvironment("BROWSER", "none")
    .WithEnvironment("VITE_PORT", webPort)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();




builder.Build().Run();
