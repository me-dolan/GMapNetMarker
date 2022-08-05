using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTask
{
    public partial class Form1 : Form
    {

        private GMapMarker _selectedMarker;
        private GMapOverlay _markersOverlay;

        private MapMarker _mapMarker;

        private string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _mapMarker = new MapMarker();
            //выбор подгрузки карты – онлайн или из ресурсов
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            //какой провайдер карт используется (в нашем случае гугл) 
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            //минимальный зум
            gMapControl1.MinZoom = 2;
            //максимальный зум
            gMapControl1.MaxZoom = 16;
            // какой используется зум при открытии
            gMapControl1.Zoom = 4;
            // точка в центре карты при открытии (центр России)
            gMapControl1.Position = new GMap.NET.PointLatLng(66.4169575018027, 94.25025752215694);
            // как приближает (просто в центр карты или по положению мыши)
            gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            // перетаскивание карты мышью
            gMapControl1.CanDragMap = true; 
            // какой кнопкой осуществляется перетаскивание
            gMapControl1.DragButton = MouseButtons.Left;
            //показывать или скрывать красный крестик в центре
            gMapControl1.ShowCenter = false;
            //показывать или скрывать тайлы
            gMapControl1.ShowTileGridLines = false;

            _markersOverlay = new GMapOverlay("marker");
            gMapControl1.Overlays.Add(_markersOverlay);

            GetAndAddAllMarker();
        }
        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

                AddMarker(lat, lng);
            }
        }
        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                double lat = item.Position.Lat;
                double lng = item.Position.Lng;

                DeleteMarker(lat, lng, item);
            }
        }
        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(_selectedMarker != null)
                {
                    PointLatLng latLng = gMapControl1.FromLocalToLatLng(e.X, e.Y);
                    _selectedMarker.Position = latLng;
                    UpdateMarkerLocation(_selectedMarker);
                }
            }
        }
        private void gMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            _selectedMarker = gMapControl1.Overlays.SelectMany(m => m.Markers).FirstOrDefault(m => m.IsMouseOver == true);
            FindCurrentMarket();
        }
        
        
        //можно вынести в отдельный класс для удобства, в идеале реализовать класс через интерфейс, репозиторий патерн
        private void GetAndAddAllMarker()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand sql = new SqlCommand("SELECT * FROM dbo.coordinates", connection);
                SqlDataReader reader = sql.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _mapMarker.Id = reader.GetInt32(0);
                        _mapMarker.tempLat = reader.GetDouble(1);
                        _mapMarker.tempLng = reader.GetDouble(2);
                        GMarkerGoogle gMarker = new GMarkerGoogle(new PointLatLng(_mapMarker.tempLat, _mapMarker.tempLng), GMarkerGoogleType.red);

                        gMarker.ToolTipText = _mapMarker.Id.ToString();
                        _markersOverlay.Markers.Add(gMarker);

                    }
                }
                reader.Close();
            }
        }

        private void AddMarker(double lat, double lng)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO coordinates VALUES(@lat, @lng)", connection);
                SqlParameter latParameter = new SqlParameter("@lat", lat);
                command.Parameters.Add(latParameter);
                SqlParameter lngParameter = new SqlParameter("@lng", lng);
                command.Parameters.Add(lngParameter);
                command.ExecuteNonQuery();
                GMarkerGoogle gMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red);
                gMarker.ToolTipText = _mapMarker.Id.ToString();
                _markersOverlay.Markers.Add(gMarker);
            }
        }

        private void DeleteMarker(double lat, double lng, GMapMarker item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand sql = new SqlCommand("DELETE FROM coordinates WHERE Latitude=@lat AND Longitude=@lng", connection);
                SqlParameter latParameter = new SqlParameter("@lat", lat);
                sql.Parameters.Add(latParameter);
                SqlParameter lngParameter = new SqlParameter("@lng", lng);
                sql.Parameters.Add(lngParameter);
                sql.ExecuteNonQuery();
            }
            _markersOverlay.Markers.Remove(item);
        }

        private void FindCurrentMarket()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (_selectedMarker != null)
                {
                    SqlCommand command = new SqlCommand("SELECT Id FROM coordinates WHERE Latitude=@lat AND Longitude=@lng", connection);
                    SqlParameter latParameter = new SqlParameter("@lat", _selectedMarker.Position.Lat);
                    command.Parameters.Add(latParameter);
                    SqlParameter lngParameter = new SqlParameter("@lng", _selectedMarker.Position.Lng);
                    command.Parameters.Add(lngParameter);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            _mapMarker.Id = reader.GetInt32(0);
                        }
                    }
                    reader.Close();
                }
            }
        }

        private void UpdateMarkerLocation(GMapMarker marker)
        {
            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("UPDATE coordinates SET Latitude=@lat, Longitude=@lng WHERE Id=@id", connection);
                SqlParameter latParameter = new SqlParameter("@lat", marker.Position.Lat);
                command.Parameters.Add(latParameter);
                SqlParameter lngParameter = new SqlParameter("@lng", marker.Position.Lng);
                command.Parameters.Add(lngParameter);
                SqlParameter idParametr = new SqlParameter("@id", _mapMarker.Id);
                command.Parameters.Add(idParametr);
                command.ExecuteNonQuery();
            }
        }
    }
}
