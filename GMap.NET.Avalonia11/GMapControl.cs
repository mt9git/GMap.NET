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
