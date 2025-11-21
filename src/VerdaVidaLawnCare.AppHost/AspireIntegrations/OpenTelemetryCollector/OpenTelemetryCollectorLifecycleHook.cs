using System.Diagnostics;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VerdaVidaLawnCare.AppHost.AspireIntegrations.OpenTelemetryCollector;


//Based on source from https://github.com/practical-otel/opentelemetry-aspire-collector by @martinjt.
//stolen wholesale from https://github.com/dotnet/aspire-samples/blob/main/samples/Metrics/MetricsApp.AppHost/OpenTelemetryCollector/
internal static class OpenTelemetryCollectorLifecycleHook
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static Task AfterEndpointsAllocatedAsync(ResourceEndpointsAllocatedEvent evt, CancellationToken cancellationToken)
    {
        var logger = evt.Services.GetRequiredService<ResourceLoggerService>().GetLogger(evt.Resource);
        
        // Skip if this is the collector resource itself
        if (evt.Resource is OpenTelemetryCollectorResource)
        {
            return Task.CompletedTask;
        }

        // We need to find the collector from the distributed application
        // Since ResourceEndpointsAllocatedEvent doesn't have ApplicationModel, we'll use a different approach
        // Store collector reference when it's created, or find it via the services
        var appModel = evt.Services.GetRequiredService<DistributedApplicationModel>();
        var collectorResource = appModel.Resources.OfType<OpenTelemetryCollectorResource>().FirstOrDefault();
        if (collectorResource == null)
        {
            logger.LogDebug($"No {nameof(OpenTelemetryCollectorResource)} resource found, skipping telemetry configuration.");
            return Task.CompletedTask;
        }

        var endpoint = collectorResource.GetEndpoint(OpenTelemetryCollectorResource.OtlpGrpcEndpointName);
        if (!endpoint.Exists)
        {
            logger.LogDebug($"No {OpenTelemetryCollectorResource.OtlpGrpcEndpointName} endpoint for the collector, skipping telemetry configuration.");
            return Task.CompletedTask;
        }

        // Check if we've already added our callback
        var existingCallbacks = evt.Resource.Annotations.OfType<EnvironmentCallbackAnnotation>().ToList();
        if (existingCallbacks.Any())
        {
            return Task.CompletedTask;
        }

        evt.Resource.Annotations.Add(new EnvironmentCallbackAnnotation((EnvironmentCallbackContext context) =>
        {
            if (context.EnvironmentVariables.ContainsKey(OtelExporterOtlpEndpoint))
            {
                logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector.", evt.Resource.Name);

                context.EnvironmentVariables[OtelExporterOtlpEndpoint] = endpoint;
            }
        }));

        return Task.CompletedTask;
    }
}

public class OpenTelemetryCollectorResource(string name) : ContainerResource(name)
{
    internal const string OtlpGrpcEndpointName = "grpc";
    internal const string OtlpHttpEndpointName = "http";
}


public static class OpenTelemetryCollectorResourceBuilderExtensions
{
    private const string DashboardOtlpUrlVariableName = "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL";
    private const string DashboardOtlpApiKeyVariableName = "AppHost:OtlpApiKey";
    private const string DashboardOtlpUrlDefaultValue = "http://localhost:18889";
    private const string OTelCollectorImageName = "ghcr.io/open-telemetry/opentelemetry-collector-releases/opentelemetry-collector-contrib";
    private const string OTelCollectorImageTag = "0.123.0";

    public static IResourceBuilder<OpenTelemetryCollectorResource> AddOpenTelemetryCollector(this IDistributedApplicationBuilder builder, string name, string configFileLocation)
    {
        builder.AddOpenTelemetryCollectorInfrastructure();

        var url = builder.Configuration[DashboardOtlpUrlVariableName] ?? DashboardOtlpUrlDefaultValue;
        var isHttpsEnabled = url.StartsWith("https", StringComparison.OrdinalIgnoreCase);

        var dashboardOtlpEndpoint = new HostUrl(url);

        var resource = new OpenTelemetryCollectorResource(name);
        var resourceBuilder = builder.AddResource(resource)
            .WithImage(OTelCollectorImageName, OTelCollectorImageTag)
            .WithEndpoint(targetPort: 4317, name: OpenTelemetryCollectorResource.OtlpGrpcEndpointName, scheme: isHttpsEnabled ? "https" : "http")
            .WithEndpoint(targetPort: 4318, name: OpenTelemetryCollectorResource.OtlpHttpEndpointName, scheme: isHttpsEnabled ? "https" : "http")
            .WithBindMount(configFileLocation, "/etc/otelcol-contrib/config.yaml")
            .WithEnvironment("ASPIRE_ENDPOINT", $"{dashboardOtlpEndpoint}")
            .WithEnvironment("ASPIRE_API_KEY", builder.Configuration[DashboardOtlpApiKeyVariableName])
            .WithEnvironment("ASPIRE_INSECURE", isHttpsEnabled ? "false" : "true");

        if (isHttpsEnabled && builder.ExecutionContext.IsRunMode && builder.Environment.IsDevelopment())
        {
            DevCertHostingExtensions.RunWithHttpsDevCertificate(resourceBuilder, "HTTPS_CERT_FILE", "HTTPS_CERT_KEY_FILE", (_, __) =>
            {
                // Set TLS details using YAML path via the command line. This allows the values to be added to the existing config file.
                // Setting the values in the config file doesn't work because adding the "tls" section always enables TLS, even if there is no cert provided.
                resourceBuilder.WithArgs(
                    @"--config=yaml:receivers::otlp::protocols::grpc::tls::cert_file: ""dev-certs/dev-cert.pem""",
                    @"--config=yaml:receivers::otlp::protocols::grpc::tls::key_file: ""dev-certs/dev-cert.key""",
                    @"--config=/etc/otelcol-contrib/config.yaml");
            });
        }

        return resourceBuilder;
    }
}

internal static class OpenTelemetryCollectorServiceExtensions
{
    public static IDistributedApplicationBuilder AddOpenTelemetryCollectorInfrastructure(this IDistributedApplicationBuilder builder)
    {
        builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>(async (evt, ct) => 
            await OpenTelemetryCollectorLifecycleHook.AfterEndpointsAllocatedAsync(evt, ct));

        return builder;
    }
}
public static class DevCertHostingExtensions
{
    /// <summary>
    /// Injects the ASP.NET Core HTTPS developer certificate into the resource via the specified environment variables when
    /// <paramref name="builder"/>.<see cref="IResourceBuilder{T}.ApplicationBuilder">ApplicationBuilder</see>.<see cref="IDistributedApplicationBuilder.ExecutionContext">ExecutionContext</see>.<see cref="DistributedApplicationExecutionContext.IsRunMode">IsRunMode</see><c> == true</c>.<br/>
    /// If the resource is a <see cref="ContainerResource"/>, the certificate files will be bind mounted into the container.
    /// </summary>
    /// <remarks>
    /// This method <strong>does not</strong> configure an HTTPS endpoint on the resource.
    /// Use <see cref="ResourceBuilderExtensions.WithHttpsEndpoint{TResource}"/> to configure an HTTPS endpoint.
    /// </remarks>
    public static IResourceBuilder<TResource> RunWithHttpsDevCertificate<TResource>(
        this IResourceBuilder<TResource> builder, string certFileEnv, string certKeyFileEnv, Action<string, string>? onSuccessfulExport = null)
        where TResource : IResourceWithEnvironment
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode && builder.ApplicationBuilder.Environment.IsDevelopment())
        {
            builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>(async (e, ct) =>
            {
                var logger = e.Services.GetRequiredService<ResourceLoggerService>().GetLogger(builder.Resource);

                // Export the ASP.NET Core HTTPS development certificate & private key to files and configure the resource to use them via
                // the specified environment variables.
                var (exported, certPath, certKeyPath) = await TryExportDevCertificateAsync(builder.ApplicationBuilder, logger);

                if (!exported)
                {
                    // The export failed for some reason, don't configure the resource to use the certificate.
                    return;
                }

                if (builder.Resource is ContainerResource containerResource)
                {
                    // Bind-mount the certificate files into the container.
                    const string DEV_CERT_BIND_MOUNT_DEST_DIR = "/dev-certs";

                    var certFileName = Path.GetFileName(certPath);
                    var certKeyFileName = Path.GetFileName(certKeyPath);

                    var bindSource = Path.GetDirectoryName(certPath) ?? throw new UnreachableException();

                    var certFileDest = $"{DEV_CERT_BIND_MOUNT_DEST_DIR}/{certFileName}";
                    var certKeyFileDest = $"{DEV_CERT_BIND_MOUNT_DEST_DIR}/{certKeyFileName}";

                    builder.ApplicationBuilder.CreateResourceBuilder(containerResource)
                        .WithBindMount(bindSource, DEV_CERT_BIND_MOUNT_DEST_DIR, isReadOnly: true)
                        .WithEnvironment(certFileEnv, certFileDest)
                        .WithEnvironment(certKeyFileEnv, certKeyFileDest);
                }
                else
                {
                    builder
                        .WithEnvironment(certFileEnv, certPath)
                        .WithEnvironment(certKeyFileEnv, certKeyPath);
                }

                if (onSuccessfulExport is not null)
                {
                    onSuccessfulExport(certPath, certKeyPath);
                }
            });
        }

        return builder;
    }

    private static async Task<(bool, string CertFilePath, string CertKeyFilPath)> TryExportDevCertificateAsync(IDistributedApplicationBuilder builder, ILogger logger)
    {
        // Exports the ASP.NET Core HTTPS development certificate & private key to PEM files using 'dotnet dev-certs https' to a temporary
        // directory and returns the path.
        // TODO: Check if we're running on a platform that already has the cert and key exported to a file (e.g. macOS) and just use those instead.
        var appNameHash = builder.Configuration["AppHost:Sha256"]![..10];
        var tempDir = Path.Combine(Path.GetTempPath(), $"aspire.{appNameHash}");
        var certExportPath = Path.Combine(tempDir, "dev-cert.pem");
        var certKeyExportPath = Path.Combine(tempDir, "dev-cert.key");

        if (File.Exists(certExportPath) && File.Exists(certKeyExportPath))
        {
            // Certificate already exported, return the path.
            logger.LogDebug("Using previously exported dev cert files '{CertPath}' and '{CertKeyPath}'", certExportPath, certKeyExportPath);
            return (true, certExportPath, certKeyExportPath);
        }

        if (File.Exists(certExportPath))
        {
            logger.LogTrace("Deleting previously exported dev cert file '{CertPath}'", certExportPath);
            File.Delete(certExportPath);
        }

        if (File.Exists(certKeyExportPath))
        {
            logger.LogTrace("Deleting previously exported dev cert key file '{CertKeyPath}'", certKeyExportPath);
            File.Delete(certKeyExportPath);
        }

        if (!Directory.Exists(tempDir))
        {
            logger.LogTrace("Creating directory to export dev cert to '{ExportDir}'", tempDir);
            Directory.CreateDirectory(tempDir);
        }

        string[] args = ["dev-certs", "https", "--export-path", $"\"{certExportPath}\"", "--format", "Pem", "--no-password"];
        var argsString = string.Join(' ', args);

        logger.LogTrace("Running command to export dev cert: {ExportCmd}", $"dotnet {argsString}");
        var exportStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = argsString,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        var exportProcess = new Process { StartInfo = exportStartInfo };

        Task? stdOutTask = null;
        Task? stdErrTask = null;

        try
        {
            try
            {
                if (exportProcess.Start())
                {
                    stdOutTask = ConsumeOutput(exportProcess.StandardOutput, msg => logger.LogInformation("> {StandardOutput}", msg));
                    stdErrTask = ConsumeOutput(exportProcess.StandardError, msg => logger.LogError("! {ErrorOutput}", msg));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start HTTPS dev certificate export process");
                return default;
            }

            var timeout = TimeSpan.FromSeconds(5);
            var exited = exportProcess.WaitForExit(timeout);

            if (exited && File.Exists(certExportPath) && File.Exists(certKeyExportPath))
            {
                logger.LogDebug("Dev cert exported to '{CertPath}' and '{CertKeyPath}'", certExportPath, certKeyExportPath);
                return (true, certExportPath, certKeyExportPath);
            }

            if (exportProcess.HasExited && exportProcess.ExitCode != 0)
            {
                logger.LogError("HTTPS dev certificate export failed with exit code {ExitCode}", exportProcess.ExitCode);
            }
            else if (!exportProcess.HasExited)
            {
                exportProcess.Kill(true);
                logger.LogError("HTTPS dev certificate export timed out after {TimeoutSeconds} seconds", timeout.TotalSeconds);
            }
            else
            {
                logger.LogError("HTTPS dev certificate export failed for an unknown reason");
            }
            return default;
        }
        finally
        {
            await Task.WhenAll(stdOutTask ?? Task.CompletedTask, stdErrTask ?? Task.CompletedTask);
        }

        static async Task ConsumeOutput(TextReader reader, Action<string> callback)
        {
            char[] buffer = new char[256];
            int charsRead;

            while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                callback(new string(buffer, 0, charsRead));
            }
        }
    }
}
