﻿using System.Drawing;
using Microsoft.Extensions.ObjectPool;
using SixLabors.ImageSharp;
using XorShiftAddSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection.Metadata;
namespace Games
{
    namespace CreateMap
    {
        public class CreateMapImg
        {
            private XorShiftAddPool XorRand;
            public CreateMapImg(XorShiftAddPool xorRand)
            {
                XorRand = xorRand;
            }
            public void createMono(List<NoisePram> noisePram, int ImageHight = 500, int ImageWidth = 500, int StarX = 0, int StartY = 0, String SavePath = "..\\test.png")
            {
                GetNoise getNoise = new GetNoise(XorRand);
                //空の画像を生成
                var img = new Image<Rgba32>(ImageHight, ImageWidth);
                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        float blue = (float)getNoise.OctavesNoise(noisePram, (double)i, (double)j);
                        if (blue < 0.5)
                        {
                            img[i, j] = new Rgba32(0, 0, 256);
                        }
                        else
                            img[i, j] = new Rgba32(0, 0, 0);
                    }
                    //img[i, j] = new Rgba32(0, 0, blue);

                }
                img.Save(SavePath);
            }
        }


        /// <summary>
        /// ノイズ生成クラス
        /// </summary>
        public class GetNoise
        {
            private XorShiftAddPool XorRand;

            /// <summary>
            /// 0~1のノイズを生成
            /// </summary>
            /// <param name="xorRand">生成に使用する乱数器</param>
            public GetNoise(XorShiftAddPool xorRand)
            {
                XorRand = xorRand;
            }

            /// <summary>
            /// オクターブノイズを生成する
            /// </summary>
            /// <param name="noisePram">ノイズパラメータリスト</param>
            /// <param name="x">x座標</param>
            /// <param name="y">y座標</param>
            /// <param name="z">z座標</param>
            /// <returns>0~1のノイズ</returns>
            public double OctavesNoise(List<NoisePram> noisePram, double x, double y, double z = 0)
            {
                double total = 0;
                double amplitude = 10;
                double maxValue = 0;
                double density = 0;
                for (int i = 0; i < noisePram.Count; i++)
                {
                    double frequency = noisePram[i].Frequency;
                    for (int j = 0; j < noisePram[i].Octaves; j++)
                    {
                        double a = CreateNoise(
                            ((x + noisePram[i].OffsetX) / noisePram[i].Scale) * frequency,
                            ((y + noisePram[i].OffsetY) / noisePram[i].Scale) * frequency) * amplitude;
                        total += a;

                        maxValue += amplitude;

                        amplitude *= noisePram[i].Persistence;
                        frequency *= 2;

                    }
                    switch (noisePram[i].Mode)
                    {
                        case 0:
                            density += total / maxValue;
                            break;
                        case 1:
                            density *= total / maxValue;
                            break;
                        case 2:
                            density -= total / maxValue;
                            break;
                        case 3:
                            density /= total / maxValue;
                            break;
                        default:
                            density += total / maxValue;
                            break;
                    }
                }
                return density;
            }
            /// <summary>
            /// 座標からパーリンノイズを生成する
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns>0~1の範囲のノイズ</returns>
            internal double CreateNoise(double x, double y, double z = 0)
            {
                //ここでプールから乱数生成器を借りてくる
                var rnd = XorRand.Get();

                double xf = x - Math.Floor(x);
                double yf = y - Math.Floor(y);
                double zf = z - Math.Floor(z);

                int xa = (int)Math.Floor(x);
                int ya = (int)Math.Floor(y);
                int za = (int)Math.Floor(z);

                //取り出した小数部を滑らかにする
                double u = smootherStep(xf);
                double v = smootherStep(yf);
                double w = smootherStep(zf);

                //0~255の乱数を取得
                int p000 = (int)GetIndex(rnd.NextDouble());
                int p010 = (int)GetIndex(rnd.NextDouble());
                int p001 = (int)GetIndex(rnd.NextDouble());
                int p011 = (int)GetIndex(rnd.NextDouble());
                int p100 = (int)GetIndex(rnd.NextDouble());
                int p110 = (int)GetIndex(rnd.NextDouble());
                int p101 = (int)GetIndex(rnd.NextDouble());
                int p111 = (int)GetIndex(rnd.NextDouble());

                //この辺よくわからない
                double g000 = GetGrad(p000, xf, yf, zf);
                double g100 = GetGrad(p100, xf - 1, yf, zf);
                double g010 = GetGrad(p010, xf, yf - 1, zf);
                double g001 = GetGrad(p001, xf, yf, zf - 1);
                double g110 = GetGrad(p110, xf - 1, yf - 1, zf);
                double g011 = GetGrad(p011, xf, yf - 1, zf - 1);
                double g101 = GetGrad(p101, xf - 1, yf, zf - 1);
                double g111 = GetGrad(p111, xf - 1, yf - 1, zf - 1);

                //この辺もよくわからない
                double x0 = lerp(g000, g100, u);
                double x1 = lerp(g010, g110, u);
                double x2 = lerp(g001, g101, u);
                double x3 = lerp(g011, g111, u);
                double y0 = lerp(x0, x1, v);
                double y1 = lerp(x2, x3, v);
                double z0 = lerp(y0, y1, w);

                //プールに返却
                XorRand.Return(rnd);
                return (z0 + 1) / 2;
            }

            /// <summary>
            /// 格子点の固有ベクトルを求める
            /// </summary>
            /// <param name="index">0~255までのどれか</param>
            /// <param name="x">x座標の小数部</param>
            /// <param name="y">y座標の小数部</param>
            /// <param name="z">z座標の小数部 3Dでない場合は0</param>
            /// <returns></returns>
            private double GetGrad(int index, double x, double y, double z = 0)
            {
                //格子点8つ分の固有ベクトルを求める
                //indexから最初の4ビットを取り出す
                int h = index & 15;

                //indexの最上位ビット(MSB)が0であれば u=xとする そうでなければy
                double u = h < 8 ? x : y;

                double v;

                // 第1および第2ビットが0の場合、v = yとする
                if (h < 4)
                {
                    v = y;
                }

                //最初と2番目の上位ビットが1の場合、v = xとする
                else if (h == 12 || h == 14)
                {
                    v = x;
                }

                // 第1および第2上位ビットが等しくない場合（0/1、1/0）、v = zとする
                else
                {
                    v = z;
                }
                // 最後の2ビットを使って、uとvが正か負かを判断する そしてそれらの加算を返す
                return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
            }

            /// <summary>
            /// 0~1の乱数を0~2048に変換する
            /// </summary>
            /// <param name="seed">0~1の値</param>
            /// <returns></returns>
            private double GetIndex(double rnd)
            {
                double index = Math.Round(rnd * 2048);
                return index;

            }

            /// <summary>
            /// 補完関数 滑らかにするっぽい
            /// </summary>
            /// <param name="x">小数部</param>
            /// <returns></returns>
            public double smootherStep(double x)
            {
                return x * x * x * (x * (x * 6 - 15) + 10);
            }

            public double lerp(double a, double b, double t)
            {
                return a + (b - a) * t;
            }
        }

        /// <summary>
        /// オクターブノイズ用パラメータクラス
        /// </summary>
        /// <
        public class NoisePram
        {
            /// <summary>
            /// オクターブ
            /// </summary>
            public int Octaves { get; set; }

            /// <summary>
            /// よくわからない
            /// </summary>
            public double Persistence { get; set; }
            public double Frequency { get; set; }

            /// <summary>
            /// 拡大率
            /// </summary>
            public int Scale { get; set; }

            /// <summary>
            /// 合成モード 0=加算 1=乗算 2=減算 3=除算
            /// </summary>
            public int Mode { get; set; } = 0;

            public double OffsetX { get; set; } = 0;
            public double OffsetY { get; set; } = 0;
        }
    }

}