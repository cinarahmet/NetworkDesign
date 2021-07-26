using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public class xDocks
{
    private readonly String _city;

    private readonly String _district;

    private readonly String _id;

    private readonly String _region;

    private readonly Double _longitude;

    private readonly Double _latitude;

    //private readonly Double _distance_threshold;

    private readonly Double _min_xdock_cap;

    private readonly Double _hub_points;

    private Double _lm_demand;

    private Double _fm_demand;

    private readonly Boolean _already_opened;

    private readonly Boolean _type_value;

    

    public xDocks(String city,String district,String id,String region, Double longitude, Double latitude,Double min_xdock_cap,Double Hub_Points,Double demand,Boolean already_opened,Boolean type_value)
    {
        _city = city;
        _district = district; 
        _id = id;
        _region = region;
        _longitude = longitude;
        _latitude = latitude;
        //_distance_threshold = distance_threshold;
        _lm_demand = demand;
        _fm_demand = 0;
        _already_opened = already_opened;
        _type_value = type_value;
        _min_xdock_cap = min_xdock_cap;
        _hub_points = Hub_Points;
    }

    public Boolean If_Already_Opened()
    {
        return _already_opened;
    }

    public Boolean If_Agency()
    {
        return _type_value;
    }

    //public Double Get_Distance_Threshold()
    //{
    //    return _distance_threshold;
    //}

    public String Get_District()
    {
        return _district;
    }

    public string Get_City()
    {
        return _city;
    }

    public string Get_Region()
    {
        return _region;
    }
    public Double Get_Min_Cap()
    {
        return _min_xdock_cap;
    }
    public string Get_Id()
    {
        return _id;
    }

    public Double Get_Longitude()
    {
        return _longitude;
    }

    public Double Get_Latitude()
    {
        return _latitude;
    }

    public Double Get_LM_Demand()
    {
        return _lm_demand;
    }

    public Double Get_FM_Demand()
    {
        return _fm_demand;
    }

    public void Add_FM_Demand(double demand)
    {
        _fm_demand += demand;
    }

    public void Add_LM_Demand(double demand)
    {
        _lm_demand += demand;
    }
   
    public Double Get_Hub_Point()
    {
        return _hub_points;
    }
}

    