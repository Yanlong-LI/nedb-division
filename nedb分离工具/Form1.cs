using nedb;
using nedb.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace nedb分离工具
{
    public partial class Form1 : Form
    {
        private List<Dictionary<string, string>> dicc = new List<Dictionary<string, string>>();//<string, Dictionary<string, object>>();
        public Form1()
        {
            InitializeComponent();
            string exeName = Process.GetCurrentProcess().MainModule.FileName;
            if(File.Exists(exeName + ".config"))
            textBox1.Text = Settings.Default.dbPath;
            else
            {
                File.WriteAllText(exeName + ".config", Resources.nedb分离工具_exe);
                reload();
            }
                
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";
            dialog.Filter = "nedb文件(*.db)|*.db";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
                Settings.Default.dbPath = dialog.FileName;
                Settings.Default.Save();
            }
        }
        public DataTable StuList()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("序号");
            dt.Columns.Add("内容");
            string sFilePath = string.Empty;
            dicc.Clear();
            dataGridViewWhere.Rows.Clear();
            checkedListBox1.Items.Clear();

            ////// 判断选择路径是否存在
            if (!File.Exists(this.textBox1.Text.ToString()))
            {
                MessageBox.Show("选取文件有误，无权限或文件不存在");
                return dt;
            }

            try
            {
                int i = 0;
                using (StreamReader sr = new StreamReader(textBox1.Text.ToString()))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() == "")
                        {
                            continue;
                        }
                        //写入数据表
                        DataRow dr = dt.NewRow();
                        dr["内容"] = line.ToString();
                        dr["序号"] = ++i;
                        dt.Rows.Add(dr);

                        //将json字符串解码为对象
                        Dictionary<string, object> dic = JsonToDictionary(line);
                        Dictionary<string, string> dd = new Dictionary<string, string>();

                        foreach(KeyValuePair<string,object> item in dic)
                        {
                            dd.Add(item.Key, item.Value.ToString());
                        }

                        dicc.Add(dd);
                        foreach(string k in dic.Keys)
                        {
                            var type  = dic[k].GetType();
                            if (!checkedListBox1.Items.Contains(k) && type.Name== "String")
                            {
                                checkedListBox1.Items.Add(k);
                            }
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = StuList();
        }
        /// <summary>
        /// 将json数据反序列化为Dictionary
        /// </summary>
        /// <param name="jsonData">json数据</param>
        /// <returns></returns>
        private Dictionary<string, object> JsonToDictionary(string jsonData)
        {
            //实例化JavaScriptSerializer类的新实例
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                //将指定的 JSON 字符串转换为 Dictionary<string, object> 类型的对象
                //jss.Deserialize<Dictionary<string, string>>(jsonData);
                return jss.Deserialize<Dictionary<string, object>>(jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string name = this.textBox1.Text;  //文件名
            string path = string.Empty;  //文件路径
            SaveFileDialog save = new SaveFileDialog();
            save.Title = "请选择保存路径";
            save.Filter = "nedb文件(*.db)|*.db";

            if (save.ShowDialog() == DialogResult.OK)
                path = save.FileName;
            else
                return;

            


            using (System.IO.FileStream file = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (System.IO.TextWriter text = new System.IO.StreamWriter(file, System.Text.Encoding.Default))
                {
                    int i = 0;
                    foreach (Dictionary<string,string> item in dicc)
                    {
                        Boolean write = true;
                        foreach (DataGridViewRow row in dataGridViewWhere.Rows)
                        {
                            var lamada = GetWherePredicate(row);
                            var dc = item.Where(lamada.Compile()).FirstOrDefault();
                            if (dc.Key == null)
                            {
                                write = false;
                                break;
                            }
                            
                        }
                        if(write)
                        text.WriteLine(dataGridView1.Rows[i].Cells["内容"].Value);
                        i++;
                    }

                }
            }
        }
        Expression<Func<KeyValuePair<string,string>, bool>> GetWherePredicate(DataGridViewRow row)
        {
            var p = Expression.Parameter(typeof(KeyValuePair<string, string>), "p");
            var expr1 = Expression.Equal(Expression.Constant(1), Expression.Constant(1));
            //foreach(DataGridViewRow row in dataGridViewWhere.Rows) { 
                try
                {
                    Dictionary<string, string> tt = new Dictionary<string, string>();
                    tt.Add(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString());
                    var expr2 = Expression.Equal(Expression.Property(p, "Key"), Expression.Constant(tt.First().Key));
                    var expr3 = Expression.Equal(Expression.Property(p, "Value"), Expression.Constant(tt.First().Value));

                    expr3 = Expression.And(expr2, expr3);
                    expr1 = Expression.And(expr1, expr3);
                }
                catch (Exception e) {
                    MessageBox.Show("你选择的筛选条件："+row.Cells[0].Value.ToString()+"，必须填写筛选值");
                }
            //}
            var expr4 = Expression.Lambda<Func<KeyValuePair<string, string>, bool>>(expr1, p);
            return expr4;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string fieldName = checkedListBox1.Items[e.Index].ToString();

            if (e.NewValue== System.Windows.Forms.CheckState.Checked) { 
            
            dataGridViewWhere.Rows.Add(fieldName,"");
            }
            else
            {
               foreach(DataGridViewRow row in dataGridViewWhere.Rows)
                {
                    
                    if ((string)row.Cells[0].Value == fieldName)
                    {
                        dataGridViewWhere.Rows.Remove(row);
                    }
                }
            }
        }

        private void dataGridViewWhere_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridViewWhere.CurrentCell = this.dataGridViewWhere.Rows[e.RowIndex].Cells[1];//获取当前单元格
            this.dataGridViewWhere.BeginEdit(true);//将单元格设为编辑状态
        }
        private about about;

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (about == null || about.IsDisposed == true)
                about = new about();
            //this.Hide();//隐藏现在这个窗口
            about.Show();//新窗口显现
            about.Activate();
        }

        private void 检查更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("你当前使用为离线版本，无法进行自升级。");
        }
        private void reload()
        {
            Application.ExitThread();
            Thread thtmp = new Thread(new ParameterizedThreadStart(run));
            object appName = Application.ExecutablePath;
            Thread.Sleep(1);
            thtmp.Start(appName);
        }
        private void run(Object obj)
        {
            Process ps = new Process();
            ps.StartInfo.FileName = obj.ToString();
            ps.Start();
            Application.Exit();
        }
    }
  
}
