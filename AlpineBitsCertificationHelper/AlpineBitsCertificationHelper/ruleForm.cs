using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace AlpineBitsTestClient
{
    public enum AuditResult
    {
        OK = 1,
        NOTOK = 0,
        EVALUATE = -1
    }

    public partial class ruleForm : Form
    {
        public AlpineBitsServer myServer = new AlpineBitsServer();


        private void setCredentials()
        {
            comboBox1.DataSource = Enum.GetValues(typeof(AuditResult));
            this.myServer.UserName = txtUsername.Text;
            this.myServer.Password = txtPassword.Text;
            this.myServer.X_AlpineBits_ClientID = txtClientID.Text;
            this.myServer.X_AlpineBits_ProtocolVersion = txtClientProtocolVersion.Text;
            this.myServer.ServerURL = txtServer.Text;
            this.myServer.InvokeZipped = cBGZIPSend.Checked;
            this.myServer.AcceptResponseGZIPEncoded = cBGZIP.Checked;
            this.myServer.HotelCode = txtHotelCode.Text;

        }
        private void ReadServerParams()
        {
            txtUsername.Text = ABMethods.getAppSetting("Username");
            txtPassword.Text = ABMethods.getAppSetting("Password");
            txtClientID.Text = ABMethods.getAppSetting("ClientID");
            txtClientProtocolVersion.Text = ABMethods.getAppSetting("ProtocolVersion");
            txtServer.Text = ABMethods.getAppSetting("ServerUrl");
            cBGZIP.Checked = ABMethods.getAppSetting("BGZIPReturn") == "yes" ? true : false;
            cBGZIPSend.Checked = ABMethods.getAppSetting("BGZIPSend") == "yes" ? true : false;
            txtHotelCode.Text = ABMethods.getAppSetting("HotelCode");
            setCredentials();
        }
        private void SaveServerParams()
        {
            ABMethods.setAppSetting("Username", txtUsername.Text);
            ABMethods.setAppSetting("Password", txtPassword.Text);
            ABMethods.setAppSetting("ClientID", txtClientID.Text);
            ABMethods.setAppSetting("ProtocolVersion", txtClientProtocolVersion.Text);
            ABMethods.setAppSetting("ServerUrl", txtServer.Text);
            ABMethods.setAppSetting("BGZIPReturn", cBGZIP.Checked ? "yes" : "no");
            ABMethods.setAppSetting("BGZIPSend", cBGZIPSend.Checked ? "yes" : "no");
            ABMethods.setAppSetting("HotelCode", txtHotelCode.Text);
            setCredentials();
        }
        public ruleForm(AlpineBitsServer srvAlpineBits)
        {
            InitializeComponent();
            myServer = srvAlpineBits;
            ReadServerParams();
            this.Text = "Certification";
        }

        private void rules2017BindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.rules2017BindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.alpinebitsDataSet);

        }

        private void ruleForm_Load(object sender, EventArgs e)
        {
            // TODO: Diese Codezeile lädt Daten in die Tabelle "alpinebitsDataSet.Rules2017". Sie können sie bei Bedarf verschieben oder entfernen.
            this.rules2017TableAdapter.Fill(this.alpinebitsDataSet.Rules2017);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (rules2017BindingSource.Filter == null || rules2017BindingSource.Filter.Length == 0)
                rules2017BindingSource.Filter = "rule_target_applicateion like '%SERVER%'";
            else
                rules2017BindingSource.Filter += " and rule_target_applicateion like '%SERVER%'";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            call_paramTextBox.Text = File.ReadAllText(openFileDialog1.FileName);
            var sXML = call_paramTextBox.Text.Replace("{HOTELCODE}", myServer.HotelCode);
            sXML = sXML.Replace("{HOTELNAME}", "");
            sXML = sXML.Replace("{YEAR}", DateTime.Now.Year.ToString());
            call_paramTextBox.Text = sXML;

            ABMethods.highlightText(call_paramTextBox);


        }


        private async void button2_Click_1(object sender, EventArgs e)
        {
            SaveServerParams();

            button2.Text = " loading...";
            var Response = await AlpineBitsRequest.ProcessRequest(myServer, call_actionTextBox.Text, call_paramTextBox.Text);
            AlpineBitsRequest.LogXMLFile(call_paramTextBox.Text, rule_idTextBox.Text + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "_" + call_actionTextBox.Text + "_RQ_");
            string LogPath = AppDomain.CurrentDomain.BaseDirectory + @"\tmp\RQ.xml";
            File.WriteAllText(LogPath, call_paramTextBox.Text);
            label16.Text = await TestingMachine.Call(myServer.X_AlpineBits_ProtocolVersion, LogPath);

            AlpineBitsRequest.LogXMLFile(Response.ResponseBody, rule_idTextBox.Text + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "_" + call_actionTextBox.Text + "_RS_");
            LogPath = AppDomain.CurrentDomain.BaseDirectory + @"\tmp\RS.xml";
            File.WriteAllText(LogPath, Response.ResponseBody);
            label17.Text = await TestingMachine.Call(myServer.X_AlpineBits_ProtocolVersion, LogPath);
            call_resultHeadersTextBox.Text = Response.ResponseHeaders;
            button2.Text = "Process request ...";
            label1.Text = Response.StatusCode.ToString();
            label14.Text = Response.Encoding.ToString();
            tabControl1.SelectTab(3);
            call_resultTextBox.Text = Response.ResponseBody;
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }



        private void call_actionTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void call_actionLabel_Click(object sender, EventArgs e)
        {

        }

        private void audit_resultLabel_Click(object sender, EventArgs e)
        {

        }

        private void call_resultLabel_Click(object sender, EventArgs e)
        {

        }

        private void call_resultTextBox_TextChanged(object sender, EventArgs e)
        {
            var x = "asdf";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(@".\response.xml", call_resultTextBox.Text);
            System.Diagnostics.Process.Start(@".\response.xml");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(@".\request.xml", call_paramTextBox.Text);
            System.Diagnostics.Process.Start(@".\request.xml");
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            rules2017BindingSource.Filter = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (rules2017BindingSource.Filter == null || rules2017BindingSource.Filter.Length == 0)
                rules2017BindingSource.Filter = "rule_target_applicateion like '%CLIENT%'";
            else
                rules2017BindingSource.Filter += " and rule_target_applicateion like '%CLIENT%'";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            rules2017BindingSource.Filter = "rule_context = 'FREE_ROOMS'";

        }

        private void button10_Click(object sender, EventArgs e)
        {
            rules2017BindingSource.Filter = "rule_context = 'GUEST_REQUESTS'";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            rules2017BindingSource.Filter = "rule_context = 'INVENTORY'";
        }

        private void button12_Click(object sender, EventArgs e)
        {
            rules2017BindingSource.Filter = "rule_context = 'RATEPLAN'";
        }
        private void button18_Click(object sender, EventArgs e)
        {
            if (rules2017BindingSource.Filter == null || rules2017BindingSource.Filter.Length == 0)

                rules2017BindingSource.Filter = "rule_type = 'MUST'";
            else
                rules2017BindingSource.Filter += " and rule_type = 'MUST'";
        }
        private void button19_Click(object sender, EventArgs e)
        {
            if (rules2017BindingSource.Filter == null || rules2017BindingSource.Filter.Length == 0)

                rules2017BindingSource.Filter = "audit_result = -1 ";
            else
                rules2017BindingSource.Filter += " and audit_result = -1 ";

        }
        private void button13_Click(object sender, EventArgs e)
        {
            string ErrorText;
            ABMethods.XSD_Validation(call_actionTextBox.Text, call_resultTextBox.Text, false, out ErrorText);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            audit_result_textTextBox.Text = "OK";
            comboBox1.Text = "OK";
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string ErrorText;
            ABMethods.XSD_Validation(call_actionTextBox.Text, call_paramTextBox.Text, true, out ErrorText);
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void call_paramTextBox_TextChanged_1(object sender, EventArgs e)
        {
            call_paramTextBox.Text = call_paramTextBox.Text.Replace("{HOTELCODE}", myServer.HotelCode);
            call_paramTextBox.Text = call_paramTextBox.Text.Replace("{HOTELNAME}", "Hotel AlpineBits");
            call_paramTextBox.Text = call_paramTextBox.Text.Replace("{YEAR}", DateTime.Now.Year.ToString());
            ABMethods.highlightText(call_paramTextBox);
        }

        private void call_paramTextBox_Leave(object sender, EventArgs e)
        {
            ABMethods.highlightText(call_paramTextBox);
        }

        private void ruleForm_Activated(object sender, EventArgs e)
        {
            ABMethods.highlightText(call_paramTextBox);
        }

        private void rules2017BindingSource_CurrentChanged(object sender, EventArgs e)
        {
            ABMethods.highlightText(call_paramTextBox);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            audit_result_textTextBox.Text = "The tested application does not support this optional feature/capability.";
            comboBox1.Text = "OK";
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void rule_idTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void rule_chapter_descLabel_Click(object sender, EventArgs e)
        {

        }

        private void rule_idLabel_Click(object sender, EventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {
            Export_Data_To_Word("template.docx");

        }




        private void Export_Data_To_Word(string filename)
        {
            var ds = rules2017TableAdapter.GetData();
            var table = new System.Data.DataTable();
            table.Columns.Add("Rule", typeof(string));
            table.Columns.Add("Chapter", typeof(string));
            table.Columns.Add("Page", typeof(string));
            table.Columns.Add("OK", typeof(string));
            table.Columns.Add("AuditComment", typeof(string));
            (from tbl in ds.AsEnumerable()
             where tbl.audit_result1 > -1
             select new
             {
                 Rule = tbl.Field<string>("rule_text"),
                 Chapter = tbl.Field<string>("rule_chapter_number") + " - " + tbl.Field<string>("rule_chapter_desc"),
                 Page = tbl.Field<string>("rule_page_number"),
                 OK = tbl.Field<int>("audit_result1"),
                 AuditComment = tbl.Field<string>("audit_result_text")
             }).Aggregate(table, (dt, r) => { dt.Rows.Add(r.Rule, r.Chapter, r.Page, (r.OK.ToString() == "1" ? "OK" : "FAILED"), r.AuditComment); return dt; });
            System.Data.DataTable DGV = table;
            if (DGV.Rows.Count != 0)
            {
                object RowCount = DGV.Rows.Count;
                object ColumnCount = DGV.Columns.Count;
                Object[,] DataArray = new object[(int)RowCount + 1, (int)ColumnCount + 1];

                //add rows
                int r = 0;
                for (int c = 0; c <= (int)ColumnCount - 1; c++)
                {
                    for (r = 0; r <= (int)RowCount - 1; r++)
                    {
                        DataArray[r, c] = DGV.Rows[r].ItemArray[c].ToString();
                    } //end row loop
                } //end column loop

                Word.Application oApp = new Word.Application();
                Word.Document oDoc = oApp.Documents.Open(AppDomain.CurrentDomain.BaseDirectory + "/template.docx");
                oDoc.Application.Visible = true;

                //page orintation
                oDoc.PageSetup.Orientation = Word.WdOrientation.wdOrientLandscape;


                Paragraph oRange = oDoc.Content.Paragraphs.Add();

                string oTemp = "";
                for (r = 0; r <= (int)RowCount - 1; r++)
                {
                    for (int c = 0; c <= (int)ColumnCount - 1; c++)
                    {
                        oTemp = oTemp + DataArray[r, c] + "\t";

                    }
                }

                //table format
                oRange.Range.InsertBefore(oTemp);

                object Separator = Word.WdTableFieldSeparator.wdSeparateByTabs;
                object ApplyBorders = true;
                object AutoFit = true;
                object AutoFitBehavior = Word.WdAutoFitBehavior.wdAutoFitContent;

                oRange.Range.ConvertToTable(ref Separator, ref RowCount, ref ColumnCount,
                                      Type.Missing, Type.Missing, ref ApplyBorders,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, ref AutoFit, ref AutoFitBehavior, Type.Missing);

                oRange.Range.Select();

                Paragraph Testingdata = oDoc.Content.Paragraphs.Add();

                Range range = Testingdata.Range;
                // Testing data
                range.Paragraphs.TabStops.Add(56, WdTabAlignment.wdAlignTabRight);
                range.Text = "URL \t " + myServer.ServerURL + "\r";
                range.Text += "HotelCode \t " + myServer.HotelCode + "\r";
                range.Text += "X_AlpineBits_ClientID \t " + myServer.X_AlpineBits_ClientID + "\r";
                range.Text += "X_AlpineBits_ProtocolVersion \t " + myServer.X_AlpineBits_ProtocolVersion + "\r";

                // Capabilities
                Paragraph assets = oDoc.Content.Paragraphs.Add();
                assets.Range.ListFormat.ApplyBulletDefault();

                foreach (string item in listBox1.Items)
                {
                    string bulletItem = item + "\n";
                    assets.Range.InsertBefore(bulletItem);
                }


                oDoc.Application.Selection.Tables[1].Select();
                oDoc.Application.Selection.Tables[1].Rows.AllowBreakAcrossPages = 0;
                oDoc.Application.Selection.Tables[1].Rows.Alignment = 0;
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.InsertRowsAbove(1);
                oDoc.Application.Selection.Tables[1].Rows[1].Select();

                //header row style
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Bold = 1;
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Name = "Tahoma";
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Size = 14;

                //add header row manually
                for (int c = 0; c <= (int)ColumnCount - 1; c++)
                {
                    oDoc.Application.Selection.Tables[1].Cell(1, c + 1).Range.Text = DGV.Columns[c].ColumnName;
                }

                //table style
                //     oDoc.Application.Selection.Tables[1].set_Style("Grid Table 4 - Accent 5");
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                //save the file
                //oDoc.SaveAs2(filename);

                //NASSIM LOUCHANI
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Process.Start(@".\OTA\OpenTravel_CodeList_2015_06_03.xlsm");
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            SaveServerParams();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void tabPage3_Enter(object sender, EventArgs e)
        {
            var Response = await AlpineBitsRequest.ProcessRequest(myServer, "getCapabilities", "");
            var capabilities = Response.ResponseBody.Replace("OK:", "").Split(',');
            listBox1.Items.Clear();

            foreach (string item in capabilities)
            {
                listBox1.Items.Add(item);
            }

            Response = await AlpineBitsRequest.ProcessRequest(myServer, "getVersion", "");
            txtServerAction.Text = Response.ResponseBody.Replace("OK:", "");
        }

        private void tabServersettings_Enter(object sender, EventArgs e)
        {
            ReadServerParams();
        }

        private void tabServersettings_Leave(object sender, EventArgs e)
        {
            SaveServerParams();
        }

        private void tabServersettings_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void call_paramTextBox_MultilineChanged(object sender, EventArgs e)
        {
            ABMethods.highlightText(call_paramTextBox);

        }

        private void ruleForm_Shown(object sender, EventArgs e)
        {
            this.Text = "Certification";
        }

        private void ruleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveServerParams();
            this.rules2017BindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.alpinebitsDataSet);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            audit_result_textTextBox.Text = "This rule is obsolete for the tested application.";
            comboBox1.Text = "OK";
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;

        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (rules2017BindingSource.Filter == null || rules2017BindingSource.Filter.Length == 0)

                rules2017BindingSource.Filter = "audit_result = 0 ";
            else
                rules2017BindingSource.Filter += " and audit_result = 0 ";

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void audit_result1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = audit_result1ComboBox.SelectedIndex;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            audit_result_textTextBox.Text = "NOT OK";
            comboBox1.Text = "NOTOK";
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            comboBox1.Text = "OK";
            audit_result1ComboBox.SelectedIndex = comboBox1.SelectedIndex;
        }
    }
}
