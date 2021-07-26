using System;
using System.Collections.Generic;
using System.Linq;
using FMLMDelivery.Classes;

namespace FMLMDelivery
{
    class Program
    {
        static void Main(string[] args)
        {
            var demand_point = new List<DemandPoint>();
            var potential_xDocks = new List<xDocks>();
            var xDocks = new List<xDocks>();
            var agency = new List<xDocks>();
            var hubs = new List<Hub>();
            var potential_hubs = new List<Hub>();
            var partial_xDocks = new List<xDocks>();
            var parameters = new List<Parameters>();
            var xDock_neighborhood_assignments = new Dictionary<xDocks, List<Mahalle>>();
            var courier_parameter_list = new List<Double> {100,150,120,2};
            var courier_list = new Dictionary<xDocks, List<Mahalle>>();


            ////This variable decides which solution methıd will be run. If true; every city individually assigned, else regions are assigned as a whole
            //var discrete_solution = true;
            //var hub_demand_coverage = 0.97;
            ////Provide the month index (1-January, 12-December)
            //var month = 12;
            //var reader = new CSVReader("Demand Points Disctrict 2021.csv", "Potential xDocks District 2021.csv", "2021 Seller File.csv", "Parameters_new.csv","", month);
            //reader.Read();
            //demand_point = reader.Get_Demand_Points();
            //potential_xDocks = reader.Get_XDocks();
            //agency = reader.Get_Agency();
            //var prior_small_sellers = reader.Get_Prior_Small_Sellers();
            //var regular_small_sellers = reader.Get_Regular_Small_Sellers();
            //var prior_big_sellers = reader.Get_Prior_Big_Sellers();
            //var regular_big_sellers = reader.Get_Regular_Big_Sellers();
            //var parameter_list = reader.Get_Parameter_List();
            //var distance_matrix = reader.Get_Distance_Matrix();
            //var partial_solution = false;
            //var only_courier_assignmnets = false;
            
            //if (!partial_solution)
            //{
            //    var runner = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution,"",hub_demand_coverage,false, xDock_neighborhood_assignments,courier_parameter_list,distance_matrix);
            //    (xDocks, hubs) = runner.Run();
            //    Console.ReadKey();
            //}
            //else if (only_courier_assignmnets)
            //{
            //    var partial_reader = new CSVReader("", "", "", "", "Output/Mahalle xDock Atamaları.csv", month);
            //    partial_reader.Read_xDock_Neighborhood_Assignments();
            //    xDock_neighborhood_assignments = partial_reader.Get_xDock_neighborhood_Assignments();
            //}
            //else
            //{
            //    var partial_reader = new CSVReader("", "Output/Kısmi Çalıştırma Dosyası.csv", "", "","Output/Mahalle xDock Atamaları.csv", month);
            //    partial_reader.Read_Partially();
            //    partial_xDocks = partial_reader.Get_Partial_Solution_Xdocks();
            //    xDock_neighborhood_assignments = partial_reader.Get_xDock_neighborhood_Assignments();
            //    var runner_partial = new Runner(demand_point, potential_xDocks, partial_xDocks, agency, prior_small_sellers, regular_small_sellers, prior_big_sellers, regular_big_sellers, parameter_list, partial_solution, discrete_solution,"",hub_demand_coverage,false, xDock_neighborhood_assignments,courier_parameter_list,distance_matrix);
            //    (xDocks, hubs) = runner_partial.Run();
            //    Console.ReadKey();
            //}
        }
    }
}