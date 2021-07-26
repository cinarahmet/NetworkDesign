using FMLMDelivery.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Nancy.Json;

namespace Core_Form
{
    public partial class Network_Design_Form_Core : Form
    {
        private string demand_file = "";
        private string pot_xDock_file = "";
        private string seller_file = "";
        private string presolved_xDock_file = "";
        private string parameter_file = "";
        private string mahalle_xdock_file = "";
        private String directory = "";
        private Boolean partial_solution = new Boolean();
        private string direct_initial = "";
        private double hub_demand_coverage = 0.95;
        private Boolean only_cities = new Boolean();
        private Boolean only_courier_assignments = new bool();
        private Dictionary<String, int> month_dict = new Dictionary<string, int>();
        private Boolean full_run = new bool();
        private List<String> courier_assignment_list = new List<string>();
        private Boolean Error = false;
        
        public Network_Design_Form_Core()
        {
            InitializeComponent();
            Demand_box.Enabled = false;
            Pot_xDock_Box.Enabled = false;
            Seller_Box.Enabled = false;
            Parameter_Box.Enabled = false;
            Presolved_box.Enabled = false;
            Outbut_loc.Enabled = false;
            Hub_Cov_Box.Enabled = false;
            send_button.Enabled = false;
            Run_CitybyCity.Checked= false;
            Full_Run.Checked = false;
            Partial_Run.Checked = false;
            _threshold.Enabled = false;
            Min_cap_courier.Enabled = false;
            Km_başı_paket.Enabled = false;
            Mahalle_xDock_Ataması.Enabled = false;
            Create_Dictionary_Month();
        }
        private void output_box_Enter(object sender, EventArgs e)
        {

        }
        
        

        private void Mahalle_xDock_Ataması_TextChanged(object sender, EventArgs e)
        {

        }

        
        

        private void _threshold_TextChanged(object sender, EventArgs e)
        {

        }

        private void Min_Cap_TextChanged(object sender, EventArgs e)
        {

        }

        private void Km_başı_paket_TextChanged(object sender, EventArgs e)
        {

        }
        private void yes_button_CheckedChanged(object sender, EventArgs e)
        {
            if (Full_Run.Checked)
            {
                Presolved_box.Enabled = false;
                Demand_box.Enabled = true;
                Pot_xDock_Box.Enabled = true;
                Partial_Run.Enabled = true;
                Seller_Box.Enabled = true;
                Parameter_Box.Enabled = true;
                Presolved_box.Enabled = false;
                Outbut_loc.Enabled = true;
                Hub_Cov_Box.Enabled = true;
                send_button.Enabled = true;
                _threshold.Enabled = true;
                Min_cap_courier.Enabled = true;
                Km_başı_paket.Enabled = true;
                Mahalle_xDock_Ataması.Enabled = false;

                full_run = true;
                partial_solution = false;
                only_cities = false;
                only_courier_assignments = false;
            }
           
        }
        private void no_button_CheckedChanged(object sender, EventArgs e)
        {
            if(Partial_Run.Checked)
            {
                Demand_box.Enabled = true;
                Pot_xDock_Box.Enabled = true;
                Partial_Run.Enabled = true;
                Seller_Box.Enabled = true;
                Parameter_Box.Enabled = true;
                Presolved_box.Enabled = true;
                Outbut_loc.Enabled = true;
                Hub_Cov_Box.Enabled = true;
                send_button.Enabled = true;
                Presolved_box.Enabled = true;
                Mahalle_xDock_Ataması.Enabled = true;
                _threshold.Enabled = true;
                Min_cap_courier.Enabled = true;
                Km_başı_paket.Enabled = true;

                full_run = false;
                partial_solution = true;
                only_cities = false;
                only_courier_assignments = false;


            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

            if (Run_CitybyCity.Checked)
            {
                Demand_box.Enabled = true;
                Pot_xDock_Box.Enabled = false;
                Partial_Run.Enabled = true;
                Seller_Box.Enabled = false;
                Parameter_Box.Enabled = true;
                Presolved_box.Enabled = false;
                Outbut_loc.Enabled = true;
                Hub_Cov_Box.Enabled = false;
                send_button.Enabled = true;
                _threshold.Enabled = true;
                Min_cap_courier.Enabled = true;
                Km_başı_paket.Enabled = true;
                Mahalle_xDock_Ataması.Enabled = false;

                full_run = true;
                partial_solution = false;
                only_cities = true;
                only_courier_assignments = false;
            }
        }
        private void Courier_Checked(object sender, EventArgs e)
        {
            if (Courier_Run.Checked)
            {
                Demand_box.Enabled = false;
                Pot_xDock_Box.Enabled = false;
                Partial_Run.Enabled = true;
                Seller_Box.Enabled = false;
                Parameter_Box.Enabled = false;
                Presolved_box.Enabled = false;
                Outbut_loc.Enabled = true;
                Hub_Cov_Box.Enabled = false;
                send_button.Enabled = true;
                _threshold.Enabled = true;
                Min_cap_courier.Enabled = true;
                Km_başı_paket.Enabled = true;
                Mahalle_xDock_Ataması.Enabled = true;


                full_run = false;
                partial_solution = false;
                only_cities = false;
                only_courier_assignments = true;

            }

        }
        private void Network_Design_Form_Load(object sender, EventArgs e)
        {
           
        }
        private void Demand_box_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            direct_initial = openFileDialog1.FileName;
            Demand_box.Text = direct_initial;
        }
        
        private void Pot_xdock_box_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = direct_initial;
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            var direct = openFileDialog1.FileName;
            Pot_xDock_Box.Text = direct;

        }

        private void Seller_box_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = direct_initial;
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            var direct = openFileDialog1.FileName;
            Seller_Box.Text = direct;
        }
        
        private void Parameter_box_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = direct_initial;
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            var direct = openFileDialog1.FileName;
            Parameter_Box.Text = direct;
        }
        private void Presolved_box_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = direct_initial;
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            var direct = openFileDialog1.FileName;
            Presolved_box.Text = direct;

        }
        private void Mahalle_xDock_Click(object sender, EventArgs e)
        {
            if(direct_initial != null)
            {
                openFileDialog1.InitialDirectory = direct_initial;
            }
            else
            {
                openFileDialog1.InitialDirectory = @"C:\";
            }
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            direct_initial = openFileDialog1.FileName;
            Mahalle_xDock_Ataması.Text = direct_initial;
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

            month_dict = dict;
        }
       
        private void Close_The_Form(object sender, FormClosingEventArgs e)
        {

        }
        private void Courier_Runner_Writer(Dictionary<xDocks,List<Mahalle>> mahalle_list, List<Double> parameter_list,String output_loc)
        {
            for (int i = 0; i < mahalle_list.Count; i++)
            {
                var courier_assignment = new Courier_Assignment(mahalle_list.ElementAt(i).Key, mahalle_list[mahalle_list.ElementAt(i).Key], parameter_list[0], parameter_list[1], parameter_list[2],parameter_list[3]);
                courier_assignment.Run_Assignment_Procedure();
                var list = courier_assignment.Return_Courier_Assignments();
                courier_assignment_list.AddRange(list);
            }
            var header_xdock_demand_point = "xDock İl,xDock İlçe,xDock Mahalle,xDock Enlem,xDock Boylam,Kurye Id,Atanan Mahalle,Mahalle İlçe,Mahalle Enlem,Mahalle Boylam,Mahalleye Götüreceği Paket,Tahmini Uzaklık,Kapasite Aşımı";
            var write_the_xdocks = new Csv_Writer(courier_assignment_list, "Kurye Atamaları", header_xdock_demand_point, output_loc);
            write_the_xdocks.Write_Records();
            var json_courier = courier_assignment_list.ToArray();
            
            var runtime = DateTime.Now.ToString(" dd MMMM HH;mm;ss ");
            var javaScriptSerializer = new JavaScriptSerializer();
            var resultJson = JsonConvert.SerializeObject(json_courier);
            File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Output of Courier Assignment  " + runtime + ".json", resultJson);
        }
        private void Outbut_loc_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();         
            var direct = folderBrowserDialog1.SelectedPath;
            Outbut_loc.Text = direct;
        }

        /// <summary>
        /// This function takes all the given parameters and Run types as an input and runs the relative parts of the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void send_button_Click(object sender, EventArgs e)
        {
            demand_file = Demand_box.Text ;
            pot_xDock_file = Pot_xDock_Box.Text;
            seller_file = Seller_Box.Text ;
            parameter_file = Parameter_Box.Text;
            presolved_xDock_file = Presolved_box.Text ;
            mahalle_xdock_file = Mahalle_xDock_Ataması.Text;

            if (Min_cap_courier.Text == "Default" || _threshold.Text== "Default" || Max_cap_courier.Text=="Default" || Km_başı_paket.Text== "Default")
            {
                Min_cap_courier.Text = "100";
                Max_cap_courier.Text = "150";
                _threshold.Text = "120";
                Km_başı_paket.Text = "2";
            }
            var courier_min_cap = Convert.ToDouble(Min_cap_courier.Text);
            var desired_efficiency = Convert.ToDouble(_threshold.Text);
            var courier_max_cap = Convert.ToDouble(Max_cap_courier.Text);
            var compensation = Convert.ToDouble(Km_başı_paket.Text);
            var courier_parameter_list = new List<Double> { courier_min_cap,courier_max_cap, desired_efficiency, compensation};
            if (Hub_Cov_Box.Text != "") hub_demand_coverage = Convert.ToDouble(Hub_Cov_Box.Text);
            directory = Outbut_loc.Text;


            var demand_point = new List<DemandPoint>();
            var potential_xDocks = new List<xDocks>();
            var hub_point = new List<Hub>();
            var xDocks = new List<xDocks>();
            var agency = new List<xDocks>();
            var hubs = new List<Hub>();
            var potential_hubs = new List<Hub>();
            var partial_xDocks = new List<xDocks>();
            var parameters = new List<Parameters>();
            var xDock_neighborhood_assignments = new Dictionary<xDocks, List<Mahalle>>();
            var total_json = new Dictionary<String, String[]>();
            //This variable decides which solution method will be run. If true; every city individually assigned, else regions are assigned as a whole
            var discrete_solution = true;

            //Creating String Array to store the inputs for creating a Json file
            String[] run_type = { full_run.ToString(), partial_solution.ToString(), only_cities.ToString(), only_courier_assignments.ToString() };
            String[] courier_params = { courier_min_cap.ToString(), courier_max_cap.ToString(), desired_efficiency.ToString(), compensation.ToString()};            
            String[] hub_coverage = { hub_demand_coverage.ToString()};
            
            //Will be revised
            if (full_run)
            {   
                var reader = new CSVReader(demand_file, pot_xDock_file, seller_file, parameter_file, "");

                reader.Read();
                var error_list = reader.Get_Input_Failure_List();
                if (error_list.Count == 1)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_XDocks();
                    hub_point = reader.Get_Hub_Points();
                    agency = reader.Get_Agency();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();
                    var distance_matrix = reader.Get_Distance_Matrix();


                    var runner = new Runner(demand_point, potential_xDocks,hub_point, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, directory, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list,distance_matrix);
                    (xDocks, hubs) = await Task.Run(() => runner.Run());
                    //Console.ReadKey();
                }
                else
                {
                    Error = true;
                    File.WriteAllLines(directory+"\\Error_Log.csv", error_list.Select(x => string.Join(",", x)), Encoding.UTF8);
                    //Report error and create a log file
                }
                total_json = reader.Get_Input_Dictionary();
                Create_Write_Json_File(total_json, run_type, courier_params, hub_coverage);
                
            }
            else if (only_courier_assignments)
            {
                var partial_reader = new CSVReader("", "", "", "", mahalle_xdock_file);
                partial_reader.Read_xDock_Neighborhood_Assignments();
                var error_list = partial_reader.Get_Input_Failure_List();
                if (error_list.Count == 1)
                {
                    xDock_neighborhood_assignments = partial_reader.Get_xDock_neighborhood_Assignments();
                    Courier_Runner_Writer(xDock_neighborhood_assignments, courier_parameter_list, directory);
                }
                else
                {
                    Error = true;
                    File.WriteAllLines(directory + "\\Error_Log.csv", error_list.Select(x => string.Join(",", x)), Encoding.UTF8);
                    //report error and create a log file
                }
                total_json = partial_reader.Get_Input_Dictionary();
                Create_Write_Json_File(total_json, run_type, courier_params, hub_coverage);

            }
            else if (partial_solution)
            {
                var reader = new CSVReader(demand_file, pot_xDock_file, seller_file, parameter_file, "");

                reader.Read();
                var error_list = reader.Get_Input_Failure_List();
                if (error_list.Count == 1)
                {
                    demand_point = reader.Get_Demand_Points();
                    potential_xDocks = reader.Get_XDocks();
                    hub_point = reader.Get_Hub_Points();
                    agency = reader.Get_Agency();
                    var prior_small_sellers = reader.Get_Prior_Small_Sellers();
                    var regular_small_sellers = reader.Get_Regular_Small_Sellers();
                    var prior_big_sellers = reader.Get_Prior_Big_Sellers();
                    var regular_big_sellers = reader.Get_Regular_Big_Sellers();
                    var parameter_list = reader.Get_Parameter_List();
                    var distance_matrix = reader.Get_Distance_Matrix();

                    var partial_reader = new CSVReader("", presolved_xDock_file, "", "", mahalle_xdock_file);
                    partial_reader.Read_Partial_Solution_Xdocks();
                    partial_xDocks = partial_reader.Get_Partial_Solution_Xdocks();
                    partial_reader.Read_xDock_Neighborhood_Assignments();
                    xDock_neighborhood_assignments = partial_reader.Get_xDock_neighborhood_Assignments();
                    var runner_partial = new Runner(demand_point, potential_xDocks, hub_point, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution, directory, hub_demand_coverage, only_cities, xDock_neighborhood_assignments, courier_parameter_list,distance_matrix);
                    (xDocks, hubs) = await Task.Run(() => runner_partial.Run());

                    total_json = partial_reader.Get_Input_Dictionary();
                    
                    //Console.ReadKey();
                }
                else
                {
                    Error = true;
                    File.WriteAllLines(directory + "\\Error_Log.csv", error_list.Select(x => string.Join(",", x)), Encoding.UTF8);
                    //report error and create a log file
                }
                var new_total_json = reader.Get_Input_Dictionary();
                total_json = total_json.Union(new_total_json).ToDictionary(a => a.Key, b => b.Value);
                Create_Write_Json_File(total_json, run_type, courier_params, hub_coverage);

            }
            if (!Error)
            {
                var path = directory + "\\";
                var dirname = new DirectoryInfo(path).Name;
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Information;
                MessageBox.Show("Çalıştırma Bitti! Sonuçları " + "'" + dirname + "'" + " dosyasında bulabilirsiniz.", "Bilgi", buttons, icon);
            }
            else
            {
                var path = directory + "\\";
                var dirname = new DirectoryInfo(path).Name;
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Information;
                MessageBox.Show("Input Dosyasında okuma hatası tespit edildi. Ayrıntıları " + "'" + dirname + "'" + " dosyasında bulabilirsiniz.", "Bilgi", buttons, icon);
            }
            
            
        }
        private void Create_Write_Json_File(Dictionary<String,String[]> total_input, String[] run_type, String[] courier, String[] Hub)
        {
            total_input.Add("Run Type", run_type);
            total_input.Add("Courier Parameters", courier);
            total_input.Add("Hub Coverage", Hub);

            var resultjson = JsonConvert.SerializeObject(total_input);
            var runtime = DateTime.Now.ToString(" dd MMMM HH;mm;ss ");
            String list_to_write = "";

            if (Convert.ToBoolean(run_type[0]) && Convert.ToBoolean(run_type[2]))
            {
                var cities = total_input["Parameters"].ToList();
                var city = new List<String>();
                
                for (int i = 0; i < cities.Count; i++)
                {
                    var stringset= cities[i].Split(",");
                    if (stringset[2]=="1")
                    {
                        city.Add(stringset[0]);

                    }
                    list_to_write = String.Join(",",city.ToArray());
                }

                if (city.Count<=5)
                {   
                    File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Input of " + list_to_write  + runtime + ".json", resultjson);
                }
                else
                {
                    File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Input of multiple cities" + runtime + ".json", resultjson);
                }
                
            }
            else if (Convert.ToBoolean(run_type[3]))
            {
                File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Input of  Courier Assignment Run Type " + runtime + ".json", resultjson);

            }
            else if (Convert.ToBoolean(run_type[1]))
            {
                File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Input of Partial Run Type" + runtime + ".json", resultjson);
            }
            else
            {
                File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Input of Full Run" + runtime + ".json", resultjson);
            }
             
        }
        private void Retrospective_Run(object sender, EventArgs e)
        {
            new Retrorespective_Run().Show();
            this.Hide();
        }
    }
}
