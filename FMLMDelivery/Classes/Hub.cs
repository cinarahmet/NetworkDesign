using System;

public class Hub
{

    private String _city;

    private String _district;

    private String _id;

    private readonly String _region;

    private readonly Double _longitude;

    private readonly Double _latitude;

    private readonly Double _dist_thres;

    private readonly Double _hub_points;

    private  Double _lm_capacity;

    private Double _fm_capacity;

    private Double _chute_capacity;

    private readonly Boolean _already_opened;

    public Hub(String city, String district,String id,String region, Double longitude, Double latitude,Double dist_thres, Double hub_points, Double capacity,Double chute_capacity, Boolean already_opened)
    {
        _city = city;
        _district = district;
        _id = id;
        _region = region;
        _longitude = longitude;
        _latitude = latitude;
        _dist_thres = dist_thres;
        _lm_capacity = capacity;
        _fm_capacity = capacity;
        _already_opened = already_opened;
        _hub_points = hub_points;
        _chute_capacity = chute_capacity;
    }


    public Boolean If_Already_Opened()
    {
        return _already_opened;
    }

    public string Get_City()
    {
        return _city;
    }

    public string Get_Id()
    {
        return _id;
    }

    public string Get_Region()
    {
        return _region;
    }

    public Double Get_Distance_Threshold()
    {
        return _dist_thres;
    }

    public String Get_District()
    {
        return _district;
    }

    public Double Get_Longitude()
    {
        return _longitude;
    }

    public Double Get_Latitude()
    {
        return _latitude;
    }

    public Double Get_LM_Capacity()
    {
        return _lm_capacity;
    }

    public Double Get_FM_Capacity()
    {
        return _fm_capacity;
    }

    public void Set_FM_Capacity(double capacity)
    {
        _fm_capacity = capacity;
    }
    
    public void Set_LM_Capacity(double capacity)
    {
        _lm_capacity = capacity;
    }
    public Double Get_Hub_Points()
    {
        return _hub_points;
    }

    public Double Get_Chute_Capacity()
    {
        return _chute_capacity;
    }
}
