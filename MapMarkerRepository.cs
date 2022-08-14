using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TestTask
{
    public class MapMarkerRepository
    {
        private string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private MapMarker _mapMarker;

        public MapMarkerRepository()
        {
            _mapMarker = new MapMarker();
        }

        public List<GMarkerGoogle> GetAndAddAllMarker()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                List <GMarkerGoogle> listMarker = new List<GMarkerGoogle>();
                try
                {
                    connection.Open();
                    SqlCommand sql = new SqlCommand("SELECT Id, Latitude, Longitude FROM dbo.coordinates", connection);
                    SqlDataReader reader = sql.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            
                            
                            _mapMarker.Id = reader.GetInt32(0);
                            _mapMarker.tempLat = reader.GetDouble(1);
                            _mapMarker.tempLng = reader.GetDouble(2);
                            GMarkerGoogle gMarker = new GMarkerGoogle(new PointLatLng(_mapMarker.tempLat, _mapMarker.tempLng), GMarkerGoogleType.red)
                            {
                                ToolTipText = _mapMarker.Id.ToString(),
                                Tag = _mapMarker.Id
                            };
                            listMarker.Add(gMarker);
                        }
                        reader.Close();
                    }
                    return listMarker;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public GMarkerGoogle AddMarker(double lat, double lng)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("INSERT INTO coordinates VALUES(@lat, @lng)", connection);
                    SqlParameter latParameter = new SqlParameter("@lat", lat);
                    command.Parameters.Add(latParameter);
                    SqlParameter lngParameter = new SqlParameter("@lng", lng);
                    command.Parameters.Add(lngParameter);
                    command.ExecuteNonQuery();
                    GMarkerGoogle gMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red)
                    { 
                        ToolTipText = _mapMarker.Id.ToString(),
                        Tag = _mapMarker.Id
                    };
                    return gMarker;
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public void DeleteMarker(GMapMarker item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand sql = new SqlCommand("DELETE FROM coordinates WHERE Id=@id", connection);
                    SqlParameter idParam = new SqlParameter("@Id", item.Tag);
                    sql.Parameters.Add(idParam);
                    sql.ExecuteNonQuery();
                }
                catch(SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void UpdateMarkerLocation(GMapMarker marker)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("UPDATE coordinates SET Latitude=@lat, Longitude=@lng WHERE Id=@id", connection);
                    SqlParameter latParameter = new SqlParameter("@lat", marker.Position.Lat);
                    command.Parameters.Add(latParameter);
                    SqlParameter lngParameter = new SqlParameter("@lng", marker.Position.Lng);
                    command.Parameters.Add(lngParameter);
                    SqlParameter idParametr = new SqlParameter("@id", marker.Tag.ToString());
                    command.Parameters.Add(idParametr);
                    command.ExecuteNonQuery();
                }
                catch(SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
