using DeathsTerminus.Assets;
using DeathsTerminus.NPCs.CataBoss;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace DeathsTerminus.Common
{
    public static class ThreadLoading
    {
        internal static void LoadSuns(object callContext)
        {
            if (DeathsTerminus.ReleaseAsync)
                return;
            ILog logger = (ILog)callContext;
            int textureFramesX = 10;
            int textureFramesY = 6;
            int eclipseFrameSize = CataBossSky.ECLIPSE_FRAME_SIZE;
            TextureCache.EclipseTexture = new Ref<Texture2D>(new Texture2D(Main.spriteBatch.GraphicsDevice, eclipseFrameSize * textureFramesX, eclipseFrameSize * textureFramesY, false, SurfaceFormat.Color));
            List<Color> list = new List<Color>();
            for (int k = 0; k < textureFramesY; k++)
            {
                for (int j = 0; j < TextureCache.EclipseTexture.Value.Height / textureFramesY; j++)
                {
                    for (int l = 0; l < textureFramesX; l++)
                    {
                        for (int i = 0; i < TextureCache.EclipseTexture.Value.Width / textureFramesX; i++)
                        {
                            float x = (2 * i / (float)(TextureCache.EclipseTexture.Value.Width / textureFramesX - 1) - 1);
                            float y = (2 * j / (float)(TextureCache.EclipseTexture.Value.Height / textureFramesY - 1) - 1);

                            float radius = (float)Math.Sqrt(x * x + y * y);
                            float angle = (float)Math.Atan2(x, y);

                            int frame = k * textureFramesX + l;

                            float waveFunction = (float)(
                                    1 / 4f * Math.Cos(12 * (angle + 12f) + frame * MathHelper.TwoPi / 12 + 12f) +
                                    1 / 4f * Math.Cos(-15 * (angle + 15f) + frame * MathHelper.TwoPi / 15 + 15f) +
                                    1 / 4f * Math.Cos(-20 * (angle + 20f) + frame * MathHelper.TwoPi / 20 + 20f) +
                                    1 / 4f * Math.Cos(30 * (angle + 30f) + frame * MathHelper.TwoPi / 30 + 30f)
                                );

                            float luminosityFactor = (float)Math.Pow((1 - Math.Pow(radius, 4)), 2);
                            float waviness = (float)(0.5f * Math.Exp(-50 * Math.Pow(radius - 0.8f, 2)));
                            float index = luminosityFactor * (1 - waviness * waveFunction);
                            float eclipseLuminosityFactor = (float)Math.Pow(1 - Math.Pow(1 - Math.Pow(radius, 2), 64), Math.Pow(64, 4));

                            float hue = (index / 4f - 1 / 12f) % 1;
                            float saturation = (float)Math.Pow(eclipseLuminosityFactor, 2);
                            float luminosity = luminosityFactor * eclipseLuminosityFactor;
                            Color color = Main.hslToRgb(hue, saturation, luminosity);

                            int r = color.R;//448 - (int)(512 * index);
                            int g = color.G;//384 - (int)(512 * index);
                            int b = color.B;//256 - (int)(512 * index);
                            int alpha = radius >= 1 ? 0 : (int)(255 * index);

                            //list.Add(new Color(r, g, b, alpha));
                            list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
                        }
                    }
                }
            }
            if (DeathsTerminus.ReleaseAsync)
                return;
            TextureCache.EclipseTexture.Value.SetData(list.ToArray());
            logger.Info("Finished the (" + nameof(TextureCache.EclipseTexture) + ") texture.");

            if (DeathsTerminus.ReleaseAsync)
                return;
            TextureCache.BlueSunTexture = new Ref<Texture2D>(new Texture2D(Main.spriteBatch.GraphicsDevice, eclipseFrameSize * textureFramesX, eclipseFrameSize * textureFramesY, false, SurfaceFormat.Color));
            list = new List<Color>();
            for (int k = 0; k < textureFramesY; k++)
            {
                for (int j = 0; j < TextureCache.EclipseTexture.Value.Height / textureFramesY; j++)
                {
                    for (int l = 0; l < textureFramesX; l++)
                    {
                        for (int i = 0; i < TextureCache.EclipseTexture.Value.Width / textureFramesX; i++)
                        {
                            float x = (2 * i / (float)(TextureCache.EclipseTexture.Value.Width / textureFramesX - 1) - 1);
                            float y = (2 * j / (float)(TextureCache.EclipseTexture.Value.Height / textureFramesY - 1) - 1);

                            float radius = (float)Math.Sqrt(x * x + y * y);
                            float angle = (float)Math.Atan2(x, y);

                            int frame = k * textureFramesX + l;

                            float waveFunction = (float)(
                                    1 / 8f * Math.Cos(12 * (angle + 12f) + frame * MathHelper.TwoPi / 12 + 12f) +
                                    1 / 8f * Math.Cos(-15 * (angle + 15f) + frame * MathHelper.TwoPi / 15 + 15f) +
                                    1 / 8f * Math.Cos(-20 * (angle + 20f) + frame * MathHelper.TwoPi / 20 + 20f) +
                                    1 / 8f * Math.Cos(30 * (angle + 30f) + frame * MathHelper.TwoPi / 30 + 30f)
                                );

                            float luminosityFactor = (float)Math.Pow((1 - Math.Pow(radius, 4)), 2);
                            float waviness = (float)(0.5f * Math.Exp(-50 * Math.Pow(radius - 0.8f, 2)));
                            float index = luminosityFactor * (1 - waviness * waveFunction);

                            float eclipseLuminosityFactor = Math.Max(0, Math.Min(1, (radius - 0.6f) * 32));
                            eclipseLuminosityFactor = (float)Math.Sqrt(Math.Pow(eclipseLuminosityFactor, 2) + Math.Pow(
                                    Math.Max(0, Math.Min(1, Math.Cos((angle + frame * MathHelper.TwoPi / 180) * 3) * 6 + 1))
                                , 2));
                            eclipseLuminosityFactor = Math.Max(eclipseLuminosityFactor, Math.Max(0, Math.Min(1, (0.2f - radius) * 64)));
                            eclipseLuminosityFactor = eclipseLuminosityFactor * Math.Max(0, Math.Min(1, (radius - 0.1f) * 64));
                            eclipseLuminosityFactor = Math.Max(0, Math.Min(1, eclipseLuminosityFactor));

                            float hue = (index / 4f + 1 / 2.5f) % 1;
                            float saturation = (float)Math.Pow(eclipseLuminosityFactor, 2);
                            float luminosity = luminosityFactor * eclipseLuminosityFactor;
                            Color color = Main.hslToRgb(hue, saturation, luminosity);

                            int r = color.R;//448 - (int)(512 * index);
                            int g = color.G;//384 - (int)(512 * index);
                            int b = color.B;//256 - (int)(512 * index);
                            int alpha = radius >= 1 ? 0 : (int)(255 * index);

                            //list.Add(new Color(r, g, b, alpha));
                            list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
                        }
                    }
                }
            }
            if (DeathsTerminus.ReleaseAsync)
                return;
            TextureCache.BlueSunTexture.Value.SetData(list.ToArray());

            logger.Info("Finished the (" + nameof(TextureCache.BlueSunTexture) + ") texture.");

            if (DeathsTerminus.ReleaseAsync)
                return;
            TextureCache.RainbowSunTexture = new Ref<Texture2D>(new Texture2D(Main.spriteBatch.GraphicsDevice, eclipseFrameSize * textureFramesX, eclipseFrameSize * textureFramesY, false, SurfaceFormat.Color));
            list = new List<Color>();
            for (int k = 0; k < textureFramesY; k++)
            {
                for (int j = 0; j < TextureCache.EclipseTexture.Value.Height / textureFramesY; j++)
                {
                    for (int l = 0; l < textureFramesX; l++)
                    {
                        for (int i = 0; i < TextureCache.EclipseTexture.Value.Width / textureFramesX; i++)
                        {
                            float x = (2 * i / (float)(TextureCache.EclipseTexture.Value.Width / textureFramesX - 1) - 1);
                            float y = (2 * j / (float)(TextureCache.EclipseTexture.Value.Height / textureFramesY - 1) - 1);

                            float radius = (float)Math.Sqrt(x * x + y * y);
                            float angle = (float)Math.Atan2(x, y);

                            int frame = k * textureFramesX + l;

                            float waveFunction = (float)(
                                    1 / 8f * Math.Cos(12 * (angle + 12f) + frame * MathHelper.TwoPi / 12 + 12f) +
                                    1 / 8f * Math.Cos(-15 * (angle + 15f) + frame * MathHelper.TwoPi / 15 + 15f) +
                                    1 / 8f * Math.Cos(-20 * (angle + 20f) + frame * MathHelper.TwoPi / 20 + 20f) +
                                    1 / 8f * Math.Cos(30 * (angle + 30f) + frame * MathHelper.TwoPi / 30 + 30f)
                                );

                            float luminosityFactor = (float)Math.Pow((1 - Math.Pow(radius, 4)), 2);
                            float waviness = (float)(0.5f * Math.Exp(-50 * Math.Pow(radius - 0.8f, 2)));
                            float index = luminosityFactor * (1 - waviness * waveFunction);

                            float hue = (index / 4f + frame / 60f) % 1;
                            float saturation = 1;
                            float luminosity = luminosityFactor;
                            Color color = Main.hslToRgb(hue, saturation, luminosity);

                            int r = color.R;//448 - (int)(512 * index);
                            int g = color.G;//384 - (int)(512 * index);
                            int b = color.B;//256 - (int)(512 * index);
                            int alpha = radius >= 1 ? 0 : (int)(255 * index);

                            //list.Add(new Color(r, g, b, alpha));
                            list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
                        }
                    }
                }
            }
            if (DeathsTerminus.ReleaseAsync)
                return;
            TextureCache.RainbowSunTexture.Value.SetData(list.ToArray());

            logger.Info("Finished the (" + nameof(TextureCache.RainbowSunTexture) + ") texture.");
        }
    }
}