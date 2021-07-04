using System.IO;
using System.Text;

namespace MB.Winsplits.API {
  public class Utf8StringWriter : StringWriter {
    public override Encoding Encoding {
      get { return new UTF8Encoding(false); }
    }
  }
}