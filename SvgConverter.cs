using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Svg2Path
{
    public static class SvgConverter
    {
        /// <summary>
        /// Open SVG file, convert the SVG paths to XAML path. 
        /// </summary>
        /// <param name="file">SVG file</param>
        /// <param name="specifiedSize">Specify the size, if not, please use Size.Empty</param>
        /// <param name="readSize">read the size defined in the SVG file or not</param>
        /// <returns></returns>
        public static async Task<Viewbox> ConvertFromFileToViewboxAsync(StorageFile file, Size specifiedSize, bool readSize = true, bool readColor = false)
        {
            try
            {
                Viewbox viewBox = new Viewbox();
                Grid rootGrid = new Grid();

                using (var stream = await OpenFileAsync(file))
                {
                    var datas = ReadStreamAndConvertToPath(stream);

                    if (readSize)
                    {
                        var size = datas.Item1;
                        viewBox.Width = size.Width;
                        viewBox.Height = size.Height;
                    }
                    else if (specifiedSize != Size.Empty)
                    {
                        viewBox.Width = specifiedSize.Width;
                        viewBox.Height = specifiedSize.Height;
                    }
                    else viewBox.Width = 100;

                    var paths = datas.Item2;
                    foreach (var path in paths)
                    {
                        if (readColor) path.Fill = datas.Item3;
                        rootGrid.Width = viewBox.Width;
                        rootGrid.Height = viewBox.Height;
                        rootGrid.Children.Add(path);
                    }
                    viewBox.Child = rootGrid;
                    return viewBox;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Read the SVG file and return it as stream
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static async Task<Stream> OpenFileAsync(StorageFile file)
        {
            var stream = await file.OpenStreamForReadAsync();
            return stream;
        }

        /// <summary>
        /// Read the SVG file as XML
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns></returns>
        private static Tuple<Size, List<Windows.UI.Xaml.Shapes.Path>, SolidColorBrush> ReadStreamAndConvertToPath(Stream stream)
        {
            List<Windows.UI.Xaml.Shapes.Path> pathsToReturn = new List<Windows.UI.Xaml.Shapes.Path>();
            SolidColorBrush color = new SolidColorBrush(Colors.White);
            try
            {
                //Load XAML document
                var root = XElement.Load(stream);

                //Get some info.
                var widthInfo = root.Attributes().Where(a => a.Name.LocalName == "width").FirstOrDefault();
                var heightInfo = root.Attributes().Where(a => a.Name.LocalName == "height").FirstOrDefault();
                var width = widthInfo.Value.Replace("px", string.Empty);
                var height = heightInfo.Value.Replace("px", string.Empty);
                var size = new Size(double.Parse(width), double.Parse(height));

                //Get all paths
                var paths = root.Descendants().Where(e => e.Name.LocalName == "path");

                //Get all polygons
                var polygons = root.Descendants().Where(e => e.Name.LocalName == "polygon");

                foreach (var path in paths)
                {
                    Windows.UI.Xaml.Shapes.Path newPath = new Windows.UI.Xaml.Shapes.Path()
                    {
                        Fill = new SolidColorBrush(Colors.White),
                        Stretch = Stretch.None,
                    };

                    var d = path.Attributes().Where(e => e.Name.LocalName == "d");
                    var fill = path.Attributes().Where(e => e.Name.LocalName == "fill");
                    var dataSrc = d.FirstOrDefault().Value;
                    if (fill != null && fill.Count() > 0)
                    {
                        var fillData = fill.FirstOrDefault().Value;
                        if (fillData != null) color = new SolidColorBrush(ColorConverter.Hex2Color(fillData));
                    }

                    var binding = new Binding
                    {
                        Source = dataSrc,
                    };
                    BindingOperations.SetBinding(newPath, Windows.UI.Xaml.Shapes.Path.DataProperty, binding);

                    pathsToReturn.Add(newPath);
                }

                foreach (var polygon in polygons)
                {
                    Windows.UI.Xaml.Shapes.Path newPath = new Windows.UI.Xaml.Shapes.Path() { Fill = new SolidColorBrush(Colors.White), Stretch = Stretch.Uniform };

                    var point = polygon.Attributes().Where(a => a.Name.LocalName == "points");
                    var dataStr = point.FirstOrDefault().Value;

                    var binding = new Binding()
                    {
                        Source = dataStr.StartsWith("M") ? dataStr : "M" + dataStr,
                    };
                    BindingOperations.SetBinding(newPath, Windows.UI.Xaml.Shapes.Path.DataProperty, binding);

                    pathsToReturn.Add(newPath);
                }

                return Tuple.Create(size, pathsToReturn, color);

            }
            catch (Exception e)
            {
                return Tuple.Create(new Size(), pathsToReturn, color);
            }
        }
    }
}
