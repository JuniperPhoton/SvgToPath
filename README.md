# SvgToPath
A library that helps you convert SVG file to XAML paths. 

Up to now ,it's quite simple and thus don't suit any of cases that you may meet.

The SVG file that the library can work with should:

> Contains paths or polygens.

So if your SVG file contains `Rect` or `Line`, this library will just ignore them. So feel free to make contributions to this project ;-)

**NOTE**

There is a workaround that allows you display a SVG file that contains other elements like `Rect` or `Line`: Convert them to `Path`.

> Open the file with `Adobe Illustrator ` , select all the elements and right click:Build multi-paths and then save it as SVG. 

