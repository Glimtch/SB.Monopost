using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Crm.Sdk.Messages;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Blun.ConfigurationManager;

namespace TerminalAPCapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static CrmServiceClient _service;
        private static string[] lines;

        public MainWindow()
        {
            InitializeComponent();
            var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();

            _service = new CrmServiceClient(config.GetConnectionString("MonoPost"));

            lines = System.IO.File.ReadAllLines("config.txt");

            this.Title = lines[0];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string num = number.Text;
            if (num.Length < 10)
            {
                MessageBox.Show("Not correct Login");

                number.Text = "";
                password.Text = "";

                return;
            }

            string str = num.Substring(7, 6);

            if (str != lines[1])
            {
                MessageBox.Show(String.Format("Perhaps this is a parcel from another department // {0}", str));

                number.Text = "";
                password.Text = "";

                return;
            }


            var query = new QueryExpression("sb_delivery")
            {
                ColumnSet = new ColumnSet(true),
            };

            query.Criteria.AddCondition("sb_name", ConditionOperator.Equal, num);

            var delivery = _service.RetrieveMultiple(query).Entities;

            foreach (var ent in delivery)
                if (((string)ent["sb_password"] == password.Text) && (number.Text == (string)ent["sb_name"]) 
                    && (ent.Contains("sb_dateexpiring")) && (!ent.Contains("sb_datefullfilled"))
                    )
                {

                    var orderRef = (EntityReference)ent["sb_orderid"];
                    var order = _service.Retrieve("sb_order", orderRef.Id, new ColumnSet(true));

                    var reciplentRef = (EntityReference)order["sb_recipientid"];
                    var reciplent = _service.Retrieve("contact", reciplentRef.Id, new ColumnSet(true));

                    MessageBox.Show(String.Format(" Dear {0} {1}, your order in the cell number {2}! Thanks for using our services. Have a nice day", 
                        reciplent["firstname"], reciplent["lastname"], ent["sb_deliverto_apccell"]));

                    DateTime now = DateTime.Now;
                    now.AddHours(3);

                    ent["sb_datefullfilled"] = now;

                    _service.Update(ent);

                    number.Text = "";
                    password.Text = "";

                }
            else
                {
                    MessageBox.Show("Not correct values");

                    number.Text = "";
                    password.Text = "";

                    return;
                }
           
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
