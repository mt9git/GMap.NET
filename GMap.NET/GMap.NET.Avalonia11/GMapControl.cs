using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace GMap.NET.Avalonia
{
    /// <summary>
    ///     GMap.NET control for Avalonia (Windows Presentation)
    /// </summary>
    public partial class GMapControl : Control, Interface, IDisposable
    {
        # region GMap.NET Control
        private readonly Core _core = new Core();

        private bool _showTileGridLines;

        /// <summary>
        /// Current selected area in the map
        /// </summary>
        private RectLatLng _selectedArea;
        /// <summary>
        ///     use circle for selection
        /// </summary>
        public bool SelectionUseCircle { get; } = false;
        public RectLatLng? BoundsOfMap { get; }

        [Category("GMap.NET")]
        [Description("maximum zoom level of map")]
        public int MaxZoom
        {
            get { return _core.MaxZoom; }
            set { _core.MaxZoom = value; }
        }

        [Category("GMap.NET")]
        [Description("minimum zoom level of map")]
        public int MinZoom
        {
            get { return _core.MinZoom; }
            set { _core.MinZoom = value; }
        }

        [Category("GMap.NET")]
        [Description("map zooming type for mouse wheel")]
        public MouseWheelZoomType MouseWheelZoomType
        {
            get { return _core.MouseWheelZoomType; }
            set { _core.MouseWheelZoomType = value; }
        }

        [Category("GMap.NET")]
        [Description("enable map zoom on mouse wheel")]
        public bool MouseWheelZoomEnabled
        {
            get { return _core.MouseWheelZoomEnabled; }
            set { _core.MouseWheelZoomEnabled = value; }
        }

        [Category("GMap.NET")]
        public PointerUpdateKind DragButton { get; } = PointerUpdateKind.LeftButtonPressed;

        [Category("GMap.NET")]
        public bool ShowTileGridLines
        {
            get { return _showTileGridLines; }
            set
            {
                _showTileGridLines = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        ///     retry count to get tile
        /// </summary>
        [Browsable(false)]
        public int RetryLoadTile
        {
            get { return _core.RetryLoadTile; }
            set { _core.RetryLoadTile = value; }
        }

        /// <summary>
        ///     how many levels of tiles are staying decompresed in memory
        /// </summary>
        [Browsable(false)]
        public int LevelsKeepInMemory
        {
            get { return _core.LevelsKeepInMemory; }
            set { _core.LevelsKeepInMemory = value; }
        }

        [Browsable(false)]
        public RectLatLng SelectedArea
        {
            get { return _selectedArea; }
            set
            {
                _selectedArea = value;
                InvalidateVisual();
            }
        }

        #endregion

        #region Avalonia
        public static readonly StyledProperty<ObservableCollection<GMapMarker>> MarkersProperty =
            AvaloniaProperty.Register<GMapControl, ObservableCollection<GMapMarker>>(
                nameof(Markers));
        public ObservableCollection<GMapMarker> Markers
        {
            get { return GetValue(MarkersProperty); }
            private set { SetValue(MarkersProperty, value); }
        }

        private static DataTemplate _dataTemplateInstance;
        private static ItemsPanelTemplate _itemsPanelTemplateInstance;
        private static Style _styleInstance;

        /// <summary>
        ///     current markers overlay offset
        /// </summary>
        internal readonly TranslateTransform MapTranslateTransform = new TranslateTransform();
        internal readonly TranslateTransform MapOverlayTranslateTransform = new TranslateTransform();
        internal ScaleTransform? MapScaleTransform = new ScaleTransform();
        internal RotateTransform MapRotateTransform = new RotateTransform();

        /// <summary>
        ///     pen for empty tile borders
        /// </summary>
        public Pen EmptyTileBorders { get; } = new(Brushes.White, 1.0);

        /// <summary>
        ///     pen for Selection
        /// </summary>
        public Pen SelectionPen { get; } = new(Brushes.Blue, 2.0);

        /// <summary>
        ///     background of selected area
        /// </summary>
        public Brush SelectedAreaFill { get; } =
            new SolidColorBrush(Color.FromArgb(33, Colors.RoyalBlue.R, Colors.RoyalBlue.G, Colors.RoyalBlue.B));

        /// <summary>
        ///     pen for empty tile background
        /// </summary>
        public ISolidColorBrush EmptyTileBrush = Brushes.Navy;

        /// <summary>
        ///     text on empty tiles
        /// </summary>
        public FormattedText EmptyTileText { get; } =
            GetFormattedText("We are sorry, but we don't\nhave imagery at this zoom\n     level for this region.", 16);

        protected bool DesignModeInConstruct
        {
            get { return Design.IsDesignMode; }
        }

        private Canvas _mapCanvas;

        /// <summary>
        ///     markers overlay
        /// </summary>
        internal Canvas MapCanvas
        {
            get
            {
                if (_mapCanvas == null)
                {
                    if (VisualChildren.Count > 0)
                    {
                        _mapCanvas = this.GetVisualDescendants().FirstOrDefault(w => w is Canvas) as Canvas;
                        _mapCanvas.RenderTransform = MapTranslateTransform;
                    }
                }

                return _mapCanvas;
            }
        }

        public GMaps Manager
        {
            get { return GMaps.Instance; }
        }

        private static FormattedText GetFormattedText(string text, int size)
        {
            return new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("GenericSansSerif"),
                size, Brushes.AntiqueWhite);
        }
        #endregion


        #region GMap.NET Interfaces

        public PointLatLng Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public GPoint PositionPixel => throw new NotImplementedException();

        public string CacheLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsDragging => throw new NotImplementedException();

        public RectLatLng ViewArea => throw new NotImplementedException();

        public GMapProvider MapProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanDragMap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public RenderMode RenderMode => throw new NotImplementedException();

        public event PositionChanged OnPositionChanged;
        public event TileLoadComplete OnTileLoadComplete;
        public event TileLoadStart OnTileLoadStart;
        public event MapDrag OnMapDrag;
        public event MapZoomChanged OnMapZoomChanged;
        public event MapTypeChanged OnMapTypeChanged;

        public GPoint FromLatLngToLocal(PointLatLng point)
        {
            throw new NotImplementedException();
        }

        public PointLatLng FromLocalToLatLng(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void ReloadMap()
        {
            throw new NotImplementedException();
        }

        public bool ShowExportDialog()
        {
            throw new NotImplementedException();
        }

        public bool ShowImportDialog()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
