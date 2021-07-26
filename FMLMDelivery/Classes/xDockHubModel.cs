using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text;
using ILOG.CPLEX;
using ILOG.Concert;
using System.Device.Location;
using ChoETL;

namespace FMLMDelivery
{
    class xDockHubModel
    {
        /// <summary>
        /// Maximum number of XDock that can be assigned to a single Hub.
        /// </summary>
        private Double max_num_xdock_assigned = 400;

        private Double min_num_xdock_assigned = 4;

        /// <summary>
        /// Min number of XDock that can be assigned to a single Hub.
        /// </summary>
        private Double min_hub_capacity = 50000;

        /// <summary>
        /// Total amount of demand for each hub
        /// </summary>
        private Double max_hub_capaticity = 4500000;

        /// <summary>
        /// Number of Xdocks
        /// </summary>
        private readonly Int32 _numOfXdocks;

        /// <summary>
        /// Number of Hubs
        /// </summary>
        private Int32 _numOfHubs;

        /// <summary>
        /// List of xDocks 
        /// </summary>
        private List<xDocks> _xDocks;

        /// <summary>
        /// List of potential hubs
        /// </summary>
        private List<Hub> _hubs;

        /// <summary>
        /// List of sellers
        /// </summary>
        private List<Seller> _sellers;

        /// <summary>
        /// x[i, j] € {0,1} denotes whether Xdock i is assigned to hub j
        /// </summary>
        private List<List<INumVar>> x;

        /// <summary>
        /// y[j] € {0,1} denotes whether opened hub on location j
        /// </summary>
        private List<INumVar> y;

        /// <summary>
        /// s[k,j] € {0,1} denotes whether seller k is assigned to hub j
        /// </summary>
        private List<List<INumVar>> s;

        /// <summary>
        /// z[i] € {0,1} denotes whether xDock i is covered 
        /// </summary>
        private List<INumVar> z;

        /// <summary>
        /// a[i,j] € {0,1} denotes whether xDock i is in the range of hub j.
        /// </summary>
        private List<List<Double>> a;

        /// <summary>
        /// a_seller[i,j] € {0,1} whether seller i is in the range of Hub j.
        /// </summary>
        private List<List<Double>> a_seller;

        /// <summary>
        /// Linearization variable: Created in order to eliminate arg min cost func. for distances of un-covered xDocks. 
        /// </summary>
        private List<List<INumVar>> f;

        /// <summary>
        /// Minimum distance of unassigned dock i from hubs
        /// </summary>
        private List<INumVar> mu;

        /// <summary>
        /// Total Cost for unassigned xDocks
        /// </summary>
        private List<INumVar> k;

        /// <summary>
        /// d[i,j] € {0,1} is the distance matrix for all xDock i's and hub j's
        /// </summary>
        private List<List<Double>> d;

        /// <summary>
        /// dseller[i,j] is the distance matrix for all seller i's and Hub j's
        /// </summary>
        private List<List<Double>> d_seller;

        /// <summary>
        /// Revised distance matrix with notion of adding smalle sellers the minimum distance of xdocks and recalculating the seller to hub distance matrix.
        /// </summary>
        private List<List<Double>> d_sellerhub;

        /// <summary>
        /// Demand of each seller
        /// </summary>
        private List<Double> seller_demand;

        /// <summary>
        /// A sufficiently big number
        /// </summary>
        private Int32 M_1 = 100000;

        /// <summary>
        /// A sufficiently big number
        /// </summary>
        /// 
        private Int32 M_2 = 100000;

        /// <summary>
        /// A sufficiently big number
        /// </summary>
        /// 
        private Int32 M_3 = 100000;

        /// <summary>
        /// Opening cost for hub j
        /// </summary>
        private List<Double> c;

        /// <summary>
        /// Cplex object
        /// </summary>
        private readonly Cplex _solver;

        /// <summary>
        /// Objective instance which stores the objective function
        /// </summary>
        private ILinearNumExpr _objective;

        /// <summary>
        /// How many seconds the solver worked..
        /// </summary>
        private double _solutionTime;

        /// <summary>
        /// Solution status: 0 - Optimal; 1 - Feasible...
        /// </summary>
        private Cplex.Status _status;

        /// <summary>
        /// Time limit is given in seconds.
        /// </summary>
        private readonly long _timeLimit = 36000;

        /// <summary>
        /// Gap limit is given in percentage
        /// </summary>
        private readonly double _gap = 0.01;

        /// <summary>u
        /// if cost is incurred
        /// </summary>
        private Boolean _cost_incurred;

        /// <summary>
        /// if capacity constraint is incurred
        /// </summary>
        private Boolean _capacity_incurred;

        /// <summary>
        /// If minimum hub model is incurred
        /// </summary>
        private Boolean _min_hub_model;

        /// <summary>
        /// Objective Value
        /// </summary>
        private Double _objVal;

        private Dictionary<Int32,String> country_names;

        /// <summary>
        /// total number of hubs opened
        /// </summary>
        private Int32 p;

        /// <summary>
        /// LM Demand of each xDock
        /// </summary>
        private List<Double> x_dock_LM_demand= new List<double>();
        /// <summary>
        /// Initial Solution for xDock assignments
        /// </summary>
        private List<Double> _xdock_assignments= new List<double>();
        /// <summary>
        /// Initial Solution for seller assignments
        /// </summary>
        private List<Double> _seller_assignments=new List<double>();

        private List<List<Double>> _total_assignments=new List<List<double>>();
        /// <summary>
        /// Initial Solution for hub openings
        /// </summary>
        private List<Double> _opened_hubs=new List<double>();
        /// <summary>
        /// 
        /// </summary>
        private List<Double> x_dock_FM_demand;

        /// <summary>
        /// Weigted demand for 
        /// </summary>
        private Boolean _demand_weighted;

        /// <summary>
        /// Total LM demand of the xdocks
        /// </summary>
        private Double total_LM_demand = 0;

        private Double xDocks_FM_total_demand = 0;

        /// <summary>
        /// Total demand coverage that is required from the system
        /// </summary>
        private Double _demand_covarage;

        private Boolean phase_2;

        private int num_hubs = 0;

        private List<Hub> new_hubs;

        private double _numOfSeller;

        private double total_demand_seller = 0;

        private double seller_demand_coverage = 1.0;

        private List<Seller> assigned_seller;

        private List<String> records;

        private List<String> record_stats = new List<String>();

        private List<Double> number_of_chute_usage = new List<Double>();

        private Dictionary<String, List<Double>> _distance_matrix;

        private Double average_xdock_cap = 4000;

        private Dictionary<String, Double> common_volume = new Dictionary<string, double>();


        public xDockHubModel(List<xDocks> xDocks, List<Hub> hubs, List<Seller> sellers, Boolean Demandweight,Boolean min_hub_model,Double Demand_Covarage,Boolean Phase2, Int32 P ,Dictionary<String,List<Double>> distance_matrix,Boolean cost_incurred = false, Boolean capacity_incurred = false)
        {

            _solver = new Cplex();
            //_solver.SetParam(Cplex.DoubleParam.TiLim, val: _timeLimit);
            _solver.SetParam(Cplex.DoubleParam.EpGap, _gap);
            _xDocks = xDocks;
            _hubs = hubs;
            _sellers = sellers;
            _numOfXdocks = xDocks.Count;
            _numOfHubs = hubs.Count;
            _numOfSeller = sellers.Count;
            _cost_incurred = cost_incurred;
            _capacity_incurred = capacity_incurred;
            _min_hub_model = min_hub_model;
            p = P;
            _demand_weighted = Demandweight;
            _demand_covarage = Demand_Covarage;
            phase_2 = Phase2;
            _distance_matrix = distance_matrix;


            x = new List<List<INumVar>>();
            s = new List<List<INumVar>>();
            y = new List<INumVar>();
            z = new List<INumVar>();
            a = new List<List<Double>>();
            a_seller = new List<List<double>>();
            f = new List<List<INumVar>>();
            mu = new List<INumVar>();
            k = new List<INumVar>();
            d = new List<List<double>>();
            d_seller = new List<List<double>>();
            d_sellerhub = new List<List<double>>();
            c = new List<double>();
            country_names = new Dictionary<int, string>();
            x_dock_LM_demand = new List<double>();
            x_dock_FM_demand = new List<double>();
            seller_demand = new List<double>();
            new_hubs = new List<Hub>();
            assigned_seller = new List<Seller>();
            records = new List<String>();
            record_stats = new List<String>();
        }

        public Double Calculate_Distances(double long_1, double lat_1, double long_2, double lat_2)
        {
            var sCoord = new GeoCoordinate(lat_1, long_1);
            var eCoord = new GeoCoordinate(lat_2, long_2);

            return sCoord.GetDistanceTo(eCoord)/1000;
        }
        
        private void Get_Assignments()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                var value = Math.Round(_solver.GetValue(y[j]));
                _opened_hubs.Add(value);
            }

            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    var value = Math.Round(_solver.GetValue(x[i][j]));
                    _xdock_assignments.Add(value);
                }
            }

            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j <_numOfHubs; j++)
                {
                    var value = Math.Round(_solver.GetValue(s[i][j]));
                    _seller_assignments.Add(value);
                }
            }

            _total_assignments.Add(_opened_hubs);
            _total_assignments.Add(_xdock_assignments);
            _total_assignments.Add(_seller_assignments);
        }

        public List<List<Double>> Return_Initial_Assignments()
        {
            return _total_assignments;
        }

        public void Provide_Intitial_Solution(List<List<Double>> assignments)
        {
            _opened_hubs = assignments[0];
            _xdock_assignments = assignments[1];
            _seller_assignments = assignments[2];
        }

        private void Eliminate_Hub_Points()
        {
            List<string> keyList = new List<string>(this._distance_matrix.Keys);
            List<Hub> eliminated_list = new List<Hub>();
            for (int j = 0; j < _numOfHubs; j++)
            {
                var key3 = _hubs[j].Get_City();
                if (key3 == "İSTANBUL AVRUPA" || key3 == "İSTANBUL ASYA")
                {
                    key3 = "İSTANBUL";
                }
                var key4 = _hubs[j].Get_District();
                var key5 = key3 + "-" + key4;
                if (!keyList.Contains(key5))
                {
                    eliminated_list.Add(_hubs[j]);
                }
            }
            for (int i = 0; i < eliminated_list.Count; i++)
            {
                _hubs.Remove(eliminated_list[i]);
            }

            _numOfHubs = _hubs.Count;
        }

        private void Get_Distance_Matrix()
        {
            List<string> keyList = new List<string>(this._distance_matrix.Keys);

            //Calculating the distance matrix
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var key1 = _xDocks[i].Get_City();
                var key2 = _xDocks[i].Get_District();
                if (key1 == "İSTANBUL AVRUPA" || key1 == "İSTANBUL ASYA")
                {
                    key1 = "İSTANBUL";
                }
                var key = key1 + "-" + key2;
                var dist_mat_exist = true;
                var distance_list = new List<Double>();
                if (_distance_matrix.ContainsKey(key))
                {
                    distance_list = _distance_matrix[key];
                }
                else
                {
                    dist_mat_exist = false;
                }
                
                var d_i = new List<double>();
                for (int j = 0; j < _hubs.Count; j++)
                {
                    var d_ij = 0.0;
                    if (dist_mat_exist)
                    {
                        var key3 = _hubs[j].Get_City();
                        if (key3 == "İSTANBUL AVRUPA" || key3 == "İSTANBUL ASYA")
                        {
                            key3 = "İSTANBUL";
                        }
                        var key4 = _hubs[j].Get_District();
                        var key5 = key3 + "-" + key4;
                        if (keyList.Contains(key5))
                        {
                            var index2 = keyList.FindIndex(x => x == key5);
                            d_ij = distance_list[index2];

                        }
                        else
                        {
                            var lat1 = _xDocks[i].Get_Latitude();
                            var lat2 = _hubs[j].Get_Latitude();
                            var long1 = _xDocks[i].Get_Longitude();
                            var long2 = _hubs[j].Get_Longitude();
                            d_ij = Calculate_Distances(long1, lat1, long2, lat2);
                        }
                    }
                    else
                    {
                        var lat1 = _xDocks[i].Get_Latitude();
                        var lat2 = _hubs[j].Get_Latitude();
                        var long1 = _xDocks[i].Get_Longitude();
                        var long2 = _hubs[j].Get_Longitude();
                        d_ij = Calculate_Distances(long1, lat1, long2, lat2);
                    }
                    d_i.Add(d_ij);
                }
                d.Add(d_i);
            }

            //for (int i = 0; i < _numOfXdocks; i++)
            //{
            //    var d_i = new List<double>();
            //    for (int j = 0; j < _numOfHubs; j++)
            //    {
            //        var long_1 = _xDocks[i].Get_Longitude();
            //        var lat_1 = _xDocks[i].Get_Latitude();
            //        var long_2 = _hubs[j].Get_Longitude();
            //        var lat_2 = _hubs[j].Get_Latitude();
            //        var d_ij = Calculate_Distances(long_1, lat_1, long_2, lat_2);
            //        d_i.Add(d_ij);
            //    }
            //    d.Add(d_i);
            //}


        }
        private void Get_Distance_Matrix_Seller()
        {   //Calculating distance matrix for sellers
            List<string> keyList = new List<string>(this._distance_matrix.Keys);
            var bird_flew_dist = 0;
            for (int i = 0; i < _numOfSeller; i++)
            {
                var key1 =_sellers[i].Get_City();
                if (key1 == "İSTANBUL AVRUPA" ||  key1 == "İSTANBUL ASYA")
                {
                    key1 = "İSTANBUL";
                }
                var key2 = _sellers[i].Get_District();
                var key = key1 + "-" + key2;
                var distance_list = new List<Double>();
                var dist_list_exist = true;
                if (_distance_matrix.ContainsKey(key))
                {
                    distance_list = _distance_matrix[key];

                }
                else
                {
                    dist_list_exist = false;
                }
                var d_k = new List<double>();
                for (int j = 0; j < _hubs.Count; j++)
                {
                    var d_ij = 0.0;
                    if (dist_list_exist)
                    {
                        var key3 = _hubs[j].Get_City();
                        if (key3 == "İSTANBUL AVRUPA" || key3 == "İSTANBUL ASYA")
                        {
                            key3 = "İSTANBUL";
                        }
                        var key4 = _hubs[j].Get_District();
                        var key5 = key3 + "-" + key4;
                        if (keyList.Contains(key5))
                        {
                            var index2 = keyList.FindIndex(x => x == key5);
                            d_ij = distance_list[index2];
                        }
                        else
                        {
                            var lat1 = _sellers[i].Get_Latitude();
                            var lat2 = _hubs[j].Get_Latitude();
                            var long1 = _sellers[i].Get_Longitude();
                            var long2 = _hubs[j].Get_Longitude();
                            d_ij = Calculate_Distances(long1, lat1, long2, lat2);
                            bird_flew_dist += 1;
                        }
                        
                    }
                    else
                    {
                        var lat1 = _sellers[i].Get_Latitude();
                        var lat2 = _hubs[j].Get_Latitude();
                        var long1 = _sellers[i].Get_Longitude();
                        var long2 = _hubs[j].Get_Longitude();
                        d_ij = Calculate_Distances(long1, lat1, long2, lat2);
                        bird_flew_dist++;
                    }
                    

                    d_k.Add(d_ij);
                }
                d_seller.Add(d_k);

            }


            //for (int i = 0; i < _numOfSeller; i++)
            //{
            //    var d_k = new List<double>();
            //    for (int j = 0; j < _numOfHubs; j++)
            //    {
            //        var long_1 = _sellers[i].Get_Longitude();
            //        var lat_1 = _sellers[i].Get_Latitude();
            //        var long_2 = _hubs[j].Get_Longitude();
            //        var lat_2 = _hubs[j].Get_Latitude();
            //        var d_ij = Calculate_Distances(long_1, lat_1, long_2, lat_2);

            //        d_k.Add(d_ij);
            //    }
            //    d_seller.Add(d_k);
            //}

        }

        public Double GetObjVal()
        {
            return _objVal;
        }


        //For cost incurred model
        private void Get_Cost_Parameters()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                var c_j = _hubs[j].Get_LM_Capacity();
                c.Add(c_j);
            }
        }

        //Will be revised
        private void Create_Distance_Threshold_Matrix()
        {
            //Create a[i,j] matrix
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var longtitude = _xDocks[i].Get_Longitude();                
                var a_i = new List<Double>();

                for (int j = 0; j < _numOfHubs; j++)
                {
                    var threshold = _hubs[j].Get_Distance_Threshold();
                    if (d[i][j] <= threshold)
                    {
                        var a_ij = 1;
                        a_i.Add(a_ij);
                    }
                    else
                    {
                        var a_ij = 0;
                        a_i.Add(a_ij);
                    }
                }
                a.Add(a_i);
            }
        }

        private void Create_Distance_Threshold_Seller()
        {   //Create a_seller[i,j] matrix
            for (int i = 0; i < _numOfSeller; i++)
            {
                var threshold = _sellers[i].Get_Distance_Threshold();
                var a_k = new List<Double>();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    if (d_seller[i][j] <= threshold)
                    {
                        var a_ij = 1;
                        a_k.Add(a_ij);
                    }
                    else
                    {
                        var a_ij = 0;
                        a_k.Add(a_ij);
                    }
                }
                a_seller.Add(a_k);
            }

        }

        private void Create_Chute_Usage_List()
        {
            for (int i = 0; i < _xDocks.Count; i++)
            {
                var chute_number = Math.Round(_xDocks[i].Get_LM_Demand() / average_xdock_cap);
                number_of_chute_usage.Add(chute_number);
            }
        }
        public void Run()
        {
            Get_Parameters();
            Build_Model();
            if (_demand_weighted)
            {
                AddInitialSolution();
            }
            Solve();
            if ((_status == Cplex.Status.Feasible || _status == Cplex.Status.Optimal))
            {
                Get_Assignments();
                Get_Stats();
                Create_Country_Names();
                Get_Num_Hubs();
                Get_new_Hubs();
                Get_Assigned_Sellers();
                Get_Hub_Xdock_Seller();
            }
            Print();
            ClearModel();
            
        }

        public void Get_Assigned_Sellers()
        {
            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    if (_solver.GetValue(s[i][j]) > 0.9)
                    {
                        assigned_seller.Add(_sellers[i]);
                    }
                }
            }
        }
        private void Get_Hub_Xdock_Seller()
        {
            var count = 0;
            var count2 = 0;
            for (int j = 0; j < _hubs.Count; j++)
            {
                if (_solver.GetValue(y[j]) > 0.9)
                {   count+= 1;
                    var hub_ranking = "HUB" + count;
                    var hub_city = _hubs[j].Get_City();
                    var hub_district = _hubs[j].Get_District();
                    var hub_id = _hubs[j].Get_Id();
                    var hub_long = _hubs[j].Get_Longitude();
                    var hub_lat = _hubs[j].Get_Latitude();

                    for (int i = 0; i < _xDocks.Count; i++)
                    {
                        count2 += 1;
                        if (_solver.GetValue(x[i][j]) > 0.9)
                        {                            
                            var type = "xDock";
                            var number_of_xdock = "xDock" + count2;
                            var xdock_city = _xDocks[i].Get_City();
                            var xdock_county = _xDocks[i].Get_District();
                            var xdock_lm_demand = _xDocks[i].Get_LM_Demand();
                            var xdock_fm_demand = _xDocks[i].Get_FM_Demand();
                            var xdock_id = _xDocks[i].Get_Id();
                            var xdock_distance = d[i][j];
                            var result = $"{hub_city},{hub_district},{hub_id},{hub_long},{hub_lat},{type},{number_of_xdock},{xdock_city},{xdock_county},{xdock_id},{xdock_lm_demand},{xdock_fm_demand},{xdock_distance}";
                            records.Add(result);
                        }
                    }
                    count2 = 0;
                    for (int k = 0; k < _sellers.Count; k++)
                    {
                        if (_solver.GetValue(s[k][j]) > 0.9)
                        {
                            var type = "Big Seller";
                            var seller_name = _sellers[k].Get_Name();
                            var seller_district = _sellers[k].Get_District();
                            var seller_city = _sellers[k].Get_City();
                            var seller_id = "";
                            var seller_lm_demand = "";
                            var seller_fm_demand = _sellers[k].Get_Demand();
                            var seller_distance = d_seller[k][j];
                            var result = $"{hub_city},{hub_district},{hub_id},{hub_long},{hub_lat},{type},{seller_name},{seller_city},{seller_district},{seller_id},{seller_lm_demand},{seller_fm_demand},{seller_distance}";
                            records.Add(result);
                        }
                    }

                    
                }
            }
        }
        public List<String> Get_Hub_Xdock_Seller_Info()
        {
            return records;
        }
        private void Get_Stats()
        {
            var part = "All";
            var model = "xDock_Hub_Seller Model";
            var time = _solutionTime;
            var gap_to_optimal = (_solver.GetMIPRelativeGap())*100;
            var status = _status;
            var result = $"{part},{model},{_demand_covarage},{status},{time},{gap_to_optimal}";
            record_stats.Add(result);

        }
        public List<String> Get_Xdock_Hub_Stats()
        {
            return record_stats;
        }
        public List<Seller> Return_Assigned_Big_Sellers()
        {
            return assigned_seller;
        }

        public List<Hub> Return_New_Hubs()
        {
            return new_hubs;
        }

        private void Get_new_Hubs()
        {
            
            for (int j = 0; j < _numOfHubs; j++)
            {
                if (_status == Cplex.Status.Feasible || _status == Cplex.Status.Optimal)
                {
                    if (_solver.GetValue(y[j]) > 0.9)
                    {
                        
                        var city = _hubs[j].Get_City();
                        var district = _hubs[j].Get_District();
                        var id = _hubs[j].Get_Id();
                        var region = _hubs[j].Get_Region();
                        var valueslat = _hubs[j].Get_Latitude();
                        var valueslong = _hubs[j].Get_Longitude();
                        var dist = _hubs[j].Get_Distance_Threshold();
                        var hub_point = _hubs[j].Get_Hub_Points();
                        var lm_distribution = 0.0;
                        var fm_distribution = 0.0;
                        var chute_cap_distribution = 0.0;
                        for (int i = 0; i < _numOfXdocks; i++)
                        {
                            if (_solver.GetValue(x[i][j]) > 0.9)
                            {
                                lm_distribution += _xDocks[i].Get_LM_Demand();
                                fm_distribution += _xDocks[i].Get_FM_Demand();
                                chute_cap_distribution += number_of_chute_usage[i];
                            }
                        }
                        for (int i = 0; i < _numOfSeller; i++)
                        {
                            if (_solver.GetValue(s[i][j]) > 0.9)
                            {
                                fm_distribution += _sellers[i].Get_Demand();
                            }
                        }
                        var already_opened = _hubs[j].If_Already_Opened();

                        var new_hub = new Hub(city, district, id, region, valueslong, valueslat, dist,hub_point, lm_distribution,chute_cap_distribution, already_opened);
                        new_hub.Set_FM_Capacity(fm_distribution);
                        new_hubs.Add(new_hub);
                    }
                }

            }
        }

        //In order to return to minimum number of hubs that gives a feasible solution
        private void Get_Num_Hubs()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                if (_solver.GetValue(y[j])>0.9)
                {
                    num_hubs += 1;
                }
            }
        }

        //In order to return to minimum number of hubs that gives a feasible solution
        public Int32 Return_num_Hubs()
        {
            return num_hubs;
        }

        //For returning the Id's of the solution
        private void Create_Country_Names()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                if ((_status == Cplex.Status.Feasible || _status == Cplex.Status.Optimal))
                {
                    if (_solver.GetValue(y[j]) > 0.9)
                    {
                        country_names.Add(j, _hubs[j].Get_Id());
                    }
                }
                
            }

        }

        public Dictionary<Int32, String> Get_Country_Names()
        {
            return country_names;
        }

        private void Print()
        {
            if (!(_status == Cplex.Status.Feasible || _status == Cplex.Status.Optimal))
            {
                Console.WriteLine("Solution is neither optimal nor feasible!");
                return;

            }
            _objVal = Math.Round(_solver.GetObjValue(), 2);
            var stats = _solver.GetStatus();
            Console.WriteLine("Objective value is {0}\n", _objVal);
            Console.WriteLine("Solution status is {0}\n", stats);
            var n_var = _solver.NbinVars;
            Console.WriteLine("Number of variables : {0}", n_var);

            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    if (_solver.GetValue(s[i][j]) > 0.9)
                    {
                        Console.WriteLine("s[{0},{1}] = {2}", i, j, _solver.GetValue(s[i][j]));
                    }
                }
            }
         

            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    if (_solver.GetValue(x[i][j] ) > 0.9)
                    {
                        Console.WriteLine("x[{0},{1}] = {2}", i, j, _solver.GetValue(x[i][j]));
                    }
                   
                }
            }


            for (int j = 0; j < _numOfHubs; j++)
            {
                if (_solver.GetValue(y[j])>0.9)
                {
                    Console.WriteLine("y[{0}] = {1}", j, _solver.GetValue(y[j]));
                }
                
            }

            if (_cost_incurred)
            {
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    if (_solver.GetValue(z[i]) > 0.9)
                    {
                        Console.WriteLine("z[{0}] = {1}", i, _solver.GetValue(z[i]));
                    }

                    Console.WriteLine("k[{0}] = {1}", i, _solver.GetValue(k[i]));
                    Console.WriteLine("mu[{0}] = {1}", i, _solver.GetValue(mu[i]));

                }
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    for (int j = 0; j < _numOfHubs; j++)
                    {
                        Console.WriteLine("f[{0},{1}]] = {2}", i, j, _solver.GetValue(f[i][j]));

                    }
                }
            }
           
        }

        private void Get_Parameters()
        {
            Eliminate_Hub_Points();
            Get_Distance_Matrix_Seller();
            Get_Distance_Matrix();
            Get_Cost_Parameters();
            Create_Distance_Threshold_Matrix();
            Create_Distance_Threshold_Seller();
            Get_Demand_Parameters();
            Get_Total_Demand();
            Create_Chute_Usage_List();
            Create_Common_Volume_Dictionary();
        }

        private void Get_Total_Demand()
        {
            for (int i = 0; i < _numOfXdocks; i++)
            {
                total_LM_demand = x_dock_LM_demand[i] + total_LM_demand;
                xDocks_FM_total_demand += x_dock_FM_demand[i];
            }
            for (int j = 0; j < _numOfSeller; j++)
            {
                total_demand_seller += _sellers[j].Get_Demand();
            }
        }
        private void Get_Demand_Parameters()
        {
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var lm_i = _xDocks[i].Get_LM_Demand();
                var fm_i = _xDocks[i].Get_FM_Demand();
                x_dock_LM_demand.Add(lm_i);
                x_dock_FM_demand.Add(fm_i);
            }

            for (int i = 0; i < _numOfSeller; i++)
            {
                var d_k = _sellers[i].Get_Demand();
                seller_demand.Add(d_k);
            }
        }

        private void Create_Common_Volume_Dictionary()
        {
            common_volume.Add("İSTANBUL ASYA", 4);
            common_volume.Add("İSTANBUL AVRUPA", 4);
            common_volume.Add("İZMİR", 12);
            common_volume.Add("ANKARA", 12);
            common_volume.Add("BURSA", 8);
            common_volume.Add("ADANA", 8);
        }

        private void Solve()
        {
            Console.WriteLine("Algorithm starts running at {0}", DateTime.Now);
            var startTime = DateTime.Now;

            _solver.Solve();
            _solutionTime = ((DateTime.Now - startTime).Hours * 60 * 60 + (DateTime.Now - startTime).Minutes * 60 + (DateTime.Now - startTime).Seconds);
            _status = _solver.GetStatus();
            _solver.ExportModel("Assignment.lp");
            Console.WriteLine("Algorithm stops running at {0}", DateTime.Now);
        }

        private void Build_Model()
        {
            Console.WriteLine("Model construction starts at {0}", DateTime.Now);
            CreateDecisionVariables();
            CreateObjective();
            CreateConstraints();
            Console.WriteLine("Model construction ends at {0}", DateTime.Now);
        }



        private void CreateConstraints()
        {
            //Valid_Inequality_Constraint();
            CoverageConstraints();
            Open_Hub_Constraint();
            Nonnegativity_Constraint();

            //Chute_Capacity_Constraint();
            if (_cost_incurred)
            {
                UnAssigned_XDock_Constraints();
                Capacity_Constraint();
            }
            if (_capacity_incurred)
            {
                TotalHubConstraint();
                Capacity_Constraint();
                //Min_X_Dock_Constraint();
            }
            if (_demand_weighted)
            {
                if (!(_capacity_incurred))
                {
                    TotalHubConstraint();
                    Capacity_Constraint();
                  //  Min_X_Dock_Constraint();
                }
                if (phase_2)
                {
                    Demand_Coverage_Constraint();
                    Seller_Capacity_Constraint();
                    Seller_Assignment_Constraint();
                    Seller_Demand_Satisfaction_Constraint();
                }

            }
            if (_min_hub_model)
            {

                Demand_Coverage_Constraint();
                Capacity_Constraint();
                Seller_Capacity_Constraint();
                Seller_Assignment_Constraint();
                Seller_Demand_Satisfaction_Constraint();
            }
           
        }

        private void Nonnegativity_Constraint()
        {
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    _solver.AddGe(x[i][j], 0);
                }
            }
            for (int j = 0; j < _numOfHubs; j++)
            {
                _solver.AddGe(y[j], 0);
            }
            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    _solver.AddGe(s[i][j], 0);
                }
            }
        }

        //y[j]*beta <= ∑x[i,j]*a[i,j]*demand[i]
        private void Min_X_Dock_Constraint()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    constraint.AddTerm(x[i][j], a[i][j]*(x_dock_LM_demand[i]+ x_dock_FM_demand[i]));
                }
                constraint.AddTerm(y[j], -min_hub_capacity);
                _solver.AddGe(constraint,0);
            }

            for (int j = 0; j < _numOfHubs; j++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    constraint.AddTerm(x[i][j], a[i][j]);
                }
                constraint.AddTerm(y[j], -min_num_xdock_assigned);
                _solver.AddGe(constraint, 0);
            }
        }

        private void Seller_Assignment_Constraint()
        {
            for (int i = 0; i < _numOfSeller; i++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(s[i][j], a_seller[i][j]);
                }
                _solver.AddEq(constraint, 1);
            }
        }
        private void Seller_Capacity_Constraint()
        {
            //for (int j = 0; j < _numOfHubs; j++)
            //{
            //    var constraint = _solver.LinearNumExpr();
            //    for (int i = 0; i < _numOfSeller; i++)
            //    {
            //        constraint.AddTerm(s[i][j], a_seller[i][j] * _sellers[i].Get_Demand());
            //    }
            //    for (int k = 0; k < _numOfXdocks; k++)
            //    {
            //        constraint.AddTerm(x[k][j], a[k][j] * _xDocks[k].Get_FM_Demand());
            //    }
            //    constraint.AddTerm(y[j], -(_hubs[j].Get_FM_Capacity())/2);
            //    _solver.AddLe(constraint, 0);
            //}
        }


        private void Seller_Demand_Satisfaction_Constraint()
        {
            var constraint = _solver.LinearNumExpr();
            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(s[i][j], a_seller[i][j] * seller_demand[i]);
                }
            }
            _solver.AddGe(constraint, total_demand_seller*seller_demand_coverage);
        }

        //∑∑x[i,j]*a[i,j]*d[i] >= covarage_percentage*demand
        private void Demand_Coverage_Constraint()
        {
            var constraint = _solver.LinearNumExpr();
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(x[i][j], x_dock_LM_demand[i]*a[i][j]);
                }
            }
            _solver.AddGe(constraint, total_LM_demand*_demand_covarage);

            var constraint_2 = _solver.LinearNumExpr();
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint_2.AddTerm(x[i][j], x_dock_FM_demand[i] * a[i][j]);
                }
            }
            _solver.AddGe(constraint_2, xDocks_FM_total_demand * _demand_covarage);
        }
        //∑y[j]=num_of_cluster
        private void TotalHubConstraint()
        {
            var constraint = _solver.LinearNumExpr();
            for (int j = 0; j < _numOfHubs; j++)
            {
                constraint.AddTerm(y[j], 1);
            }
            _solver.AddEq(constraint, p);
        }

        //If any hub is already open
        private void Open_Hub_Constraint()
        {
            
            for (int i = 0; i < _numOfHubs; i++)
            {
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(y[i], 1);
                if (_hubs[i].If_Already_Opened())
                {
                    _solver.AddEq(constraint, 1);
                }
                else if (_hubs[i].Get_Hub_Points()==1.5)
                {
                    _solver.AddEq(constraint, 0);
                }
                else
                {
                    _solver.AddLe(constraint, 1);
                }
            }
        }

        //y[j]*alfa >= ∑x[i,j]*a[i,j]*demand[i]
        //Might be revised
        private void Capacity_Constraint()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    var demand_included = x_dock_LM_demand[i] * a[i][j];
                    constraint.AddTerm(x[i][j], demand_included);
                }

                for (int k = 0; k < _numOfSeller; k++)
                {
                    var demand_included_2 = _sellers[k].Get_Demand() * a_seller[k][j];
                    constraint.AddTerm(s[k][j], demand_included_2);
                }
                if (common_volume.ContainsKey(_hubs[j].Get_City())& _min_hub_model)
                {
                    var percentage = common_volume[_hubs[j].Get_City()];
                    //demand_included_2 = demand_included_2 * (100 - percentage) / 100;
                    _hubs[j].Set_LM_Capacity(_hubs[j].Get_LM_Capacity() * (100 + percentage) / 100);
                }
                constraint.AddTerm(y[j], -_hubs[j].Get_LM_Capacity());
                _solver.AddLe(constraint, 0);
            }

            //for (int j = 0; j < _numOfHubs; j++)
            //{
            //    var constraint = _solver.LinearNumExpr();

            //    for (int k = 0; k < _numOfSeller; k++)
            //    {
            //        var demand_included_2 = _sellers[k].Get_Demand() * a_seller[k][j];
            //        constraint.AddTerm(s[k][j], demand_included_2);
            //    }
            //    constraint.AddTerm(y[j], -_hubs[j].Get_LM_Capacity());
            //    _solver.AddLe(constraint, 0);
            //}
            
            //for (int j = 0; j < _numOfHubs; j++)
            //{
            //    var constraint = _solver.LinearNumExpr();
            //    for (int i = 0; i < _numOfXdocks; i++)
            //    {
            //        constraint.AddTerm(x[i][j], a[i][j]);
            //    }
            //    constraint.AddTerm(y[j], -max_num_xdock_assigned);
            //    _solver.AddLe(constraint, 0);
            //}

        }

        private void Chute_Capacity_Constraint()
        {
            for (int j = 0; j < _numOfHubs; j++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    constraint.AddTerm(x[i][j], number_of_chute_usage[i]);
                }
                constraint.AddTerm(y[j], -_hubs[j].Get_Chute_Capacity());
                _solver.AddLe(constraint, 0);
            }
        }


        private void UnAssigned_XDock_Constraints()
        {
            //mu[i] <= y[j]*d[i][j]
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                     var constraint = _solver.LinearNumExpr();
                    constraint.AddTerm(mu[i], 1);
                    constraint.AddTerm(y[j], -d[i][j]+M_1);
                    _solver.AddLe(constraint, M_1);
                }
            }

            //mu[i] >= y[j]*d[i][j]-(1-f[i][j])*M_1
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    var constraint = _solver.LinearNumExpr();
                    constraint.AddTerm(mu[i], 1);
                    constraint.AddTerm(y[j], -d[i][j]+M_1);
                    constraint.AddTerm(f[i][j], -M_1);
                    _solver.AddGe(constraint, -M_1+M_1);
                }
            }

            //∑f[i][j] >= 1
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
            
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(f[i][j], 1);
                }
                _solver.AddGe(constraint, 1);
            }

            //k[i] <= M_2*(1-z[i])
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(k[i], 1);
                constraint.AddTerm(z[i], M_2);
                _solver.AddLe(constraint, M_2);
            }

            //k[i] <= mu[i]
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(k[i], 1);
                constraint.AddTerm(mu[i], -1);
                _solver.AddLe(constraint, 0);
            }

            //k[i] >= mu[i]-z[i]*M_3
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(k[i], 1);
                constraint.AddTerm(mu[i], -1);
                constraint.AddTerm(z[i], M_3);
                _solver.AddGe(constraint, 0);
            }

            //k[i] >= 0
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                constraint.AddTerm(k[i], 1);
                _solver.AddGe(constraint, 0);
            }
        }
        private void Valid_Inequality_Constraint()
        {
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(x[i][j], a[i][j]);
                    constraint.AddTerm(-1, y[j]);
                    _solver.AddLe(constraint, 0);
                }
            }
        }
        private void CoverageConstraints()
        {

            //∑x[i,j]*a[i,j] = z[i]/1/<=1
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var constraint = _solver.LinearNumExpr();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    constraint.AddTerm(x[i][j], a[i][j]);
                }
                if (_cost_incurred)
                {
                    _solver.AddEq(constraint, z[i]);
                }
                else if (_capacity_incurred || _demand_weighted)
                {
                    if (!phase_2)
                    {
                        _solver.AddEq(constraint, 1);
                    }
                    else
                    {
                        _solver.AddLe(constraint, 1);
                    }
                    
                }
                else if (_min_hub_model)
                {
                    _solver.AddLe(constraint, 1);
                }
                
            }
            

           
        }


        /// <summary>
        /// 
        /// </summary>
        private void CreateObjective()
        {
            _objective = _solver.LinearNumExpr();
            /// <summary>
            /// Create objective function which tries to minimizes number of hubs while also tries to minimizes unassigned XDocks.
            /// </summary>
            if (_cost_incurred)
            {

                for (int j = 0; j < _numOfHubs; j++)
                {
                    _objective.AddTerm(y[j], c[j]);
                }

                for (int i = 0; i < _numOfXdocks; i++)
                {
                    _objective.AddTerm(k[i], 1);
                }
            }
            //Minimizes distance between hub j and assigned xDock i
            if (_capacity_incurred)
            {
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    for (int j = 0; j < _numOfHubs; j++)
                    {
                        _objective.AddTerm(x[i][j], d[i][j]);
                    }
                }
            }
            //Minizes the distance, between hub j and assigned xDock i  considering demand
            if (_demand_weighted)
            {
                for (int i = 0; i < _numOfXdocks; i++)
                {
                    for (int j = 0; j < _numOfHubs; j++)
                    {
                        _objective.AddTerm(x[i][j], d[i][j] * (x_dock_LM_demand[i] + x_dock_FM_demand[i])*_hubs[j].Get_Hub_Points());

                    }
                }
                for (int i = 0; i < _numOfSeller; i++)
                {
                    for (int j = 0; j < _numOfHubs; j++)
                    {
                        _objective.AddTerm(s[i][j], d_seller[i][j] * seller_demand[i]*_hubs[j].Get_Hub_Points());
                    }   
                }


            }
            //Minimizes the total number of hub j
            if (_min_hub_model)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    _objective.AddTerm(y[j], 1);
                }
            }
            _solver.AddMinimize(_objective);

        }


        private void CreateDecisionVariables()
        {
            // Create x[i,j]-variables
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var x_i = new List<INumVar>();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    var name = $"x[{i + 1}][{(j + 1)}]";
                    var x_ij = _solver.NumVar(0, 1, NumVarType.Bool, name);
                    x_i.Add(x_ij);
                }
                x.Add(x_i);
            }

            for (int i = 0; i < _numOfSeller; i++)
            {
                var s_i = new List<INumVar>();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    var name = $"s[{i + 1}][{(j + 1)}]";
                    var s_ij = _solver.NumVar(0, 1, NumVarType.Bool, name);
                    s_i.Add(s_ij);
                }
                s.Add(s_i);
            }

            //Create y[j] variables
            for (int j = 0; j < _numOfHubs; j++)
            {
                var name = $"x[{(j + 1)}]";
                var y_j = _solver.NumVar(0, 1, NumVarType.Bool, name);
                y.Add(y_j);
            }
            
            //Create z[i] variables
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var name = $"z[{(i + 1)}]";
                var z_i = _solver.NumVar(0, 1, NumVarType.Bool, name);
                z.Add(z_i);
            }

            //Create f[i,j] variables
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var f_i = new List<INumVar>();
                for (int j = 0; j < _numOfHubs; j++)
                {
                    var name = $"f[{i + 1}][{(j + 1)}]";
                    var f_ij = _solver.NumVar(0, 1,NumVarType.Bool, name);
                    f_i.Add(f_ij);
                }
                f.Add(f_i);
            }

            //Create k[i] variables
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var name = $"k[{(i + 1)}]";
                var k_i = _solver.NumVar(0, Int32.MaxValue, NumVarType.Float, name);
                k.Add(k_i);
            }

            //Create mu[i] variables
            for (int i = 0; i < _numOfXdocks; i++)
            {
                var name = $"mu[{(i + 1)}]";
                var mu_i = _solver.NumVar(0, Int32.MaxValue, NumVarType.Float, name);
                mu.Add(mu_i);
            }


        }

        private void AddInitialSolution()
        {
            _solver.AddMIPStart(y.ToArray(), _opened_hubs.ToArray());
            
            var xArray = new List<INumVar>();
            for (int i = 0; i < _numOfXdocks; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    xArray.Add(x[i][j]);
                }
            }

            _solver.AddMIPStart(xArray.ToArray(), _xdock_assignments.ToArray());

            var sArray = new List<INumVar>();
            for (int i = 0; i < _numOfSeller; i++)
            {
                for (int j = 0; j < _numOfHubs; j++)
                {
                    sArray.Add(s[i][j]);
                }
            }
            _solver.AddMIPStart(sArray.ToArray(), _seller_assignments.ToArray());
        }

        public void ClearModel()
        {
            _solver.ClearModel();
            _solver.Dispose();
        }

    }
    
}
