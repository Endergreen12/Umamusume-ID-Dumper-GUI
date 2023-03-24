using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Umamusume_ID_Dumper_GUI.Properties;

namespace Umamusume_ID_Dumper_GUI
{
    public partial class app : Form
    {
        private string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low\\Cygames\\umamusume\\master\\master.mdb";
        private SqliteConnection conn = null;
        private SqliteCommand cmd = null;
        private string[] targetField = new string[] { "index", "text" };

        public app()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeConnection();

            if (cmd != null)
            {
                UpdateStatus(2);

                conn.Open();

                if(cmbBoxData.SelectedIndex < 0)
                {
                    UpdateStatus(5);
                    return;
                }

                UpdateStatus(3);

                dataGridView1.Rows.Clear();
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataGridView1.Rows.Add(reader.GetString(0), reader.GetString(1));
                    }
                }

                conn.Close();

                UpdateStatus(4);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateStatus(0);
            cmbBoxData.SelectedIndex = 0;
        }

        private int GetDataCategoryInt(int category)
        {
            int data = -1;

            switch(category)
            {
                case 0: // Character
                    data = 6;
                    break;

                case 1: // Dress
                    data = 14;
                    break;

                case 2: // Live
                    data = 16;
                    break;
            }

            return data;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Status text is updated according to the value of the argument status.
        /// </summary>
        /// <param name="status">
        /// 0: Initial state<br />
        /// 1: Verifying that the database exists<br />
        /// 2: Opening the database<br />
        /// 3: Getting data<br />
        /// 4: Done<br />
        /// 5: An error has occurred
        /// </param>
        /// <param name="errorIndex">
        /// 0: Database existence could not be verified
        /// </param>
        private void UpdateStatus(int status = -1, int errorIndex = -1)
        {
            string statusTxt = "";

            switch(status)
            {
                case 0:
                    statusTxt = Resources.status0;
                    break;

                case 1:
                    statusTxt = Resources.status1;
                    break;

                case 2:
                    statusTxt = Resources.status2;
                    break;

                case 3:
                    statusTxt = Resources.status3;
                    break;

                case 4:
                    statusTxt = Resources.status4;
                    break;

                case 5:
                    var errorTxt = "";

                    switch(errorIndex)
                    {
                        case 0:
                            errorTxt = Resources.error0;
                            break;
                    }

                    statusTxt = Resources.status5 + ": " + errorTxt;
                    break;
            }

            lblStatus.Text = Resources.status + ": " + statusTxt;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            InitializeConnection();

            if (conn == null || cmd == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.FileName = cmbBoxData.Text + ".txt";
            saveFileDialog.Title = "";
            saveFileDialog.Filter = "(*.txt)|*.txt";

            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream;
                stream = saveFileDialog.OpenFile();
                if(stream != null)
                {
                    StreamWriter streamWriter = new StreamWriter(stream);

                    var dumpedText = "";

                    UpdateStatus(2);
                    conn.Open();
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        UpdateStatus(3);
                        while (reader.Read())
                        {
                            for (int i = 0; i < targetField.Length; i++)
                            {
                                dumpedText += reader.GetString(i);
                                if (i == targetField.Length - 1)
                                    dumpedText += "\r\n";
                                else
                                    dumpedText += "\t";
                            }
                        }
                    }

                    streamWriter.Write(dumpedText);
                    streamWriter.Flush();
                    stream.Close();
                    conn.Close();

                    UpdateStatus(4);
                }
            }
        }

        private void InitializeConnection()
        {
            UpdateStatus(1);

            if (File.Exists(dbPath))
            {
                conn = new($"Data Source={dbPath}");

                cmd = conn.CreateCommand();

                var tableName = "text_data";
                var targetFieldStr = @"""" + String.Join(@""",""", targetField) + @"""";
                var refinementField = "category";
                var refinementNum = GetDataCategoryInt(cmbBoxData.SelectedIndex);

                cmd.CommandText =
                        $@" SELECT {targetFieldStr}
                            FROM {tableName}
                        ";

                if (refinementField != "")
                    cmd.CommandText += $@" WHERE {refinementField} = {refinementNum}";
            }
            else
            {
                UpdateStatus(5, 0);
            }
        }
    }
}
