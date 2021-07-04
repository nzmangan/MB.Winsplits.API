using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MB.Winsplits.API {
  public static class XMLSerilizer {
    public static T Deserialize<T>(byte[] rawFile) {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      string utf8String = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding(1252), Encoding.UTF8, rawFile));
      return Deserialize<T>(utf8String);
    }

    public static T Deserialize<T>(string input) {
      using (StringReader stream = new(input)) {
        using (var reader = XmlReader.Create(stream)) {
          return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
        }
      }
    }

    public static string SerializeObject<T>(T toSerialize) {
      using (Utf8StringWriter textWriter = new()) {
        new XmlSerializer(typeof(T)).Serialize(textWriter, toSerialize);
        return textWriter.ToString();
      }
    }
  }
}