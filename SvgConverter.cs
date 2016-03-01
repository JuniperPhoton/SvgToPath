using JP.Utils.Debug;
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
using Windows.UI.Xaml.Shapes;

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
        public static async Task<Viewbox> ConvertFromFileToViewboxAsync(StorageFile file, Size specifiedSize, bool readSize , bool readColor ,bool isDefaultBalck)
        {
            try
            {
                Viewbox viewBox = new Viewbox();
                Grid rootGrid = new Grid();

                using (var stream = await OpenFileAsync(file))
                {
                    var datas = ReadStreamAndConvertToPath(stream, isDefaultBalck);

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
                        if (!readColor)
                        {
                            if (isDefaultBalck)
                            {
                                path.Fill = new SolidColorBrush(Colors.Black);
                            }
                            else path.Fill = new SolidColorBrush(Colors.White);
                        }
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
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(SvgConverter), nameof(ConvertFromFileToViewboxAsync));
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
        private static Tuple<Size, List<Windows.UI.Xaml.Shapes.Path>, SolidColorBrush> ReadStreamAndConvertToPath(Stream stream,bool isDefaultBlack)
        {
            List<Windows.UI.Xaml.Shapes.Path> pathsToReturn = new List<Windows.UI.Xaml.Shapes.Path>();
            SolidColorBrush defaultColor =isDefaultBlack?new SolidColorBrush(Colors.Black):new SolidColorBrush(Colors.White);
            try
            {
                //Load XAML document
                var root = XElement.Load(stream);

                //Get some info.
                var widthInfo = root.Attributes().Where(a => a.Name.LocalName == "width").FirstOrDefault();
                var heightInfo = root.Attributes().Where(a => a.Name.LocalName == "height").FirstOrDefault();

                var width = widthInfo?.Value.Replace("px", string.Empty);
                var height = heightInfo?.Value.Replace("px", string.Empty);

                var size = new Size(double.Parse(width), double.Parse(height));
                if (size.Width == 0 || size.Height == 0) size = Size.Empty;
                
                //Get all paths
                var elements = root.Descendants().Where(e => (e.Name.LocalName == "path" || e.Name.LocalName=="polygon"));

                foreach (var element in elements)
                {
                    var localName = element.Name.LocalName;
                    if(localName=="path")
                    {
                        var d = (element.Attributes().Where(e => e.Name.LocalName == "d")).FirstOrDefault();
                        var fill = (element.Attributes().Where(e => e.Name.LocalName == "fill")).FirstOrDefault();
                        var newColor= isDefaultBlack ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
                        var opacityInfo = (element.Attributes().Where(e => e.Name.LocalName == "opacity")).FirstOrDefault();

                        var opacity = 1.0d;
                        if (opacityInfo != null)
                        {
                            opacity = double.Parse(opacityInfo.Value);
                        }

                        //In some case fill 's value is "none"
                        if (fill != null)
                        {
                            try
                            {
                                newColor = new SolidColorBrush(ColorConverter.Hex2Color(fill.Value));
                            }
                            catch (Exception)
                            {

                            }
                        }

                        Windows.UI.Xaml.Shapes.Path newPath = new Windows.UI.Xaml.Shapes.Path()
                        {
                            Fill = newColor,
                            Opacity = opacity,
                        };

                        var binding = new Binding
                        {
                            Source = d.Value,
                        };
                        BindingOperations.SetBinding(newPath, Windows.UI.Xaml.Shapes.Path.DataProperty, binding);

                        pathsToReturn.Add(newPath);
                    }
                    else if(localName=="polygon")
                    {
                        var newColor = isDefaultBlack ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);

                        var point = element.Attributes().Where(a => a.Name.LocalName == "points");
                        var dataStr = point.FirstOrDefault().Value;
                        var fill = (element.Attributes().Where(a => a.Name.LocalName == "fill")).FirstOrDefault();
                        var opacityInfo = (element.Attributes().Where(e => e.Name.LocalName == "opacity")).FirstOrDefault();

                        var opacity = 1.0d;
                        if (opacityInfo != null)
                        {
                            opacity = double.Parse(opacityInfo.Value);
                        }

                        Windows.UI.Xaml.Shapes.Path newPath = new Windows.UI.Xaml.Shapes.Path() { Fill = newColor,Opacity=opacity };
                        if (fill != null) newPath.Fill = new SolidColorBrush(ColorConverter.Hex2Color(fill.Value));

                        var binding = new Binding()
                        {
                            Source = dataStr.StartsWith("M") ? dataStr : "M" + dataStr,
                        };
                        BindingOperations.SetBinding(newPath, Windows.UI.Xaml.Shapes.Path.DataProperty, binding);

                        pathsToReturn.Add(newPath);
                    }
                }
                return Tuple.Create(size, pathsToReturn, defaultColor);
            }
            catch (Exception e)
            {
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(SvgConverter), nameof(ReadStreamAndConvertToPath));
                return Tuple.Create(new Size(), pathsToReturn, defaultColor);
            }
        }
    }
}
