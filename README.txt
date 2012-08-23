Gifmo - Animated Gif Helper

Gifmo is wrapper for several utilities that can help in the creation of animated gifs.

-------------------------

Prerequisites:

MPlayer:  http://www.mplayerhq.hu/design7/dload.html

ImageMajick:  http://www.imagemagick.org/script/index.php

Gifscicle:  http://www.lcdf.org/gifsicle/


------------------------


You need to change the following code in MainWindow.xaml.cs to point to the executables you downloaded above:

/// <summary>
/// Path to the 3rd Party Components on your system
/// </summary>
private string MPlayerPath = @"c:\gifmo\mplayer";
private string ImageMajikPath = @"c:\gifmo\convert";
private string GifsiclePath = @"c:\gifmo\gifsicle"; 


-------------------------

You can adjust the pathing to taste, but by default the application expects the following folders:


C:\gifmo\temp\ -  this where the screen grabs from the video go.

c:\gifmo\completed\ - the folder where the finished animated gif is saved to.


------------------------

How It Works:

Step 1: [Select Video File]  Select a local video source in order to capture images from.

Step 2: [Capture Images] Set the start time for the capture and the duration.

Step 3: [Prep Images]  These are two custom features (NOT REQUIRED).

	[x] Reduce - will delete every other file 
	[x] Reverse Loop -  Clone images in reverse

Step 4: [Generate]  Takes /temp/ folder and converts them to an animated gif.

Step 5: Look in the /completed/ folder to see your new gif.  repeat steps to perfect.

-----------------------

TODO:

* Once images are captured, bind to grid so user can preview temp images before gif creation.
* Give user ability to delete images from temp list to create cleaner gif
* Move more image manipulation (b&w, resize, filters) to the Prep functionality.
 
