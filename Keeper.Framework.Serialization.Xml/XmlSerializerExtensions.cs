using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;

namespace Keeper.Framework.Serialization.Xml
{
    public static class XmlSerializerExtensions
    {
        public static string SerializeToXml<T>(
            this T value, 
            string? defaultXmlNamespace = default,
            bool indent = false,
            bool omitXmlDeclaration = true)
                where T : class
        {
            var (writerSettings, serializer, ns) = GetSerializer<T>(
                defaultXmlNamespace,
                indent,
                omitXmlDeclaration,
                [new(null, defaultXmlNamespace)]
            );

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, writerSettings);

            serializer.Serialize(writer, value, ns);

            return stream.ToString();
        }

        public static string SerializeSoapRequest<T>(
            this T value,
            string? defaultXmlNamespace = default)
                where T : class =>
                   $"""
                    <?xml version="1.0" encoding="utf-8"?>
                    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
                      <soap:Body>
                        {value.SerializeToXml(defaultXmlNamespace, indent: true, omitXmlDeclaration: true).Replace(Environment.NewLine, $"{Environment.NewLine}    ")}
                      </soap:Body>
                    </soap:Envelope>
                    """;

        private static (XmlWriterSettings writerSettings, XmlSerializer serializer, XmlSerializerNamespaces ns) GetSerializer<T>(
            string? defaultXmlNamespace, 
            bool indent,
            bool omitXmlDeclaration,
            XmlQualifiedName[] namespaces)
        {
            var writerSettings = new XmlWriterSettings
            {
                Indent = indent,
                OmitXmlDeclaration = omitXmlDeclaration
            };

            var type = typeof(T);
            if (defaultXmlNamespace is null)
            {
                var contractAttribute = type.GetCustomAttribute<MessageContractAttribute>();
                if (contractAttribute is not null)
                    defaultXmlNamespace = contractAttribute.WrapperNamespace;
            }

            var ns = new XmlSerializerNamespaces(namespaces);

            return (writerSettings, new XmlSerializer(type, defaultXmlNamespace), ns);
        }

        public static IEnumerable<T> DeserializeFromXml<T>(this Stream stream, string nodeName, bool keepOpen = true)
        {
            var (serializer, reader) = GetSerializerAndReader<T>(stream, nodeName, isAsync: false, keepOpen: keepOpen);

            try
            {
                do
                {
                    if (TryDeserializeNode<T>(reader, serializer, nodeName, out var val))
                        yield return val;
                }
                while (reader.Read());
            }
            finally
            {
                reader.Dispose();
            }
        }

        public static async IAsyncEnumerable<T> DeserializeFromXmlAsync<T>(
          this Stream stream,
          string nodeName,
          bool keepOpen = true,
          [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var (serializer, reader) = GetSerializerAndReader<T>(stream, nodeName, isAsync: true, keepOpen: keepOpen);

            try
            {
                do
                {
                    if (TryDeserializeNode<T>(reader, serializer, nodeName, out var val))
                        yield return val;

                    cancellationToken.ThrowIfCancellationRequested();

                } while (await reader.ReadAsync());
            }
            finally
            {
                reader.Dispose();
            }
        }

        private static bool TryDeserializeNode<T>(XmlReader reader, XmlSerializer serializer, string nodeName, [NotNullWhen(true)] out T? val)
        {
            val = default;

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == nodeName)
            {
                using var subtreeReader = reader.ReadSubtree();

                if (serializer.Deserialize(reader) is T visitDetails)
                {
                    val = visitDetails;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        private static (XmlSerializer serializer, XmlReader reader) GetSerializerAndReader<T>(
            Stream stream, 
            string nodeName, 
            bool isAsync, 
            bool keepOpen)
        {
            var reader = XmlReader.Create(stream, new XmlReaderSettings()
            {
                Async = isAsync,
                CloseInput= !keepOpen
            });

            string? defaultXmlNamespace = default;
            reader.MoveToContent();
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == nodeName)
                {
                    defaultXmlNamespace = reader.NamespaceURI;
                    break;
                }

            if (defaultXmlNamespace is null)
                throw new InvalidOperationException("Can not figure out default xml namespace.");

            var serializer = new XmlSerializer(
              type: typeof(T),
              overrides: null,
              extraTypes: null,
              root: new XmlRootAttribute(nodeName),
              defaultNamespace: defaultXmlNamespace
            );

            return (serializer, reader);
        }
    }
}
