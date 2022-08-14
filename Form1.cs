using GMap.NET;
using GMap.NET.MapProviders;
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
        private GMapOverlay _markersOverlay;
        private readonly MapMarkerRepository _MapMarkerRepository;

        public Form1()
        {
            InitializeComponent();
            _MapMarkerRepository = new MapMarkerRepository();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //выбор подгрузки карты – онлайн или из ресурсов
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            //какой провайдер карт используется (в нашем случае гугл) 
            gMapControl1.MapProvider = GoogleMapProvider.Instance;
            //минимальный зум
            gMapControl1.MinZoom = 2;
            //максимальный зум
            gMapControl1.MaxZoom = 16;
            // какой используется зум при открытии
            gMapControl1.Zoom = 4;
            // точка в центре карты при открытии (центр России)
            gMapControl1.Position = new PointLatLng(66.4169575018027, 94.25025752215694);
            // как приближает (просто в центр карты или по положению мыши)
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
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

            
            var marker = _MapMarkerRepository.GetAndAddAllMarker();
            foreach (var item in marker)
            {
                _markersOverlay.Markers.Add(item);
            }
        }
        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
                double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

                var markerGoogle = _MapMarkerRepository.AddMarker(lat, lng);
                MarkerRefresh();
                Console.WriteLine(markerGoogle.Tag.ToString());
            }
        }
        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
               _MapMarkerRepository.DeleteMarker(item);
               _markersOverlay.Markers.Remove(item);
            }
        }
        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                GMapMarker selectedMarker = gMapControl1.Overlays.SelectMany(m => m.Markers).FirstOrDefault(m => m.IsMouseOver == true);
                if (selectedMarker != null)
                {
                    PointLatLng latLng = gMapControl1.FromLocalToLatLng(e.X, e.Y);
                    selectedMarker.Position = latLng;
                    _MapMarkerRepository.UpdateMarkerLocation(selectedMarker);
                }
            }
        }

        private void MarkerRefresh()
        {
            _markersOverlay.Clear();
            var marker = _MapMarkerRepository.GetAndAddAllMarker();
            foreach (var item in marker)
            {
                _markersOverlay.Markers.Add(item);
            }
        }
    }
}
