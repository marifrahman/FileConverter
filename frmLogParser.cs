using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO; 

namespace BSSReport
{
    public partial class frmLogParser : Form
    {

        string btslistdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "adjcelldata\\");//Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "adjcelldata\\");
        
        public frmLogParser()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {

            try
            {

                OpenFileDialog ofd = new OpenFileDialog();

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtLocation.Text = ofd.FileName;
                    string filePath = txtLocation.Text.ToString();
                    List<string[]> parseData = parseFile(filePath);
                    
                    //DataTable refdt = loadBTSRef();

                    DataTable newTable = new DataTable();

                    newTable.Columns.Add(CreateDataColumn("System.String", "amnt", "Amount.", false, false, false));
                    newTable.Columns.Add(CreateDataColumn("System.String", "acountno", "Accno", false, false, false));
                    
                    
                    foreach (string[] line in parseData)
                    {
                        
                            DataRow workRow = newTable.NewRow();
                            workRow["amnt"] = line[0];
                            workRow["acountno"] = line[1];

                        newTable.Rows.Add(workRow);

                    }

                    dataGridView1.DataSource = newTable;//bindingSource;// 
                    btnExport.Enabled = true;
                }
            }
            catch (Exception xcp)
            {
                MessageBox.Show("An issue raised :" + xcp.Message.ToString());
            }
        }

   
        System.Data.DataColumn CreateDataColumn(string colType, string name, string caption, bool autoInc, bool readOnly, bool unique)
        {


            DataColumn column = new DataColumn();
            column.DataType = System.Type.GetType(colType);
            column.ColumnName = name;
            column.Caption = caption;
            column.AutoIncrement = autoInc;
            column.AutoIncrementSeed = 1;
            column.ReadOnly = readOnly;
            column.Unique = unique;
            return column;
        }

        private DataTable loadBTSRef()
        {
           
            DataTable dt = new DataTable();
            string conString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + btslistdir + "" +
                   @";Extended Properties=""Text;HDR=yes;FMT=Delimited(,)\""";
            try
            {
            OleDbConnection conn = new OleDbConnection(conString);
            OleDbDataAdapter da = new OleDbDataAdapter(@"Select * from btsreftable.csv", conn);
            
                da.Fill(dt);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Problem loading the BTS Ref info:" + exp.Message.ToString());
            }
            return dt;
        }

        private List<string[]> parseFile(string path)
        {
            List<string[]> parsedData = new List<string[]>();

            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                string amnt = "";
                string accno = "";
                while ((line = readFile.ReadLine()) != null)
                {
                    if (line.IndexOf("16,699") != -1 )
                    {
                        
                        
                        if (line.Split(new Char[] { ',' })[4] == "020")
                        {
                            amnt = line.Split(new Char[] { ',' })[2];
                            accno = line.Split(new Char[] { ',' })[5];
                            row = new string[] { amnt, accno };
                            parsedData.Add(row);

                        }
                        
                     }

                }
            }

            return parsedData;
        }

        
        
        Timer timer = new Timer();
        private void btnExport_Click(object sender, EventArgs e)
        {
            Stream reportStream; 
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "ANSI DOS(*.brs)|*.brs"; //Excel files (*.xlsx)|*.xlsx|txt files (*.txt)|*.txt|All files (*.*)|*.*
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((reportStream = saveFileDialog1.OpenFile()) != null)
                {
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                            saveToBRS(reportStream);
                            break;
                        //case 2:
                        //    //saveToExcell(reportStream);
                        //    saveToCSV(reportStream);
                        //    break;
                        //default:
                        //    saveToCSV(reportStream);
                        //    break;

                    }
                    
                    reportStream.Close();
                    showSuccessMsg("Successfuly exported the file :" + saveFileDialog1.FileName);
                    
                    

                }
            }
        }
        void showSuccessMsg(string msg)
        {
            dataGridView1.Enabled = false;
            tempLabel.Location = new Point(430, 300);
            tempLabel.Height = 200;
            tempLabel.Width = 350;
            tempLabel.Visible = true;
            tempLabel.Text = msg;
            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = (1000) * (2);             // Timer will tick evert 10 seconds
            timer.Enabled = true;                       // Enable the timer
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            tempLabel.Text = "";
            tempLabel.Location = new Point(0, 0);
            tempLabel.Height = 0;
            tempLabel.Width = 0;
            tempLabel.Visible = false;
            timer.Stop();
            timer.Enabled = false;
            dataGridView1.Enabled = true;
        }
        void saveToCSV(Stream filename)
        {
            using (StreamWriter myFile = new StreamWriter(filename))
            {
                // Export titles:
                string sHeaders = "";
                for (int j = 0; j < dataGridView1.Columns.Count; j++) 
                { 
                    sHeaders = sHeaders.ToString() + Convert.ToString(dataGridView1.Columns[j].HeaderText) + ","; 
                }
                myFile.WriteLine(sHeaders);
                // Export data.
                for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                {
                    string stLine = "";
                    for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++) 
                    { 
                        stLine = stLine.ToString() + Convert.ToString(dataGridView1.Rows[i].Cells[j].Value) + ","; 
                    }
                    myFile.WriteLine(stLine);
                }
            }
        }


        void saveToBRS(Stream filename)
        {
            using (StreamWriter myFile = new StreamWriter(filename))
            {
                // Export titles:
                string sHeaders = "#BRS#00000"+string.Format("yyyymmdd",DateTime.Now);
                //for (int j = 0; j < dataGridView1.Columns.Count; j++)
                //{
                //    sHeaders = sHeaders.ToString() + Convert.ToString(dataGridView1.Columns[j].HeaderText) + ",";
                //}
                myFile.WriteLine(sHeaders);
                myFile.WriteLine("Somethign");
                
                int amount = 0;

                for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                {

                    amount += int.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString());
                    
                    //string stLine = "";
                    
                    //for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)
                    //{
                    //    stLine = stLine.ToString() + Convert.ToString(dataGridView1.Rows[i].Cells[j].Value) + ",";
                    //}
                    
                    
                }


                myFile.WriteLine("Some thing"+" "+"0500000000000"+amount);
            }
        }


    }
 }
