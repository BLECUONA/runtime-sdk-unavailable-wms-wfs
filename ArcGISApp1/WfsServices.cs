using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISApp1
{
    public class WfsServices
    {
        private MapView _mapView;

        private readonly WfsInfos[] _allWmsInfos = {
            // OK: displaid
            new WfsInfos(
                emprise: new double[4] { 74520.90, 5985493.31, 495229.79, 6458456.52 },
                wkid: 3857,
                layerName: "MNHN:ZPS",
                url: "https://inpn-inspire.mnhn.fr/geoservices/ows"
            ),
            // KO: not displaid
            new WfsInfos(
                emprise: new double[4] { -6.0888954599999998, 41.1844102889999988, 10.9610087259999993, 50.9218000010000011 },
                wkid: 4326,
                layerName: "BDTOPO_V3:troncon_de_route",
                url: "https://wxs.ign.fr/essentiels/geoportail/wfs?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetCapabilities"
            ),
            // KO: not displaid
            new WfsInfos(
                emprise: new double[4] { 74520.90, 5985493.31, 495229.79, 6458456.52 },
                wkid: 3857,
                layerName: "BDTOPO_V3:troncon_de_route",
                url: "https://wxs.ign.fr/essentiels/geoportail/wfs"
            ),
            // KO: not displaid
            new WfsInfos(
                emprise: new double[4] { -63350.1569575526518747,6009116.9261413486674428, 1368966.2519538700580597, 7128563.3748103436082602 },
                wkid: 2154,
                layerName: "BDTOPO_V3:troncon_de_route",
                url: "https://wxs.ign.fr/essentiels/geoportail/wfs"
            )
        };


        private readonly int _indexToLoad = 3;

        public WfsServices(MapView mapView)
        {
            _mapView = mapView;
        }

        public Map getMap()=> _allWmsInfos[_indexToLoad].getMap();

        public async Task<FeatureLayer> getWfsLayer()=> await _allWmsInfos[_indexToLoad].getWfsLayer(_mapView);
    }

    internal class WfsInfos
    {
        private readonly double[] _emprise;
        private readonly int _wkid;
        private readonly string _layerName;
        private readonly string _url;
        private MapView _mapView;
        private WfsFeatureTable _wfsFeatureTable;

        public WfsInfos(double[] emprise, int wkid, string layerName, string url)
        {
            _emprise = emprise;
            _wkid = wkid;
            _layerName = layerName;
            _url = url;
        }

        public Map getMap()=> new Map(new SpatialReference(_wkid))
            {
                InitialViewpoint = new Viewpoint(
                GetEnveloppe())
            };

        public async Task<FeatureLayer> getWfsLayer(MapView mapView)
        {
            _mapView = mapView;

            // Create the feature table from URI and layer name.
            _wfsFeatureTable = new WfsFeatureTable(new Uri(_url), _layerName);

            // Set the feature request mode to manual - only manual is supported at v100.5.
            // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
            _wfsFeatureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

            _wfsFeatureTable.LoadStatusChanged += _wfsFeatureTable_LoadStatusChanged;
            // Load the table.
            await _wfsFeatureTable.LoadAsync();

            // Create a feature layer to visualize the WFS features.
            FeatureLayer wfsFeatureLayer = CreateWfsFeatureLayer(_wfsFeatureTable);

            // Apply a renderer.
            wfsFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 3));

            bool refreshWfsOnNavigationCompleted = true;
            if (refreshWfsOnNavigationCompleted)
            {
                // Use the navigation completed event to populate the table with the features needed for the current extent.
                _mapView.NavigationCompleted += Refresh_WFS_On_MapView_NavigationCompleted;
            }
            else
            {
                QueryParameters visibleExtentQuery = new QueryParameters();
                visibleExtentQuery.Geometry = GetEnveloppe();
                visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;
                await _wfsFeatureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }


            return wfsFeatureLayer;

            void _wfsFeatureTable_LoadStatusChanged(object sender, LoadStatusEventArgs e)
            {
                var status = _wfsFeatureTable.LoadStatus; // BUG here for https://wxs.ign.fr/essentiels/geoportail/wfs (identifier "BDTOPO_V3:troncon_de_route") --> LoadStatus is "Failed to load" with error "Shape type type not supported in runtime geodatabase" 
            }
        }

        private Envelope GetEnveloppe()
        {
            return new Envelope(_emprise[0], _emprise[1], _emprise[2], _emprise[3], 0.0, 0.0, new SpatialReference(_wkid));
        }

        private static FeatureLayer CreateWfsFeatureLayer(WfsFeatureTable wfsFeatureTable)=> new FeatureLayer(wfsFeatureTable);

        private async void Refresh_WFS_On_MapView_NavigationCompleted(object sender, EventArgs e)
        {
            // Get the current extent.
            Envelope currentExtent = _mapView.VisibleArea.Extent;

            // Create a query based on the current visible extent.
            QueryParameters visibleExtentQuery = new QueryParameters();
            visibleExtentQuery.Geometry = currentExtent;
            visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
                // Setting outFields to null requests all features.
                await _wfsFeatureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Couldn't populate table.");
                Debug.WriteLine(exception);
            }
        }
    }
}
