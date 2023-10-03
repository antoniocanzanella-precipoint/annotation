using MessagePack;
using MessagePack.Formatters;

namespace PreciPoint.Ims.Services.Annotation.MessagePack.Resolver;

/// <summary>
/// Allows querying for a formatter for serializing or deserializing of annotation dtos
/// </summary>
public class AnnotationDtoFormatterResolver : IFormatterResolver
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static readonly IFormatterResolver Instance = new AnnotationDtoFormatterResolver();

    private AnnotationDtoFormatterResolver() { }

    /// <inheritdoc />
    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T> Formatter;

        // generic's static constructor should be minimized for reduce type generation size!
        // use outer helper method.
        static FormatterCache()
        {
            Formatter = (IMessagePackFormatter<T>) BasicCustomResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }
}