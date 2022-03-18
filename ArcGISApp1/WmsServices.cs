using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGISApp1
{
    public class WmsServices
    {
        private enum WmsSources
        {
            SCANEM40_2154,
            SCANEM40_4326,
            SCANE100_2154,
            SCANE100_4326,
        }

        private static readonly string ApiKey = "KEY_TO_PROVIDE";

        private readonly Dictionary<WmsSources, WmsInfos> _allWmsInfos = new Dictionary<WmsSources, WmsInfos>(){
            // KO
            {WmsSources.SCANEM40_2154, new WmsInfos(
                emprise: new double[4] { -63350.1569575526518747,6009116.9261413486674428, 1368966.2519538700580597, 7128563.3748103436082602 },
                wkid: 2154,
                layerName: "SCANEM40_PYR_PNG_FXX_LAMB93",
                url: "https://wxs.ign.fr/lambert93/geoportail/r/wms?SERVICE=WMS&VERSION=1.3.0&CRS=EPSG:2154"
            )},
            //OK
            {WmsSources.SCANEM40_4326, new WmsInfos(
                emprise: new double[4] { -6.0888954599999998, 41.1844102889999988, 10.9610087259999993, 50.9218000010000011 },
                wkid: 4326,
                layerName: "SCANEM40_PYR_PNG_FXX_LAMB93",
                url: "https://wxs.ign.fr/lambert93/geoportail/r/wms?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities"
            )},
            // KO
            {WmsSources.SCANE100_2154, new WmsInfos(
                emprise: new double[4] { -63350.1569575526518747,6009116.9261413486674428, 1368966.2519538700580597, 7128563.3748103436082602 },
                wkid: 2154,
                layerName: "SCAN100_PYR-PNG_FXX_LAMB93",
                url: $"https://wxs.ign.fr/{ApiKey}/geoportail/r/wms?SERVICE=WMS&REQUEST=GetCapabilities"
            )},
            // OK
            {WmsSources.SCANE100_4326, new WmsInfos(
                emprise: new double[4] { -6.0888954599999998, 41.1844102889999988, 10.9610087259999993, 50.9218000010000011 },
                wkid: 4326,
                layerName: "SCAN100_PYR-PNG_FXX_LAMB93",
                url: $"https://wxs.ign.fr/{ApiKey}/geoportail/r/wms?SERVICE=WMS&REQUEST=GetCapabilities"
            )},
        };


        private readonly WmsSources _sourceToLoad = WmsSources.SCANE100_4326;

        public Map getMap() => _allWmsInfos[_sourceToLoad].getMap();

        public async Task<WmsLayer> getWmsLayer() => await _allWmsInfos[_sourceToLoad].getWmsLayer();
    }

    internal class WmsInfos
    {
        private readonly double[] _emprise;
        private readonly int _wkid;
        private readonly string _layerName;
        private readonly string _url;

        public WmsInfos(double[] emprise, int wkid, string layerName, string url)
        {
            _emprise = emprise;
            _wkid = wkid;
            _layerName = layerName;
            _url = url;
        }

        public Map getMap() => new Map(new SpatialReference(_wkid))
        {
            InitialViewpoint = new Viewpoint(
                new Envelope(_emprise[0], _emprise[1], _emprise[2], _emprise[3], 0.0, 0.0, new SpatialReference(_wkid)))
        };

        public async Task<WmsLayer> getWmsLayer()
        {
            var wmsLayer = new WmsLayer(new Uri(_url), new List<string> { _layerName });
            await wmsLayer.LoadAsync();
            return wmsLayer; // BUG here : wmsLayer.SpatialReference always equals to 4326
        }
    }
}
