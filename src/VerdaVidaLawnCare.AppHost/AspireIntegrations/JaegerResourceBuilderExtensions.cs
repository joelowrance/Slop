namespace VerdaVidaLawnCare.AppHost.AspireIntegrations;

public sealed class JaegerResource(string name) : ContainerResource(name)
{
    // Constants used to refer to well known-endpoint names, this is specific
    // for each resource type. Jaeger All-in-One exposes multiple endpoints.
    internal const string UiEndpointName = "ui";
    internal const string CollectorGrpcEndpointName = "collector-grpc";
    internal const string CollectorHttpEndpointName = "collector-http";
    internal const string AdminEndpointName = "admin";
    internal const string OtlpGrpcEndpointName = "otlp-grpc";
    internal const string OtlpHttpEndpointName = "otlp-http";
}

public static class JaegerResourceBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="JaegerResource"/> to the given
    /// <paramref name="builder"/> instance. Uses the "1.62" tag.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="uiPort">The UI port for accessing Jaeger web interface.</param>
    /// <returns>
    /// An <see cref="IResourceBuilder{JaegerResource}"/> instance that
    /// represents the added Jaeger resource.
    /// </returns>
    public static IResourceBuilder<JaegerResource> AddJaeger(
        this IDistributedApplicationBuilder builder,
        string name,
        int? uiPort = null)
    {
        // The AddResource method is a core API within .NET Aspire and is
        // used by resource developers to wrap a custom resource in an
        // IResourceBuilder<T> instance. Extension methods to customize
        // the resource (if any exist) target the builder interface.
        var resource = new JaegerResource(name);

        return builder.AddResource(resource)
            .WithImage(JaegerContainerImageTags.Image)
            .WithImageRegistry(JaegerContainerImageTags.Registry)
            .WithImageTag(JaegerContainerImageTags.Tag)
            .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true") // Enable OTLP to receive traces from OTel Collector
            .WithHttpEndpoint(
                targetPort: 16686,
                port: uiPort,
                name: JaegerResource.UiEndpointName)
            .WithEndpoint(
                targetPort: 14250,
                name: JaegerResource.CollectorGrpcEndpointName,
                scheme: "http")
            .WithEndpoint(
                targetPort: 14268,
                name: JaegerResource.CollectorHttpEndpointName,
                scheme: "http")
            .WithEndpoint(
                targetPort: 14269,
                name: JaegerResource.AdminEndpointName,
                scheme: "http")
            .WithEndpoint(
                targetPort: 4317,
                name: JaegerResource.OtlpGrpcEndpointName,
                scheme: "http")
            .WithEndpoint(
                targetPort: 4318,
                name: JaegerResource.OtlpHttpEndpointName,
                scheme: "http");
    }
}

// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class JaegerContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "jaegertracing/all-in-one";

    internal const string Tag = "1.74.0";
}
