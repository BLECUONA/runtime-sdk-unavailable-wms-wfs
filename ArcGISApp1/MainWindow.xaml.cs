using System.Threading.Tasks;
using System.Windows;

namespace ArcGISApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Mode { Wms, Wfs }
        public MainWindow()
        {
            InitializeComponent();
            Initialize(Mode.Wms);
        }

        private void Initialize(Mode mode)
        {
            switch (mode)
            {
                case Mode.Wms:
                    InitializeWms();
                    break;
                case Mode.Wfs:
                    InitializeWfs();
                    break;
                default:
                    break;
            }
        }

        private async Task InitializeWms()
        {
            var wmsService = new WmsServices();
            var myMap = wmsService.getMap();
            MyMapView.Map = myMap;

            var myWmsLayer = await wmsService.getWmsLayer();
            myMap.OperationalLayers.Add(myWmsLayer);
        }

        private async Task InitializeWfs()
        {
            var wfsService = new WfsServices(MyMapView);
            var myMap = wfsService.getMap();
            MyMapView.Map = myMap;

            var myWfsLayer = await wfsService.getWfsLayer();
            myMap.OperationalLayers.Add(myWfsLayer);
        }

    }
}
