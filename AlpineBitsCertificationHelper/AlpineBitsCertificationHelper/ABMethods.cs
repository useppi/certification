using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace AlpineBitsTestClient
{
    static class ABMethods
    {
        public static AlpineBitsServer GetServerCredentials()
        {
            return new AlpineBitsServer()
            {
                UserName = ABMethods.getAppSetting("Username"),
                Password = ABMethods.getAppSetting("Password"),
                X_AlpineBits_ClientID = ABMethods.getAppSetting("ClientID"),
                X_AlpineBits_ProtocolVersion = ABMethods.getAppSetting("ProtocolVersion"),
                ServerURL = ABMethods.getAppSetting("ServerUrl"),
                AcceptResponseGZIPEncoded = ABMethods.getAppSetting("BGZIPReturn") == "yes" ? true : false,
                InvokeZipped = ABMethods.getAppSetting("BGZIPSend") == "yes" ? true : false,
                HotelCode = ABMethods.getAppSetting("HotelCode")
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="XML"></param>
        /// <param name="Request">true = validate OTA Request XSD, false=validate OTA Response XSD</param>
        /// <param name="sXSDValidationErrorString"></param>
        /// <param name=""></param>

        public static void XSD_Validation(string action, string XML, bool RQXSD, out string sXSDValidationErrorString)
        {
            var xsdFile = "";
            sXSDValidationErrorString = "";
            var ABServer = ABMethods.GetServerCredentials();
            if (ABServer.X_AlpineBits_ProtocolVersion == "2014-04")
                ABMethods.ValidateXSD(XML, "./xsd/alpinebits2014-04.xsd", out sXSDValidationErrorString);
            if (ABServer.X_AlpineBits_ProtocolVersion == "2015-07")
                ABMethods.ValidateXSD(XML, "./xsd/alpinebits2015-07.xsd", out sXSDValidationErrorString);
            if (ABServer.X_AlpineBits_ProtocolVersion == "2015-07b")
                ABMethods.ValidateXSD(XML, "./xsd/alpinebits2015-07b.xsd", out sXSDValidationErrorString);
            if (sXSDValidationErrorString.Length > 0)
                MessageBox.Show("AlpineBits XSD Error: " + sXSDValidationErrorString);
            else
                MessageBox.Show("AlpineBits " + ABServer.X_AlpineBits_ProtocolVersion + " OK!");

            if (ABServer.X_AlpineBits_ProtocolVersion.Trim() != "2014-04")
            {

                switch (action)
                {

                    case "OTA_HotelDescriptiveContentNotif:Inventory":
                        if (RQXSD)
                            // FOR REQUEST VALIDATION
                            xsdFile = "./xsd/OTA2015A/OTA_HotelDescriptiveContentNotifRQ.xsd";
                        else
                            // FOR RESPONSE VALIDATION
                            xsdFile = "./xsd/OTA2015A/OTA_HotelDescriptiveContentNotifRS.xsd";
                        break;
                    case "OTA_HotelRatePlanNotif:RatePlans":
                        if (RQXSD)
                            xsdFile = "./xsd/OTA2015A/OTA_HotelRatePlanNotifRQ.xsd";
                        else
                            xsdFile = "./xsd/OTA2015A/OTA_HotelRatePlanNotifRS.xsd";
                        break;
                    case "OTA_HotelAvailNotif:FreeRooms":
                        if (RQXSD)
                            xsdFile = "./xsd/OTA2015A/OTA_HotelAvailNotifRQ.xsd";
                        else
                            xsdFile = "./xsd/OTA2015A/OTA_HotelAvailNotifRS.xsd";
                        break;
                    case "OTA_Read:GuestRequests":
                        if (RQXSD)
                            xsdFile = "./xsd/OTA2015A/OTA_ReadRQ.xsd";
                        else
                            xsdFile = "./xsd/OTA2015A/OTA_ResRetrieveRS.xsd";
                        break;
                    default:
                        MessageBox.Show("No Action given!");
                        break;
                }
                ABMethods.ValidateXSD(XML, xsdFile, out sXSDValidationErrorString);
                if (sXSDValidationErrorString.Length > 0)
                    MessageBox.Show("OTA XSD Error: " + sXSDValidationErrorString);
                else
                    MessageBox.Show("OTA " + ABServer.X_AlpineBits_ProtocolVersion + " OK!");
            }
        }
        public static string getAppSetting(string key)
        {
            //Laden der AppSettings
            Configuration config = ConfigurationManager.OpenExeConfiguration(AppDomain.CurrentDomain.BaseDirectory + "App.config");
            //Zurückgeben der dem Key zugehörigen Value
            if (config.AppSettings.Settings[key] != null)
                return config.AppSettings.Settings[key].Value;
            else return "";
        }

        public static void setAppSetting(string key, string value)
        {
            //Laden der AppSettings
            Configuration config = ConfigurationManager.
                                    OpenExeConfiguration(AppDomain.CurrentDomain.BaseDirectory + "App.config");
            //Überprüfen ob Key existiert
            if (config.AppSettings.Settings[key] != null)
            {
                //Key existiert. Löschen des Keys zum "überschreiben"
                config.AppSettings.Settings.Remove(key);
            }
            //Anlegen eines neuen KeyValue-Paars
            config.AppSettings.Settings.Add(key, value);
            //Speichern der aktualisierten AppSettings
            config.Save(ConfigurationSaveMode.Modified);
        }


        //Highlights the XML in the richTextBox
        static public void highlightText(RichTextBox rtb)
        {
            int Position = rtb.SelectionStart;
            int k = 0;

            string str = rtb.Text;

            int st, en;
            int lasten = -1;
            while (k < str.Length)
            {
                st = str.IndexOf('<', k);

                if (st < 0)
                    break;

                if (lasten > 0)
                {
                    rtb.Select(lasten + 1, st - lasten - 1);
                    rtb.SelectionColor = Color.Black;
                }

                en = str.IndexOf('>', st + 1);
                if (en < 0)
                    break;

                k = en + 1;
                lasten = en;

                if (str[st + 1] == '!')
                {
                    rtb.Select(st + 1, en - st - 1);
                    rtb.SelectionColor = Color.Green;
                    continue;

                }
                String nodeText = str.Substring(st + 1, en - st - 1);

                bool inString = false;

                int lastSt = -1;
                int state = 0;
                /* 0 = before node name
                 * 1 = in node name
                   2 = after node name
                   3 = in attribute
                   4 = in string
                   */
                int startNodeName = 0, startAtt = 0;
                for (int i = 0; i < nodeText.Length; ++i)
                {
                    if (nodeText[i] == '"')
                        inString = !inString;

                    if (inString && nodeText[i] == '"')
                        lastSt = i;
                    else
                        if (nodeText[i] == '"')
                    {
                        rtb.Select(lastSt + st + 2, i - lastSt - 1);
                        rtb.SelectionColor = Color.Blue;
                    }

                    switch (state)
                    {
                        case 0:
                            if (!Char.IsWhiteSpace(nodeText, i))
                            {
                                startNodeName = i;
                                state = 1;
                            }
                            break;
                        case 1:
                            if (Char.IsWhiteSpace(nodeText, i))
                            {
                                rtb.Select(startNodeName + st, i - startNodeName + 1);
                                rtb.SelectionColor = Color.Firebrick;
                                state = 2;
                            }
                            break;
                        case 2:
                            if (!Char.IsWhiteSpace(nodeText, i))
                            {
                                startAtt = i;
                                state = 3;
                            }
                            break;

                        case 3:
                            if (Char.IsWhiteSpace(nodeText, i) || nodeText[i] == '=')
                            {
                                rtb.Select(startAtt + st, i - startAtt + 1);
                                rtb.SelectionColor = Color.Red;
                                state = 4;
                            }
                            break;
                        case 4:
                            if (nodeText[i] == '"' && !inString)
                                state = 2;
                            break;
                    }
                }
                if (state == 1)
                {
                    rtb.Select(st + 1, nodeText.Length);
                    rtb.SelectionColor = Color.Firebrick;
                }
            }
            rtb.SelectionStart = Position;
            rtb.SelectionLength = 0;
        }


        // XML to string
        static public string xmlToString(XmlDocument doc)
        {
            XmlDocument xd = doc;
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = System.Xml.Formatting.Indented;
                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }

            return sb.ToString();
        }



        public static void ValidateXSD(string sXml, string XsdFilePath, out string sXSDValidationErrorString)
        {
            // string.Empty;
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(sXml);

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add("http://www.opentravel.org/OTA/2003/05", XsdFilePath);
                settings.Schemas.Compile();
                //   settings.ValidationEventHandler += new ValidationEventHandler(ValidationXSDCallBack);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags = XmlSchemaValidationFlags.None;
                settings.ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                XmlReader vreader = XmlReader.Create(new StringReader(document.OuterXml), settings);
                while (vreader.Read()) { }
                vreader.Close();
                sXSDValidationErrorString = "";
            }
            catch (Exception ex)
            {
                sXSDValidationErrorString = ex.Message;
            }
        }

        public static void ValidationXSDCallBack(object sender, ValidationEventArgs args)
        {
            var sXSDValidationErrorString = "";
            if (args.Severity == XmlSeverityType.Warning)
                sXSDValidationErrorString += "Warning: Matching schema not found. No validation occurred (" + args.Message + ").";
            else
                sXSDValidationErrorString += "Validation error: " + args.Message;
        }
    }
}
