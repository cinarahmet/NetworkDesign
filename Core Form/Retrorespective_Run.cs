using FMLMDelivery.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core_Form
{
    public partial class Retrorespective_Run : Form
    {   

        private List<Double> courier_parameters = new List<double>();

        private int month;

        private Double hub_coverage;

        private List<DemandPoint> demand_points = new List<DemandPoint>();

        private List<xDocks> potential_xdocks = new List<xDocks>();

        private Dictionary<String, int> month_dict = new Dictionary<string, int>();

        private String direct_initial;

        private String output_initial;

        private List<String> courier_assignment_list = new List<string>();

        private List<Boolean> run_type = new List<bool>();

        public Retrorespective_Run()
        {
            InitializeComponent();
            Create_Dictionary_Month();
            Month_Selected.Enabled = false;
            Hub_Cov_Box.Enabled = false;
            _threshold.Enabled = false;
            Min_Cap_Courier.Enabled = false;
            Max_Cap_Courier.Enabled = false;
            Km_Başı_Paket.Enabled = false;
            Stable_Run.Enabled = false;
            Different_Run.Enabled = false;

        }
        private void Create_Dictionary_Month()
        {
            var dict = new Dictionary<String, int>();
            dict.Add("Ocak", 1);
            dict.Add("Şubat", 2);
            dict.Add("Mart", 3);
            dict.Add("Nisan", 4);
            dict.Add("Mayıs", 5);
            dict.Add("Haziran", 6);
            dict.Add("Temmuz", 7);
            dict.Add("Ağustos", 8);
            dict.Add("Eylül", 9);
            dict.Add("Ekim", 10);
            dict.Add("Kasım", 11);
            dict.Add("Aralık", 12);
            dict.Add("Ay Seçiniz", 1);

            month_dict = dict;
        }
        private void Stable_Parameter(object sender, EventArgs e)
        {
            if (Stable_Run.Checked)
            {
                Month_Selected.Enabled = false;
                Hub_Cov_Box.Enabled = false;
                _threshold.Enabled = false;
                Min_Cap_Courier.Enabled = false;
                Max_Cap_Courier.Enabled = false;
                Km_Başı_Paket.Enabled = false;
            }
        }

        private void Different_Parameter(object sender, EventArgs e)
        {
            if (Different_Run.Checked)
            {
                if (run_type[0])
                {   if(!run_type[2]) Hub_Cov_Box.Enabled = true;
                    Month_Selected.Enabled = true;
                    _threshold.Enabled = true;
                    Min_Cap_Courier.Enabled = true;
                    Max_Cap_Courier.Enabled = true;
                    Km_Başı_Paket.Enabled = true;
                }else if (run_type[1])
                {
                    Month_Selected.Enabled = true;
                    Hub_Cov_Box.Enabled = true;
                    _threshold.Enabled = true;
                    Min_Cap_Courier.Enabled = true;
                    Max_Cap_Courier.Enabled = true;
                    Km_Başı_Paket.Enabled = true;
                }else if(run_type[3])
                {
                    _threshold.Enabled = true;
                    Min_Cap_Courier.Enabled = true;
                    Max_Cap_Courier.Enabled = true;
                    Km_Başı_Paket.Enabled = true;

                }
                //Month_Selected.Enabled = true;
                //Hub_Cov_Box.Enabled = true;
                //_threshold.Enabled = true;
                //Min_Cap_Courier.Enabled = true;
                //Max_Cap_Courier.Enabled = true;
                //Km_Başı_Paket.Enabled = true; 
            }
        }

        private void Stable_Clik(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            direct_initial = openFileDialog1.FileName;
            Json_File_Location.Text = direct_initial;


            // Read the run type from previous solution to enable or disable parameter inputs
            var partly_reader = new Json_Reader(Json_File_Location.Text);
            partly_reader.Stable_Parameter_Read();
            run_type = partly_reader.Get_Run_Type();
            Stable_Run.Enabled = true;
            Different_Run.Enabled = true;

        }
        private void Output_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            var direct = folderBrowserDialog1.SelectedPath;
            Output_Loc.Text = direct;
           
        }
        private void Courier_Runner_Writer(Dictionary<xDocks, List<Mahalle>> mahalle_list, List<Double> parameter_list, String output_loc)
        {
            for (int i = 0; i < mahalle_list.Count; i++)
            {
                var courier_assignment = new Courier_Assignment(mahalle_list.ElementAt(i).Key, mahalle_list[mahalle_list.ElementAt(i).Key], parameter_list[0], parameter_list[1], parameter_list[2], parameter_list[3]);
                courier_assignment.Run_Assignment_Procedure();
                var list = courier_assignment.Return_Courier_Assignments();
                courier_assignment_list.AddRange(list);
            }
            var header_xdock_demand_point = "xDock İl,xDock İlçe,xDock Mahalle,Kurye Id, Atanan Mahalle,Mahalle İlçe, Mahalle Boylam, Mahalle Enlem, Mahalleye Götüreceği Paket,Tahmini Uzaklık,Kapasite Aşımı";
            var write_the_xdocks = new Csv_Writer(courier_assignment_list, "Kurye Atamaları", header_xdock_demand_point, output_loc);
            write_the_xdocks.Write_Records();
        }
        private async void Run(object sender, EventArgs e)
        {
            //Variable initialization for running model. In ths concept some variables are tend to be empty
            var demand_point = new List<DemandPoint>();
            var potential_xDocks = new List<xDocks>();
            var xDocks = new List<xDocks>();
            var agency = new List<xDocks>();
            var hubs = new List<Hub>();
            var potential_hubs = new List<Hub>();
            var partial_xDocks = new List<xDocks>();
            var parameters = new List<Parameters>();
            var xDock_neighborhood_assignments = new Dictionary<xDocks, List<Mahalle>>();
            //This variable decides which solution method will be run. If true; every city individually assigned, else regions are assigned as a whole
            var discrete_solution = true;

            if (Different_Run.Checked)
            {
                //Getting manual inputs for Re-run
                var month = month_dict[Month_Selected.Text.ToString()];
                var courier_min_cap = Convert.ToDouble(Min_Cap_Courier.Text);
                var desired_efficiency = Convert.ToDouble(_threshold.Text);
                var courier_max_cap = Convert.ToDouble(Max_Cap_Courier.Text);
                var compensation = Convert.ToDouble(Km_Başı_Paket.Text);
                var courier_parameter_list = new List<Double> { courier_min_cap, courier_max_cap, desired_efficiency, compensation };
                var hub_demand_coverage = new Double();
                if (Hub_Cov_Box.Text!="") hub_demand_coverage = Convert.ToDouble(Hub_Cov_Box.Text);
                

                var reader = new Json_Reader(direct_initial);
                reader.Different_Parameter_Read(month);
                var full_run = reader.Get_Run_Type()[0];
                var partial_solution = reader.Get_Run_Type()[1];
                var only_cities = reader.Get_Run_Type()[2];
                var only_courier_assignments = reader.Get_Run_Type()[3];


                //Different Run combinations for different run types
                if (full_run)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_Potential_xDocks();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();

                    var distance_matrix = new Dictionary<string,List<Double>>();


                    //var runner = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, Output_Loc.Text, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list, distance_matrix);                    
                    //(xDocks, hubs) = await Task.Run(() => runner.Run());
                    //Console.ReadKey();

                }
                else if (only_courier_assignments)
                {
                    var month_for_courier = reader.Get_Run_Month();
                    xDock_neighborhood_assignments = reader.Get_xDock_Neighborhood();
                    Courier_Runner_Writer(xDock_neighborhood_assignments, courier_parameter_list, Output_Loc.Text);

                }
                else if (partial_solution)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_Potential_xDocks();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();
                    var distance_matrix = new Dictionary<string, List<Double>>();

                    partial_xDocks = reader.Get_Partial_Solution_xDocks();
                    //var runner_partial = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, Output_Loc.Text, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list,distance_matrix);
                    //(xDocks, hubs) = await Task.Run(() => runner_partial.Run());
                    //Console.ReadKey();
                }
            }
            else
            {   //Reader for the parameters
                var reader = new Json_Reader(direct_initial);
                reader.Stable_Parameter_Read();
                var full_run = reader.Get_Run_Type()[0];
                var partial_solution = reader.Get_Run_Type()[1];
                var only_cities = reader.Get_Run_Type()[2];
                var only_courier_assignments = reader.Get_Run_Type()[3];

                var month = reader.Get_Run_Month();
                var courier_min_cap = Convert.ToDouble(reader.Get_Courier_Parameters()[0]);
                var desired_efficiency = Convert.ToDouble(reader.Get_Courier_Parameters()[2]);
                var courier_max_cap = Convert.ToDouble(reader.Get_Courier_Parameters()[1]);
                var compensation = Convert.ToDouble(reader.Get_Courier_Parameters()[3]);
                var courier_parameter_list = new List<Double> { courier_min_cap, courier_max_cap, desired_efficiency, compensation };
                var hub_demand_coverage = reader.Get_Hub_Coverage();
                
                if (full_run)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_Potential_xDocks();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();
                    var distance_matrix = new Dictionary<string, List<Double>>();

                    //var runner = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, Output_Loc.Text, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list,distance_matrix);
                    //(xDocks, hubs) = await Task.Run(() => runner.Run());
                    //Console.ReadKey();

                }
                else if (only_courier_assignments)
                {
                    var month_for_courier = reader.Get_Run_Month();
                    xDock_neighborhood_assignments = reader.Get_xDock_Neighborhood();
                    Courier_Runner_Writer(xDock_neighborhood_assignments, courier_parameter_list, Output_Loc.Text);

                }
                else if (partial_solution)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_Potential_xDocks();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();
                    var distance_matrix = new Dictionary<string, List<Double>>();
                    partial_xDocks = reader.Get_Partial_Solution_xDocks();
                    //var runner_partial = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, Output_Loc.Text, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list,distance_matrix);
                    //(xDocks, hubs) = await Task.Run(() => runner_partial.Run());
                    //Console.ReadKey();
                }

            }

            var path = Output_Loc.Text + "\\";
            var dirname = new DirectoryInfo(path).Name;
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Information;
            MessageBox.Show("Çalıştırma Bitti! Sonuçları " + "'" + dirname + "'" + " dosyasında bulabilirsiniz.", "Bilgi", buttons, icon);
        }

    }
}
