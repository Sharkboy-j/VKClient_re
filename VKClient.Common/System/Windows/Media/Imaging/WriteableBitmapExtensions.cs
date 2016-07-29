using System.IO;
using System.Reflection;

namespace System.Windows.Media.Imaging
{
  public static class WriteableBitmapExtensions
  {
    public static int[,] KernelGaussianBlur5x5 = new int[5, 5]
    {
      {
        1,
        4,
        7,
        4,
        1
      },
      {
        4,
        16,
        26,
        16,
        4
      },
      {
        7,
        26,
        41,
        26,
        7
      },
      {
        4,
        16,
        26,
        16,
        4
      },
      {
        1,
        4,
        7,
        4,
        1
      }
    };
    public static int[,] KernelGaussianBlur3x3 = new int[3, 3]
    {
      {
        16,
        26,
        16
      },
      {
        26,
        41,
        26
      },
      {
        16,
        26,
        16
      }
    };
    public static int[,] KernelSharpen3x3 = new int[3, 3]
    {
      {
        0,
        -2,
        0
      },
      {
        -2,
        11,
        -2
      },
      {
        0,
        -2,
        0
      }
    };
    internal const int SizeOfArgb = 4;

    private static int ConvertColor(Color color)
    {
      int num = (int) color.A + 1;
      return (int) color.A << 24 | (int) (byte) ((int) color.R * num >> 8) << 16 | (int) (byte) ((int) color.G * num >> 8) << 8 | (int) (byte) ((int) color.B * num >> 8);
    }

    public static void Clear(this WriteableBitmap bmp, Color color)
    {
      int num1 = WriteableBitmapExtensions.ConvertColor(color);
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int num2 = width * 4;
        for (int index = 0; index < width; ++index)
          pixels[index] = num1;
        int num3 = 1;
        int num4 = 1;
        while (num4 < height)
        {
          BitmapContext.BlockCopy(bitmapContext, 0, bitmapContext, num4 * num2, num3 * num2);
          num4 += num3;
          num3 = Math.Min(2 * num3, height - num4);
        }
      }
    }

    public static void Clear(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Clear();
    }

    public static WriteableBitmap Clone(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        WriteableBitmap bmp1 = BitmapFactory.New(bitmapContext1.Width, bitmapContext1.Height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          BitmapContext.BlockCopy(bitmapContext1, 0, bitmapContext2, 0, bitmapContext1.Length * 4);
        return bmp1;
      }
    }

    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color> func)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int num = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            Color color = func(index2, index1);
            pixels[num++] = WriteableBitmapExtensions.ConvertColor(color);
          }
        }
      }
    }

    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color, Color> func)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int index1 = 0;
        for (int index2 = 0; index2 < height; ++index2)
        {
          for (int index3 = 0; index3 < width; ++index3)
          {
            int num = pixels[index1];
            Color color = func(index3, index2, Color.FromArgb((byte) (num >> 24), (byte) (num >> 16), (byte) (num >> 8), (byte) num));
            pixels[index1++] = WriteableBitmapExtensions.ConvertColor(color);
          }
        }
      }
    }

    public static int GetPixeli(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        return bitmapContext.Pixels[y * bitmapContext.Width + x];
    }

    public static Color GetPixel(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int num1 = bitmapContext.Pixels[y * bitmapContext.Width + x];
        int num2;
        int num3 = num2 = (int) (byte) (num1 >> 24);
        if (num3 == 0)
          num3 = 1;
        int num4 = 65280 / num3;
        int num5 = (int) (byte) ((num1 >> 16 & (int) byte.MaxValue) * num4 >> 8);
        int num6 = (int) (byte) ((num1 >> 8 & (int) byte.MaxValue) * num4 >> 8);
        int num7 = (int) (byte) ((num1 & (int) byte.MaxValue) * num4 >> 8);
        return Color.FromArgb((byte) num2, (byte) num5, (byte) num6, (byte) num7);
      }
    }

    public static byte GetBrightness(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        int num1 = bitmapContext.Pixels[y * bitmapContext.Width + x];
        int num2 = 16;
        byte num3 = (byte) (num1 >> num2);
        int num4 = 8;
        byte num5 = (byte) (num1 >> num4);
        byte num6 = (byte) num1;
        return (byte) ((int) num3 * 6966 + (int) num5 * 23436 + (int) num6 * 2366 >> 15);
      }
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = -16777216 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = -16777216 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = (int) a << 24 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = (int) a << 24 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = WriteableBitmapExtensions.ConvertColor(color);
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = WriteableBitmapExtensions.ConvertColor(color);
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int num = (int) a + 1;
        bitmapContext.Pixels[index] = (int) a << 24 | (int) (byte) ((int) color.R * num >> 8) << 16 | (int) (byte) ((int) color.G * num >> 8) << 8 | (int) (byte) ((int) color.B * num >> 8);
      }
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int num = (int) a + 1;
        bitmapContext.Pixels[y * bitmapContext.Width + x] = (int) a << 24 | (int) (byte) ((int) color.R * num >> 8) << 16 | (int) (byte) ((int) color.G * num >> 8) << 8 | (int) (byte) ((int) color.B * num >> 8);
      }
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, int color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = color;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, int color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = color;
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      bmp.Blit(destRect, source, sourceRect, Colors.White, BlendMode);
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect)
    {
      bmp.Blit(destRect, source, sourceRect, Colors.White, WriteableBitmapExtensions.BlendMode.Alpha);
    }

    public static void Blit(this WriteableBitmap bmp, Point destPosition, WriteableBitmap source, Rect sourceRect, Color color, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      Rect destRect = new Rect(destPosition, new Size(sourceRect.Width, sourceRect.Height));
      bmp.Blit(destRect, source, sourceRect, color, BlendMode);
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, Color color, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      if ((int) color.A == 0)
        return;
      int num1 = (int) destRect.Width;
      int num2 = (int) destRect.Height;
      using (BitmapContext bitmapContext1 = source.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        using (BitmapContext bitmapContext2 = bmp.GetBitmapContext())
        {
          int width1 = bitmapContext1.Width;
          int width2 = bitmapContext2.Width;
          int height = bitmapContext2.Height;
          Rect rect = new Rect(0.0, 0.0, (double) width2, (double) height);
          rect.Intersect(destRect);
          if (rect.IsEmpty)
            return;
          int[] pixels1 = bitmapContext1.Pixels;
          int[] pixels2 = bitmapContext2.Pixels;
          int length1 = bitmapContext1.Length;
          int length2 = bitmapContext2.Length;
          int num3 = (int) destRect.X;
          int num4 = (int) destRect.Y;
          int num5 = 0;
          int num6 = 0;
          int num7 = 0;
          int num8 = 0;
          int num9 = (int) color.A;
          int num10 = (int) color.R;
          int num11 = (int) color.G;
          int num12 = (int) color.B;
          bool flag = color != Colors.White;
          int num13 = (int) sourceRect.Width;
          double num14 = sourceRect.Width / destRect.Width;
          double num15 = sourceRect.Height / destRect.Height;
          int num16 = (int) sourceRect.X;
          int num17 = (int) sourceRect.Y;
          int num18 = -1;
          int num19 = -1;
          double num20 = (double) num17;
          int num21 = num4;
          for (int index1 = 0; index1 < num2; ++index1)
          {
            if (num21 >= 0 && num21 < height)
            {
              double num22 = (double) num16;
              int index2 = num3 + num21 * width2;
              int num23 = num3;
              int num24 = pixels1[0];
              if (BlendMode == WriteableBitmapExtensions.BlendMode.None && !flag)
              {
                int num25 = (int) num22 + (int) num20 * width1;
                int num26 = num23 < 0 ? -num23 : 0;
                int num27 = num23 + num26;
                int num28 = width1 - num26;
                int num29 = num27 + num28 < width2 ? num28 : width2 - num27;
                if (num29 > num13)
                  num29 = num13;
                if (num29 > num1)
                  num29 = num1;
                BitmapContext.BlockCopy(bitmapContext1, (num25 + num26) * 4, bitmapContext2, (index2 + num26) * 4, num29 * 4);
              }
              else
              {
                for (int index3 = 0; index3 < num1; ++index3)
                {
                  if (num23 >= 0 && num23 < width2)
                  {
                    if ((int) num22 != num18 || (int) num20 != num19)
                    {
                      int index4 = (int) num22 + (int) num20 * width1;
                      if (index4 >= 0 && index4 < length1)
                      {
                        num24 = pixels1[index4];
                        num8 = num24 >> 24 & (int) byte.MaxValue;
                        num5 = num24 >> 16 & (int) byte.MaxValue;
                        num6 = num24 >> 8 & (int) byte.MaxValue;
                        num7 = num24 & (int) byte.MaxValue;
                        if (flag && num8 != 0)
                        {
                          num8 = num8 * num9 * 32897 >> 23;
                          num5 = (num5 * num10 * 32897 >> 23) * num9 * 32897 >> 23;
                          num6 = (num6 * num11 * 32897 >> 23) * num9 * 32897 >> 23;
                          num7 = (num7 * num12 * 32897 >> 23) * num9 * 32897 >> 23;
                          num24 = num8 << 24 | num5 << 16 | num6 << 8 | num7;
                        }
                      }
                      else
                        num8 = 0;
                    }
                    if (BlendMode == WriteableBitmapExtensions.BlendMode.None)
                      pixels2[index2] = num24;
                    else if (BlendMode == WriteableBitmapExtensions.BlendMode.ColorKeying)
                    {
                      num5 = num24 >> 16 & (int) byte.MaxValue;
                      num6 = num24 >> 8 & (int) byte.MaxValue;
                      num7 = num24 & (int) byte.MaxValue;
                      if (num5 != (int) color.R || num6 != (int) color.G || num7 != (int) color.B)
                        pixels2[index2] = num24;
                    }
                    else if (BlendMode == WriteableBitmapExtensions.BlendMode.Mask)
                    {
                      int num25 = pixels2[index2];
                      int num26 = num25 >> 24 & (int) byte.MaxValue;
                      int num27 = num25 >> 16 & (int) byte.MaxValue;
                      int num28 = num25 >> 8 & (int) byte.MaxValue;
                      int num29 = num25 & (int) byte.MaxValue;
                      int num30 = num26 * num8 * 32897 >> 23 << 24 | num27 * num8 * 32897 >> 23 << 16 | num28 * num8 * 32897 >> 23 << 8 | num29 * num8 * 32897 >> 23;
                      pixels2[index2] = num30;
                    }
                    else if (num8 > 0)
                    {
                      int num25 = pixels2[index2];
                      int num26 = num25 >> 24 & (int) byte.MaxValue;
                      if ((num8 == (int) byte.MaxValue || num26 == 0) && (BlendMode != WriteableBitmapExtensions.BlendMode.Additive && BlendMode != WriteableBitmapExtensions.BlendMode.Subtractive) && BlendMode != WriteableBitmapExtensions.BlendMode.Multiply)
                      {
                        pixels2[index2] = num24;
                      }
                      else
                      {
                        int num27 = num25 >> 16 & (int) byte.MaxValue;
                        int num28 = num25 >> 8 & (int) byte.MaxValue;
                        int num29 = num25 & (int) byte.MaxValue;
                        if (BlendMode == WriteableBitmapExtensions.BlendMode.Alpha)
                          num25 = ((num8 << 8) + ((int) byte.MaxValue - num8) * num26 >> 8 << 24) + ((num5 << 8) + ((int) byte.MaxValue - num8) * num27 >> 8 << 16) + ((num6 << 8) + ((int) byte.MaxValue - num8) * num28 >> 8 << 8) + ((num7 << 8) + ((int) byte.MaxValue - num8) * num29 >> 8);
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Additive)
                        {
                          int num30 = (int) byte.MaxValue <= num8 + num26 ? (int) byte.MaxValue : num8 + num26;
                          num25 = num30 << 24 | (num30 <= num5 + num27 ? num30 : num5 + num27) << 16 | (num30 <= num6 + num28 ? num30 : num6 + num28) << 8 | (num30 <= num7 + num29 ? num30 : num7 + num29);
                        }
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Subtractive)
                          num25 = num26 << 24 | (num5 >= num27 ? 0 : num5 - num27) << 16 | (num6 >= num28 ? 0 : num6 - num28) << 8 | (num7 >= num29 ? 0 : num7 - num29);
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Multiply)
                        {
                          int num30 = num8 * num26 + 128;
                          int num31 = num5 * num27 + 128;
                          int num32 = num6 * num28 + 128;
                          int num33 = num7 * num29 + 128;
                          int num34 = (num30 >> 8) + num30 >> 8;
                          int num35 = (num31 >> 8) + num31 >> 8;
                          int num36 = (num32 >> 8) + num32 >> 8;
                          int num37 = (num33 >> 8) + num33 >> 8;
                          num25 = num34 << 24 | (num34 <= num35 ? num34 : num35) << 16 | (num34 <= num36 ? num34 : num36) << 8 | (num34 <= num37 ? num34 : num37);
                        }
                        pixels2[index2] = num25;
                      }
                    }
                  }
                  ++num23;
                  ++index2;
                  num22 += num14;
                }
              }
            }
            num20 += num15;
            ++num21;
          }
        }
      }
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp, int offset, int count)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        if (count == -1)
          count = bitmapContext.Length;
        int count1 = count * 4;
        byte[] numArray = new byte[count1];
        BitmapContext.BlockCopy(bitmapContext, offset, (Array) numArray, 0, count1);
        return numArray;
      }
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp, int count)
    {
      return bmp.ToByteArray(0, count);
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp)
    {
      return bmp.ToByteArray(0, -1);
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int offset, int count)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        BitmapContext.BlockCopy((Array) buffer, offset, bitmapContext, 0, count);
        return bmp;
      }
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int count)
    {
      return bmp.FromByteArray(buffer, 0, count);
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer)
    {
      return bmp.FromByteArray(buffer, 0, buffer.Length);
    }

    public static void WriteTga(this WriteableBitmap bmp, Stream destination)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int[] pixels = bitmapContext.Pixels;
        byte[] buffer1 = new byte[bitmapContext.Length * 4];
        int index1 = 0;
        int num1 = width << 2;
        int num2 = width << 3;
        int index2 = (height - 1) * num1;
        for (int index3 = 0; index3 < height; ++index3)
        {
          for (int index4 = 0; index4 < width; ++index4)
          {
            int num3 = pixels[index1];
            buffer1[index2] = (byte) (num3 & (int) byte.MaxValue);
            buffer1[index2 + 1] = (byte) (num3 >> 8 & (int) byte.MaxValue);
            buffer1[index2 + 2] = (byte) (num3 >> 16 & (int) byte.MaxValue);
            buffer1[index2 + 3] = (byte) (num3 >> 24);
            ++index1;
            index2 += 4;
          }
          index2 -= num2;
        }
        byte[] numArray = new byte[18];
        numArray[2] = (byte) 2;
        numArray[12] = (byte) (width & (int) byte.MaxValue);
        numArray[13] = (byte) ((width & 65280) >> 8);
        numArray[14] = (byte) (height & (int) byte.MaxValue);
        numArray[15] = (byte) ((height & 65280) >> 8);
        numArray[16] = (byte) 32;
        byte[] buffer2 = numArray;
        using (BinaryWriter binaryWriter = new BinaryWriter(destination))
        {
          binaryWriter.Write(buffer2);
          binaryWriter.Write(buffer1);
        }
      }
    }

    public static WriteableBitmap FromResource(this WriteableBitmap bmp, string relativePath)
    {
      string name = new AssemblyName(Assembly.GetCallingAssembly().FullName).Name;
      return bmp.FromContent(name + ";component/" + relativePath);
    }

    public static WriteableBitmap FromContent(this WriteableBitmap bmp, string relativePath)
    {
      using (Stream stream = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative)).Stream)
      {
        BitmapImage bitmapImage = new BitmapImage();
        Stream streamSource = stream;
        bitmapImage.SetSource(streamSource);
        int num = 0;
        bitmapImage.CreateOptions = (BitmapCreateOptions) num;
        bmp = new WriteableBitmap((BitmapSource) bitmapImage);
        bitmapImage.UriSource = null;
        return bmp;
      }
    }

    public static WriteableBitmap Convolute(this WriteableBitmap bmp, int[,] kernel)
    {
      int kernelFactorSum = 0;
      int[,] numArray = kernel;
      int upperBound1 = numArray.GetUpperBound(0);
      int upperBound2 = numArray.GetUpperBound(1);
      for (int lowerBound1 = numArray.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
      {
        for (int lowerBound2 = numArray.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
        {
          int num = numArray[lowerBound1, lowerBound2];
          kernelFactorSum += num;
        }
      }
      return bmp.Convolute(kernel, kernelFactorSum, 0);
    }

    public static WriteableBitmap Convolute(this WriteableBitmap bmp, int[,] kernel, int kernelFactorSum, int kernelOffsetSum)
    {
      int num1 = kernel.GetUpperBound(0) + 1;
      int num2 = kernel.GetUpperBound(1) + 1;
      if ((num2 & 1) == 0)
        throw new InvalidOperationException("Kernel width must be odd!");
      if ((num1 & 1) == 0)
        throw new InvalidOperationException("Kernel height must be odd!");
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext1.Pixels;
          int[] pixels2 = bitmapContext2.Pixels;
          int num3 = 0;
          int num4 = num2 >> 1;
          int num5 = num1 >> 1;
          for (int index1 = 0; index1 < height; ++index1)
          {
            for (int index2 = 0; index2 < width; ++index2)
            {
              int num6 = 0;
              int num7 = 0;
              int num8 = 0;
              int num9 = 0;
              for (int index3 = -num4; index3 <= num4; ++index3)
              {
                int num10 = index3 + index2;
                if (num10 < 0)
                  num10 = 0;
                else if (num10 >= width)
                  num10 = width - 1;
                for (int index4 = -num5; index4 <= num5; ++index4)
                {
                  int num11 = index4 + index1;
                  if (num11 < 0)
                    num11 = 0;
                  else if (num11 >= height)
                    num11 = height - 1;
                  int num12 = pixels1[num11 * width + num10];
                  int num13 = kernel[index4 + num4, index3 + num5];
                  num6 += (num12 >> 24 & (int) byte.MaxValue) * num13;
                  num7 += (num12 >> 16 & (int) byte.MaxValue) * num13;
                  num8 += (num12 >> 8 & (int) byte.MaxValue) * num13;
                  num9 += (num12 & (int) byte.MaxValue) * num13;
                }
              }
              int num14 = num6 / kernelFactorSum + kernelOffsetSum;
              int num15 = num7 / kernelFactorSum + kernelOffsetSum;
              int num16 = num8 / kernelFactorSum + kernelOffsetSum;
              int num17 = num9 / kernelFactorSum + kernelOffsetSum;
              byte num18 = num14 > (int) byte.MaxValue ? byte.MaxValue : (num14 < 0 ? (byte) 0 : (byte) num14);
              byte num19 = num15 > (int) byte.MaxValue ? byte.MaxValue : (num15 < 0 ? (byte) 0 : (byte) num15);
              byte num20 = num16 > (int) byte.MaxValue ? byte.MaxValue : (num16 < 0 ? (byte) 0 : (byte) num16);
              byte num21 = num17 > (int) byte.MaxValue ? byte.MaxValue : (num17 < 0 ? (byte) 0 : (byte) num17);
              pixels2[num3++] = (int) num18 << 24 | (int) num19 << 16 | (int) num20 << 8 | (int) num21;
            }
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Invert(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        WriteableBitmap bmp1 = BitmapFactory.New(bitmapContext1.Width, bitmapContext1.Height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext2.Pixels;
          int[] pixels2 = bitmapContext1.Pixels;
          int length = bitmapContext1.Length;
          for (int index = 0; index < length; ++index)
          {
            int num1 = pixels2[index];
            int num2 = 24;
            int num3 = num1 >> num2 & (int) byte.MaxValue;
            int num4 = 16;
            int num5 = num1 >> num4 & (int) byte.MaxValue;
            int num6 = 8;
            int num7 = num1 >> num6 & (int) byte.MaxValue;
            int num8 = (int) byte.MaxValue;
            int num9 = num1 & num8;
            int num10 = (int) byte.MaxValue - num5;
            int num11 = (int) byte.MaxValue - num7;
            int num12 = (int) byte.MaxValue - num9;
            pixels1[index] = num3 << 24 | num10 << 16 | num11 << 8 | num12;
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Crop(this WriteableBitmap bmp, int x, int y, int width, int height)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width1 = bitmapContext1.Width;
        int height1 = bitmapContext1.Height;
        if (x > width1 || y > height1)
          return BitmapFactory.New(0, 0);
        if (x < 0)
          x = 0;
        if (x + width > width1)
          width = width1 - x;
        if (y < 0)
          y = 0;
        if (y + height > height1)
          height = height1 - y;
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          for (int index = 0; index < height; ++index)
          {
            int srcOffset = ((y + index) * width1 + x) * 4;
            int destOffset = index * width * 4;
            BitmapContext.BlockCopy(bitmapContext1, srcOffset, bitmapContext2, destOffset, width * 4);
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Crop(this WriteableBitmap bmp, Rect region)
    {
      return bmp.Crop((int) region.X, (int) region.Y, (int) region.Width, (int) region.Height);
    }

    public static WriteableBitmap Resize(this WriteableBitmap bmp, int width, int height, Interpolation interpolation)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int[] numArray = WriteableBitmapExtensions.Resize(bitmapContext1, bitmapContext1.Width, bitmapContext1.Height, width, height, interpolation);
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          BitmapContext.BlockCopy((Array) numArray, 0, bitmapContext2, 0, 4 * numArray.Length);
        return bmp1;
      }
    }

    public static int[] Resize(BitmapContext srcContext, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
    {
      int[] pixels = srcContext.Pixels;
      int[] numArray = new int[width * height];
      float num1 = (float) widthSource / (float) width;
      float num2 = (float) heightSource / (float) height;
      if (interpolation == Interpolation.NearestNeighbor)
      {
        int num3 = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            float num4 = (float) index2 * num1;
            double num5 = (double) index1 * (double) num2;
            int num6 = (int) num4;
            int num7 = (int) num5;
            numArray[num3++] = pixels[num7 * widthSource + num6];
          }
        }
      }
      else if (interpolation == Interpolation.Bilinear)
      {
        int num3 = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            float num4 = (float) index2 * num1;
            double num5 = (double) index1 * (double) num2;
            int num6 = (int) num4;
            int num7 = (int) num5;
            float num8 = num4 - (float) num6;
            double num9 = (double) num7;
            float num10 = (float) (num5 - num9);
            double num11 = 1.0 - (double) num8;
            float num12 = 1f - num10;
            int num13 = num6 + 1;
            if (num13 >= widthSource)
              num13 = num6;
            int num14 = num7 + 1;
            if (num14 >= heightSource)
              num14 = num7;
            int num15 = pixels[num7 * widthSource + num6];
            int num16 = 24;
            byte num17 = (byte) (num15 >> num16);
            int num18 = 16;
            byte num19 = (byte) (num15 >> num18);
            int num20 = 8;
            byte num21 = (byte) (num15 >> num20);
            byte num22 = (byte) num15;
            int num23 = pixels[num7 * widthSource + num13];
            int num24 = 24;
            byte num25 = (byte) (num23 >> num24);
            int num26 = 16;
            byte num27 = (byte) (num23 >> num26);
            int num28 = 8;
            byte num29 = (byte) (num23 >> num28);
            byte num30 = (byte) num23;
            int num31 = pixels[num14 * widthSource + num6];
            int num32 = 24;
            byte num33 = (byte) (num31 >> num32);
            int num34 = 16;
            byte num35 = (byte) (num31 >> num34);
            int num36 = 8;
            byte num37 = (byte) (num31 >> num36);
            byte num38 = (byte) num31;
            int num39 = pixels[num14 * widthSource + num13];
            int num40 = 24;
            byte num41 = (byte) (num39 >> num40);
            int num42 = 16;
            byte num43 = (byte) (num39 >> num42);
            int num44 = 8;
            byte num45 = (byte) (num39 >> num44);
            byte num46 = (byte) num39;
            double num47 = (double) num17;
            float num48 = (float) (num11 * num47 + (double) num8 * (double) num25);
            double num49 = (double) num33;
            float num50 = (float) (num11 * num49 + (double) num8 * (double) num41);
            byte num51 = (byte) ((double) num12 * (double) num48 + (double) num10 * (double) num50);
            double num52 = (double) num19;
            float num53 = (float) (num11 * num52 * (double) num17 + (double) num8 * (double) num27 * (double) num25);
            double num54 = (double) num35;
            float num55 = (float) (num11 * num54 * (double) num33 + (double) num8 * (double) num43 * (double) num41);
            float num56 = (float) ((double) num12 * (double) num53 + (double) num10 * (double) num55);
            double num57 = (double) num21;
            float num58 = (float) (num11 * num57 * (double) num17 + (double) num8 * (double) num29 * (double) num25);
            double num59 = (double) num37;
            float num60 = (float) (num11 * num59 * (double) num33 + (double) num8 * (double) num45 * (double) num41);
            float num61 = (float) ((double) num12 * (double) num58 + (double) num10 * (double) num60);
            double num62 = (double) num22;
            float num63 = (float) (num11 * num62 * (double) num17 + (double) num8 * (double) num30 * (double) num25);
            double num64 = (double) num38;
            float num65 = (float) (num11 * num64 * (double) num33 + (double) num8 * (double) num46 * (double) num41);
            float num66 = (float) ((double) num12 * (double) num63 + (double) num10 * (double) num65);
            if ((int) num51 > 0)
            {
              num56 /= (float) num51;
              num61 /= (float) num51;
              num66 /= (float) num51;
            }
            byte num67 = (byte) num56;
            byte num68 = (byte) num61;
            byte num69 = (byte) num66;
            numArray[num3++] = (int) num51 << 24 | (int) num67 << 16 | (int) num68 << 8 | (int) num69;
          }
        }
      }
      return numArray;
    }

    public static WriteableBitmap Rotate(this WriteableBitmap bmp, int angle)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int[] pixels1 = bitmapContext1.Pixels;
        int index1 = 0;
        angle %= 360;
        WriteableBitmap bmp1;
        if (angle > 0 && angle <= 90)
        {
          bmp1 = BitmapFactory.New(height, width);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = 0; index2 < width; ++index2)
            {
              for (int index3 = height - 1; index3 >= 0; --index3)
              {
                int index4 = index3 * width + index2;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (angle > 90 && angle <= 180)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = height - 1; index2 >= 0; --index2)
            {
              for (int index3 = width - 1; index3 >= 0; --index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (angle > 180 && angle <= 270)
        {
          bmp1 = BitmapFactory.New(height, width);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = width - 1; index2 >= 0; --index2)
            {
              for (int index3 = 0; index3 < height; ++index3)
              {
                int index4 = index3 * width + index2;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else
          bmp1 = bmp.Clone();
        return bmp1;
      }
    }

    public static WriteableBitmap RotateFree(this WriteableBitmap bmp, double angle, bool crop = true)
    {
      double num1 = -1.0 * Math.PI / 180.0 * angle;
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width1 = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int pixelWidth;
        int pixelHeight;
        if (crop)
        {
          pixelWidth = width1;
          pixelHeight = height;
        }
        else
        {
          double num2 = angle / (180.0 / Math.PI);
          pixelWidth = (int) Math.Ceiling(Math.Abs(Math.Sin(num2) * (double) height) + Math.Abs(Math.Cos(num2) * (double) width1));
          pixelHeight = (int) Math.Ceiling(Math.Abs(Math.Sin(num2) * (double) width1) + Math.Abs(Math.Cos(num2) * (double) height));
        }
        int num3 = width1 / 2;
        int num4 = height / 2;
        int num5 = pixelWidth / 2;
        int num6 = pixelHeight / 2;
        WriteableBitmap bmp1 = BitmapFactory.New(pixelWidth, pixelHeight);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext2.Pixels;
          int[] pixels2 = bitmapContext1.Pixels;
          int width2 = bitmapContext1.Width;
          for (int index1 = 0; index1 < pixelHeight; ++index1)
          {
            for (int index2 = 0; index2 < pixelWidth; ++index2)
            {
              int num2 = index2 - num5;
              int num7 = num6 - index1;
              double num8 = Math.Sqrt((double) (num2 * num2 + num7 * num7));
              double num9;
              if (num2 == 0)
              {
                if (num7 == 0)
                {
                  pixels1[index1 * pixelWidth + index2] = pixels2[num4 * width2 + num3];
                  continue;
                }
                num9 = num7 >= 0 ? Math.PI / 2.0 : 3.0 * Math.PI / 2.0;
              }
              else
                num9 = Math.Atan2((double) num7, (double) num2);
              double num10 = num9 - num1;
              double num11 = num8 * Math.Cos(num10);
              double num12 = num8 * Math.Sin(num10);
              double num13 = num11 + (double) num3;
              double num14 = (double) num4 - num12;
              int x1 = (int) Math.Floor(num13);
              int y1 = (int) Math.Floor(num14);
              int x2 = (int) Math.Ceiling(num13);
              int y2 = (int) Math.Ceiling(num14);
              if (x1 >= 0 && x2 >= 0 && (x1 < width1 && x2 < width1) && (y1 >= 0 && y2 >= 0 && (y1 < height && y2 < height)))
              {
                double num15 = num13 - (double) x1;
                double num16 = num14 - (double) y1;
                Color pixel1 = bmp.GetPixel(x1, y1);
                Color pixel2 = bmp.GetPixel(x2, y1);
                Color pixel3 = bmp.GetPixel(x1, y2);
                Color pixel4 = bmp.GetPixel(x2, y2);
                double num17 = (1.0 - num15) * (double) pixel1.R + num15 * (double) pixel2.R;
                double num18 = (1.0 - num15) * (double) pixel1.G + num15 * (double) pixel2.G;
                double num19 = (1.0 - num15) * (double) pixel1.B + num15 * (double) pixel2.B;
                double num20 = (1.0 - num15) * (double) pixel1.A + num15 * (double) pixel2.A;
                double num21 = (1.0 - num15) * (double) pixel3.R + num15 * (double) pixel4.R;
                double num22 = (1.0 - num15) * (double) pixel3.G + num15 * (double) pixel4.G;
                double num23 = (1.0 - num15) * (double) pixel3.B + num15 * (double) pixel4.B;
                double num24 = (1.0 - num15) * (double) pixel3.A + num15 * (double) pixel4.A;
                int num25 = (int) Math.Round((1.0 - num16) * num17 + num16 * num21);
                int num26 = (int) Math.Round((1.0 - num16) * num18 + num16 * num22);
                int num27 = (int) Math.Round((1.0 - num16) * num19 + num16 * num23);
                int num28 = (int) Math.Round((1.0 - num16) * num20 + num16 * num24);
                if (num25 < 0)
                  num25 = 0;
                if (num25 > (int) byte.MaxValue)
                  num25 = (int) byte.MaxValue;
                if (num26 < 0)
                  num26 = 0;
                if (num26 > (int) byte.MaxValue)
                  num26 = (int) byte.MaxValue;
                if (num27 < 0)
                  num27 = 0;
                if (num27 > (int) byte.MaxValue)
                  num27 = (int) byte.MaxValue;
                if (num28 < 0)
                  num28 = 0;
                if (num28 > (int) byte.MaxValue)
                  num28 = (int) byte.MaxValue;
                int num29 = num28 + 1;
                pixels1[index1 * pixelWidth + index2] = num28 << 24 | (int) (byte) (num25 * num29 >> 8) << 16 | (int) (byte) (num26 * num29 >> 8) << 8 | (int) (byte) (num27 * num29 >> 8);
              }
            }
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Flip(this WriteableBitmap bmp, FlipMode flipMode)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int[] pixels1 = bitmapContext1.Pixels;
        int index1 = 0;
        WriteableBitmap bmp1 = (WriteableBitmap) null;
        if (flipMode == FlipMode.Horizontal)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = height - 1; index2 >= 0; --index2)
            {
              for (int index3 = 0; index3 < width; ++index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (flipMode == FlipMode.Vertical)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = 0; index2 < height; ++index2)
            {
              for (int index3 = width - 1; index3 >= 0; --index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        return bmp1;
      }
    }

    public enum BlendMode
    {
      Alpha,
      Additive,
      Subtractive,
      Mask,
      Multiply,
      ColorKeying,
      None,
    }
  }
}
