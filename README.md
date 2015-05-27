# Hidden Watermark
Embeds a hidden watermark in an image resistant to various forms of attack. 
Uses a blind DWT-DCT approach.

**Original Image** + **Watermark** = **Watermarked Image**

![Original](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/original.jpg) + ![Watermark](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/watermark.jpg) = ![Embedded Image](https://raw.githubusercontent.com/mcsyko/hiddenwatermark/master/Readme_Img/embeddedwatermark.jpg)

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
