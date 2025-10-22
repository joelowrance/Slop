namespace VerdaVidaLawnCare.AppHost.AspireIntegrations;

public sealed class GrafanaResource(string name) : ContainerResource(name) //, IResourceWithConnectionString
{
    // Constants used to refer to well known-endpoint names, this is specific
    // for each resource type. MailDev exposes an SMTP endpoint and a HTTP
    // endpoint.
    //internal const string SmtpEndpointName = "smtp";
    internal const string HttpEndpointName = "http";

    // An EndpointReference is a core .NET Aspire type used for keeping
    // track of endpoint details in expressions. Simple literal values cannot
    // be used because endpoints are not known until containers are launched.
    //private EndpointReference? _smtpReference;

    // public EndpointReference SmtpEndpoint =>
    //     _smtpReference ??= new(this, SmtpEndpointName);

    // Required property on IResourceWithConnectionString. Represents a connection
    // string that applications can use to access the MailDev server. In this case
    // the connection string is composed of the SmtpEndpoint endpoint reference.
    // public ReferenceExpression ConnectionStringExpression =>
    //     ReferenceExpression.Create(
    //         $"smtp://{SmtpEndpoint.Property(EndpointProperty.HostAndPort)}"
    //     );
}

public static class GrafanaResourceResourceBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="MailDevResource"/> to the given
    /// <paramref name="builder"/> instance. Uses the "2.1.0" tag.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="httpPort">The HTTP port.</param>
    /// <param name="smtpPort">The SMTP port.</param>
    /// <returns>
    /// An <see cref="IResourceBuilder{MailDevResource}"/> instance that
    /// represents the added MailDev resource.
    /// </returns>
    public static IResourceBuilder<GrafanaResource> AddGrafana(
        this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null)
    {
        // The AddResource method is a core API within .NET Aspire and is
        // used by resource developers to wrap a custom resource in an
        // IResourceBuilder<T> instance. Extension methods to customize
        // the resource (if any exist) target the builder interface.
        var resource = new GrafanaResource(name);

        return builder.AddResource(resource)
            .WithImage(GrafanaResourceContainerImageTags.Image)
            .WithImageRegistry(GrafanaResourceContainerImageTags.Registry)
            .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
            .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
            .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
            .WithHttpEndpoint(
                targetPort: 3000,
                port: httpPort,
                name: PrometheusResource.HttpEndpointName);

    }

    public static IResourceBuilder<GrafanaResource> WithReference(
        this IResourceBuilder<GrafanaResource> builder,
        PrometheusResource prometheus)
    {
        return builder.WithEnvironment(context =>
        {
            var prometheusEndpoint = $"{prometheus.HttpReference}/api/v1/oltp";
            context.EnvironmentVariables["PROMETHEUS_ENDPOINT"] = prometheusEndpoint;
        });
    }
}


// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class GrafanaResourceContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "grafana/grafana";

    //internal const string Tag = "v3.2.1";
}
