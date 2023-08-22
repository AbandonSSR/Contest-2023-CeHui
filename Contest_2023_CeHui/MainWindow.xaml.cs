using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Contest_2023_CeHui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateMessage(string message)
        {
            this.txtLog.AppendText($"{DateTime.Now}：{message}\n");
            this.txtLog.ScrollToEnd();
        }

        private void CleanMessage() => this.txtLog.Clear();

        private PointCloud? cloud;

        private void btnReadData_Click(object sender, RoutedEventArgs e)
        {
            CleanMessage();
            var dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Multiselect = false
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var (flag, message, data) = CoreFunction.ReadData(dialog.FileName);
                UpdateMessage($"{message}");
                if (flag && data != null)
                {
                    this.cloud = new PointCloud(data);
                    foreach (var item in data)
                    {
                        if (item.Name == "P5")
                        {
                            UpdateMessage($"P5 的坐标\n{item}\n");
                            break;
                        }
                    }
                    UpdateMessage($"点云的坐标范围\n{this.cloud.Box.Min}\n{this.cloud.Box.Max}\n");
                }
            }
        }

        private void btnCalc1_Click(object sender, RoutedEventArgs e)
        {
            if (this.cloud == null)
            {
                UpdateMessage("请先读取点云数据");
                return;
            }
            var (flag, message, grid) = CoreFunction.Calc1(in this.cloud, 10.0, 10.0);
            UpdateMessage(message);
            if (flag && grid != null)
            {
                var builder = new StringBuilder("栅格单元 C 的信息\n");
                builder.Append($"点的数量：{grid[2, 3].Points.Count}\n");
                builder.Append($"点的平均高度：{grid[2, 3].AverageZ:f3}\n");
                builder.Append($"点的高度最大值：{grid[2, 3].Box.Max.Z:f3}\n");
                builder.Append($"点的高度差：{grid[2, 3].DifferenceZ:f3}\n");
                builder.Append($"点的高度方差：{grid[2, 3].VarianceZ:f3}\n");
                UpdateMessage(builder.ToString());
            }
        }

        private void btnCalc2_Click(object sender, RoutedEventArgs e)
        {
            if (this.cloud == null)
            {
                UpdateMessage("请先读取点云数据");
                return;
            }
            var (flag, message) = CoreFunction.Calc2(in this.cloud, 0.1, 0.1);
            UpdateMessage(message);
        }
    }
}
