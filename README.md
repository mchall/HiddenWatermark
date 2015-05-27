# Hidden Watermark
Embeds a hidden watermark in an image using a blind DWT-DCT approach. 
Unlike steganographic methods, this hidden watermark if resistant to various forms of attack. Limited to a black and white 32x32 watermark image.

**Original Image** + **Watermark** = **Watermarked Image**

![Original](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/original.jpg) + ![Watermark](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/watermark.jpg) = ![Embedded Image](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/embeddedwatermark.jpg)

### Usage ###

The following code instantiates the watermark class with the bytes read from a 32x32 watermark image. Then the watermark is embedded in a file, and the watermark is retrieved afterwards. 

```C#
var watermarkBytes = File.ReadAllBytes(watermarkLocation);
Watermark watermark = new Watermark(watermarkBytes);

var fileBytes = File.ReadAllBytes(imageLocation);
var newFileBytes = watermark.EmbedWatermark(fileBytes);

var result = watermark.RetrieveWatermark(newFileBytes);
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
No Attack | ![1](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/1.jpg)
Blur | ![2](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/2.jpg)
Flip Horizontal | ![3](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/3.jpg)
Resize (1024x768->360x270) | ![4](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/4.jpg)
Visible Watermarking | ![5](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/5.jpg)
Crop | ![6](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/6.jpg)
JPEG Quality 20% | ![7](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/7.jpg)
Grayscale | ![8](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/8.jpg) *(not resistant)*
PrintScreen | ![9](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/9.jpg)
Noise | ![10](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/10.jpg)
