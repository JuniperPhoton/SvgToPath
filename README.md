# SvgToPath

###**ENGLISHT**
A UWP library that helps you convert SVG file to XAML paths. 

Up to now ,it's quite simple and thus don't suit any of cases that you may meet.

The SVG file that the library can work with should:

> Contains paths or polygens.

So if your SVG file contains `Rect` or `Line`, this library will just ignore them. So feel free to make contributions to this project ;-)

**NOTE**

There is a workaround that allows you display a SVG file that contains other elements like `Rect` or `Line`: Convert them to `Path`.

> Open the file with `Adobe Illustrator ` , select all the elements and right click:Build multi-paths and then save it as SVG. 

###**CHINESE**

这是一个将 SVG 文件转为 XAML path 的 UWP 工程。

目前还是相对比较简单，在使用之前，请确保：

> 你的 SVG 文件包含 path 或者 polygen 这两种元素（以文本的形式打开就可以看到）。

因此，如果你的 SVG 包含 `Rect` 或者 `Line` 等的元素，我这里会暂时忽略他们。

**一个暂时的解决方案** 

如果你确实要显示包含除了 path 跟 polygen 之外元素的 SVG 文件，可以先将其全部转换为 path，具体操作是：

> 使用 AI 打开 SVG 文件，选中全部的元素，右键：建立复合路径。然后再次以SVG 格式保存即可。

