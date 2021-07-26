using FMLMDelivery.MetaHeuristics;
using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;


namespace FMLMDelivery.Classes
{
    public class Runner
    {
        private Boolean is_genetic = true;
        private List<xDocks> xDocks;
        private List<DemandPoint> demand_Points;
        private List<xDocks> agency;
        private List<Seller> _prior_small_seller;
        private List<Seller> _prior_big_seller;
        private List<Seller> _regular_small_seller;
        private double total_demand;
        private List<String> writer_seller = new List<String>();
        private List<xDocks>  new_xDocks = new List<xDocks>();
        private List<Hub> potential_hub_locations = new List<Hub>();
        private List<String> writer_xdocks = new List<string>();
        private List<DemandPoint> city_points = new List<DemandPoint>();
        private List<xDocks> pot_xDock_loc = new List<xDocks>();
        private List<String> temp_writer = new List<String>();
        private List<xDocks> temp_xDocks = new List<xDocks>();
        private List<Hub> temp_hubs = new List<Hub>();       
        private List<String> temp_stats = new List<String>();
        private List<String> stats_writer = new List<String>();
        private List<Double> gap_list = new List<double>();
        private List<xDocks> partial_xdocks = new List<xDocks>();
        private Boolean partial_run = new Boolean();
        private List<Parameters> _parameters;
        private Boolean _discrete_solution;
        private String _output_files;
        private double _hub_demand_coverage;
        private bool _only_cities;
        private List<Double> _courier_parameters;
        private Dictionary<xDocks, List<Mahalle>> _courier_document;
        private List<String> courier_writer = new List<String>();
        private Dictionary<String, String[]> total_json_log = new Dictionary<string, string[]>();
        private List<String> xdock_mahalle;
        private Double max_chute_capacity = 150;
        private Dictionary<String, List<Double>> _distance_matrix;



        public Runner(List<DemandPoint> _demand_points, List<xDocks> _xDocks,List<Hub> _Hubs,List<xDocks> _partial_xdocks, List<xDocks> _agency, List<Seller> prior_small, List<Seller> regular_small, List<Seller> prior_big, List<Seller> regular_big,List<Parameters> parameters,Boolean _partial_run,Boolean discrete_solution, string Output_files,double hub_demand_coverage,Boolean only_cities,Dictionary<xDocks,List<Mahalle>> courier_file, List<Double> courier_parameters,Dictionary<String,List<Double>> distance_matrix)
        {
            partial_xdocks=_partial_xdocks;
            xDocks = _xDocks;
            demand_Points = _demand_points;
            potential_hub_locations = _Hubs;
            agency = _agency;
            _prior_big_seller = prior_big;
            _prior_small_seller = prior_small;
            _regular_small_seller = regular_small;
            _parameters = parameters;
            partial_run = _partial_run;
            _discrete_solution = discrete_solution;
            _output_files = Output_files;
            _hub_demand_coverage = hub_demand_coverage;
            _only_cities = only_cities;
            _courier_parameters = courier_parameters;
            _courier_document = courier_file;
            _distance_matrix = distance_matrix;
        }

        private Tuple<List<xDocks>, List<String>,List<String>> Run_Demand_Point_xDock_Model(List<DemandPoint> demandPoints, List<xDocks> xDocks,Double demand_cov, String key,double gap)
        {   var stats = new List<String>();
            var _demand_points = demandPoints;
            var _pot_xDocks = xDocks;
            var _key = key;
            var min_model_model = true;
            var demand_weighted_model = false;
            //Phase 2 takes the solution of min_model as an input and solve same question with demand weighted objective
            var phase_2 = false;
            var demand_covarage = demand_cov;
            var objVal = 0.0;
            var new_xDocks = new List<xDocks>();
            var p = 0;
            var first_phase = new DemandxDockModel(_demand_points, _pot_xDocks, _key, demand_weighted_model, min_model_model, demand_covarage, phase_2, p,false, gap, 3600);
            first_phase.Run();
            var _status_check = first_phase.Return_Status();
            while (!_status_check)
            {
                demand_covarage -= 0.01;
                first_phase = new DemandxDockModel(_demand_points, _pot_xDocks, _key, demand_weighted_model, min_model_model, demand_covarage, phase_2, p, false, gap,3600);
                first_phase.Run();
                _status_check = first_phase.Return_Status();
            }
            
            objVal = first_phase.GetObjVal(); 
            new_xDocks = first_phase.Return_XDock();
            stats.AddRange(first_phase.Get_Model_Stats_Info());
            var min_num = first_phase.Return_Num_Xdock();
            var opened_xDocks = first_phase.Return_Opened_xDocks();
            var assignments = first_phase.Return_Assignments();
            var heuristic_assignments = first_phase.Return_Heuristic_Assignment();
            var list_assign = new List<List<Double>>();

            //if (is_genetic)
            //{
            //    var heuristic = new Genetic_Algorithm(opened_xDocks, _pot_xDocks, _demand_points, _parameters, demand_covarage, min_num, key);
            //    heuristic.Run();
            //    (opened_xDocks, assignments) = heuristic.Return_Best_Solution();
            //    list_assign = heuristic.Return_Assignments_District();
            //    demand_covarage = heuristic.Return_Covered_Demand();
            //}
            //var xdocks = new List<xDocks>();
            //(xdocks, xdock_mahalle) = Print_Solutions(opened_xDocks, list_assign, _demand_points, _pot_xDocks);


            //if (!is_genetic)
            //{
            //    var heuristic1 = new Simulated_Annealing(opened_xDocks, _pot_xDocks, _demand_points, _parameters, demand_covarage, min_num, key);
            //    heuristic1.Run();
            //    (opened_xDocks, assignments, demand_covarage) = heuristic1.Return_Heuristic_Results();
            //}

            //var heuristic_particle = new Particle_Swarm(opened_xDocks, _pot_xDocks, _demand_points, _parameters, demand_covarage, min_num, key);
            //heuristic_particle.Run();


            // Part 2 for county - xDock pair
            min_model_model = false;
            demand_weighted_model = true;
            phase_2 = true;
            first_phase = new DemandxDockModel(_demand_points, _pot_xDocks, _key, demand_weighted_model, min_model_model, demand_covarage, phase_2, min_num, true, gap, 3600, false);
            first_phase.Provide_Initial_Solution(opened_xDocks, assignments);
            first_phase.Run();
            objVal = first_phase.GetObjVal();
            //xDocks are assigned
            new_xDocks = first_phase.Return_XDock();            
            stats.AddRange(first_phase.Get_Model_Stats_Info());
            var assignment_dictionary = first_phase.Return_xDock_Mahalle();
            xdock_mahalle = first_phase.Get_Xdock_County_Info();
            //Run_Courier_Problem(assignment_dictionary);

            return Tuple.Create(new_xDocks, xdock_mahalle, stats);
        }
        private Tuple<List<xDocks>, List<String>> Print_Solutions(List<Double> opened_xdocks, List<List<Double>> assignments, List<DemandPoint> demand_points, List<xDocks> pot_xdocks)
        {
            var loopcount = (assignments.Count / pot_xdocks.Count);
            var count = 0;
            var list_xdocks = new List<xDocks>();
            var list_demand = new List<DemandPoint>();
            var record_list = new List<String>();
            for (int i = 0; i < opened_xdocks.Count; i++)
            {
                if (opened_xdocks[i] == 1)
                {   var xdock_city = pot_xdocks[i].Get_City();
                    var xdock_district = pot_xdocks[i].Get_District();
                    var xdock_id = pot_xdocks[i].Get_Id();
                    var xdock_lat = pot_xdocks[i].Get_Latitude();
                    var xdock_long = pot_xdocks[i].Get_Longitude();
                    list_xdocks.Add(pot_xdocks[i]);
                    count += 1;
                    for (int k = 0; k < assignments[count-1].Count; k++)
                    {
                        if (assignments[count-1][k] == 1)
                        {
                            var x_dock_ranking = "Xdock" + count;
                            var demand_point_city = demand_points[k].Get_City();
                            var demand_point_district = demand_points[k].Get_District();
                            var demand_point_id = demand_points[k].Get_Id();
                            var demand_point_lat = demand_points[k].Get_Latitude();
                            var demand_point_long = demand_points[k].Get_Longitude();
                            var demand = demand_points[k].Get_Demand();
                            var distance_xdock_county = 0;
                            var result = $"{xdock_city},{xdock_district},{xdock_id},{xdock_lat},{xdock_long},{demand_point_city},{demand_point_district},{demand_point_id},{demand_point_lat},{demand_point_long},{distance_xdock_county},{demand}";
                            record_list.Add(result);
                        }
                    }

                }
            }

            return Tuple.Create(list_xdocks, record_list);
            
        }
        private void Run_Courier_Problem(Dictionary<xDocks,List<Mahalle>> mahalle_assigments)
        {
            for (int i = 0; i < mahalle_assigments.Count; i++)
            {
                var courier_assignment = new Courier_Assignment(mahalle_assigments.ElementAt(i).Key, mahalle_assigments.ElementAt(i).Value, _courier_parameters[0], _courier_parameters[1], _courier_parameters[2], _courier_parameters[3]);
                courier_assignment.Run_Assignment_Procedure();
                var list = courier_assignment.Return_Courier_Assignments();
                courier_writer.AddRange(list);
            }
        }
        private Tuple<List<DemandPoint>, List<xDocks>> Get_City_Information(string key)
        {
            var city_points = new List<DemandPoint>();
            var pot_xDock_loc = new List<xDocks>();
            for (int i = 0; i < demand_Points.Count; i++)
            {
                if (demand_Points[i].Get_City() == key)
                {
                    city_points.Add(demand_Points[i]);
                }
            }
            for (int j = 0; j < xDocks.Count; j++)
            {
                if (xDocks[j].Get_City() == key)
                {
                    pot_xDock_loc.Add(xDocks[j]);
                }
            }
            return Tuple.Create(city_points, pot_xDock_loc);
        }

        private void Partial_Run(string key, double demand_coverage, double gap)
        {
            (city_points, pot_xDock_loc) = Get_City_Information(key);
            var elimination_phase = new PointEliminator(city_points, pot_xDock_loc);
            elimination_phase.Run();
            pot_xDock_loc = elimination_phase.Return_Filtered_xDocx_Locations();
            (temp_xDocks, temp_writer, temp_stats) = Run_Demand_Point_xDock_Model(city_points, pot_xDock_loc, demand_coverage, key, gap);
            stats_writer.AddRange(temp_stats);
            new_xDocks.AddRange(temp_xDocks);
            writer_xdocks.AddRange(temp_writer);
        }
        //Will be removed
        //private void Add_Main_Hub(int i,string _district,string _id, double _capacity, double _max_chute)
        //{
        //    var city = xDocks[i].Get_City();
        //    var district = xDocks[i].Get_District();
        //    var id = xDocks[i].Get_Id();
        //    var region = xDocks[i].Get_Region();
        //    var longitude = xDocks[i].Get_Longitude();
        //    var latitude = xDocks[i].Get_Latitude();
        //    //var dist_thres = xDocks[i].Get_Distance_Threshold();
        //    var hub_point = xDocks[i].Get_Hub_Point();
        //    var capacity = _capacity;
        //    var chute_cap = _max_chute;
        //    var already_opened = true;
        //    var open_hub = new Hub(city, district, id, region, longitude, latitude,0,hub_point, capacity, chute_cap,already_opened);
        //    potential_hub_locations.Add(open_hub);
        //}
        //Will be removed
        //private void Add_Already_Open_Main_Hubs()
        //{
        //    for (int i = 0; i < xDocks.Count; i++)
        //    {
        //        if (xDocks[i].Get_District() == "TUZLA" && xDocks[i].Get_Id() == "KİRAZ")
        //        {
        //            Add_Main_Hub(i, "TUZLA", "KİRAZ", 800000, 175);
        //        }
        //        if (xDocks[i].Get_District() == "ESENYURT" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "ESENYURT", "Merkez", 800000, 175);
        //        }
        //        //if (xDocks[i].Get_District() == "ESENLER" && xDocks[i].Get_Id() == "Merkez")
        //        //{
        //        //    Add_Main_Hub(i, "ESENLER", "Merkez", 2000000, 175);
        //        //}
        //        if (xDocks[i].Get_District() == "KAĞITHANE" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            //Add_Main_Hub(i, "KAĞITHANE", "Merkez", 2000000, 175);
        //        }
        //        else if (xDocks[i].Get_District() == "YÜREĞİR" && xDocks[i].Get_Id() == "İNCİRLİK CUMHURİYET")
        //        {
        //            Add_Main_Hub(i, "YÜREĞİR", "İNCİRLİK CUMHURİYET", 1000000, 100);
        //        }
        //        else if (xDocks[i].Get_District() == "BAŞAKŞEHİR" && xDocks[i].Get_Id() == "İKİTELLİ OSB")
        //        {
        //            Add_Main_Hub(i, "BAŞAKŞEHİR", "İKİTELLİ OSB", 2000000,96);
        //        }
        //        else if (xDocks[i].Get_District() == "ÇEKMEKÖY" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "ÇEKMEKÖY", "Merkez", 2000000, 96);
        //        }
        //        else if (xDocks[i].Get_District() == "KEMALPAŞA" && xDocks[i].Get_Id() == "KIZILÜZÜM")
        //        {
        //            Add_Main_Hub(i, "KEMALPAŞA", "KIZILÜZÜM", 1000000,98);
        //        }
        //        else if(xDocks[i].Get_District() == "KAHRAMANKAZAN" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "KAHRAMANKAZAN", "Merkez", 1000000, 175);
        //        }else if (xDocks[i].Get_District() == "KAYAPINAR" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "KAYAPINAR", "Merkez", 1000000, 100);
        //        }else if(xDocks[i].Get_District() == "YAKUTİYE" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "YAKUTİYE", "Merkez", 200000, 100);
        //        }else if(xDocks[i].Get_District() == "MERZİFON" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            //Add_Main_Hub(i, "MERZİFON", "Merkez", 50000, 100);
        //        }
        //        else if (xDocks[i].Get_District() == "MURATPAŞA" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "MURATPAŞA", "Merkez", 1000000, 175);
        //        }
        //        else if (xDocks[i].Get_District() == "NİLÜFER" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //            Add_Main_Hub(i, "NİLÜFER", "Merkez", 1000000, 175);
        //        }
        //        else if (xDocks[i].Get_City() == "DENİZLİ" && xDocks[i].Get_District() == "MERKEZ")
        //        {
        //            Add_Main_Hub(i, "DENİZLİ", "Merkez", 1000000, 175);
        //        }
        //        else if ((xDocks[i].Get_District() == "AKÇAKOCA" && xDocks[i].Get_Id() == "Merkez"))
        //        {
        //            Add_Main_Hub(i, "AKÇAKOCA", "Merkez", 1000000, 175);
        //        }
        //        else if ((xDocks[i].Get_District() == "KARATAY" && xDocks[i].Get_Id() == "Merkez"))
        //        {
        //            Add_Main_Hub(i, "KARATAY", "Merkez", 1000000, 175);
        //        }
        //        else if (xDocks[i].Get_District() == "YILDIRIM" && xDocks[i].Get_Id() == "Merkez")
        //        {
        //           // Add_Main_Hub(i, "YILDIRIM", "Merkez", 1000000, 175);
        //        }
        //        //else if (xDocks[i].Get_District() == "ÇANKAYA" && xDocks[i].Get_Id() == "Merkez")
        //        //{
        //        //    Add_Main_Hub(i, "ÇANKAYA", "Merkez", 1000000, 175);
        //        //}
        //    }
        //}

        private Tuple<List<Seller>,List<Seller>> Second_Phase()
        {
            total_demand = 0;
            for (int i = 0; i < _prior_small_seller.Count; i++)
            {
                total_demand += _prior_small_seller[i].Get_Demand();
            }
            var assigned_prior_sellers = new List<Seller>();
            var assigned_regular_sellers = new List<Seller>();
            var second_phase = new SmallSellerxDockModel(_prior_small_seller, new_xDocks, true);
            second_phase.Run();
            assigned_prior_sellers = second_phase.Return_Assigned_Seller();
            new_xDocks = second_phase.Return_Updated_xDocks();
            var covered_demand = second_phase.Return_Covered_Demand();
            var remaining_demand = total_demand - covered_demand;
            writer_seller.AddRange(second_phase.Get_Seller_Xdock_Info());
            stats_writer.AddRange(second_phase.Get_Small_Seller_Model_Stat());

            second_phase = new SmallSellerxDockModel(_regular_small_seller, new_xDocks, false, remaining_demand);
            second_phase.Run();
            assigned_regular_sellers = second_phase.Return_Assigned_Seller();
            new_xDocks = second_phase.Return_Updated_xDocks();
            var cov_demand = second_phase.Return_Covered_Demand();
            writer_seller.AddRange(second_phase.Get_Seller_Xdock_Info());
            stats_writer.AddRange(second_phase.Get_Small_Seller_Model_Stat());
            var header = "xDocks İl,xDocks İlçe,xDocks Mahalle,xDocks Enlem,xDokcs Boylam,Seller İsmi,Seller İl,Seller İlçe,Seller Uzaklık,Seller Gönderi Adeti";
            //var stats_header = "Part,Model,Demand Coverage,Status,Time,Gap";
            //var writer_small_seller = new Csv_Writer(writer_seller, "Küçük Tedarikçi xDock Atamaları", header, _output_files);
           // var stat_writer = new Csv_Writer(stats_writer, "Statü", stats_header, _output_files);
            //writer_small_seller.Write_Records();
            //if (_only_cities) stat_writer.Write_Records();


            return Tuple.Create(assigned_prior_sellers, assigned_regular_sellers);
        }

        private Tuple<List<Hub>, List<Seller>> Third_Phase()
        {
            var new_hubs = new List<Hub>();
            var assigned_big_sellers = new List<Seller>();
            var min_model_model = true;
            var demand_weighted_model = false;
            var phase_2 = false;
            //xDock-Seller-Hub Assignment//Will be revised
            var third_phase = new xDockHubModel(new_xDocks, potential_hub_locations, _prior_big_seller, demand_weighted_model, min_model_model, _hub_demand_coverage, phase_2, 0,_distance_matrix);
            third_phase.Run();
            var assignments = third_phase.Return_Initial_Assignments();
            var num_clusters = third_phase.Return_num_Hubs();
            min_model_model = false;
            demand_weighted_model = true;
            phase_2 = true;//Will be revised
            third_phase = new xDockHubModel(new_xDocks, potential_hub_locations, _prior_big_seller, demand_weighted_model, min_model_model, _hub_demand_coverage, phase_2, num_clusters,_distance_matrix);
            third_phase.Provide_Intitial_Solution(assignments);
            third_phase.Run();
            var objVal = third_phase.GetObjVal();
            new_hubs = third_phase.Return_New_Hubs();
            assigned_big_sellers = third_phase.Return_Assigned_Big_Sellers();
            var header_hub = "Hub İl,Hub İlçe,Hub Mahalle,Hub Boylam,Hub Enlem,Atanma Türü,Unique Id,İl,İlçe,Mahalle,LM Talep/Gönderi,FM Gönderi,Distance";
            var stats_header = "Part,Model,Demand Coverage,Status,Time,Gap";
            stats_writer.AddRange(third_phase.Get_Xdock_Hub_Stats());
            var writer_hub_seller = new Csv_Writer(third_phase.Get_Hub_Xdock_Seller_Info(), "Büyük Tedarikçi xDock Hub Atamaları", header_hub, _output_files);
            //var stat_writer = new Csv_Writer(stats_writer, "Statü", stats_header, _output_files);
            writer_hub_seller.Write_Records();
            //stat_writer.Write_Records();

            return Tuple.Create(new_hubs, assigned_big_sellers);
        }

        public Tuple<List<xDocks>, List<Hub>> Run()
        {   var new_hubs = new List<Hub>();
            var assigned_prior_sellers = new List<Seller>();
            var assigned_regular_sellers = new List<Seller>();
            var assigned_big_sellers = new List<Seller>();
            /* This method firstly calls Demand-xDock model with the minimum xDock objective in given demand covarage. After solving the model with this object, the method takes the number of xDock
             * and re-solved the model with demand-distance weighted objective given the number of xDocks and identifies the optimal locations for xDocks. After xDocks are identified, xDock-Hub model
             * is called with the minimum hub objective and after the model is solved, with the given numer of hub the model is resolved in order to obtain demand-distance weighted locations for hubs. 
             */
            
            if (!partial_run)
            {
                for (int i = 0; i < _parameters.Count; i++)
                {   
                    if (_parameters[i].Get_Activation())
                    {
                        Partial_Run(_parameters[i].Get_Key(), 1.0, Gap_Converter_1(_parameters[i].Get_Size()));
                    }
                }


                var header_xdock_demand_point = "xDocks İl,xDocks İlçe,xDock Mahalle,xDocks Enlem,xDokcs Boylam,Talep Noktası İl,Talep Noktası ilçe,Talep Noktası Mahalle,Talep Noktası Enlem,Talep Noktası Boylam,Uzaklık,Talep Noktası Talebi";
                var write_the_xdocks = new Csv_Writer(writer_xdocks, "Mahalle xDock Atamaları", header_xdock_demand_point, _output_files);
                write_the_xdocks.Write_Records();
                var json_xdocks = writer_xdocks.ToArray();
                total_json_log.Add("Mahalle xDock Atamaları", json_xdocks);

                string[] new_header_already_opened = { "İl", "İlçe", "Mahalle", "Bölge", "Enlem", "Boylam", "Açık xDock veya Acente", "Minimum Kapasite", "LM Hacim", "FM Hacim" };
                String header_already_opened = String.Join(",", new_header_already_opened) + Environment.NewLine;
                String csv_new = header_already_opened + String.Join(Environment.NewLine, new_xDocks.Select(d => $"{d.Get_City()},{d.Get_District()},{d.Get_Id()},{d.Get_Region()},{d.Get_Latitude()},{d.Get_Longitude()},{d.If_Already_Opened()},{d.Get_Min_Cap()},{d.Get_LM_Demand()},{d.Get_FM_Demand()}"));
                System.IO.File.WriteAllText(@"" + _output_files + "\\Kısmi Çalıştırma Dosyası.csv", csv_new, Encoding.UTF8);
                string[] json_partial_run = csv_new.Split("\r\n");
                total_json_log.Add("Kısmi Çalıştırma Dosyası", json_partial_run);


                if (!_only_cities)
                {   //Seller-xDock Assignment
                    (assigned_prior_sellers, assigned_regular_sellers) = Second_Phase();
                    //xDock-Seller-Hub Assignment
                    (new_hubs, assigned_big_sellers) = Third_Phase();
                }
            }
            else
            {   new_xDocks = partial_xdocks;
                //Courier Assignment 
                //Run_Courier_Problem(_courier_document);
                //Seller-xDock Assignment
                (assigned_prior_sellers, assigned_regular_sellers) = Second_Phase();
                //xDock-Seller-Hub Assignment
                (new_hubs, assigned_big_sellers) = Third_Phase();
            }

            if (!_only_cities)
            {
                //Seller-xDock Assignment
                string[] new_hubs_headers = { "İl", "İlçe", "Mahalle", "Enlem", "Boylam", "LM Talep", "FM Gönderi" };
                String headers_hub = String.Join(",", new_hubs_headers) + Environment.NewLine;
                String csv7 = headers_hub + String.Join(Environment.NewLine, new_hubs.Select(d => $"{d.Get_City()},{d.Get_District()},{d.Get_Id()},{d.Get_Latitude()},{d.Get_Longitude()},{d.Get_LM_Capacity()},{d.Get_FM_Capacity()}"));
                System.IO.File.WriteAllText(@"" + _output_files + "\\Açılmış Hublar Listesi.csv", csv7, Encoding.UTF8);
                string[] json_opened_hub = csv7.Split("\r\n");
                total_json_log.Add("Açılmış Hublar Listesi", json_opened_hub);


                string[] big_seller = { "İsim", "ID", "Enlem", "Boylam", "Gönderi" };
                String big_s = String.Join(",", big_seller) + Environment.NewLine;
                String csv5 = big_s + String.Join(Environment.NewLine, assigned_big_sellers.Select(d => $"{d.Get_Name()},{d.Get_Id()},{d.Get_Latitude()},{d.Get_Longitude()},{d.Get_Demand()}"));
                System.IO.File.WriteAllText(@"" + _output_files + "\\Atanmış Büyük Tedarikçiler.csv", csv5, Encoding.UTF8);
                string[] json_assigned_big_sellers = csv5.Split("\r\n");
                total_json_log.Add("Atanmış Büyük Tedarikçiler", json_assigned_big_sellers);


                string[] prior_small_seller = { "İsim", "ID", "Enlem", "Boylam", "Gönderi" };
                String p_small = String.Join(",", prior_small_seller) + Environment.NewLine;
                String csv3 = p_small + String.Join(Environment.NewLine, assigned_prior_sellers.Select(d => $"{d.Get_Name()},{d.Get_Id()},{d.Get_Latitude()},{d.Get_Longitude()},{d.Get_Demand()}"));
                System.IO.File.WriteAllText(@"" + _output_files + "\\Atanmış Öncelikli Küçük Tedarikçiler.csv", csv3, Encoding.UTF8);
                string[] json_prior_small_seller = csv3.Split("\r\n");
                total_json_log.Add("Atanmış Öncelikli Küçük Tedarikçiler", json_prior_small_seller);

                string[] regular_small_seller = { "İsim", "ID", "Enlem", "Boylam", "Gönderi" };
                String r_small = String.Join(",", regular_small_seller) + Environment.NewLine;
                String csv4 = r_small + String.Join(Environment.NewLine, assigned_regular_sellers.Select(d => $"{d.Get_Name()},{d.Get_Id()},{d.Get_Latitude()},{d.Get_Longitude()},{d.Get_Demand()}"));
                System.IO.File.WriteAllText(@"" + _output_files + "\\Atanmış Sıradan Küçük Tedarikçiler.csv", csv4, Encoding.UTF8);
                string[] json_regular_small_seller = csv4.Split("\r\n");
                total_json_log.Add("Atanmış Sıradan Küçük Tedarikçiler", json_regular_small_seller);
            }

            string[] new_xdocks_headers_2 = { "İl", "İlçe", "Mahalle", "Enlem", "Boylam", "LM Talep", "FM Gönderi", "Açık xDock veya Acente" };
            String headers_xdock_2 = String.Join(",", new_xdocks_headers_2) + Environment.NewLine;
            String csv2 = headers_xdock_2 + String.Join(Environment.NewLine, new_xDocks.Select(d => $"{d.Get_City()},{d.Get_District()},{d.Get_Id()},{d.Get_Latitude()},{d.Get_Longitude()},{d.Get_LM_Demand()},{d.Get_FM_Demand()},{d.If_Already_Opened()}"));
            System.IO.File.WriteAllText(@"" + _output_files + "\\Açılmış xDocklar Listesi.csv", csv2, Encoding.UTF8);
            string[] json_opened_xdocks = csv2.Split("\r\n");
            total_json_log.Add("Açılmış xDocklar Listesi", json_opened_xdocks);

            var header_courier = "xDock İl,xDock İlçe,xDock Mahalle,xDock Enlem,xDock Boylam,Kurye Id,Atanan Mahalle,Mahalle İlçe,Mahalle Enlem,Mahalle Boylam,Mahalleye Götüreceği Paket,Tahmini Uzaklık,Kapasite Aşımı";
            var write_courier = new Csv_Writer(courier_writer, "Kurye Atamaları", header_courier, _output_files);
            write_courier.Write_Records();
            var json_courier_assignments = courier_writer.ToArray();
            total_json_log.Add("Kurye Atamaları", json_courier_assignments);

            var stats_header = "Part,Model,Demand Coverage,Status,Time,Gap";
            var stat_writer = new Csv_Writer(stats_writer, "Statü", stats_header, _output_files);
            stat_writer.Write_Records();
            var json_stats = stats_writer.ToArray();
            total_json_log.Add("Statü", json_stats);

            Create_Output_Log_Json(total_json_log);



            Console.WriteLine("Hello World!");


                return Tuple.Create(new_xDocks, new_hubs);

        }
        private Double Gap_Converter_1(String Size)
        {   var gap = 0.0;
            if (Size == "Small")
            {
                gap = 0.01;
            }
            else
            {
                gap = 0.025;
            }

            return gap;
        }
        //Will be removed
        //private List<Hub> Convert_to_Potential_Hubs(List<xDocks> new_XDocks)
        //{
        //    var potential_Hubs = new List<Hub>();
        //    var city_demand_dictionary = new Dictionary<String, Double>();
        //    var disabled = new List<String> { "ŞIRNAK", "SİİRT", "HAKKARİ", "KİLİS","KAHRAMANMARAŞ" };
        //    for (int i = 0; i < new_xDocks.Count; i++)
        //    {
        //        if (!city_demand_dictionary.ContainsKey(new_xDocks[i].Get_City()))
        //        {
        //            city_demand_dictionary.Add(new_xDocks[i].Get_City(), new_xDocks[i].Get_LM_Demand());
        //        }
        //        else
        //        {
        //            var old_demand = city_demand_dictionary[new_xDocks[i].Get_City()];
        //            var new_demand = new_xDocks[i].Get_LM_Demand();
        //            city_demand_dictionary[new_XDocks[i].Get_City()] = old_demand + new_demand;
        //        }
        //    }

        //    for (int i = 0; i < new_XDocks.Count; i++)
        //    {
        //        if (!disabled.Contains(new_XDocks[i].Get_City()))
        //        {
        //            var city = new_XDocks[i].Get_City();
        //            var district = new_XDocks[i].Get_District();
        //            var id = new_XDocks[i].Get_Id();
        //            var region = new_XDocks[i].Get_Region();
        //            var longitude = new_XDocks[i].Get_Longitude();
        //            var latitude = new_XDocks[i].Get_Latitude();
        //            //var dist_thres = new_XDocks[i].Get_Distance_Threshold();
        //            var hub_point = new_XDocks[i].Get_Hub_Point();
        //            var capacity = 2000000;
        //            //if (city_demand_dictionary[new_xDocks[i].Get_City()] <=60000) capacity = 100000;
        //            var chute_cap = max_chute_capacity;
        //            var already_opened = false;
        //            var potential_hub = new Hub(city, district, id, region, longitude, latitude,0, hub_point, capacity, chute_cap,already_opened);
        //            potential_Hubs.Add(potential_hub);
        //        }
        //    }

        //    return potential_Hubs;
        //}
        //Will be removed
        //private List<Hub> Add_Istanbul_Locations(List<xDocks> total_pot_xdocks)
        //{
        //    var potential_Hubs = new List<Hub>();
        //    for (int i = 0; i < total_pot_xdocks.Count; i++)
        //    {
        //        if (total_pot_xdocks[i].Get_City()=="İSTANBUL AVRUPA" || total_pot_xdocks[i].Get_City() == "İSTANBUL ASYA")
        //        {
        //            var city = total_pot_xdocks[i].Get_City();
        //            var district = total_pot_xdocks[i].Get_District();
        //            var id = total_pot_xdocks[i].Get_Id();
        //            var region = total_pot_xdocks[i].Get_Region();
        //            var longitude = total_pot_xdocks[i].Get_Longitude();
        //            var latitude = total_pot_xdocks[i].Get_Latitude();
        //            //var dist_thres = total_pot_xdocks[i].Get_Distance_Threshold();
        //            var hub_point = total_pot_xdocks[i].Get_Hub_Point();
        //            var capacity = 2000000;
        //            var chute_cap = max_chute_capacity;
        //            var already_opened = false;
        //            var potential_hub = new Hub(city, district, id, region, longitude, latitude,0, hub_point, capacity, chute_cap, already_opened);
        //            potential_Hubs.Add(potential_hub);
        //        }
        //    }

        //    return potential_Hubs;
        //}
        private void Create_Output_Log_Json(Dictionary<String,String[]> json_files)
        {
            var runtime = DateTime.Now.ToString(" dd MMMM HH;mm;ss ");
            var javaScriptSerializer = new JavaScriptSerializer();
            var resultJson = JsonConvert.SerializeObject(json_files);
            
            if (!partial_run && _only_cities)
            {
                var city = new List<String>();
                string list_to_write = "";
                for (int i = 0; i < _parameters.Count; i++)
                {
                    
                    if (_parameters[i].Get_Activation())
                    {
                        city.Add(_parameters[i].Get_Key());
                    }
                }

                list_to_write = String.Join(",", city.ToArray());

                if (city.Count<=5)
                {
                    File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Output of " + list_to_write + runtime + ".json", resultJson);
                }
                else
                {
                    File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Output of multiple cities" + runtime + ".json", resultJson);
                }
            }
            else if (partial_run)
            {
                File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Output of Partial Run Type" + runtime + ".json", resultJson);
            }
            else
            {
                File.WriteAllText(@"C:\Users\Public\RetroRestpectiveRun\Output of Full Run" + runtime + ".json", resultJson);
            }
            
        }
        //private void Modify_xDocks(List<xDocks> new_xDocks)
        //{
        //    for (int i = 0; i < agency.Count; i++)
        //    {
        //        new_xDocks.Add(agency[i]);
        //    }
        //}
    }
}
 