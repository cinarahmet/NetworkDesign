using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChoETL;
using FMLMDelivery.Classes;
using Nancy.Json;
using Newtonsoft.Json;

public class CSVReader
{
    private readonly List<DemandPoint> _demand_point = new List<DemandPoint>();

    private readonly List<xDocks> _xDocks = new List<xDocks>();

    private readonly List<Hub> _Hub = new List<Hub>();

    private readonly List<xDocks> _agency = new List<xDocks>();

    private readonly List<Seller> _prior_small_seller = new List<Seller>();

    private readonly List<xDocks> _partial_xdocks = new List<xDocks>();

    private List<Seller> _regular_small_seller = new List<Seller>();

    private readonly List<Seller> _prior_big_seller = new List<Seller>();

    private List<Seller> _regular_big_seller = new List<Seller>();

    private readonly List<Seller> _total_seller = new List<Seller>();

    private readonly string _demand_point_file;

    private readonly string _xDocks_file;

    private readonly string _seller_file;

    private readonly string _parameter_file;

    private readonly string _xDock_neighborhood_assignments_file;

    private readonly string _dist_matrix_file;

    private Dictionary<String, Double> region_county_threshold = new Dictionary<string, double>();

    private Dictionary<String, Double> region_xDock_threshold = new Dictionary<string, double>();

    private List<Parameters> _parameters = new List<Parameters>();

    private Double scope_out_threshold = 10;

    private Dictionary<xDocks, List<Mahalle>> _xDock_neighborhood_assignments = new Dictionary<xDocks, List<Mahalle>>();

    private List<String> failure_list = new List<string> {"Dosya İsmi, Satır, Satır İçeriği"};

    private Dictionary<String, String[]> total_dictionary_of_Inputs= new Dictionary<string, string[]>();

    private Dictionary<String,List<Double>> dist_matrix = new Dictionary<string, List<double>>() ;

    private List<String> city_list;


    public CSVReader(string county_file, string xDock_file, string Seller_file,string parameter_file,string assignments)
    {
        _demand_point_file = county_file;
        _xDocks_file = xDock_file;
        _seller_file = Seller_file;
        _parameter_file = parameter_file;
        _xDock_neighborhood_assignments_file = assignments;
        _dist_matrix_file = "İl İlçe Distance Matrix.csv";
    }

    //private void Create_Demand_Point_Region_Threshold()
    //{
    //    var s_1 = "Akdeniz";
    //    var value = 30;
    //    var s_2 = "Marmara";
    //    var value_2 = 30;
    //    var s_3 = "İç Anadolu";
    //    var value_3 = 30;
    //    var s_4 = "Ege";
    //    var value_4 = 30;
    //    var s_5 = "Güneydoğu";
    //    var value_5 = 30;
    //    var s_6 = "Karadeniz";
    //    var value_6 = 30;
    //    var s_7 = "Güneydoğu Anadolu";
    //    var value_7 = 30;
    //    var s_8 = "Doğu Anadolu";
    //    var value_8 = 30;
    //    region_county_threshold.TryAdd(s_1, value);
    //    region_county_threshold.TryAdd(s_2, value_2);
    //    region_county_threshold.TryAdd(s_3, value_3);
    //    region_county_threshold.TryAdd(s_4, value_4);
    //    region_county_threshold.TryAdd(s_5, value_5);
    //    region_county_threshold.TryAdd(s_6, value_6);
    //    region_county_threshold.TryAdd(s_7, value_7);
    //    region_county_threshold.TryAdd(s_8, value_8);
    //}

    //private void Create_xDock_Region_Threshold()
    //{
    //    var s_1 = "Akdeniz";
    //    var value = 300;
    //    var s_2 = "Marmara";
    //    var value_2 = 300;
    //    var s_3 = "İç Anadolu";
    //    var value_3 = 300;
    //    var s_4 = "Ege";
    //    var value_4 = 300;
    //    var s_5 = "Güneydoğu";
    //    var value_5 = 300;
    //    var s_6 = "Karadeniz";
    //    var value_6 = 200;
    //    var s_7 = "Güneydoğu Anadolu";
    //    var value_7 = 300;
    //    var s_8 = "Doğu Anadolu";
    //    var value_8 = 300;
    //    region_xDock_threshold.TryAdd(s_1, value);
    //    region_xDock_threshold.TryAdd(s_2, value_2);
    //    region_xDock_threshold.TryAdd(s_3, value_3);
    //    region_xDock_threshold.TryAdd(s_4, value_4);
    //    region_xDock_threshold.TryAdd(s_5, value_5);
    //    region_xDock_threshold.TryAdd(s_6, value_6);
    //    region_xDock_threshold.TryAdd(s_7, value_7);
    //    region_xDock_threshold.TryAdd(s_8, value_8);
    //}

    public void Read()
    {
        Read_Distance_Matrix();
        //Create_Demand_Point_Region_Threshold();
        //Read_Demand_Point();
        Read_Demand_Points_and_xDock();
        Read_Potential_Hub_Points();
        Read_Parameters();
        Read_Sellers();
    }

    private void Read_Distance_Matrix()
    {
        using (var sr = File.OpenText(_dist_matrix_file))
        {
            String s = sr.ReadLine();
            city_list = s.Split(',').ToList();
            var index = 0;
            while ((s = sr.ReadLine()) != null)
            {
                var dist_list = s.Split(',').ToList();
                var d_list = dist_list.Select(a => double.Parse(a)).ToList();
                dist_matrix.Add(city_list[index], d_list);
                index += 1;
            }
        }
    }

    //private void Read_Demand_Point()
    //{
    //    using (var sr = File.OpenText(_demand_point_file))
    //    {
    //        String s = sr.ReadLine();
    //        var index = 1;
    //        while ((s = sr.ReadLine()) != null)
    //        {
    //            index += 1;
    //            try
    //            { 
    //                var line = s.Split(',');
    //                var demand_point_City = line[0];
    //                var demand_point_district = line[1];
    //                var demand_point_ID = line[2];
    //                var demand_point_region = line[3];
    //                var demand_point_lat = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
    //                var demand_point_long = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
    //                var demand_point_dis_thres = Convert.ToDouble(line[6], System.Globalization.CultureInfo.InvariantCulture);
    //                if (demand_point_dis_thres == 0.0)
    //                {
    //                    demand_point_dis_thres = region_county_threshold[demand_point_region];
    //                }
    //                var demand_point_Demand = Convert.ToDouble(line[_month - 1]);
    //                if (demand_point_Demand != 0.0)
    //                {
    //                    //demand_point_Demand = Convert.ToDouble(line[_month - 1]) / Math.Ceiling(Convert.ToDouble(line[_month - 1]) / 4000);
    //                }
    //                if (demand_point_Demand > 5.0)
    //                {
    //                    var demand_point = new DemandPoint(demand_point_City, demand_point_district, demand_point_ID, demand_point_region, demand_point_long, demand_point_lat,  demand_point_dis_thres, demand_point_Demand);
    //                    _demand_point.Add(demand_point);

    //                    //if (Math.Ceiling(Convert.ToDouble(line[_month - 1]) / 4000) > 1)
    //                    //{
    //                    //    for (int i = 2; i <= Math.Ceiling(Convert.ToDouble(line[_month - 1]) / 4000); i++)
    //                    //    {
    //                    //        var demand_point_City_ = line[0];
    //                    //        var demand_point_district_ = line[1];
    //                    //        var demand_point_ID_ = line[2] + " " + i;
    //                    //        var demand_point_Region_ = line[3];
    //                    //        var demand_point_long_ = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
    //                    //        var demand_point_lat_ = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
    //                    //        var demand_point_dis_thres_ = Convert.ToDouble(line[6], System.Globalization.CultureInfo.InvariantCulture);
    //                    //        var demand_point_Demand_ = Convert.ToDouble(line[_month - 1]) / Math.Ceiling(Convert.ToDouble(line[_month - 1]) / 4000);
    //                    //        if (demand_point_Demand_ > scope_out_threshold)
    //                    //        {
    //                    //            var demand_point_ = new DemandPoint(demand_point_City_, demand_point_district_, demand_point_ID_, demand_point_Region_, demand_point_long_, demand_point_lat_, demand_point_dis_thres_, demand_point_Demand_);
    //                    //            _demand_point.Add(demand_point_);
    //                    //        }
    //                    //    }
    //                    //}
    //                }
    //            }catch(Exception ex)
    //            {
    //                var file_name = new DirectoryInfo(_demand_point_file).Name;
    //                var line_index = index.ToString();
    //                var failed_line = s.Replace(",","/");
    //                var failure = $"{file_name},{line_index},{failed_line}";
    //                failure_list.Add(failure);
    //            }
    //        }
    //    }
    //    var lines = System.IO.File.ReadAllLines(_demand_point_file);
    //    total_dictionary_of_Inputs.Add("Demand Points", lines);
    //}

    public void Read_Partially()
    {
        Read_Partial_Solution_Xdocks();
        Read_xDock_Neighborhood_Assignments();
    }

    public void Read_Partial_Solution_Xdocks()
    {
        using(var sr = File.OpenText(_xDocks_file))
        {
            String s = sr.ReadLine();
            var index = 1;
            while ((s = sr.ReadLine()) != null)
            {   index+= 1;
                try
                {
                    var line = s.Split(',');
                    var xDock_City = line[0];
                    var xDock_District = line[1];
                    var xDock_Id = line[2];
                    var xDock_region = line[3];
                    var type_value = false;
                    var xDock_lat = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
                    var xDock_long = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
                    var Already_Opened = Convert.ToBoolean(line[6], System.Globalization.CultureInfo.InvariantCulture);
                    //var xDock_dist_threshold = Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
                    var xDock_min_cap = Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
                    var hub_point= Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
                    var xdock_demand = Convert.ToDouble(line[9], System.Globalization.CultureInfo.InvariantCulture);
                    var xDock = new xDocks(xDock_City, xDock_District, xDock_Id, xDock_region, xDock_long, xDock_lat, xDock_min_cap,hub_point, xdock_demand, Already_Opened, type_value);
                    _partial_xdocks.Add(xDock);
                }
                catch(Exception ex)
                {
                    var file_name = new DirectoryInfo(_xDocks_file).Name;
                    var line_index = index.ToString();
                    var failed_line = s.Replace(",", "/");
                    var failure = $"{file_name},{line_index},{failed_line}";
                    failure_list.Add(failure);
                }
            }
            var lines = System.IO.File.ReadAllLines(_xDocks_file);
            total_dictionary_of_Inputs.Add("Partial Solution xDocks", lines);
        }
    }

    public void Read_xDock_Neighborhood_Assignments()
    {
        using (var sr = File.OpenText(_xDock_neighborhood_assignments_file))
        {
            String s = sr.ReadLine();
            var index = 1;
            while ((s = sr.ReadLine()) != null)
            {   index+= 1;
                try
                {
                    var line = s.Split(',');
                    if(line[0]!= "Atanmayan Talep Noktası")
                    {
                        var xdock_city = line[0];
                        var xdock_district = line[1];
                        var xdock_id = line[2];
                        var xdock_lat = Convert.ToDouble(line[3], System.Globalization.CultureInfo.InvariantCulture);
                        var xdock_long = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
                        var demand_point_city = line[5];
                        var demand_point_district = line[6];
                        var demand_point_id = line[7];
                        var demand_point_lat = Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
                        var demand_point_long = Convert.ToDouble(line[9], System.Globalization.CultureInfo.InvariantCulture);
                        var distance_xdock_county = Convert.ToDouble(line[10], System.Globalization.CultureInfo.InvariantCulture);
                        var demand = Convert.ToDouble(line[11], System.Globalization.CultureInfo.InvariantCulture);                        
                        var dummy_xDock = new xDocks(xdock_city, xdock_district, xdock_id, "a", xdock_long, xdock_lat, 1250,1,4000, false, false);
                        var neighborhood = new Mahalle(demand_point_id, demand_point_district, demand_point_long, demand_point_lat, demand);
                        var neighborhood_list = new List<Mahalle>();
                        var list_contains = _xDock_neighborhood_assignments.Keys.Where(x => x.Get_City() == xdock_city && x.Get_District() == xdock_district && x.Get_Id() == xdock_id).ToList();

                        if (list_contains.Count > 0)
                        {
                            _xDock_neighborhood_assignments[list_contains[0]].Add(neighborhood);

                        }
                        else
                        {
                            neighborhood_list.Add(neighborhood);
                            _xDock_neighborhood_assignments.Add(dummy_xDock, neighborhood_list);
                        }
                    }
                   
                }
                catch(Exception ex)
                {
                    var file_name = new DirectoryInfo(_xDock_neighborhood_assignments_file).Name;
                    var line_index = index.ToString();
                    var failed_line = s.Replace(",", "/");
                    var failure = $"{file_name},{line_index},{failed_line}";
                    failure_list.Add(failure);
                }
            }
            var lines = System.IO.File.ReadAllLines(_xDock_neighborhood_assignments_file);
            total_dictionary_of_Inputs.Add("Xdock Neighbourhood Assignments", lines);
        }
    }


    private void Read_Parameters()
    {
        using (var sr = File.OpenText(_parameter_file))
        {
            String s = sr.ReadLine();
            var index = 1;
            while ((s = sr.ReadLine()) != null)
            {
                index+= 1;
                try
                {
                    var line = s.Split(',');
                    var distinct_city = line[0];
                    var size = line[1];
                    var active = Convert.ToBoolean(Convert.ToDouble(line[2], System.Globalization.CultureInfo.InvariantCulture));
                    var parameter = new Parameters(distinct_city, size, active);
                    _parameters.Add(parameter);
                }
                catch(Exception ex)
                {
                    var file_name = new DirectoryInfo(_parameter_file).Name;
                    var line_index = index.ToString();
                    var failed_line = s.Replace(",", "/");
                    var failure = $"{file_name},{line_index},{failed_line}";
                    failure_list.Add(failure);
                }
                
            }
        }
        var lines = System.IO.File.ReadAllLines(_parameter_file);
        total_dictionary_of_Inputs.Add("Parameters", lines);
    }
    private void Read_Demand_Points_and_xDock()
    {
        using (var sr = File.OpenText(_demand_point_file))
        {
            String s = sr.ReadLine();
            var index = 1;
            while ((s = sr.ReadLine()) != null)
            {
                index += 1;
                try
                {
                    var line = s.Split(',');
                    var city= line[0];
                    var district = line[1];
                    var id = line[2];
                    var region = line[3];
                    var latitude = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
                    var longitude = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
                    var already_opened = Convert.ToDouble(line[6]);
                    var already_opened_boolean = false;
                    if (already_opened == 1) already_opened_boolean = true;
                    var distance_threshold=Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
                    var demand= Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
                    var min_xdock_cap= Convert.ToDouble(line[9], System.Globalization.CultureInfo.InvariantCulture);
                    var max_xdock_cap= Convert.ToDouble(line[10], System.Globalization.CultureInfo.InvariantCulture);
                    if (demand > 5)
                    {
                        var demand_point = new DemandPoint(city, district, id, region, longitude, latitude, distance_threshold, demand);
                        _demand_point.Add(demand_point);
                    }
                    if (max_xdock_cap > scope_out_threshold)
                    {
                        var potential_xdock_point = new xDocks(city, district, id, region, longitude, latitude, min_xdock_cap,1.5, max_xdock_cap, already_opened_boolean,false);
                        _xDocks.Add(potential_xdock_point);
                    }

                }
                catch(Exception Ex)
                {
                    var file_name = new DirectoryInfo(_demand_point_file).Name;
                    var line_index = index.ToString();
                    var failed_line = s.Replace(",", "/");
                    var failure = $"{file_name},{line_index},{failed_line}";
                    failure_list.Add(failure);
                }
            }
        }
        var lines = System.IO.File.ReadAllLines(_demand_point_file);
        total_dictionary_of_Inputs.Add("Demand/xDock File", lines);
    }

    private void Read_Potential_Hub_Points()
    {
        using (var sr = File.OpenText(_xDocks_file))
        {
            String s = sr.ReadLine();
            var index = 1;
            while ((s = sr.ReadLine()) != null)
            {
                index += 1;
                try
                {
                    var line = s.Split(',');
                    var city = line[0];
                    var district = line[1];
                    var id = line[2];
                    var region = line[3];
                    var latitude = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
                    var longitude = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
                    var already_opened = Convert.ToDouble(line[6], System.Globalization.CultureInfo.InvariantCulture);
                    var already_opened_boolean = false;
                    if (already_opened == 1) already_opened_boolean = true;
                    var distance_threshold= Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
                    var max_hub_cap= Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
                    var eliminated_potential_point = 1.0;
                    if (max_hub_cap == 0) eliminated_potential_point = 1.5;

                    var hub_info = new Hub(city, district, id, region, longitude, latitude, distance_threshold, eliminated_potential_point, max_hub_cap, 150, already_opened_boolean);
                    _Hub.Add(hub_info);
                }
                catch (Exception Ex)
                {
                    var file_name = new DirectoryInfo(_demand_point_file).Name;
                    var line_index = index.ToString();
                    var failed_line = s.Replace(",", "/");
                    var failure = $"{file_name},{line_index},{failed_line}";
                    failure_list.Add(failure);
                }
            }
        }
        var lines = System.IO.File.ReadAllLines(_xDocks_file);
        total_dictionary_of_Inputs.Add("Hub Points File", lines);
    }
    //private void Read_XDock()
    //{
    //    using (var sr = File.OpenText(_xDocks_file))
    //    {
    //        String s = sr.ReadLine();
    //        var index = 1;
    //        while ((s = sr.ReadLine()) != null)
    //        {
    //            index += 1;
    //            try
    //            {
    //                var line = s.Split(',');
    //                var xDock_City = line[0];
    //                var xDock_District = line[1];
    //                var xDock_Id = line[2];
    //                var xDock_region = line[3];
    //                var xDock_lat = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
    //                var xDock_long = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
    //                var xDock_dist_threshold = Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
    //                var xDock_min_cap = Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
    //                var hub_point = Convert.ToDouble(line[9], System.Globalization.CultureInfo.InvariantCulture);
    //                if (hub_point <= 1)
    //                {
    //                    hub_point = 1.5;
    //                }
    //                else
    //                {
    //                    var log_value = Math.Log(hub_point, 10)/2;
    //                    hub_point = (1.5 - log_value);
    //                }
    //                if (xDock_dist_threshold == 0.0)
    //                {
    //                    xDock_dist_threshold = region_xDock_threshold[xDock_region];
    //                }
    //                var Already_Opened = Convert.ToDouble(line[6]);
    //                var xDock_Already_Opened = false;
    //                if (Already_Opened == 1.0)
    //                {
    //                    xDock_Already_Opened = true;
    //                }
    //                var xDock_Capacity = Convert.ToDouble(line[_month+2]);

    //                if (xDock_Capacity > scope_out_threshold)
    //                {
    //                    var type_value = false;
    //                    var xDock = new xDocks(xDock_City, xDock_District, xDock_Id, xDock_region, xDock_long, xDock_lat, xDock_dist_threshold, xDock_min_cap,hub_point, xDock_Capacity, xDock_Already_Opened, type_value);
    //                    _xDocks.Add(xDock);

    //                }
    //            }
    //            catch(Exception Ex)
    //            {
    //                var file_name = new DirectoryInfo(_xDocks_file).Name;
    //                var line_index = index.ToString();
    //                var failed_line = s.Replace(",", "/");
    //                var failure = $"{file_name},{line_index},{failed_line}";
    //                failure_list.Add(failure);
    //            }
    //        }
    //    }
    //    var lines = System.IO.File.ReadAllLines(_xDocks_file);
    //    total_dictionary_of_Inputs.Add("Potential_xDocks", lines);
    //}
    public void Read_Sellers()
    {
        if (_seller_file != "")
        {
            using (var sr = File.OpenText(_seller_file))
            {
                String s = sr.ReadLine();
                var index = 1;
                while ((s = sr.ReadLine()) != null)
                {
                    index += 1;
                    try
                    {
                        var line = s.Split(",");
                        var seller_name = line[0];
                        var seller_id = line[1];
                        var seller_city = line[2];
                        var seller_district = line[3];
                        var seller_priority = Convert.ToDouble(line[4], System.Globalization.CultureInfo.InvariantCulture);
                        var seller_lat = Convert.ToDouble(line[5], System.Globalization.CultureInfo.InvariantCulture);
                        var seller_long = Convert.ToDouble(line[6], System.Globalization.CultureInfo.InvariantCulture);
                        var seller_demand = Convert.ToDouble(line[7], System.Globalization.CultureInfo.InvariantCulture);
                        var seller_dist = Convert.ToDouble(line[8], System.Globalization.CultureInfo.InvariantCulture);
                        var seller_size = line[9];
                        if (seller_size == "Small")
                        {
                            var small_seller = new Seller(seller_name, seller_id, seller_city, seller_district, seller_priority, seller_long, seller_lat, seller_demand, seller_dist, seller_size);
                            if (seller_priority == 1)
                            {
                                _prior_small_seller.Add(small_seller);
                            }
                            else
                            {
                                _regular_small_seller.Add(small_seller);
                            }
                        }
                        else
                        {
                            var big_seller = new Seller(seller_name, seller_id, seller_city, seller_district, seller_priority, seller_long, seller_lat, seller_demand, seller_dist, seller_size);
                            if (seller_priority == 1)
                            {
                                _prior_big_seller.Add(big_seller);
                            }
                            _regular_big_seller.Add(big_seller);
                        }
                        var total_seller = new Seller(seller_name, seller_id, seller_city, seller_district, seller_priority, seller_long, seller_lat, seller_demand, seller_dist, seller_size);
                        _total_seller.Add(total_seller);
                    }
                    catch(Exception ex)
                    {
                        var file_name = new DirectoryInfo(_seller_file).Name;
                        var line_index = index.ToString();
                        var failed_line = s.Replace(",", "/");
                        var failure = $"{file_name},{line_index},{failed_line}";
                        failure_list.Add(failure);
                    }
                }
            }
            var lines = System.IO.File.ReadAllLines(_seller_file);
            total_dictionary_of_Inputs.Add("Sellers", lines);
        }
    }
    

    public Dictionary<String,String[]> Get_Input_Dictionary()
    {
        return total_dictionary_of_Inputs;
    }
    public Dictionary<xDocks, List<Mahalle>> Get_xDock_neighborhood_Assignments()
    {
        return _xDock_neighborhood_assignments;
    }

    public List<xDocks> Get_XDocks()
    {
        return _xDocks;
    }

    public List<DemandPoint> Get_Demand_Points()
    {
        return _demand_point;
    }

    public List<Hub> Get_Hub_Points()
    {
        return _Hub;
    }
    public List<xDocks> Get_Agency()
    {
        return _agency;
    }
    public List<Seller> Get_Prior_Small_Sellers()
    {
        return _prior_small_seller;
    }
    public List<Seller> Get_Prior_Big_Sellers()
    {
        return _prior_big_seller;
    }
    public List<Seller> Get_Regular_Small_Sellers()
    {
        return _regular_small_seller;
    }
    public List<Seller> Get_Regular_Big_Sellers()
    {
        return _regular_big_seller;
    }

    public List<Seller> Get_Total_Sellers()
    {
        return _total_seller;
    }
    public List<xDocks> Get_Partial_Solution_Xdocks()
    {
        return _partial_xdocks;
    }

    public List<Parameters> Get_Parameter_List()
    {
        return _parameters;
    }
    public List<String> Get_Input_Failure_List()
    {
        return failure_list;
    }
    public Dictionary<string, List<Double>> Get_Distance_Matrix()
    {
        return dist_matrix;
    }
}
