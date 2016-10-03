# Hidden Watermark
Embeds a hidden watermark in an image using a blind DWT-DCT approach. 
Unlike steganographic methods, this hidden watermark is resistant to various forms of attack. Limited to a black and white 32x32 watermark image.

**Original Image** + **Watermark** = **Watermarked Image**

![Original](http://mchall.github.io/Images/Watermark/original.jpg) + ![Watermark](http://mchall.github.io/Images/Watermark/watermark.jpg) = ![Embedded Image](http://mchall.github.io/Images/Watermark/embeddedwatermark.jpg)

> Image source: http://commons.wikimedia.org/wiki/Garden#/media/File:Rikugien3.jpg

### Usage ###

The following code instantiates the default watermark class with the default watermark image. Then the watermark is embedded in a file, and the watermark is retrieved afterwards. 

```C#
var fileBytes = File.ReadAllBytes(imageLocation);
var newFileBytes = Watermark.Default.EmbedWatermark(fileBytes);

var result = Watermark.Default.RetrieveWatermark(newFileBytes);
```

The following code instantiates the watermark class with the bytes read from a 32x32 watermark image and turns crop support on. Then the watermark is embedded in a file and simultaneously checked if an embedded watermark already exists. 

```C#
var watermarkBytes = File.ReadAllBytes(watermarkLocation);
Watermark watermark = new Watermark(watermarkBytes, true);

var fileBytes = File.ReadAllBytes(imageLocation);
var result = watermark.RetrieveAndEmbedWatermark(fileBytes);
```

### Watermark recovery results ###

Attack | Recovered Watermark
------------ | -------------
No Attack | ![1](http://mchall.github.io/Images/Watermark/1.jpg)
Blur | ![2](http://mchall.github.io/Images/Watermark/2.jpg)
Flip Horizontal | ![3](http://mchall.github.io/Images/Watermark/3.jpg)
Resize (1024x768->360x270) | ![4](http://mchall.github.io/Images/Watermark/4.jpg)
Visible Watermarking | ![5](http://mchall.github.io/Images/Watermark/5.jpg)
Crop | ![6](http://mchall.github.io/Images/Watermark/6.jpg)
JPEG Quality 20% | ![7](http://mchall.github.io/Images/Watermark/7.jpg)
Grayscale | ![8](http://mchall.github.io/Images/Watermark/8.jpg) *(not resistant)*
PrintScreen | ![9](http://mchall.github.io/Images/Watermark/9.jpg)
Noise | ![10](http://mchall.github.io/Images/Watermark/10.jpg)

## Acknowledgements ##
**Accord.NET:**
http://accord-framework.net/
