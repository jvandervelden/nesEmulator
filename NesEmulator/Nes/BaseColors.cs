﻿using CorePixelEngine;

namespace TestPGE.Nes
{
    public static class BasePixels
    {
        public static Pixel GetPixel(int colorIndex, int rEmp, int gEmp, int bEmp)
        {
            return new Pixel(Palette[colorIndex].r + rEmp, Palette[colorIndex].g + gEmp, Palette[colorIndex].b + bEmp, Palette[colorIndex].a);
        }

        public static readonly Pixel[] Palette = new Pixel[] {
            new Pixel(84, 84, 84),
            new Pixel(0, 30, 116),
            new Pixel(8, 16, 144),
            new Pixel(48, 0, 136),
            new Pixel(68, 0, 100),
            new Pixel(92, 0, 48),
            new Pixel(84, 4, 0),
            new Pixel(60, 24, 0),
            new Pixel(32, 42, 0),
            new Pixel(8, 58, 0),
            new Pixel(0, 64, 0),
            new Pixel(0, 60, 0),
            new Pixel(0, 50, 60),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0),
            new Pixel(152, 150, 152),
            new Pixel(8, 76, 196),
            new Pixel(48, 50, 236),
            new Pixel(92, 30, 228),
            new Pixel(136, 20, 176),
            new Pixel(160, 20, 100),
            new Pixel(152, 34, 32),
            new Pixel(120, 60, 0),
            new Pixel(84, 90, 0),
            new Pixel(40, 114, 0),
            new Pixel(8, 124, 0),
            new Pixel(0, 118, 40),
            new Pixel(0, 102, 120),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0),
            new Pixel(236, 238, 236),
            new Pixel(76, 154, 236),
            new Pixel(120, 124, 236),
            new Pixel(176, 98, 236),
            new Pixel(228, 84, 236),
            new Pixel(236, 88, 180),
            new Pixel(236, 106, 100),
            new Pixel(212, 136, 32),
            new Pixel(160, 170, 0),
            new Pixel(116, 196, 0),
            new Pixel(76, 208, 32),
            new Pixel(56, 204, 108),
            new Pixel(56, 180, 204),
            new Pixel(60, 60, 60),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0),
            new Pixel(236, 238, 236),
            new Pixel(168, 204, 236),
            new Pixel(188, 188, 236),
            new Pixel(212, 178, 236),
            new Pixel(236, 174, 236),
            new Pixel(236, 174, 212),
            new Pixel(236, 180, 176),
            new Pixel(228, 196, 144),
            new Pixel(204, 210, 120),
            new Pixel(180, 222, 120),
            new Pixel(168, 226, 144),
            new Pixel(152, 226, 180),
            new Pixel(160, 214, 228),
            new Pixel(160, 162, 160),
            new Pixel(0, 0, 0),
            new Pixel(0, 0, 0)
       };
    }
}
