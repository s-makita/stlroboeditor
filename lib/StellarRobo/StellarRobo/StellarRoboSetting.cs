using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace StellarRobo
{
    public class StellarRoboSetting
    {
        public string Logfile;
        public string RotateType;
        public string RotateCount;
        public string HistoryCount;
        public string Dump;
        public string Mailer_Host;
        public string Mailer_Port;
        public string Mailer_User;
        public string Mailer_Password;
        public string Snmp_Host;
        public string Snmp_Port;
        public string MailAddressFrom;
        public string MailAddressTo;
        public string MailDisplayNameFrom;
        public string MailDisplayNameTo;
        public string MailSubject;
        public string EngineID;
        public string EnableSsl;

        public StellarRoboSetting()
        {
            Logfile = string.Empty;
            RotateType = string.Empty;
            RotateCount = string.Empty;
            HistoryCount = string.Empty;
            Dump = string.Empty;
            Mailer_Host = string.Empty;
            Mailer_Port = string.Empty;
            Mailer_User = string.Empty;
            Mailer_Password = string.Empty;
            Snmp_Host = string.Empty;
            Snmp_Port = string.Empty;
            MailAddressFrom = string.Empty;
            MailAddressTo = string.Empty;
            MailDisplayNameFrom = string.Empty;
            MailDisplayNameTo = string.Empty;
            MailSubject = string.Empty;
            EngineID = string.Empty;
            EnableSsl = string.Empty;
        }

        public static StellarRoboSetting LoadData(string path)
        {
            StellarRoboSetting setting = null;
            XmlSerializer serializer = new XmlSerializer(typeof(StellarRoboSetting));
            XmlReaderSettings xml_settings = new XmlReaderSettings()
            {
                CheckCharacters = false
            };

            using (StreamReader stream_reader = new StreamReader(path, Encoding.UTF8))
            using (XmlReader xml_reader = XmlReader.Create(stream_reader, xml_settings))
            {
                setting = (StellarRoboSetting)serializer.Deserialize(xml_reader);
            }

            return setting;
        }

        public static void SaveData(string path, StellarRoboSetting setting)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(StellarRoboSetting));

            using (StreamWriter stream_writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(stream_writer, setting);
                stream_writer.Flush();
            }
        }
    }
}
