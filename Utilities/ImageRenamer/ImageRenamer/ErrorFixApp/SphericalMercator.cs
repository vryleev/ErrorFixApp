using System;
using System.Configuration;

namespace ErrorFixApp
{
  public class MPoint
  {
    public MPoint()
    {
      X = 0.0;
      Y = 0.0;
    }
    public MPoint(double x, double y)
    {
      X = x;
      Y = y;
    }

    public readonly double X;
    public readonly double Y;
  }
  
  public class SphericalMercator
  {
    public static MPoint FromLonLat(MPoint point) => SphericalMercator.FromLonLat(point.X, point.Y);

    public static MPoint FromLonLat(double lon, double lat) => new MPoint(6378137.0 * (Math.PI / 180.0 * lon), 6378137.0 * Math.Log(Math.Tan(Math.PI / 4.0 + Math.PI / 180.0 * lat * 0.5)));

    public static MPoint ToLonLat(MPoint point) => SphericalMercator.ToLonLat(point.X, point.Y);

    public static MPoint ToLonLat(double x, double y)
    {
      double num = Math.PI / 2.0 - 2.0 * Math.Atan(Math.Exp(-y / 6378137.0));
      return new MPoint(x / 6378137.0 / (Math.PI / 180.0), num / (Math.PI / 180.0));
    }

    public static MPoint FromUnigineToLonLat(double x, double y)
    {
      double originX = Convert.ToDouble(ConfigurationManager.AppSettings.Get("OriginX"));
      double originY = Convert.ToDouble(ConfigurationManager.AppSettings.Get("OriginY"));
      double scaleX = Convert.ToDouble(ConfigurationManager.AppSettings.Get("ScaleX"));
      double scaleY = Convert.ToDouble(ConfigurationManager.AppSettings.Get("ScaleY"));
      MPoint sphPoint = new MPoint(originX + x / scaleX, originY + y / scaleY);
      return SphericalMercator.ToLonLat(sphPoint);
    }
  }
}
